using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.Save;

public class PlayerSingle : MonoBehaviour, ISaveable
{
    // 物理相关
    private Rigidbody2D rb;          // 2D刚体组件，控制玩家移动物理
                                     // 基础移动速度
    private float inputX;            // 水平输入值（-1/0/1）
    private float inputY;            // 垂直输入值（-1/0/1）
    private Vector2 movementInput;   // 最终的移动输入向量

    // 状态控制
    private bool isMoving;           // 是否处于移动状态（用于动画）
    private bool inputDisable;       // 输入禁用标记（场景切换/使用工具/暂停时禁用输入）
    private bool useTool;            // 是否正在使用工具（动画状态）

    // 动画相关
    private Animator[] animators;    // 角色子物体的所有Animator组件（多部位动画）
    private float mouseX;            // 鼠标点击位置相对玩家的X偏移（工具使用时的朝向）
    private float mouseY;            // 鼠标点击位置相对玩家的Y偏移（工具使用时的朝向）

    // 存档相关
    public string GUID => GetComponent<DataGUID>().guid; // 唯一标识（存档时绑定玩家位置）

    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();

        rb = GetComponent<Rigidbody2D>();

        inputDisable = true;

    }

    public void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void Update()
    {
        //当加载场景之前玩家不能动
        if (!inputDisable)
            PlayerInput();
        else
            isMoving = false;
        SwitchAnimation();
    }
    private void FixedUpdate()
    {
        if (!inputDisable)
            Movement();
    }
    private void OnEnable()
    {
        // 绑定游戏核心事件
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;  // 场景卸载前
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadedEvent;      // 场景加载后
        EventHandler.MoveToPosition += OnMoveToPosition;                  // 移动到指定位置
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;            // 鼠标点击（使用工具/物品）
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;      // 游戏状态更新（暂停/游玩）
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;            // 开始新游戏
        EventHandler.EndGameEvent += OnEndGameEvent;                      // 结束游戏

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

    }

    private void OnEndGameEvent()
    {
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
       // inputDisable = false; // 启用输入
        transform.position = Settings.playerStartPos; // 移动到初始出生点
    }

    private void OnMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        // 非种子/商品/家具类 → 是工具（锄头/水壶/斧头等），需要播放工具动画
        if (itemDetails.itemType != ItemType.Seed && itemDetails.itemType != ItemType.Commodity && itemDetails.itemType != ItemType.Furniture)
        {
            // 计算鼠标相对玩家的偏移（修正Y轴偏移，匹配角色锚点）
            mouseX = mouseWorldPos.x - transform.position.x;
            mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);

            // 简化朝向：只保留X/Y中绝对值更大的方向（上下/左右，避免斜向工具动画）
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
                    // 启动工具使用协程（播放动画+延迟执行动作）
                    StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
                    break;
                }
            }


        }
        else
        {
            // 种子/商品/家具 → 直接执行动作（无需工具动画）
            EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        }


    }

    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        useTool = true;          // 标记使用工具状态
        inputDisable = true;   // 禁用输入（动画播放中不能移动）
        yield return null;     // 等待一帧（确保状态生效）

        // 给所有Animator设置工具使用触发和朝向
        foreach (var anim in animators)
        {

            anim.SetTrigger("useTool");       // 触发工具使用动画
            anim.SetFloat("InputX", mouseX);   // 设置动画朝向X
            anim.SetFloat("InputY", mouseY);  // 设置动画朝向Y


        }

        yield return new WaitForSeconds(0.45f); // 等待动画播放到关键帧（执行实际动作）
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails); // 触发工具动作（锄地/浇水等）

        yield return new WaitForSeconds(0.2f); // 等待动画结束
        useTool = false;       // 取消工具使用状态
        inputDisable = false;    // 恢复输入
    }

    private void OnMoveToPosition(Vector3 vector)
    {
        transform.position = vector;  // 响应“移动到指定位置”事件（如传送、读档后定位）
    }

    private void OnAfterSceneLoadedEvent()
    {
        inputDisable = false; // 场景加载完成后启用输入
    }

    private void OnBeforeSceneUnloadEvent()
    {
        inputDisable = true; // 场景卸载前禁用输入（避免移动中切换场景）
    }


    /// <summary>
    /// 移动
    /// </summary>
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


            anim.SetBool("isMoving", isMoving);
            if (isMoving)
            {
                anim.SetFloat("InputX", inputX);
                anim.SetFloat("InputY", inputY);
            }
        }
    }
    /// <summary>
    /// 玩家击退
    /// </summary>
    /// <param name="enemy"></param>
    public void Knockback(Transform enemy, float knockbackForce, float stunTime)
    {
        inputDisable = true;
        Vector2 direction = (transform.position - enemy.position).normalized;
        rb.velocity = direction * knockbackForce;
        StartCoroutine(KnockbackCounter(stunTime));
    }
    /// <summary>
    /// 击退后恢复
    /// </summary>
    /// <returns></returns>
    IEnumerator KnockbackCounter(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.velocity = Vector2.zero;
        inputDisable = false;
    }

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add(this.name, new SerializableVector3(transform.position));
        return saveData;

    }

    public void RestoreData(GameSaveData saveData)
    {
        var targetPosition = saveData.characterPosDict[this.name].ToVector3();
        transform.position = targetPosition;

    }
}
