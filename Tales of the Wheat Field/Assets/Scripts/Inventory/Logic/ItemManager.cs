
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MFarm.Save;

namespace MFarm.Inventory
{
    public class ItemManager : MonoBehaviour,ISaveable
    {
        public Item itemPrefab;

        private Transform itemParent;

        public Item bounceItemPrefab;

        private Transform PlayerTransform=>FindObjectOfType<Player>().transform;

        public string GUID => GetComponent<DataGUID>().guid;

        //记录场景Item
        private Dictionary<string,List<SceneItem>>sceneItemDict=new Dictionary<string,List<SceneItem>>();
        //记录场景家具
        private Dictionary<string, List<SceneFurniture>> sceneFurnitureDict = new Dictionary<string, List<SceneFurniture>>();
        //记录场景蓝图加载时是否重复
        private Dictionary<string, bool> sceneisFurniture =new Dictionary<string, bool>();

        public void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }

        private void OnEnable()
        {
            EventHandler.InstantiateItemInScene += OnInstantiateItemInScene;

            EventHandler.DropItemEvent += OnDropItemEvent;

            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;

            EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadedEvent;

            //蓝图
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;
            //新游戏
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

        private void OnDestroy() 
        {
            EventHandler.InstantiateItemInScene -= OnInstantiateItemInScene;

            EventHandler.DropItemEvent -= OnDropItemEvent;

            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;

            EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadedEvent;
            //蓝图
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;
            //新游戏
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

       

        /// <summary>
        /// 实例化蓝图
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="pos"></param>
        private void OnBuildFurnitureEvent(int ID,Vector3 pos)
        {
            
           BluePrintDetails bluePrintDetails=InventoryManager.Instance.bluePrintData.GetBluePrintDetails(ID);
           var buildItem = Instantiate(bluePrintDetails.buildPrefab, pos+new Vector3(0.5f,0.5f,0), Quaternion.identity, itemParent);

            if (buildItem.GetComponent<Box>())
            {
                buildItem.GetComponent<Box>().index=InventoryManager.Instance.BoxDataAmount;
                buildItem.GetComponent<Box>().InitBox(buildItem.GetComponent<Box>().index);
            }

        }
        private void OnStartNewGameEvent(int obj)
        {
            sceneItemDict.Clear();
            sceneFurnitureDict.Clear();
            sceneisFurniture.Clear();
        }

        private void OnBeforeSceneUnloadEvent()
        {
            GetAllSceneItems();
            GetAllSceneFurniture();
        }

        private void OnAfterSceneLoadedEvent()
        {
            itemParent = GameObject.FindWithTag("ItemParent").transform;

            RecreateAllItems();
           
            RebuildFurniture();
        }

      

        private void OnInstantiateItemInScene(int ID, Vector3 pos)
        {
            var item = Instantiate(bounceItemPrefab, pos, Quaternion.identity, itemParent);

            item.itemID = ID;

            item.GetComponent<ItemBounce>().InitBounceItem(pos, Vector3.up);

        }

        private void OnDropItemEvent(int ID, Vector3 mousePos,ItemType itemType)
        {
            if(itemType==ItemType.Seed)return;
            //TODO:扔东西的效果
            var item = Instantiate(bounceItemPrefab, PlayerTransform.position, Quaternion.identity, itemParent);

            item.itemID = ID;

            var dir = (mousePos - PlayerTransform.position).normalized;

            item.GetComponent<ItemBounce>().InitBounceItem(mousePos, dir);
        }
        /// <summary>
        /// 场景销毁前获取当前场景中的item
        /// </summary>
        private void GetAllSceneItems()
        {
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            if (sceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneFurniture))
            {
                if (currentSceneFurniture != null)
                {
                    foreach (var scenefurniture in currentSceneFurniture)
                    {
                        var key = SceneManager.GetActiveScene().name + scenefurniture.itemID + scenefurniture.boxIndex;
                        if (sceneisFurniture.ContainsKey(key))
                        {
                            
                            sceneisFurniture[key] = false;
                        }
                        else
                        {
                            
                            sceneisFurniture.Add(key, false);
                        }
                            
                    }
                }
            }

            List<SceneItem> currentSceneItems = new List<SceneItem>();

            //获取当前场景中所有的item
            foreach (var item in FindObjectsOfType<Item>()) 
            {
                SceneItem sceneItem = new SceneItem
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position)
                };

                currentSceneItems.Add(sceneItem);
            }
            //查找sceneItemDict是否有该场景的名字
            if (sceneItemDict.ContainsKey(SceneManager.GetActiveScene().name))
            {

                //更新数据
                sceneItemDict[SceneManager.GetActiveScene().name] = currentSceneItems;
            }
            else//没有
            {
                sceneItemDict.Add(SceneManager.GetActiveScene().name, currentSceneItems);
            }


            


        }
        /// <summary>
        /// 刷新重建当前场景物品
        /// </summary>
        private void RecreateAllItems()
        {
            List<SceneItem> currentSceneItems= new List<SceneItem>();

            if(sceneItemDict.TryGetValue(SceneManager.GetActiveScene().name,out currentSceneItems))
            {
                if (currentSceneItems != null)
                {
                    //清场
                    foreach(var item in FindObjectsOfType<Item>())
                    {
                        Destroy(item.gameObject);
                    }
                    foreach (var item in currentSceneItems)
                    {
                        Item newItem = Instantiate(itemPrefab, item.position.ToVector3(),Quaternion.identity,itemParent);

                        newItem.Init(item.itemID);
                    }

                }
            }
        }
        /// <summary>
        /// 获得场景中所有的家具
        /// </summary>
        private void GetAllSceneFurniture()
        {
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();

            //获取当前场景中所有的item
            foreach (var item in FindObjectsOfType<Furniture>())
            {
                SceneFurniture sceneFurniture = new SceneFurniture
                {
                    itemID = item.itemID,
                    position = new SerializableVector3(item.transform.position+new Vector3(-0.5f,-0.5f,0))
                };
                if (item.GetComponent<Box>())
                    sceneFurniture.boxIndex = item.GetComponent<Box>().index;
              
                   
                currentSceneFurniture.Add(sceneFurniture);
            }
            //查找sceneFurnitureDict是否有该场景的名字
            if (sceneFurnitureDict.ContainsKey(SceneManager.GetActiveScene().name))
            {

                //更新数据
                sceneFurnitureDict[SceneManager.GetActiveScene().name] = currentSceneFurniture;
            }
            else//没有
            {
                sceneFurnitureDict.Add(SceneManager.GetActiveScene().name, currentSceneFurniture);
            }
        }

        public bool GetisGridFurniture(Vector3 position)
        {
            GetAllSceneFurniture();
            List<SceneFurniture> sceneFurnitures= sceneFurnitureDict[SceneManager.GetActiveScene().name];
            for (int i = 0; i < sceneFurnitures.Count; i++)
            {             
                if (sceneFurnitures[i].position.ToVector3() == position)
                {
               
                    return false;
                    
                }
            }
           
            return true;
            
        }

       


        /// <summary>
        /// 重建蓝图建筑
        /// </summary>
        private void RebuildFurniture()
        {
            List<SceneFurniture> currentSceneFurniture = new List<SceneFurniture>();
            if(sceneFurnitureDict.TryGetValue(SceneManager.GetActiveScene().name, out currentSceneFurniture))
            {
                if(currentSceneFurniture != null)
                {
                    foreach(var scenefurniture in currentSceneFurniture)
                    {
                        var key = SceneManager.GetActiveScene().name + scenefurniture.itemID + scenefurniture.boxIndex;
                        if (sceneisFurniture.ContainsKey(key)){
                            if (sceneisFurniture[key])
                            {
                               
                                continue;
                            }
                               
                        }
                        BluePrintDetails bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(scenefurniture.itemID);
                        var buildItem = Instantiate(bluePrintDetails.buildPrefab, scenefurniture.position.ToVector3() + new Vector3(0.5f, 0.5f, 0), Quaternion.identity, itemParent);
                        if (sceneisFurniture.ContainsKey(key))
                        {
                            
                            sceneisFurniture[key]=true;
                        }
                           

                        if (buildItem.GetComponent<Box>())
                        {
                            buildItem.GetComponent<Box>().InitBox(scenefurniture.boxIndex);
                        }
                    }
                }
            }
        }

        public GameSaveData GenerateSaveData()
        {
            GetAllSceneFurniture();
            GetAllSceneItems();

            GameSaveData saveData = new GameSaveData();
            saveData.sceneItemDict=this.sceneItemDict;
            saveData.sceneFurnitureDict=this.sceneFurnitureDict;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            this.sceneItemDict = saveData.sceneItemDict;
            this.sceneFurnitureDict=saveData.sceneFurnitureDict;

            RecreateAllItems();
            RebuildFurniture();

        }
    }

}
