using System;
using System.Collections;
using System.Collections.Generic;
using MFarm.Dialogue;
using UnityEngine;

public static class EventHandler
{
    // 声明一个事件：当库存更新时触发
    // 参数1：InventoryLocation - 指示哪个库存（如玩家背包、商店）
    // 参数2：List<InventoryItem> - 库存中的物品列表
    public static event Action<InventoryLocation, List<InventoryItem>> UpdateInventoryUI;

    // 触发事件的静态方法
    public static void CallUpdataInventoryUI(InventoryLocation location, List<InventoryItem> list)
    {
        UpdateInventoryUI?.Invoke(location, list);
    }
    /// <summary>
    /// 在场景中实例化物体
    /// </summary>
    public static event Action<int, Vector3> InstantiateItemInScene;

    public static void CallInstantiateItemInScene(int ID, Vector3 pos)
    {
        InstantiateItemInScene?.Invoke(ID, pos);
    }
    /// <summary>
    /// 人物扔东西
    /// </summary>
    public static event Action<int, Vector3, ItemType> DropItemEvent;
    public static void CallDropItemEvent(int ID, Vector3 pos, ItemType itemType)
    {
        DropItemEvent?.Invoke(ID, pos, itemType);
    }
    /// <summary>
    /// 点击物品事件
    /// </summary>
    public static event Action<ItemDetails, bool> ItemSelectedEvent;

    public static void CallItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        ItemSelectedEvent.Invoke(itemDetails, isSelected);
    }

   
    /// <summary>
    /// 切换联机状态
    /// </summary>
    public static Action<GameMode> SwitchGameModeEvent;
    public static void CallSwitchGameModeEvent(GameMode gameMode)
    {
        SwitchGameModeEvent.Invoke(gameMode);
    }

    public static event Action<int, int,int ,Season> GameMinuteEvent;

    public static void CallGameMinuteEvent(int minute, int hour,int day, Season season)
    {
        GameMinuteEvent?.Invoke(minute, hour, day, season);
    }
    /// <summary>
    /// 时间
    /// </summary>
    public static event Action<int, int, int, int, Season> GameDateEvent;

    public static void CallGameDateEvent(int hour, int day, int month, int year, Season season)
    {
        GameDateEvent?.Invoke(hour, day, month, year, season);
    }

    public static event Action<int, Season> GameDayEvent;
    public static void CallGameDayEvent(int day, Season season)
    {
        GameDayEvent?.Invoke(day, season);
    }


    /// <summary>
    /// 切换场景
    /// </summary>
    public static event Action<string, Vector3> TransitionEvent;

    public static void CallTransitionEvent(string sceneName, Vector3 pos)
    {
        TransitionEvent?.Invoke(sceneName, pos);
    }
    /// <summary>
    /// 卸载场景之前要做的事情
    /// </summary>
    public static event Action BeforeSceneUnloadEvent;
    public static void CallBeforeSceneUnloadEvent()
    {
        BeforeSceneUnloadEvent?.Invoke();
    }
    /// <summary>
    /// 加载场景之后要做的事情
    /// </summary>
    public static event Action AfterSceneLoadEvent;
    public static void CallAfterSceneLoadedEvent()
    {
        AfterSceneLoadEvent?.Invoke();
    }

    /// <summary>
    /// 移动player位置
    /// </summary>
    public static event Action<Vector3> MoveToPosition;
    public static void CallMoveToPosition(Vector3 targetPosition)
    {
        MoveToPosition?.Invoke(targetPosition);
    }
  /// <summary>
  /// 鼠标点击事件
  /// </summary>
    public static event Action<Vector3, ItemDetails> MouseClickedEvent;

    public static void CallMouseClickedEvent(Vector3 pos, ItemDetails itemDetails)
    {
        MouseClickedEvent?.Invoke(pos, itemDetails);
    }

    public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimation;
    public static void CallExecuteActionAfterAnimation(Vector3 pos, ItemDetails itemDetails)
    {
        ExecuteActionAfterAnimation?.Invoke(pos, itemDetails);
    }
    /// <summary>
    /// 植物种植事件
    /// </summary>
    public static event Action<int, TileDetails> PlantSeedEvent;
    public static void CallPlantSeedEvent(int ID, TileDetails tile)
    {
        PlantSeedEvent?.Invoke(ID, tile);
    }
    /// <summary>
    /// // 玩家位置收获物品事件
    /// </summary>
    public static event Action<int> HarvestAtPlayerPosition;
    public static void CallHarvestAtPlayerPosition(int ID)
    {
        HarvestAtPlayerPosition?.Invoke(ID);
    }
    //刷新当前地图
    public static event Action RefreshCurrentMap;
    public static void CallRefreshCurrentMap()
    {
        RefreshCurrentMap?.Invoke();
    }
    //粒子特效
    public static event Action<ParticaleEffectType, Vector3> ParticleEffectEvent;
    public static void CallParticleEffectEvent(ParticaleEffectType effectType, Vector3 pos)
    {
        ParticleEffectEvent?.Invoke(effectType,pos);
     }
    /// <summary>
    /// 生成作物
    /// </summary>
    public static event Action GenerateCropEvent;
    public static void CallGenerateCropEvent()
    {
        GenerateCropEvent?.Invoke();
    }
    /// <summary>
    /// 对话模板
    /// </summary>
    public static event Action<DialoguePiece> ShowDialogueEvent;

    public static void CallShowDialogueEvent(DialoguePiece piece)
    {
        ShowDialogueEvent?.Invoke(piece);
    }
    /// <summary>
    /// 确认模板
    /// </summary>
    public static event Action<ConfirmPiece> ShowConfirmEvent;

    public static void CallShowConfirmEvent(ConfirmPiece piece)
    {
        ShowConfirmEvent?.Invoke(piece);
    }
    /// <summary>
    /// 商店的打开
    /// </summary>
    public static event Action<SlotType, InventoryBag_SO> BaseBayOpenEvent;
    public static void CallBaseBayOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
    {
        BaseBayOpenEvent?.Invoke(slotType, bag_SO);
    }

    /// <summary>
    /// 商店的关闭
    /// </summary>
    public static event Action<SlotType, InventoryBag_SO> BaseBayCloseEvent;
    public static void CallBaseBayCloseEvent(SlotType slotType, InventoryBag_SO bag_SO)
    {
        BaseBayCloseEvent?.Invoke(slotType, bag_SO);
    }
    /// <summary>
    /// 游戏状态切换（开始和关闭）
    /// </summary>
    public static event Action<GameState> UpdateGameStateEvent;
    public static void CallUpdateGameStateEvent(GameState gameState)
    {
        UpdateGameStateEvent?.Invoke(gameState);
    }
    /// <summary>
    /// 通用背包界面之间的交互
    /// </summary>
    public static event Action<ItemDetails, bool> ShowTradeUI;
    public static void CallShowTradeUI(ItemDetails item,bool isSell)
    {
        ShowTradeUI?.Invoke(item, isSell);
    }
    /// <summary>
    /// 蓝图实现
    /// </summary>
    public static event Action<int,Vector3> BuildFurnitureEvent;
    public static void CallBuildFurnitureEvent(int ID,Vector3 pos)
    {
        BuildFurnitureEvent?.Invoke(ID,pos);
    }
    /// <summary>
    /// 切换灯光
    /// </summary>
    public static event Action<Season, LightShift, float> LightShiftChangeEvent;
    public static void CallLightShiftChangeEvent(Season season, LightShift lightShift,float time)
    {
        LightShiftChangeEvent?.Invoke(season, lightShift, time);
    }
    /// <summary>
    /// 音效
    /// </summary>
    public static event Action<SoundDetails> InitSoundEffect;
    public static void CallInitSoundEffect(SoundDetails soundDetails)
    {
        InitSoundEffect?.Invoke(soundDetails);
    }
    /// <summary>
    /// 播放声音事件
    /// </summary>
    public static event Action<SoundName> PlaySoundEvent;
    public static void CallPlaySoundEvent(SoundName soundName)
    {
        PlaySoundEvent?.Invoke(soundName);
    }
    /// <summary>
    /// 升级事件
    /// </summary>
    public static event Action UpgradeEvent;
    public static void CallUpgradeEvent()
    {
        UpgradeEvent?.Invoke();
    }

    public static event Action PlayerAttack;
    public static void CallPlayerAttack()
    {
        PlayerAttack?.Invoke();
    }
    
    public static event Action PlayerAnimNone;
    public static void CallPlayerAnimNone()
    {
        PlayerAnimNone?.Invoke();
    }


    /// <summary>
    /// 更新StatsUI
    /// </summary>
    public static event Action UpdataStatsUIEvent;
    public static void CallUpdataStatsUIEvent()
    {
        UpdataStatsUIEvent?.Invoke();
    }

    /// <summary>
    /// 使用物品效果
    /// </summary>
    public static event Action<ItemDetails> usageEffectEvent;
    public static void CallusageEffectEvent(ItemDetails itemDetails)
    {
        usageEffectEvent?.Invoke(itemDetails);
    }

    /// <summary>
    /// 新游戏开始初始化游戏
    /// </summary>
    public static event Action<int> StartNewGameEvent;
    public static void CallStartNewGameEvent(int index)
    {
        StartNewGameEvent?.Invoke(index);
    }


    public static event Action EndGameEvent;
    public static  void CallEndGameEvent()
    {
        EndGameEvent?.Invoke();
    }
}




