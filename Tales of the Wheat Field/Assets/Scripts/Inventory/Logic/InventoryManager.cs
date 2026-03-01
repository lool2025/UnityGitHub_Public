
using System.Collections.Generic;
using UnityEngine;
using MFarm.Save;

namespace MFarm.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>,ISaveable
    {
        [Header("背包数据")]
        public ItemDataList_SO ItemDataList_SO;

       

        [Header("背包数据")]
        public InventoryBag_SO playerBag;
        public InventoryBag_SO playerBagTemp;
        [Header("蓝图数据")]
        public BluPrintDataList_SO bluePrintData;

        private InventoryBag_SO currentBoxBag;

    

        private Dictionary<string,List<InventoryItem>> boxDataDict=new Dictionary<string,List<InventoryItem>>();

        
        public int BoxDataAmount => boxDataDict.Count;

        public string GUID => GetComponent<DataGUID>().guid;

        private void OnEnable()
        {
            EventHandler.DropItemEvent += OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
            //蓝图
            EventHandler.BuildFurnitureEvent += OnBuildFurnitureEvent;

            EventHandler.BaseBayOpenEvent += OnBaseBayOpenEvent;
            //新游戏
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        }

     

        private void OnDisable()
        {
            EventHandler.DropItemEvent -= OnDropItemEvent;
            EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
            //蓝图
            EventHandler.BuildFurnitureEvent -= OnBuildFurnitureEvent;

            EventHandler.BaseBayOpenEvent += OnBaseBayOpenEvent;

            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        }

      

        public void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();
        }
        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.K))
            {
                cheatitam();
            }
        }

        public void cheatitam()
        {

            AddItem(1001, 1);
            AddItem(1002, 1);
            AddItem(1003, 100);
            AddItem(1006, 100);
            AddItem(1009, 1);
            AddItem(1010, 1);
            AddItem(1011, 1);
            AddItem(1023, 1);
            AddItem(1024, 100);
            AddItem(1025, 100);
            AddItem(1027, 1);
            AddItem(1028, 100);
            AddItem(1008, 100);
            AddItem(1013, 100);
        }


        private void OnStartNewGameEvent(int obj)
        {
            playerBag = Instantiate(playerBagTemp);
            StatsManager.Instance.playerMoney=Settings.playerStartMoney;
            boxDataDict.Clear();
            EventHandler.CallUpdataInventoryUI(InventoryLocation.Player,playerBag.itemList);

        }


        private void OnBaseBayOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
        {
            currentBoxBag = bag_SO;
        }

        private void OnBuildFurnitureEvent(int ID, Vector3 pos)
        {
            RemoveItem(ID, 1);
            BluePrintDetails bluePrintDetails=bluePrintData.GetBluePrintDetails(ID);
            foreach(var item in bluePrintDetails.resourceItem)
            {
                RemoveItem(item.itemID,item.itemAmount);
            }
        }

        private void OnHarvestAtPlayerPosition(int ID)
        {
            var index = GetItemIndexInBag(ID);

            AddItemAtIndex(ID, index, 1);

            //更新UI
            EventHandler.CallUpdataInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }


        private void OnDropItemEvent(int ID, Vector3 pos, ItemType itemType)
        {
            RemoveItem(ID, 1);
        }

        /// <summary>
        /// 通过ID返回物品信息
        /// </summary>
        /// <param name="ID">ItemID</param>
        /// <returns></returns>
        public ItemDetails GetItemDetails(int ID)
        {
            return ItemDataList_SO.ItemDetailsList.Find(i=>i.itemID == ID);
        }
        /// <summary>
        /// 添加物品到player背包
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestory">是否删除</param>
        public void AddItem(Item item ,bool toDestory)
        {
            
            var index=GetItemIndexInBag(item.itemID);

            AddItemAtIndex(item.itemID, index,1);

            item.Playanimation();

            if (toDestory) 
            { 
                  Destroy(item.gameObject,0.5f);
            }
            //更新UI

            EventHandler.CallUpdataInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }
        /// <summary>
        /// 添加物品
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestory">是否删除</param>
        public void AddItem(int itemID,int shu)
        {

            var index = GetItemIndexInBag(itemID);

            AddItemAtIndex(itemID, index, shu);

            //更新UI

            EventHandler.CallUpdataInventoryUI(InventoryLocation.Player, playerBag.itemList);
        }






        /// <summary>
        /// 检擦背包是否有空位
        /// </summary>
        /// <returns>返回-1就没有</returns>
        private bool CheckBagCapacity()
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID==0)
                    return true;
            }
            return false;

        }
        /// <summary>
        /// 通过物品ID找到背包已有物品位置
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private int GetItemIndexInBag(int ID)
        {
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == ID)
                    return i;
            }
            return -1;
        }
        /// <summary>
        /// 在指定的背包序号位置添加物品
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <param name="index">序号</param>
        /// <param name="amount">增加的数量</param>
        private void AddItemAtIndex(int ID, int index, int amount)
        {
            if (index == -1&& CheckBagCapacity())//背包里没有这个东西同时背包有空位
            {
              
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                for (int i = 0; i < playerBag.itemList.Count; i++)
                {
                    if (playerBag.itemList[i].itemID == 0)
                    {
                        playerBag.itemList[i] = item;
                        break;
                    }

                }

            }
            else
            {
           
                int currentAmount = playerBag.itemList[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };

                playerBag.itemList[index] = item;//在许多游戏框架中，物品数据通常被设计为不可变对象或值类型
            }
        }
        /// <summary>
        /// Player背包范围内交换物品
        /// </summary>
        /// <param name="fromIndex">起始目标</param>
        /// <param name="targetIndex">交换目标</param>
        public void SwapItem(int fromIndex,int targetIndex)
        {

            InventoryItem currentItem = playerBag.itemList[fromIndex];
            InventoryItem targetItem = playerBag.itemList[targetIndex];

            if (targetItem.itemID != 0)
            {
                playerBag.itemList[fromIndex]=targetItem;
                playerBag.itemList[targetIndex]=currentItem;
            }
            else
            {
                playerBag.itemList[targetIndex] = currentItem;
                playerBag.itemList[fromIndex] = new InventoryItem();
            }
            EventHandler.CallUpdataInventoryUI(InventoryLocation.Player,playerBag.itemList);

        }
        /// <summary>
        /// 跨背包交换数据
        /// </summary>
        /// <param name="locationFrom"></param>
        /// <param name="fromIndex"></param>
        /// <param name="locationTarget"></param>
        /// <param name="targetIndex"></param>
        public void SwapItem(InventoryLocation locationFrom,int fromIndex, InventoryLocation locationTarget,int targetIndex)
        {
            var currentList = GetItemList(locationFrom);
            var targetList = GetItemList(locationTarget);

            InventoryItem currentItem = currentList[fromIndex];
            if (targetIndex < targetList.Count)
            {
                InventoryItem targetItem=targetList[targetIndex];
                if(targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID)//有不同的两个物品
                {
                    currentList[fromIndex]=targetItem;
                    targetList[targetIndex]=currentItem;
                }
                else if (currentItem.itemID == targetItem.itemID)
                {
                    targetItem.itemAmount += currentItem.itemAmount;
                    targetList[targetIndex] = targetItem;
                    currentList[fromIndex]= new InventoryItem();
                }
                else
                {
                    targetList[targetIndex] = currentItem;
                    currentList[fromIndex] = new InventoryItem();
                }
                EventHandler.CallUpdataInventoryUI(locationFrom,currentList);
                EventHandler.CallUpdataInventoryUI(locationTarget, targetList);
            }
        }

        /// <summary>
        /// 根据位置返回背包数据列表
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private List<InventoryItem> GetItemList(InventoryLocation location)
        {
            return location switch
            {
                InventoryLocation.Player => playerBag.itemList,
                InventoryLocation.Box => currentBoxBag.itemList,
                _ => null
            };

        }

        /// <summary>
        /// 移除指定数量的背包物品
        /// </summary>
        /// <param name="ID">物品ID</param>
        /// <param name="removeAmount">数量</param>
        private void RemoveItem(int ID ,int removeAmount)
        {
            var index=GetItemIndexInBag(ID);

            if (playerBag.itemList[index].itemAmount > removeAmount)
            {
                var amount=playerBag.itemList[index].itemAmount-removeAmount;
                var item =new InventoryItem { itemID=ID, itemAmount = amount };
                playerBag.itemList[index]=item;
                    
            }
            else if(playerBag.itemList[index].itemAmount == removeAmount)
            {
                var item = new InventoryItem ();
                playerBag.itemList[index] = item;

            }
            EventHandler.CallUpdataInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }
        /// <summary>
        /// 交易
        /// </summary>
        /// <param name="itemDetails">交易物品信息</param>
        /// <param name="amount">数量</param>
        /// <param name="isSellTrade">是否卖</param>
        public void TradeItem(ItemDetails itemDetails,int amount,bool isSellTrade)
        {
            int cost=itemDetails.itemPrice*amount;
            //获取物体背包位置
            int index = GetItemIndexInBag(itemDetails.itemID);

            if(isSellTrade)//卖
            {
                if (playerBag.itemList[index].itemAmount >= amount)
                {
                    RemoveItem(itemDetails.itemID, amount);
                    //卖出总价
                    cost=(int)(cost*itemDetails.sellPercentage);
                    StatsManager.Instance.playerMoney += cost;
                }
            }
            else if (StatsManager.Instance.playerMoney - cost >= 0)//买
            {
                if (CheckBagCapacity())
                {
                    AddItemAtIndex(itemDetails.itemID,index, amount);
                }
                StatsManager.Instance.playerMoney -= cost;
            }
            //刷新UI
            EventHandler.CallUpdataInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }

        public bool CheckStock(int ID)
        {
            var biuePrintDetails = bluePrintData.GetBluePrintDetails(ID);
            foreach (var resourceItem in biuePrintDetails.resourceItem)
            {
                var itemStock=playerBag.GetInventoryItem(resourceItem.itemID);
                if (itemStock.itemAmount >= resourceItem.itemAmount)
                {
                    continue;
                }
                else return false;

            }
            return true;
        }
        /// <summary>
        /// 查找箱子数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<InventoryItem> GetBoxDataList(string key)
        {
            if (boxDataDict.ContainsKey(key))
            {
                return boxDataDict[key];
            }
            return null;
        }

        public void AddBoxDataDict(Box box)
        {
            var key = box.name + box.index;
            if(!boxDataDict.ContainsKey(key))
                boxDataDict.Add(key,box.boxBagData.itemList);
        }

        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.playerMoney = StatsManager.Instance.playerMoney;
            //保存player背包数据
            saveData.inventoryDict = new Dictionary<string, List<InventoryItem>>();
            saveData.inventoryDict.Add(playerBag.name, playerBag.itemList);
            //保存场景箱子数据
            foreach (var item in boxDataDict)
            {
                saveData.inventoryDict.Add(item.Key, item.Value);
            }
            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
            StatsManager.Instance.playerMoney = saveData.playerMoney;
            playerBag=Instantiate(playerBagTemp);
            playerBag.itemList = saveData.inventoryDict[playerBag.name];

            foreach (var item in saveData.inventoryDict)
            {
                if(boxDataDict.ContainsKey(item.Key))
                {
                    boxDataDict[item.Key]=item.Value;
                }
            }
            EventHandler.CallUpdataInventoryUI(InventoryLocation.Player,playerBag.itemList);
        }
    }
}

