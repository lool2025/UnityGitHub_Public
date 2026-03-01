using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 重命名方法避免冲突，优化执行逻辑
public class InitEnemy : MonoBehaviour
{
    public int enemyId = 1001; // 要初始化的怪物ID

    // 改用Start执行，确保EnemyManager单例已初始化
    private void Start()
    {
        // 1. 校验EnemyManager是否存在
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("EnemyManager单例未找到！请确保场景中有EnemyManager物体");
            return;
        }

        // 2. 获取怪物配置
        EventDetails config = EnemyManager.Instance.GetItemDetails(enemyId);
        if (config == null)
        {
            Debug.LogError($"找不到ID为{enemyId}的怪物配置，请检查enemyId是否正确");
            return;
        }

        // 3. 执行初始化
        InitializeEnemyProperties(gameObject, config);
    }

    /// <summary>
    /// 初始化怪物的所有属性（核心方法，重命名避免和类名冲突）
    /// </summary>
    /// <param name="enemyObj">怪物对象</param>
    /// <param name="config">怪物配置数据</param>
    public void InitializeEnemyProperties(GameObject enemyObj, EventDetails config)
    {
        if (enemyObj == null || config == null)
        {
            Debug.LogError("初始化失败：怪物对象或配置为空");
            return;
        }

        // 初始化战斗相关属性
        if (enemyObj.TryGetComponent(out Enemy_Combat enemyCombat))
        {
            enemyCombat.attackPower = config.attackPower;
            enemyCombat.knockbackForce = config.knockbackForce;
            enemyCombat.stunTime = config.stunTime;
        }
        else
        {
            Debug.LogWarning($"{enemyObj.name} 缺少 Enemy_Combat 组件！");
        }

        // 初始化移动相关属性
        if (enemyObj.TryGetComponent(out Enemy_Movement enemyMovement))
        {
            enemyMovement.speed = config.speed;
            enemyMovement.attackingRange = config.attackingRange;
            enemyMovement.rangeofPursuit = config.rangeofPursuit;
            enemyMovement.attackCooldown = config.attackCooldown;
            enemyMovement.delayAttackTime = config.delayAttackTime;
        }
        else
        {
            Debug.LogWarning($"{enemyObj.name} 缺少 Enemy_Movement 组件！");
        }

        // 初始化血量相关属性
        if (enemyObj.TryGetComponent(out Enemy_Health enemyHealth))
        {
            enemyHealth.currentHealth = config.currentHealth;
            enemyHealth.maxHealth = config.maxHealth;
            enemyHealth.experience = config.experience;
            enemyHealth.Spoils = config.spoils;
            enemyHealth.maxspoils = config.Maxspoils;
            enemyHealth.minspoils = config.Minspoils;
        }
        else
        {
            Debug.LogWarning($"{enemyObj.name} 缺少 Enemy_Health 组件！");
        }

        // 可选：给怪物命名（方便调试）
        enemyObj.name = $"{config.eventName}_{config.eventID}";
      
    }
}