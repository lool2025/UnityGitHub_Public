using UnityEngine;
using System;
using MFarm.Save;
using System.Collections.Generic;
public class TimeManager : Singleton<TimeManager>,ISaveable
{
    // 游戏时间单位
    private int gameSecond, gameMinute, gameHour, gameDay, gameMonth, gameYear;
    private Season gameSeason = Season.春天;
    private int monthInSeason = 3;

    // 时间控制参数
    public bool gameClockPause;
    private float tikTime;
    //灯光时间差
    private float timeDifference;

    public TimeSpan GameTime => new TimeSpan(gameHour,gameMinute,gameSecond);

    public string GUID => GetComponent<DataGUID>().guid;

   

    public void Start()
    {

        ISaveable saveable = this;
        saveable.RegisterSaveable();
        gameClockPause = true;
        //EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear,gameSeason);
        //EventHandler.CallGameMinuteEvent(gameMinute, gameHour,gameDay,gameSeason);

        //EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
    }

    // 每帧更新游戏时间
    public void Update()
    {
      
        if (!gameClockPause)
        {
            
            tikTime += Time.deltaTime; // 累积时间

            // 使用while循环确保所有累积时间被处理
            while (tikTime >= Settings.secondThreshold)
            {
                tikTime -= Settings.secondThreshold;
                UpdateGameTime(); // 修正方法名
            }
        }
        if (Input.GetKey(KeyCode.T)) 
        {
            for (int i = 0; i < 60; i++) 
            {
                UpdateGameTime();    
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            gameDay++;
            EventHandler.CallGameDayEvent(gameDay, gameSeason);
            EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        }
    }


    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        //新游戏
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

   

    private void OnDisable()
    {
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        //新游戏
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        gameClockPause=true;
    }

    private void OnStartNewGameEvent(int obj)
    {
        NewGameTime();
        gameClockPause = false;
    }

    private void OnUpdateGameStateEvent(GameState gameState)
    {
        gameClockPause= gameState==GameState.Pause;
        //if (gameClockPause)
        //{
        //    PauseGame();
        //}
        //else
        //{
        //    ResumeGame();
        //}
        
    }

    private void OnBeforeSceneUnloadEvent()
    {
        gameClockPause = true;
    }
    private void OnAfterSceneLoadEvent()
    {
       gameClockPause=false;

        EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
        EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);

        EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
    }

    // 设置游戏初始时间
    private void NewGameTime()
    {
        gameSecond = 0;
        gameMinute = 0;
        gameHour = 7;
        gameDay = 1;
        gameMonth = 1;
        gameYear = 2025;
        gameSeason = Season.春天;
        monthInSeason = 3;
    }

    // 更新游戏时间（修正后的逻辑）
    private void UpdateGameTime()
    {
        // 增加秒数
        gameSecond++;

        // 秒进位到分钟
        if (gameSecond > Settings.secondHold)
        {
            gameSecond = 0;
            gameMinute++;

            // 分钟进位到小时
            if (gameMinute > Settings.minuteHold)
            {
                gameMinute = 0;
                gameHour++;

                // 小时进位到天
                if (gameHour > Settings.hourHold)
                {
                    gameHour = 0;
                    gameDay++;

                    // 天进位到月
                    if (gameDay > Settings.dayHold)
                    {
                        gameDay = 1;
                        gameMonth++;
                        monthInSeason--;

                        // 月进位到年
                        if (gameMonth > 12)
                        {
                            gameMonth = 1;
                            gameYear++;

                            // 处理年份上限
                            if (gameYear > 9999)
                            {
                                gameYear = 2022;
                            }
                        }

                        // 季节逻辑
                        if (monthInSeason == 0)
                        {
                            monthInSeason = 3;
                            int seasonNumber = (int)gameSeason;
                            seasonNumber++;

                            if (seasonNumber > (int)Season.冬天) // 使用枚举上限更安全
                            {
                                seasonNumber = 0;
                            }

                            gameSeason = (Season)seasonNumber;
                        }
                    }

                    EventHandler.CallGameDayEvent(gameDay, gameSeason);
                }
                EventHandler.CallGameDateEvent(gameHour, gameDay, gameMonth, gameYear, gameSeason);
            }
            EventHandler.CallGameMinuteEvent(gameMinute, gameHour, gameDay, gameSeason);

            //切换灯光
            EventHandler.CallLightShiftChangeEvent(gameSeason, GetCurrentLightShift(), timeDifference);
        }

       
    }
    /// <summary>
    /// 返回LightShift并计算时间戳
    /// </summary>
    /// <returns></returns>
    private LightShift GetCurrentLightShift()
    {
        if (GameTime >= Settings.moringTime && GameTime < Settings.nigheTime)
        {
            timeDifference=(float)(GameTime - Settings.moringTime).TotalMinutes;
            return LightShift.Morning;
        }
        if(GameTime < Settings.moringTime || GameTime > Settings.nigheTime)
        {
            timeDifference = (float)Math.Abs((GameTime - Settings.nigheTime).TotalMinutes);

            return LightShift.Night;    
        }
        return LightShift.Morning;
    }


    // 暂停游戏
    public void PauseGame()
    {
        Time.timeScale = 0;
        // 可选：锁定帧率，避免暂停后CPU占用过高
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    
    // 恢复游戏
    public void ResumeGame()
    {
        Time.timeScale = 1;
        // 恢复帧率
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 200;
    }


    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("gameYear",gameYear); 
        saveData.timeDict.Add("gameSeason", (int)gameSeason);
        saveData.timeDict.Add("gameMonth", gameMonth);
        saveData.timeDict.Add("gameDay", gameDay);
        saveData.timeDict.Add("gameHour", gameHour);
        saveData.timeDict.Add("gameMinute", gameMinute);
        saveData.timeDict.Add("gameSecond", gameSecond);
        
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        gameYear = saveData.timeDict["gameYear"];
        gameSeason = (Season)saveData.timeDict["gameSeason"];
        gameMonth = saveData.timeDict["gameMonth"];
        gameDay = saveData.timeDict["gameDay"];
        gameHour = saveData.timeDict["gameHour"];
        gameMinute = saveData.timeDict["gameMinute"];
        gameSecond = saveData.timeDict["gameSecond"];
        
    }
}

