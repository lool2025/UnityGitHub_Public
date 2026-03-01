using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Crop : MonoBehaviour
{
    public CropDetails cropDetails;
    private int harvestActionCount;
    public TileDetails tileDetails;
    private Animator anim;
    //是否成熟
    public bool CanHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;

//    如果写成 private Transform PlayerTransform = FindObjectOfType<Player>().transform;
//    这会变成一个字段，只会在对象初始化时执行一次查找，之后值就固定了（如果玩家对象被销毁或创建新的，字段值不会更新）。
//     而用 => 定义的是属性，每次访问 PlayerTransform 时，都会重新执行 FindObjectOfType<Player>().transform 来获取最新的玩家位置信息。

    private UnityEngine.Transform PlayerTransform=>FindObjectOfType<Player>().transform;

    public void ProcessToolAction(ItemDetails tool,TileDetails tile)
    {
        tileDetails = tile;
        //工具使用次数
        int requireActionCount= cropDetails.GetTotalRequireCount(tool.itemID);
        if(requireActionCount ==-1)return;

        anim=GetComponentInChildren<Animator>();


        //点击计数器
        if(harvestActionCount<requireActionCount)
        {
            harvestActionCount++;
          
            //判断是否有动画 树木
            if (anim!=null &&cropDetails.hasAnimation)
            {
                Debug.Log("树木动画");
                if (PlayerTransform.position.x < transform.position.x)
                {
                    // 玩家在当前对象左侧，触发向右旋转动画
                    anim.SetTrigger("RotateRight");
                }
                else
                {
                    // 玩家在当前对象右侧或同一X位置，触发向左旋转动画
                    anim.SetTrigger("RotateLeft");
                }
            }
            //播放粒子
            if (cropDetails.hasParticalEffct)
            {
               
                EventHandler.CallParticleEffectEvent(cropDetails.effectType, transform.position + cropDetails.effectPos);
            }

            //播放声音
            if(cropDetails.soundEffect!=SoundName.none)
            {
                Debug.Log("播放声音");
                EventHandler.CallPlaySoundEvent(cropDetails.soundEffect);
            }
        }

        if (harvestActionCount >= requireActionCount)
        {
          
            if (cropDetails.generateAtPlayerPosition|| !cropDetails.hasAnimation)
            {
               
                //生成农作物
                SpawnHarvestItems();
            }
            else if (cropDetails.hasAnimation)
            {
               
                if (PlayerTransform.position.x < transform.position.x)
                {
                
                    anim.SetTrigger("FallingRight");
                }
                else
                {
             
                    anim.SetTrigger("FallingLeft");
                }
                EventHandler.CallPlaySoundEvent(SoundName.TreeFalling);
                StartCoroutine(HarvestAfterAnimation());
            }


        }
    }

    private IEnumerator HarvestAfterAnimation()
    {
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("END"))
        {
            yield return null;
        }
        SpawnHarvestItems();

        //转换新物体
        if (cropDetails.transferItemID > 0)
        {
          
            CreateTransferCrop();
        }
    }

    private void CreateTransferCrop()
    {
        tileDetails.seedItemID = cropDetails.transferItemID;
        tileDetails.daySinceLastHarvest = -1;
        tileDetails.growthDays = 0;

        EventHandler.CallRefreshCurrentMap();
    }


    /// <summary>
    /// 生成农作物
    /// </summary>
    public void SpawnHarvestItems()
    {
        for (int i = 0;i<cropDetails.producedItemID.Length;i++)
        {
            int amountToProduce;
            if (cropDetails.producedMinAmount[i] == cropDetails.producedMaxAmount[i])
            {
                //代表只生成指定数量的
                amountToProduce=cropDetails.producedMinAmount[i];
            }
            else
            {
                //物品随机数量
                amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i]+1);
            }

            //执行生成指定数量的物品
            for (int j = 0;j<amountToProduce;j++)
            {
                if (cropDetails.generateAtPlayerPosition)//人物背包生成物品
                {
                    
                    EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);

                }
                else//世界地形上生成的物品
                {
                    var dirX = transform.position.x > PlayerTransform.position.x ? 1 : -1;
                    var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX), transform.position.y + Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);

                    EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i], spawnPos);
                }
            }

        }

        if(tileDetails!=null)
        {
            tileDetails.daySinceLastHarvest++;

            //是否可以重复生长
            //regrowTimes表示最大再生次数，TotalGrowthDays是总生长天数
            if (cropDetails.daysToRegrow > 0 && tileDetails.daySinceLastHarvest < cropDetails.regrowTimes)
            {
                tileDetails.growthDays=cropDetails.TotalGrowthDays-cropDetails.daysToRegrow;
                //刷新
                EventHandler.CallRefreshCurrentMap();
            }
            else//不可重复生长
            {
                tileDetails.daySinceLastHarvest = -1;
                tileDetails.seedItemID = -1;
                //不留坑
                //tileDetails.daysSinceDug = -1;
            }
            Destroy(gameObject);

        }

    }

}
