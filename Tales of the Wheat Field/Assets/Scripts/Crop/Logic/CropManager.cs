
using UnityEngine;
namespace MFarm.CropPlant
{
    public class CropManager : Singleton<CropManager>
    {
        public CropDataList_SO cropData;

        private Transform cropParent;

        private Grid currentGrid;

        private Season currentSeason;
        private void OnEnable()
        {
            EventHandler.PlantSeedEvent += OnPlantSeedEvent;
            EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
        }
        private void OnDisable()
        {
            EventHandler.PlantSeedEvent -= OnPlantSeedEvent;
            EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
        }

        private void OnGameDayEvent(int arg1, Season season)
        {
            currentSeason=season;
        }

        private void OnAfterSceneLoadEvent()
        {
            currentGrid=FindObjectOfType<Grid>();
            cropParent=GameObject.FindWithTag("CropParent").transform;
        }

        private void OnPlantSeedEvent(int ID, TileDetails tileDetails)
        {
            CropDetails currentcrop=GetCropDetails(ID);
            if(currentcrop != null&& SeasonAvailable(currentcrop)&&tileDetails.seedItemID==-1)//用于第一次种植
            {
                tileDetails.seedItemID=ID;
                tileDetails.growthDays = 0;
                //显示农作物
                DisplayCropPlant(tileDetails, currentcrop);
            }
            else if(tileDetails.seedItemID != -1)//用于刷新地图
            {
                //显示农作物
                DisplayCropPlant(tileDetails, currentcrop);
            }
        }
        /// <summary>
        /// 显示农作物
        /// </summary>
        /// <param name="tileDetails">瓦片信息</param>
        /// <param name="cropDetails">种子信息</param>
        private void DisplayCropPlant(TileDetails tileDetails,CropDetails cropDetails)
        {
            //成长的阶段
            int growthStages= cropDetails.growthDays.Length;

            int currentStage = 0;
            //一共需要多少天
            int dayCounter=cropDetails.TotalGrowthDays;
            for(int i=growthStages-1; i>=0; i--)
            {
                if (tileDetails.growthDays >= dayCounter)
                {
                    currentStage = i; break;
                }
                dayCounter-=cropDetails.growthDays[i];
            }

            //获取当前阶段的Prefab
            GameObject cropPrefab = cropDetails.growthPrefabs[currentStage];

            Sprite cropSprite=cropDetails.growthSprites[currentStage];

            Vector3 pos = new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0);

            GameObject cropInstance=Instantiate(cropPrefab,pos,Quaternion.identity,cropParent);
            cropInstance.GetComponentInChildren<SpriteRenderer>().sprite=cropSprite;

            cropInstance.GetComponent<Crop>().cropDetails = cropDetails;
            cropInstance.GetComponent<Crop>().tileDetails = tileDetails;
        }




        /// <summary>
        /// 通过物品ID查找种子信息
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int ID)
        {
            return cropData.cropDetailsList.Find(c=>c.seedItemID == ID);
        }
        /// <summary>
        /// 判断当前季节是否可以耕种
        /// </summary>
        /// <param name="crop"></param>
        /// <returns></returns>
        private bool SeasonAvailable(CropDetails crop)
        {
            for (int i = 0;i<crop.seasons.Length;i++)
            {
                if (crop.seasons[i]==currentSeason)
                    return true;

            }
            return false;
        }

    }

}
