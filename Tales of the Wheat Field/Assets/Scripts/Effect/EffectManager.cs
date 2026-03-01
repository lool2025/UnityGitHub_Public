using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class EffectManager : MonoBehaviour
{

    public void OnEnable()
    {
        EventHandler.usageEffectEvent += OnusageEffectEvent;
    }
    public void OnDisable()
    {
        EventHandler.usageEffectEvent -= OnusageEffectEvent;
    }

    private void OnusageEffectEvent(ItemDetails itemDetails)
    {
        // 健壮性检查：空值/非法值过滤
        if (itemDetails == null)
        {
          
            return;
        }
        if (itemDetails.timeSize < 0)
        {
            itemDetails.timeSize = 0;
        }

        // 计算实际要增减的数值
        float adjustValue = itemDetails.isIncrease ? itemDetails.AddValue : -itemDetails.AddValue;

        // 根据属性类型执行初始修改
        switch (itemDetails.attribute)
        {
            case Attribute.speed:
                StatsManager.Instance.AdjustSpeed(adjustValue);
              
                break;
            case Attribute.maxHealth:
                StatsManager.Instance.AddMaxHealth(Mathf.RoundToInt(adjustValue)); // 转为int适配方法
              
                break;
            case Attribute.damage:
                StatsManager.Instance.Adddamage(Mathf.RoundToInt(adjustValue)); // 转为int适配方法
              
                break;
            case Attribute.defenseValue:
                StatsManager.Instance.AddDefenseValue(Mathf.RoundToInt(adjustValue)); // 转为int适配方法
              
                break;
            case Attribute.currentHealth:
                StatsManager.Instance.AddcurrentHealth(Mathf.RoundToInt(adjustValue)); // 转为int适配方法

                break;
            case Attribute.money:
                StatsManager.Instance.AddcurrentHealth(Mathf.RoundToInt(adjustValue)); // 转为int适配方法

                break;
            default:
               
                return;
        }

        // 限时效果：启动通用恢复协程
        if (itemDetails.istimeLimit)
        {
            StartCoroutine(RecoverAttributeAfterTime(itemDetails.attribute, adjustValue, itemDetails.timeSize));
        }
    }

    /// <summary>
    /// 通用属性恢复协程（适配 speed/maxHealth/damage/defenseValue）
    /// </summary>
    /// <param name="attribute">要恢复的属性类型</param>
    /// <param name="adjustedValue">已修改的属性值（用于反向恢复）</param>
    /// <param name="waitTime">等待时间（秒）</param>
    /// <returns></returns>
    private IEnumerator RecoverAttributeAfterTime(Attribute attribute, float adjustedValue, float waitTime)
    {
        // 等待指定时间
        yield return new WaitForSeconds(waitTime);

        // 反向恢复属性（数值取反）
        float recoverValue = -adjustedValue;
        switch (attribute)
        {
            case Attribute.speed:
                StatsManager.Instance.AdjustSpeed(recoverValue);
              
                break;
            case Attribute.maxHealth:
                StatsManager.Instance.AddMaxHealth(Mathf.RoundToInt(recoverValue));
              
                break;
            case Attribute.damage:
                StatsManager.Instance.Adddamage(Mathf.RoundToInt(recoverValue));
              
                break;
            case Attribute.defenseValue:                          
                StatsManager.Instance.AddDefenseValue(Mathf.RoundToInt(recoverValue));
              
                break;
            case Attribute.currentHealth:
                StatsManager.Instance.AddcurrentHealth(Mathf.RoundToInt(recoverValue));
                break;
            default:
             
                break;
        }
    }
}

