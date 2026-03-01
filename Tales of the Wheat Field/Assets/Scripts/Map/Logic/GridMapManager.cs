
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using MFarm.CropPlant;
using MFarm.Save;
namespace MFarm.Map
{
    public class GridMapManager : Singleton<GridMapManager>,ISaveable
    {
        [Header("种地瓦片切换信息")]
        public RuleTile digTile;

        public RuleTile waterTile;

        private Tilemap digTilemap;

        private Tilemap waterTilemap;

        private Season currentSeason;

        [Header("地图信息")]
        public List<MapData_SO> mapDataList;
        //场景名字+坐标和对应的瓦片信息
        private Dictionary<string,TileDetails> tileDetailsDict=new Dictionary<string,TileDetails>();
        //场景是否第一次加载
        private Dictionary<string,bool>firstLoadDict=new Dictionary<string, bool>();

        private Grid currentGrid;
        //杂草列表
        private List<ReapItem> itemsInRadius;

        public string GUID => GetComponent<DataGUID>().guid;

        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
            EventHandler.GameDayEvent += OnGameDayEvent;
            EventHandler.RefreshCurrentMap += RefreshMap;
        }

        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
            EventHandler.GameDayEvent -= OnGameDayEvent;
            EventHandler.RefreshCurrentMap -= RefreshMap;
        }

       

        private void OnAfterSceneLoadEvent()
        {
           currentGrid=FindObjectOfType<Grid>();
            //场景加载时寻找这两个标签上的Tilemap组件
            digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            waterTilemap= GameObject.FindWithTag("Water").GetComponent<Tilemap>();
           //判断场景是否是第一次加载
            if (firstLoadDict[SceneManager.GetActiveScene().name])
            {
                //预先生成农作物
                EventHandler.CallGenerateCropEvent();
                firstLoadDict[SceneManager.GetActiveScene().name]=false;
            }
           

            RefreshMap();
        }
        /// <summary>
        /// 这个函数每天调用一次
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="season"></param>
        private void OnGameDayEvent(int day, Season season)
        {
            currentSeason=season;

            foreach(var tile in tileDetailsDict)
            {
                if (tile.Value.daySinceWatered > -1)
                {
                    tile.Value.daySinceWatered = -1;
                }
                if (tile.Value.daysSinceDug > -1)
                {
                    tile.Value.daysSinceDug++;
                }

                //超时消除挖坑
                if (tile.Value.daysSinceDug > 5 && tile.Value.seedItemID == -1)
                {
                    tile.Value.daysSinceDug = -1;
                    tile.Value.canDig = true;
                    tile.Value.growthDays = -1;
                }
                if(tile.Value.seedItemID!= -1)
                {
                    tile.Value.growthDays++;
                }

            }
            RefreshMap();
        }

        public void Start()
        {

            ISaveable saveable = this;
            saveable.RegisterSaveable();
            foreach(var mapData in mapDataList)
            {
              
                firstLoadDict.Add(mapData.sceneName,true);
                InitTileDetailsDict(mapData);
            }
        }


        private void InitTileDetailsDict(MapData_SO mapData)
        {
            foreach(TileProperty tileProperty in mapData.tileProperties)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridX = tileProperty.tileCoordinate.x,
                    gridY = tileProperty.tileCoordinate.y
                };

                //字典的Key
                string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + mapData.sceneName;
                //如果有数据则刷新
                if(GetTileDetails(key) != null )
                {
                    tileDetails=GetTileDetails(key);
                }

                switch(tileProperty.gridType)
                {
                    case GridType.Diggable:
                        tileDetails.canDig=tileProperty.boolTypeValue;
                        break;
                    case GridType.DropItem:
                        tileDetails.canDropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
                        break;
                    case GridType.NPCObstacle:
                        tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
                        break;
                    

                }

                if(GetTileDetails(key)!=null)
                {
                    tileDetailsDict[key] = tileDetails;
                }
                else
                {
                    tileDetailsDict.Add(key, tileDetails);
                }

            }
        }
        /// <summary>
        /// 根据key返回瓦片信息
        /// </summary>
        /// <param name="key">x+y+地图名称</param>
        /// <returns></returns>
        public TileDetails GetTileDetails(string key)
        {
            
            if (tileDetailsDict.ContainsKey(key))
            {
                
                return tileDetailsDict[key];
            }
            return null;
        }
        /// <summary>
        /// 根据坐标网格坐标返回瓦片信息
        /// </summary>
        /// <param name="mouseGridPos">鼠标网格坐标</param>
        /// <returns></returns>
        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
        {
            string key = mouseGridPos.x + "x" + mouseGridPos.y + "y" + SceneManager.GetActiveScene().name;
            return GetTileDetails(key);
        }

        /// <summary>
        /// 执行实际工具或物品功能(执行玩家动作后)
        /// </summary>
        /// <param name="mouseWorldPos">鼠标坐标</param>
        /// <param name="itemDetails">物品信息</param>
        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            //将鼠标的坐标转化为场景表格上的坐标
            var mouseGridPos=currentGrid.WorldToCell(mouseWorldPos);
           
            var currentTile= GetTileDetailsOnMousePosition(mouseGridPos);
           
            if (currentTile != null)
            {
                Crop currentCrop = GetCropObject(mouseWorldPos);
                //WORKFLOW:物品使用实际功能
                switch (itemDetails.itemType)
                {
                    case ItemType.Seed:
                        EventHandler.CallPlantSeedEvent(itemDetails.itemID, currentTile);
                        EventHandler.CallDropItemEvent(itemDetails.itemID,mouseGridPos,itemDetails.itemType);
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Plant);
                        break;
                    case ItemType.Commodity:
                        EventHandler.CallDropItemEvent(itemDetails.itemID, mouseWorldPos, itemDetails.itemType);
                        break;
                    case ItemType.HoeTool:
                        SetDigGround(currentTile);
                        currentTile.daysSinceDug = 0;
                        currentTile.canDig = false;
                        currentTile.canDropItem=false;
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Hoe);
                        break;
                    case ItemType.WaterTool:
                        SetWaterGround(currentTile);
                        currentTile.daySinceWatered = 0;
                        //音效
                        EventHandler.CallPlaySoundEvent(SoundName.Water);
                        break;
                    case ItemType.BreakTool:
                    case ItemType.ChopTool:
    
                        currentCrop?.ProcessToolAction(itemDetails, currentCrop.tileDetails);                     
                        break;
                    case ItemType.CollectTool:
                       
                        //Crop currentCrop = GetCropObject(mouseWorldPos);
                        //执行收割方法
                        currentCrop?.ProcessToolAction(itemDetails,currentTile);
                        break;
                    case ItemType.ReapTool:
                        var reapCount = 0;
                        for (int i = 0; i < itemsInRadius.Count; i++)
                        {
                            EventHandler.CallParticleEffectEvent(ParticaleEffectType.ReapableScenery,itemsInRadius[i].transform.position+Vector3.up);
                            itemsInRadius[i].SpawnHarvestItems();
                            Destroy(itemsInRadius[i].gameObject);
                            reapCount++;
                            if(reapCount>=Settings.reapAmount)
                                break;
                        }
                        EventHandler.CallPlaySoundEvent(SoundName.Reap);
                        break;
                    case ItemType.Furniture:
                        //在地图上生成物品 ItemManager
                        //移除蓝图InventoryManager
                        //移除所需物品InventoryManager
                        EventHandler.CallBuildFurnitureEvent(itemDetails.itemID,mouseGridPos);
                        break;
                    case ItemType.SwordTool:
                        EventHandler.CallPlayerAttack();
                        break;


                }
                UpdateTileDetails(currentTile);
            }
        }
        /// <summary>
       ///通过物理方法判断鼠标点击位置的农作物
        /// </summary>
        /// <param name="mouseWorldPos">位置</param>
        /// <returns></returns>
        //public Crop GetCropObject(Vector3 mouseWorldPos)
        //{
        //    Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);
            
        //    Crop currentCrop= null;
        //    for(int i = 0;i< colliders.Length; i++)
        //    {
                
        //        if (colliders[i].GetComponent<Crop>())
        //        {
        //            Debug.Log($"游戏对象: {colliders[i].gameObject.name}"+ "currentCrop是否存在:" + colliders[i].GetComponent<Crop>());
        //            currentCrop =colliders[i].GetComponent<Crop>();
        //        }
        //    }
        //    return currentCrop;
        //}


        public Crop GetCropObject(Vector3 mouseWorldPos)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPos);

            foreach (var collider in colliders)
            {
                // 找到第一个带有Crop组件的对象就返回
                Crop crop = collider.GetComponent<Crop>();
                if (crop != null)
                {
                
                    return crop;
                }
            }

            // 遍历完所有碰撞体都没找到
          
            return null;
        }
        /// <summary>
        /// 返回工具内的杂草
        /// </summary>
        /// <param name="tool">ItemDetails</param>
        /// <returns></returns>
        public bool HaveReapableItemsInRadius(Vector3 mouseWorldPos ,ItemDetails tool)
        {
            itemsInRadius=new List<ReapItem> ();

            Collider2D[] colliders = new Collider2D[20];

            Physics2D.OverlapCircleNonAlloc(mouseWorldPos, tool.itemUseRadius, colliders);

            if(colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if(colliders[i] != null)
                    {
                        if (colliders[i].GetComponent<ReapItem>())
                        {
                            var item=colliders[i].GetComponent<ReapItem>();
                            itemsInRadius.Add(item);
                        }
                    }
                }
            }
            return itemsInRadius.Count > 0;
        }


        /// <summary>
        /// 显示挖坑瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos =new Vector3Int(tile.gridX, tile.gridY, 0);
            if (digTilemap != null)
            {
              
                digTilemap.SetTile(pos, digTile);
            }
        }
        /// <summary>
        /// 显示浇水瓦片
        /// </summary>
        /// <param name="tile"></param>
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridX, tile.gridY, 0);
            if (waterTilemap != null)
            {
                waterTilemap.SetTile(pos, waterTile);
            }
        }
        /// <summary>
        /// 更新瓦片信息
        /// </summary>
        /// <param name="tileDetails"></param>
        public void UpdateTileDetails(TileDetails tileDetails)
        {
            string key = tileDetails.gridX + "x" + tileDetails.gridY + "y" + SceneManager.GetActiveScene().name;
            if(tileDetailsDict.ContainsKey(key))
            {
                tileDetailsDict[key] = tileDetails;
            }
            else
            {
                tileDetailsDict.Add(key, tileDetails);
            }
        }
        /// <summary>
        /// 刷新当前地图
        /// </summary>
        private void RefreshMap()
        {
            if(digTilemap!= null)
            {
                digTilemap.ClearAllTiles();
            }
            if(waterTilemap!= null)
            {
                waterTilemap.ClearAllTiles();
            }

            foreach (var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }

            DisplayMap(SceneManager.GetActiveScene().name);
        }


        /// <summary>
        /// 显示地图瓦片
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        private void DisplayMap(string sceneName)
        {
            foreach(var tile in tileDetailsDict)
            {
                var key = tile.Key;
                var tileDetails=tile.Value;

                if (key.Contains(sceneName))
                {
                    if (tileDetails.daysSinceDug > -1)
                    {
                        SetDigGround(tileDetails);
                    }
                    if (tileDetails.daySinceWatered > -1)
                    {
                        SetWaterGround(tileDetails);
                    }
                    if (tileDetails.seedItemID > -1)
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemID, tileDetails);
                }
            }
        }
        /// <summary>
        /// 获取地图尺寸和原点
        /// </summary>
        /// <param name="sceneName">地图名字</param>
        /// <param name="gridDimensions">尺寸</param>
        /// <param name="gridOrigin">原点</param>
        /// <returns></returns>
        public bool GetGridDimensions(string sceneName,out Vector2Int gridDimensions,out Vector2Int gridOrigin)
        {
            gridDimensions = Vector2Int.zero;
            gridOrigin = Vector2Int.zero;

            foreach (var mapData in mapDataList)
            {
                if(mapData.sceneName == sceneName)
                {
                    gridDimensions.x=mapData.gridWidth;
                    gridDimensions.y=mapData.gridHeight;

                    gridOrigin.x = mapData.originX;
                    gridOrigin.y = mapData.originY;

                    return true;

                }
            }
            return false;

        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.tileDetailsDict = this.tileDetailsDict;
            saveData.firstLoadDict = this.firstLoadDict;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.tileDetailsDict=saveData.tileDetailsDict;
            this.firstLoadDict =saveData.firstLoadDict;

        }
    }
}

