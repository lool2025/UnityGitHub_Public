using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Inventory;
using Unity.Netcode;

public class AnimatorOvrride : NetworkBehaviour
{
    private Animator[] animators;   //子物体的Animator组件
    //举起物品图片
    public SpriteRenderer holdItem;

    [Header("各部分动画列表")]
    public List<AnimatorType> animatorTypes;

    public Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();

    // 网络变量 - 同步当前动画类型
    private NetworkVariable<int> networkCurrentPartType = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();

        foreach (var anim in animators)
        {
            animatorNameDict.Add(anim.name, anim);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsLocalPlayer)
        {
            // 其他玩家监听网络变量变化
            networkCurrentPartType.OnValueChanged += OnPartTypeChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsLocalPlayer)
        {
            networkCurrentPartType.OnValueChanged -= OnPartTypeChanged;
        }
    }

    private void OnPartTypeChanged(int oldValue, int newValue)
    {
        if (!IsLocalPlayer)
        {
            // 其他玩家根据网络变量切换动画
            if (newValue >= 0)
            {
                SwitchAnimator((PartType)newValue);
            }
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
        EventHandler.PlayerAnimNone += OnBeforeSceneUnloadEvent;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
        EventHandler.PlayerAnimNone -= OnBeforeSceneUnloadEvent;
    }

    private void OnHarvestAtPlayerPosition(int ID)
    {
        if (!IsLocalPlayer) return; // 只有本地玩家处理

        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(ID).itemOnWorldSprite;
        if (holdItem.enabled == false)
        {
            StartCoroutine(ShowItem(itemSprite));
        }
    }

    private IEnumerator ShowItem(Sprite itemSprite)
    {
        holdItem.sprite = itemSprite;
        holdItem.enabled = true;
        yield return new WaitForSeconds(0.4f);
        holdItem.enabled = false;
    }

    public void OnBeforeSceneUnloadEvent()
    {
        if (!IsLocalPlayer) return; // 只有本地玩家处理

        holdItem.enabled = false;
        SwitchAnimator(PartType.None);

        // 同步到网络变量
        if (IsLocalPlayer)
        {
            networkCurrentPartType.Value = -1;
        }
    }

    /// <summary>
    /// 根据不同的物品来选择动画
    /// </summary>
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        if (!IsLocalPlayer) return; // 只有本地玩家处理

        PartType currentType = itemDetails.itemType switch
        {
            ItemType.Seed => PartType.Carry,
            ItemType.Commodity => PartType.Carry,
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.CollectTool => PartType.Collect,
            ItemType.ChopTool => PartType.Chop,
            ItemType.BreakTool => PartType.Break,
            ItemType.ReapTool => PartType.Reap,
            ItemType.SwordTool => PartType.Sword,
            _ => PartType.None
        };

        if (isSelected == false)
        {
            currentType = PartType.None;
            holdItem.enabled = false;
        }
        else
        {
            if (currentType == PartType.Carry)
            {
                holdItem.sprite = itemDetails.itemOnWorldSprite;
                holdItem.enabled = true;
            }
            else
            {
                holdItem.enabled = false;
            }
        }

        SwitchAnimator(currentType);

        // 同步到网络变量
        networkCurrentPartType.Value = (int)currentType;
    }

    /// <summary>
    /// 根据不同的PartType来切换动画
    /// </summary>
    public void SwitchAnimator(PartType partType)
    {
        foreach (var item in animatorTypes)
        {
            if (item.partType == partType)
            {
                if (animatorNameDict.ContainsKey(item.partName.ToString()))
                {
                    animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
                }
            }
        }
    }
}