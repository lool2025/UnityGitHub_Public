using UnityEditor;
using UnityEngine;
/// <summary>
/// 农作物的各种信息
/// </summary>
[System.Serializable]
public class CropDetails 
{
    public int seedItemID;
    [Header("不同阶段需要的天数")]
    public int[] growthDays;
    /// <summary>
    /// 一共需要多少天
    /// </summary>
    public int TotalGrowthDays
    {
        get
        {
            int amount = 0;
            foreach (var days in growthDays)
            {
                amount += days;
            }
            return amount;
        }
    }

    [Header("不同生长阶段物品Prefab")]
    public GameObject[] growthPrefabs;

    [Header("不同阶段的图片")]
    public Sprite[] growthSprites;

    [Header("可种植的季节")]
    public Season[] seasons;


    [Space]
    [Header("收割工具")]
    public int[] harvestToolItemID;

    [Header("每种工具使用次数")]
    public int[] requireActionCount;

    [Header("转换新物品ID")]
    public int transferItemID;

    [Space]
    [Header("收割果实信息")]
    public int[] producedItemID;
    //掉落数量的最大和最小
    public int[] producedMinAmount;
    public int[] producedMaxAmount;
    //掉落范围
    public Vector2 spawnRadius;

    [Header("再次生长时间")]
    public int daysToRegrow;
    //再次生长的次数
    [Header("最大再生次数")]
    public int regrowTimes;

    [Header("Options")]
    //是否在player的身上生成
    public bool generateAtPlayerPosition;
    //有没有动画
    public bool hasAnimation;
    //有没有例子特效
    public bool hasParticalEffct;
    //TODO:特效，音效等

    public SoundName soundEffect;
    public ParticaleEffectType effectType;
    public Vector3 effectPos;

    /// <summary>
    /// 检查当前工具是否可用
    /// </summary>
    /// <param name="toolID">工具ID</param>
    /// <returns></returns>
    public bool CheckToolAvailable(int toolID)
    {
        foreach(var tool in harvestToolItemID)
        {
            if (tool==toolID)
                return true;

        }
        return false;
    }
    /// <summary>
    /// 获取工具需要使用的次数
    /// </summary>
    /// <param name="toolID"></param>
    /// <returns></returns>
    public int GetTotalRequireCount(int toolID)
    {
        for (int i = 0; i < harvestToolItemID.Length; i++)
        {
            if (harvestToolItemID[i] == toolID)
                return requireActionCount[i];
        }
        return -1;
    }
}
