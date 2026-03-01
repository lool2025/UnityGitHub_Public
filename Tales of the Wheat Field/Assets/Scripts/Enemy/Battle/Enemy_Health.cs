using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    [Header("当前生命")] public int currentHealth;
    [Header("最大生命")] public int maxHealth;
    [Header("掉落经验值")] public int experience;

    public int[] Spoils;
    public int[] maxspoils;
    public int[] minspoils;

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;

        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else if(currentHealth <= 0)
        {
            ///掉落物品经验
            StatsManager.Instance.experienceAdd(experience);
            Destroy(gameObject);
        }
    }
  
    public void SpawnHarvestItems()
    {
        for (int i = 0; i < Spoils.Length; i++)
        {
            int amountToProduce;
            if (maxspoils[i] == minspoils[i])
            {
                //代表只生成指定数量的
                amountToProduce = maxspoils[i];
            }
            else
            {
                //物品随机数量
                amountToProduce = Random.Range(minspoils[i], maxspoils[i] );
            }

            //执行生成指定数量的物品
            for (int j = 0; j < amountToProduce; j++)
            {
              
                    
                    var spawnPos = new Vector3(transform.position.x + Random.Range(-2, 2), transform.position.y + Random.Range(-2,2), 0);

                    EventHandler.CallInstantiateItemInScene(Spoils[i], spawnPos);
                
            }

        }

      

    }

 
}
