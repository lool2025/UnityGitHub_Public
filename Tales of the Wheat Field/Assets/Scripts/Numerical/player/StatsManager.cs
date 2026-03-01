using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance {  get; private set; }
    [Header("PlayerHealth")]
    [Header("当前生命")] public int currentHealth ;
    [Header("最大生命")] public int maxHealth;
    [Header("防御")] public int defenseValue;
    [Header("等级")] public int level;
    [Header("经验")] public int experience;
    [Header("经验最大值")][SerializeField] public float maxexperience;

    [Header("属性升级点")] public int upgrade;


    [Header("PlayerCombat")]
    [Header("攻击力")] public int damage;
    [Header("击退力度")] public float knockbackForce;
    [Header("眩晕时间")] public float knocjbackTime;
    [Header("击退时间")] public float stunTime;

    [Header("Player")]
    [Header("速度")] public float speed;

    [Header("金钱")]
    public int playerMoney;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(Instance);
        }

    }

    /// <summary>
    /// 增加当前生命值（可加可减，最终值不小于0）
    /// </summary>
    /// <param name="valueToAdd">要增加的值（负数则减少）</param>
    public void AddcurrentHealth(int valueToAdd)
    {
        int newValue = currentHealth + valueToAdd;
        // 确保最终值非负
        currentHealth = Mathf.Clamp(newValue, 0, maxHealth);
        EventHandler.CallUpdataStatsUIEvent();
    }

    /// <summary>
    /// 增加最大生命值（可加可减，最终值不小于0）
    /// </summary>
    /// <param name="valueToAdd">要增加的值（负数则减少）</param>
    public void AddMaxHealth(int valueToAdd)
    {
        int newValue = maxHealth + valueToAdd;
        // 确保最终值非负
        maxHealth = Mathf.Max(newValue, 0);
        EventHandler.CallUpdataStatsUIEvent();
    }
    /// <summary>
    /// 攻击
    /// </summary>
    /// <param name="valueToAdd"></param>
    public void Adddamage(int valueToAdd)
    {
        int newValue = damage + valueToAdd;
        // 确保最终值非负
        damage = Mathf.Max(newValue, 0);
        EventHandler.CallUpdataStatsUIEvent();
    }


    /// <summary>
    /// 增加防御值（最终值不小于0）
    /// </summary>
    /// <param name="valueToAdd">要增加的值</param>
    public void AddDefenseValue(int valueToAdd)
    {
        defenseValue = Mathf.Max(defenseValue + valueToAdd, 0);
        EventHandler.CallUpdataStatsUIEvent();
    }



    /// <summary>
    /// 调整速度（按增量修改，最终值大于0）
    /// </summary>
    /// <param name="valueToAdd">速度增量（可正可负）</param>
    public void AdjustSpeed(float valueToAdd)
    {
        float newValue = speed + valueToAdd;
        speed = Mathf.Max(newValue, 0.1f); // 保留最小速度0.1，避免完全静止
        EventHandler.CallUpdataStatsUIEvent();
    }

    /// <summary>
    /// 增加当前生命值（可加可减，最终值不小于0）
    /// </summary>
    /// <param name="valueToAdd">要增加的值（负数则减少）</param>
    public void AddplayerMoney(int valueToAdd)
    {
        int newValue = playerMoney + valueToAdd;
        // 确保最终值非负
        playerMoney = Mathf.Clamp(newValue, 0, maxHealth);
        EventHandler.CallUpdataStatsUIEvent();
    }


    /// <summary>
    /// 计算每级所需要的需求
    /// </summary>
    /// <returns></returns>
    public float Experiencerequirements(int level)
    {
        maxexperience = Settings.ExperienceBegins * Mathf.Pow(Settings.expGrowthRate, level - 1);
        return maxexperience; 
    }
    /// <summary>
    /// 经验百分比
    /// </summary>
    /// <returns></returns>
    public float Proportionexperience()
    {
        Debug.Log("experience   " + experience + "  Experiencerequirement:" + Experiencerequirements(level)+"   "+ experience / Experiencerequirements(level));
        return Mathf.Round(experience / Experiencerequirements(level) * 10000f) / 10000f ;
    }

    public void  experienceAdd(int value)
    {
        if((experience+value)> Experiencerequirements(level))
        {
            experience=(int)(experience+value - Experiencerequirements(level));
            level++;
            upgrade++;
            EventHandler.CallUpgradeEvent();

        }
        else
        {
            experience+=value;
        }
        
    }

   

}
