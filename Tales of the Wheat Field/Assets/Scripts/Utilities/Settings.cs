using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings 
{
    //修改树的透明度的修改时间
    public const float itemFadeDuration = 0.35f;
    //修改树的透明度
    public const float targetAloha = 0.5f;

    //时间相关

    public const float secondThreshold = 0.01f;//数值越小时间越快

    public const int secondHold = 59;

    public const int minuteHold = 59;

    public const int hourHold = 23;
    public const int dayHold = 30;
    public const int seasonHold = 3;

    public const float fadeDuration = 1.5f;
    //割草数量限制
    public const int reapAmount = 2;
    //NPC移动
    public const float gridCellSize = 1;
    public const float gridCellDiagonalSize = 1.41f;

    public const float pixelSize = 0.05f;//像素距离20*20

    public const float animationBreakTime = 5f;//动画时间间隔

    public const int maxGridSize = 9999;

    //灯光
    public const float lightChangeDuration = 25f;
    public static TimeSpan moringTime=new TimeSpan(5,0,0);
    public static TimeSpan nigheTime = new TimeSpan(19, 0, 0);

    public static Vector3 playerStartPos = new Vector3(-13.5f, -14.8f,0);
   
    public static int  playerStartMoney = 100;

    [Tooltip("防御减伤计算系数（值越大，减伤增长越慢）")]
    public static float defenseCoefficient = 100f; // 可根据游戏平衡调整
    [Tooltip("最大减伤比例（固定60%）")]
    public static float maxDamageReduction = 0.6f; // 60% = 0.6
    [Tooltip("经验增长倍率")]
    public static float expGrowthRate = 1.2f;
    [Tooltip("经验初始")]
    public static int ExperienceBegins = 100;


    [Tooltip("攻击增长")]
    public static int damageGrowth = 1;
    [Tooltip("防御增长")]
    public static int defenseValueGrowth = 1;
    [Tooltip("生命增长")]
    public static int maxHealthGrowth = 5;
    [Tooltip("速度增长")]
    public static float speedGrowth = 0.2f;


}
