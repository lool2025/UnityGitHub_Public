using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Save;
using Unity.Netcode;

public class Player : NetworkBehaviour, ISaveable
{
    // 物理相关
    private Rigidbody2D rb;
    private float inputX;
    private float inputY;
    private Vector2 movementInput;

    // 状态控制
    private bool isMoving;
    private bool inputDisable;
    private bool useTool;

    // 动画相关
    private Animator[] animators;
    private float mouseX;
    private float mouseY;

    // 引用AnimatorOverride脚本
    private AnimatorOvrride animatorOverride;

    // 网络变量 - 用于同步动画基础状态
    private NetworkVariable<bool> networkIsMoving = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<float> networkInputX = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<float> networkInputY = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<float> networkMouseX = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<float> networkMouseY = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // 网络变量 - 用于同步工具动画类型
    /// <summary>
    /// 当前工具类型
    /// </summary>
    private NetworkVariable<int> networkCurrentToolType = new NetworkVariable<int>(
        -1,  // -1 表示 None
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    /// <summary>
    /// 是否正在使用工具
    /// </summary>
    private NetworkVariable<bool> networkIsUsingTool = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // 本地变量
    private float lastSentInputX;
    private float lastSentInputY;
    private float lastSentMouseX;
    private float lastSentMouseY;
    private bool lastSentIsMoving;
    private int lastSentToolType = -1;
    private bool lastSentIsUsingTool;

    // 同步阈值（避免频繁同步）
    private float syncThreshold = 0.05f;

    // 存档相关
    public string GUID => GetComponent<DataGUID>().guid;

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        animatorOverride = GetComponent<AnimatorOvrride>();
        inputDisable = true;
    }

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);

        if (IsLocalPlayer)
        {
            // 本地玩家
            ISaveable saveable = this;
            saveable.RegisterSaveable();
            inputDisable = false;

            // 初始化网络变量
            networkIsMoving.Value = false;
            networkInputX.Value = 0;
            networkInputY.Value = 0;
            networkMouseX.Value = 0;
            networkMouseY.Value = 0;
            networkCurrentToolType.Value = -1;
            networkIsUsingTool.Value = false;
        }
        else
        {
            // 其他玩家：禁用输入 + 关闭刚体
            inputDisable = true;
            rb.isKinematic = true;

            // 订阅网络变量变化来同步动画
            SubscribeToAnimationChanges();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsLocalPlayer)
        {
            UnsubscribeFromAnimationChanges();
        }
    }

    /// <summary>
    /// 订阅动画相关的网络变量变化
    /// </summary>
    private void SubscribeToAnimationChanges()
    {
        networkIsMoving.OnValueChanged += OnIsMovingChanged;
        networkInputX.OnValueChanged += OnInputXChanged;
        networkInputY.OnValueChanged += OnInputYChanged;
        networkMouseX.OnValueChanged += OnMouseXChanged;
        networkMouseY.OnValueChanged += OnMouseYChanged;
        networkCurrentToolType.OnValueChanged += OnToolTypeChanged;
        networkIsUsingTool.OnValueChanged += OnIsUsingToolChanged;
    }

    private void UnsubscribeFromAnimationChanges()
    {
        networkIsMoving.OnValueChanged -= OnIsMovingChanged;
        networkInputX.OnValueChanged -= OnInputXChanged;
        networkInputY.OnValueChanged -= OnInputYChanged;
        networkMouseX.OnValueChanged -= OnMouseXChanged;
        networkMouseY.OnValueChanged -= OnMouseYChanged;
        networkCurrentToolType.OnValueChanged -= OnToolTypeChanged;
        networkIsUsingTool.OnValueChanged -= OnIsUsingToolChanged;
    }

    // 网络变量变化回调
    private void OnIsMovingChanged(bool oldValue, bool newValue)
    {
        if (!IsLocalPlayer)
        {
            SetAllAnimatorsBool("isMoving", newValue);
        }
    }

    private void OnInputXChanged(float oldValue, float newValue)
    {
        if (!IsLocalPlayer)
        {
            SetAllAnimatorsFloat("InputX", newValue);
        }
    }

    private void OnInputYChanged(float oldValue, float newValue)
    {
        if (!IsLocalPlayer)
        {
            SetAllAnimatorsFloat("InputY", newValue);
        }
    }

    private void OnMouseXChanged(float oldValue, float newValue)
    {
        if (!IsLocalPlayer)
        {
            SetAllAnimatorsFloat("mouseX", newValue);
        }
    }

    private void OnMouseYChanged(float oldValue, float newValue)
    {
        if (!IsLocalPlayer)
        {
            SetAllAnimatorsFloat("mouseY", newValue);
        }
    }

    private void OnToolTypeChanged(int oldValue, int newValue)
    {
        if (!IsLocalPlayer && animatorOverride != null)
        {
            // 将int转换回PartType并应用
            if (newValue >= 0)
            {
                animatorOverride.SwitchAnimator((PartType)newValue);
            }
        }
    }

    private void OnIsUsingToolChanged(bool oldValue, bool newValue)
    {
        if (!IsLocalPlayer && newValue)
        {
            // 其他玩家播放工具使用动画
            PlayToolAnimationClientRpc(networkMouseX.Value, networkMouseY.Value);
        }
    }

    private void Update()
    {
        if (!IsLocalPlayer) return;

        if (!inputDisable)
            PlayerInput();
        else
            isMoving = false;

        SwitchAnimation();

        // 同步动画参数到网络变量
        SyncAnimationParameters();
    }

    private void FixedUpdate()
    {
        if (!IsLocalPlayer) return;

        if (!inputDisable)
            Movement();
    }

    /// <summary>
    /// 同步动画参数到网络变量
    /// </summary>
    private void SyncAnimationParameters()
    {
        // 同步移动相关参数
        if (Mathf.Abs(inputX - lastSentInputX) > syncThreshold)
        {
            networkInputX.Value = inputX;
            lastSentInputX = inputX;
        }

        if (Mathf.Abs(inputY - lastSentInputY) > syncThreshold)
        {
            networkInputY.Value = inputY;
            lastSentInputY = inputY;
        }

        if (Mathf.Abs(mouseX - lastSentMouseX) > syncThreshold)
        {
            networkMouseX.Value = mouseX;
            lastSentMouseX = mouseX;
        }

        if (Mathf.Abs(mouseY - lastSentMouseY) > syncThreshold)
        {
            networkMouseY.Value = mouseY;
            lastSentMouseY = mouseY;
        }

        if (isMoving != lastSentIsMoving)
        {
            networkIsMoving.Value = isMoving;
            lastSentIsMoving = isMoving;
        }
    }

    /// <summary>
    /// 设置所有Animator的bool参数
    /// </summary>
    private void SetAllAnimatorsBool(string parameter, bool value)
    {
        foreach (var anim in animators)
        {
            anim.SetBool(parameter, value);
        }
    }

    /// <summary>
    /// 设置所有Animator的float参数
    /// </summary>
    private void SetAllAnimatorsFloat(string parameter, float value)
    {
        foreach (var anim in animators)
        {
            anim.SetFloat(parameter, value);
        }
    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition += OnMoveToPosition;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadedEvent;
        EventHandler.MoveToPosition -= OnMoveToPosition;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
    }

    /// <summary>
    /// 物品选中事件处理 - 同步工具类型
    /// </summary>
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        if (!IsLocalPlayer) return;

        if (animatorOverride == null) return;

        int toolType = -1;

        if (isSelected)
        {
            // 根据物品类型确定PartType
            PartType partType = itemDetails.itemType switch
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

            toolType = (int)partType;
        }

        // 同步工具类型到网络变量
        if (toolType != lastSentToolType)
        {
            networkCurrentToolType.Value = toolType;
            lastSentToolType = toolType;
        }
    }

    private void OnEndGameEvent()
    {
        if (IsLocalPlayer)
            inputDisable = true;
    }

    private void OnUpdateGameStateEvent(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.GamePlay:
                inputDisable = false;
                break;
            case GameState.Pause:
                inputDisable = true;
                break;
        }
    }

    private void OnStartNewGameEvent(int obj)
    {
        if (IsLocalPlayer)
        {
            inputDisable = false;
            transform.position = Settings.playerStartPos;
        }
    }

    private void OnMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        if (!IsLocalPlayer) return;

        if (itemDetails.itemType != ItemType.Seed &&
            itemDetails.itemType != ItemType.Commodity &&
            itemDetails.itemType != ItemType.Furniture)
        {
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);

            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
            {
                mouseY = 0;
            }
            else
            {
                mouseX = 0;
            }

            foreach (var anim in animators)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
                    break;
                }
            }
        }
        else
        {
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }
    }

    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        if (!IsLocalPlayer) yield break;

        useTool = true;
        inputDisable = true;

        // 同步工具使用状态
        networkIsUsingTool.Value = true;

        yield return null;

        // 播放工具动画（本地）
        foreach (var anim in animators)
        {
            anim.SetTrigger("useTool");
            anim.SetFloat("InputX", mouseX);
            anim.SetFloat("InputY", mouseY);
        }

        // 通知其他客户端播放工具动画
        PlayToolAnimationClientRpc(mouseX, mouseY);

        yield return new WaitForSeconds(0.45f);
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);

        yield return new WaitForSeconds(0.2f);
        useTool = false;
        inputDisable = false;

        // 结束工具使用状态
        networkIsUsingTool.Value = false;
    }

    /// <summary>
    /// 通知所有客户端播放工具动画
    /// </summary>
    [ClientRpc]
    private void PlayToolAnimationClientRpc(float animMouseX, float animMouseY)
    {
        // 本地玩家已经播放过，不需要重复播放
        if (IsLocalPlayer) return;

        foreach (var anim in animators)
        {
            anim.SetTrigger("useTool");
            anim.SetFloat("InputX", animMouseX);
            anim.SetFloat("InputY", animMouseY);
        }
    }

    private void OnMoveToPosition(Vector3 vector)
    {
        if (IsLocalPlayer)
            transform.position = vector;
    }

    private void OnAfterSceneLoadedEvent()
    {
        if (IsLocalPlayer)
            inputDisable = false;
    }

    private void OnBeforeSceneUnloadEvent()
    {
        if (IsLocalPlayer)
            inputDisable = true;
    }

    private void PlayerInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");
        inputX = 0.5f * inputX;
        inputY = 0.5f * inputY;

        if (inputX != 0 && inputY != 0)
        {
            inputX = 0.6f * inputX;
            inputY = 0.6f * inputY;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            inputX = 2f * inputX;
            inputY = 2f * inputY;
        }

        movementInput = new Vector2(inputX, inputY);
        isMoving = movementInput != Vector2.zero;
    }

    private void Movement()
    {
        rb.MovePosition(rb.position + movementInput * StatsManager.Instance.speed * Time.deltaTime);
    }

    private void SwitchAnimation()
    {
        foreach (var anim in animators)
        {
            anim.SetBool("isMoving", isMoving);
            anim.SetFloat("mouseX", mouseX);
            anim.SetFloat("mouseY", mouseY);

            if (isMoving)
            {
                anim.SetFloat("InputX", inputX);
                anim.SetFloat("InputY", inputY);
            }
        }
    }

    public void Knockback(Transform enemy, float knockbackForce, float stunTime)
    {
        if (!IsLocalPlayer) return;

        inputDisable = true;
        Vector2 direction = (transform.position - enemy.position).normalized;
        rb.velocity = direction * knockbackForce;
        StartCoroutine(KnockbackCounter(stunTime));
    }

    IEnumerator KnockbackCounter(float stunTime)
    {
        if (!IsLocalPlayer) yield break;

        yield return new WaitForSeconds(stunTime);
        rb.velocity = Vector2.zero;
        inputDisable = false;
    }

    public GameSaveData GenerateSaveData()
    {
        if (!IsLocalPlayer) return null;

        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add(this.name, new SerializableVector3(transform.position));
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        if (!IsLocalPlayer) return;

        var targetPosition = saveData.characterPosDict[this.name].ToVector3();
        transform.position = targetPosition;
    }
}