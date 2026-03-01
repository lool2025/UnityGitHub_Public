using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    [Header("攻击力")]public int attackPower;
    [Header("击退力")] public float knockbackForce;
    [Header("击退时间")] public float stunTime;

    [Header("攻击检测范围")] public Transform attackPoint;
    public float weaponRange;
    public LayerMask playerLayer;


    /// <summary>
    /// 攻击
    /// </summary>
    public void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);
        if (hits.Length > 0)
        {
            // 1. 计算减伤比例
            float reduction = CalculateDamageReduction(StatsManager.Instance.defenseValue);
                 
         
            // 2. 保证伤害至少为1（避免0伤害，可选）
            int finalDamage = Mathf.RoundToInt(Mathf.Max(1, attackPower * (1 - reduction)));
        
            hits[0].GetComponent<PlayerHealth>().ChangeHealthValue(finalDamage);
            hits[0].GetComponent<Player>().Knockback(transform, knockbackForce,stunTime);
            
        }
        
    }

    public float CalculateDamageReduction(float defenseValue)
    {
        // 1. 防御值不能为负
        defenseValue = Mathf.Max(0, defenseValue);

        // 2. 基础减伤计算（经典的非线性公式，避免减伤无限增长）
        // 公式说明：防御越高，减伤越接近1，但会被后续上限限制
        float baseReduction = defenseValue / (defenseValue + Settings.defenseCoefficient);

        // 3. 限制减伤上限为60%，同时保证下限为0
        float finalReduction = Mathf.Clamp(baseReduction, 0f, Settings.maxDamageReduction);

        return finalReduction;
    }

}
