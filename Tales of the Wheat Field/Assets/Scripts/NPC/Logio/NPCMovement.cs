using System;
using System.Collections;
using System.Collections.Generic;
using MFarm.AStar;
using MFarm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour, ISaveable
{
    //行为数据
    public ScheduleDataList_SO scheduleData;

    private SortedSet<ScheduleDetails> scheduleSet;

    private ScheduleDetails currentSchedule;

    [SerializeField] public string currentScene;

    private string targetScene;

    private Vector3Int currentGridPosition;

    private Vector3Int targetGridPosition;

    private Vector3 nextWorldPosition;

    private Vector3Int nextGridPosition;
    public string StartScene { set => currentScene = value; }

    [Header("移动属性")]
    public float normalSpeed = 2f;
    private float minSpeed = 1;
    private float maxSpeed = 3;

    private Vector2 dir;
    /// <summary>
    /// 是否在移动（到达终点）
    /// </summary>
    public bool isMoving;

    private Grid grid;

    //Components
    private Rigidbody2D rb;

    private SpriteRenderer spriteRenderer;

    private BoxCollider2D coll;

    private Animator anim;

    private Stack<MovementStep> movementSteps;

    private bool npcMove;

    private TimeSpan GameTime => TimeManager.Instance.GameTime;

    public string GUID => GetComponent<DataGUID>().guid;

    private bool isInitialised;

    private Coroutine npcMoveRoutine;

    [Header("npc能否对话")]
    public bool interactable;

    private bool sceneLoaded;

    public bool isFirstLoad;
    public Season currentSeason;

    //计时器
    private float animationBreakTime;
    private bool canPlayStopAnimation;

    private AnimationClip stopAnimationClip;

    public AnimationClip blankANimationClip;
    private AnimatorOverrideController animOverride;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        movementSteps = new Stack<MovementStep>();

        animOverride=new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController=animOverride;
        scheduleSet = new SortedSet<ScheduleDetails>();
        foreach (ScheduleDetails schedule in scheduleData.scheduleList)
        {
            scheduleSet.Add(schedule);
        }
    }


    public void Update()
    {
        if(sceneLoaded)
        {
            SwitchAnimation();
        }
        //计时器
        animationBreakTime-=Time.deltaTime;
        canPlayStopAnimation = animationBreakTime <= 0;
    }
    

    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }


    private void OnEndGameEvent()
    {
        sceneLoaded = false;
        npcMove=false;
        if(npcMoveRoutine != null)
        {
            StopCoroutine(npcMoveRoutine);
        }
    }


    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        isInitialised = false;
        isFirstLoad = true;
    }

    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        int time=(hour*100)+minute;
        currentSeason = season;
        ScheduleDetails matchSchedule=null;
        foreach(var schedule in scheduleSet)
        {
            if (schedule.Time ==time)
            {
                if(schedule.day!=day&&schedule.day!=0)
                    continue;
                if (schedule.season != season)
                    continue;
                matchSchedule = schedule;
            }else if(schedule.Time>time)
            {
                break;
            }
        }
        if (matchSchedule != null)
        {
            BuildPath(matchSchedule);
        }
    }

    private void FixedUpdate()
    {
        if (sceneLoaded)
        {
            Movenent();
        }
           
    }
    private void OnBeforeSceneUnloadEvent()
    {
        sceneLoaded = false;
    }


    private void OnAfterSceneLoadEvent()
    {
        grid = FindObjectOfType<Grid>();
        CheckVisiable();

        if (!isInitialised)
        {
            InitNPC();
            isInitialised = true;
        }
        sceneLoaded = true;
        //判断是否是第一次加载
        if(!isFirstLoad)
        {
            currentGridPosition = grid.WorldToCell(transform.position);

            var schedule=new ScheduleDetails(0,0,0,0,currentSeason,targetScene,(Vector2Int)targetGridPosition,stopAnimationClip,interactable);
            BuildPath(schedule);
            isFirstLoad = true;
        }


    }

    private void CheckVisiable()
    {


        if (currentScene == SceneManager.GetActiveScene().name)
        {

            SetActiveInScene();
        }
        else
        {

            SetInactiveInScene();
        }
    }

    private void InitNPC()
    {
        targetScene = currentScene;
        currentGridPosition = grid.WorldToCell(transform.position);
        //确保位置处于网格的中心
        transform.position = new Vector3(currentGridPosition.x + Settings.gridCellSize / 2f, currentGridPosition.y + Settings.gridCellSize / 2f, 0);

        targetGridPosition = currentGridPosition;
    }

    private void Movenent()
    {

        if (!npcMove) // 检查NPC是否正在移动（防止重复移动）
        {
            if (movementSteps.Count > 0) // 检查是否有未执行的移动步骤
            {
                MovementStep step = movementSteps.Pop();
                currentScene = step.sceneName;
                CheckVisiable();
             
                nextGridPosition = (Vector3Int)step.gridCoordinate;
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);
              
                MoveToGridPosition(nextGridPosition, stepTime);
            }else if (!isMoving && canPlayStopAnimation)
            {
                StartCoroutine(SetStopAnimation());
            }


        }
    }

    private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
    {
         npcMoveRoutine= StartCoroutine(MoveRoutine(gridPos, stepTime));
    }

    private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
    {
        npcMove = true; // 标记NPC正在移动（阻止`Movement()`重复触发）
        nextWorldPosition = GetWorldPosition(gridPos); // 将网格坐标转换为世界坐标（实际移动的目标位置）
       
        // 检查目标到达时间是否晚于当前游戏时间（如果还没到移动截止时间，则需要平滑移动）
        if (stepTime > GameTime)
        {
            
            // 计算剩余移动时间（目标时间 - 当前游戏时间，单位：秒）
            float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
          
            // 计算当前位置到目标位置的直线距离
            float distance = Vector3.Distance(transform.position, nextWorldPosition);
            // 计算移动速度：确保速度不低于最小速度（minSpeed），且基于距离和剩余时间动态调整
            // Settings.secondThreshold 可能是一个时间阈值（如1秒），用于避免速度过小
            float speed = Mathf.Max(minSpeed, (distance / timeToMove / Settings.secondThreshold));
         
            // 限制速度不超过最大速度（maxSpeed）
            if (speed <= maxSpeed)
            {
                // 循环移动：直到距离目标位置小于一个像素单位（Settings.pixelSize，避免因浮点数误差无法停止）
                while (Vector3.Distance(transform.position, nextWorldPosition) > Settings.pixelSize)
                {
                    dir = (nextWorldPosition - transform.position).normalized; // 计算单位方向向量（确保移动方向正确）

                    // 计算每帧的位置偏移（与之前代码呼应，基于速度和固定时间步长）
                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime);
                    // 通过刚体（Rigidbody2D）移动：确保与物理系统同步（适合有碰撞的场景）
                    rb.MovePosition(rb.position + posOffset);
                
                    yield return new WaitForFixedUpdate(); // 等待下一个物理帧（与FixedUpdate同步，移动更平滑）
                   
                }
            }
        }
      
        // 若已到目标时间（或速度超出上限），直接瞬移到目标位置（避免超时未到达）
        rb.position = nextWorldPosition;
        currentGridPosition = gridPos; // 更新当前网格位置
        nextGridPosition = currentGridPosition; // 重置下一个网格位置（避免残留）
        transform.position = rb.position; // 同步Transform位置与刚体位置（防止偏差）

        npcMove = false; // 标记移动结束（允许下一次移动）
    }

    /// <summary>
    /// 根据ScheduleDetails构建路径
    /// </summary>
    /// <param name="schedule"></param>
    public void BuildPath(ScheduleDetails schedule)
    {
        movementSteps.Clear();

        currentSchedule = schedule;
        targetScene=schedule.targetScene;
        targetGridPosition= (Vector3Int)schedule.targetGridPosition;

        this.interactable=schedule.interactable;
        stopAnimationClip=schedule.clipAtStop;
        if (schedule.targetScene == currentScene)
        {
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)currentGridPosition, schedule.targetGridPosition, movementSteps);
        }else if(schedule.targetScene != currentScene)
        {
            SceneRoute sceneRoute=NPCManager.Instance.GetSceneRoute(currentScene,schedule.targetScene);
            if (sceneRoute != null)
            {
                for (int i = 0;i<sceneRoute.scenePathList.Count;i++)
                {
                    Vector2Int fromPos,gotoPos;
                    ScenePath path = sceneRoute.scenePathList[i];
                    if (path.fromGridCell.x >= Settings.maxGridSize)
                    {
                        fromPos = (Vector2Int)currentGridPosition;
                    }
                    else
                    {
                        fromPos=path.fromGridCell;
                    }
                    if(path.gotoGridCell.x >= Settings.maxGridSize)
                    {
                        gotoPos = schedule.targetGridPosition;
                    }
                    else
                    {
                        gotoPos=path.gotoGridCell;
                    }
                    AStar.Instance.BuildPath(path.sceneName,fromPos, gotoPos, movementSteps);
                }
            }
        }

        if (movementSteps.Count > 1)
        {        
            //更新每一步对应的时间戳
            UpdateTimeOnPath();
        }

    }

    private void UpdateTimeOnPath()
    {
        MovementStep previousStep = null;
       
        TimeSpan currentGameTime = GameTime;

        foreach (MovementStep step in movementSteps)
        {
           
            if (previousStep == null)
            {
                previousStep = step;
            }
            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            TimeSpan gridMovementStepTime;
            if (MoveInDiagonal(step, previousStep))//每一步消耗的时间
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            else
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));
            //累加获取下一步的时间戳
            currentGameTime = currentGameTime.Add(gridMovementStepTime);
           
            //循环
            previousStep = step;
        }
     
    }
    /// <summary>
    /// 判断npc是否走斜方向
    /// </summary>
    /// <param name="currentStep"></param>
    /// <param name="nextStep"></param>
    /// <returns></returns>
    private bool MoveInDiagonal(MovementStep currentStep, MovementStep nextStep)
    {
        return (currentStep.gridCoordinate.x != nextStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != nextStep.gridCoordinate.y);
    }
    /// <summary>
    /// 网格坐标返回世界坐标中心
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    private Vector3 GetWorldPosition(Vector3Int gridPos)
    {
        Vector3 worldPos = grid.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2, worldPos.y + Settings.gridCellSize / 2);
    }


    private void SwitchAnimation()
    {
        isMoving=transform.position!=GetWorldPosition(targetGridPosition);
        anim.SetBool("isMoving",isMoving);
        if(isMoving)
        {
            anim.SetBool("Exit", true);
            anim.SetFloat("DirX",dir.x);
            anim.SetFloat("DirY",dir.y);
        }
        else
        {
            anim.SetBool("Exit", false);
        }


    }

    private IEnumerator SetStopAnimation()
    {
        //强制面向镜头
        anim.SetFloat("DirX", 0);
        anim.SetFloat("DirY", -1);

        animationBreakTime=Settings.animationBreakTime;

        if (stopAnimationClip != null)
        {
            animOverride[blankANimationClip] = stopAnimationClip;
            anim.SetBool("EventAnimation",true);
            yield return null;
            anim.SetBool("EventAnimation", false);
        }
        else
        {
            animOverride[stopAnimationClip] = blankANimationClip;
            anim.SetBool("EventAnimation", false);
        }
    }


    #region 设置NPC显示情况
    private void SetActiveInScene()
    {
        spriteRenderer.enabled = true;
        coll.enabled = true;
       
         transform.GetChild(0).gameObject.SetActive(true);
    }

    private void SetInactiveInScene()
    {
        spriteRenderer.enabled = false;
        coll.enabled = false;
       
        transform.GetChild(0).gameObject.SetActive(false);
    }
    #endregion
    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add("targetGridPosition",new SerializableVector3(targetGridPosition));
        saveData.characterPosDict.Add("currentPosition", new SerializableVector3(transform.position));
        saveData.dataSceneName = currentScene;
        saveData.targetScene=this.targetScene;
        if(stopAnimationClip != null)
        {
            saveData.animationInstanceID = stopAnimationClip.GetInstanceID();
        }
        saveData.interactable=this.interactable;
        Debug.Log("currentSeason"+currentSeason);
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("currentSeason", (int)currentSeason);
        return saveData;

    }

    public void RestoreData(GameSaveData saveData)
    {
        isInitialised=true;
        isFirstLoad = false;
        currentScene =saveData.dataSceneName;
        targetScene =saveData.targetScene;

        Vector3 pos =saveData.characterPosDict["currentPosition"].ToVector3();
        Vector3Int gridpos = saveData.characterPosDict["targetGridPosition"].ToVector3Int();

        transform.position = pos;
        targetGridPosition = gridpos;

        if(saveData.animationInstanceID!=0)
        {
            this.stopAnimationClip=Resources.InstanceIDToObject(saveData.animationInstanceID)as AnimationClip;
        }

        this.interactable = saveData.interactable;
        this.currentSeason = (Season)saveData.timeDict["currentSeason"];
    }
  




}
