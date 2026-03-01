using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Inventory;

public class AnimatorOvrrideSingle : MonoBehaviour
{
    //单例：自己添加
    public static AnimatorOvrrideSingle Instance { get; private set; }

    private Animator[] animators;   //子物体的Animator组件
    //举起物品图片
    public SpriteRenderer holdItem;

    [Header("各部分动画列表")]
    public List<AnimatorType> animatorTypes;

    public Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();

    private void Awake()
    {  ////单例：自己添加
        // 确保只有一个实例存在
        if (Instance == null)
        {
            Instance = this;
            // 可选：如果希望在场景切换时不销毁该实例
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 如果已有实例，则销毁当前对象
            Destroy(gameObject);
        }

        animators = GetComponentsInChildren<Animator>();

        foreach (var anim in animators)
        {
            animatorNameDict.Add(anim.name, anim);
        }
    }

    private void OnEnable()
    {
        // 物品选中/取消选中事件
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        //场景切换中取消举起和恢复原始动画
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        // 玩家位置收获物品事件
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
    }

    private void OnHarvestAtPlayerPosition(int ID)
    {
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
        //将举起的东西放下
        holdItem.enabled = false;
        //还原动画
        SwitchAnimator(PartType.None);
    }
    /// <summary>
    /// 根据不同的物品来选择动画
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        //TODO:不同的工具返回不同的动画在这里补全
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
            // ItemType.Furniture => PartType.Carry,
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
    }
    /// <summary>
    /// 根据不同的PartType来切换
    /// </summary>
    /// <param name="partType"></param>animator
    private void SwitchAnimator(PartType partType)
    {

        foreach (var item in animatorTypes)
        {
            if (item.partType == partType)
            {

                animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;

            }
        }
    }
}
