using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace MFarm.Inventory
{
    public class InventoryUI : MonoBehaviour
    {

        public ItemTooltip itemTooltip;

        [Header("拖拽图片")]
        public Image dragItem;


        [Header("玩家背包")]
        [SerializeField]private GameObject bagUI;

        private bool bagOpened;

        [Header("通用背包")]
        [SerializeField] private GameObject baseBag;
        public GameObject shopSlotPrefab;

        [Header("Box")]
        public GameObject boxSlotPrefab;

        [Header("详细页面")]
        public GameObject ItemToolTip;
        [Header("鼠标部件")]
        public GameObject cursorCanvas;

        [SerializeField] private SlotUI[] playerSlots;//所有的格子

        [SerializeField] private List<SlotUI> baseBagSlots;

        [Header("交易UI")]
        public TradeUI tradeUI;
        [Header("Player金钱数量")]
        public TextMeshProUGUI playerMoneyText;
        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            //场景卸载是取消高亮选择
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
            //商店开启
            EventHandler.BaseBayOpenEvent += OnBaseBayOpenEvent;
            EventHandler.BaseBayCloseEvent += OnBaseBayCloseEvent;
            //交易
            EventHandler.ShowTradeUI += OnShowTradeUI;
        }

     
        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            //场景卸载是取消高亮选择
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
            EventHandler.BaseBayOpenEvent -= OnBaseBayOpenEvent;
            EventHandler.BaseBayCloseEvent -= OnBaseBayCloseEvent;

            EventHandler.ShowTradeUI -= OnShowTradeUI;
        }

      

        public void Start()
        {
            //给每个格子序号
            for (int i = 0; i < playerSlots.Length; i++) 
            { 
                playerSlots[i].slotIndex = i;
            }
            bagOpened=bagUI.activeInHierarchy;

            playerMoneyText.text= StatsManager.Instance.playerMoney.ToString();
        }


        private void OnShowTradeUI(ItemDetails item, bool isSell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(item, isSell);
        }


        /// <summary>
        /// 打开通用背包界面
        /// </summary>
        /// <param name="slottype"></param>
        /// <param name="bagData"></param>
        private void OnBaseBayOpenEvent(SlotType slottype, InventoryBag_SO bagData)
        {
            GameObject prefab = slottype switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null
            };

            baseBag.SetActive(true);

            baseBagSlots = new List<SlotUI>();
           
            for (int i = 0; i < bagData.itemList.Count; i++)
            {
              
                var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<SlotUI>();
                slot.slotIndex = i;
                baseBagSlots.Add(slot);
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());

            if (slottype == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-1, 0.5f);
                bagUI.SetActive(true);
                bagOpened=true;
            }

            //更新UI
            OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList);
        }
        /// <summary>
        /// 关闭通用背包界面
        /// </summary>
        /// <param name="slottype"></param>
        /// <param name="bagData"></param>
        private void OnBaseBayCloseEvent(SlotType slottype, InventoryBag_SO bagData)
        {
            baseBag.SetActive(false);
            itemTooltip.gameObject.SetActive(false);
            UpdateSlotHightlight(-1);

            GameObject currentItem = baseBag.transform.GetChild(0).gameObject;
            if (currentItem != null)
            {
                for (int i = currentItem.transform.childCount - 1; i >= 0; i--)
                {

                    Destroy(currentItem.transform.GetChild(i).gameObject);

                }
            }
            //foreach (var slot in baseBagSlots)
            //{
                
            //    Destroy(slot.gameObject);
            //}
            baseBagSlots.Clear();

            if (slottype == SlotType.Shop)
            {
                bagUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                bagUI.SetActive(false);
                bagOpened = false;
            }

        }
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)//将这个方法传入EventHandler,在EventHandler中使用这个方法，简单的来说将这个值由EventHandler来填写
        {
            switch (location) 
            {
                case InventoryLocation.Player:
                    for (int i = 0; i < playerSlots.Length; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            playerSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            playerSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
                case InventoryLocation.Box:
                    for (int i = 0; i < baseBagSlots.Count; i++)
                    {
                        if (list[i].itemAmount > 0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);
                        }
                        else
                        {
                            baseBagSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
            }
            playerMoneyText.text= StatsManager.Instance.playerMoney.ToString();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                OpenBagUI();
                ItemToolTip.gameObject.SetActive(false);
                cursorCanvas.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// 打开背包UI，Button调用事件
        /// </summary>
        public void OpenBagUI()
        {
            bagOpened=!bagOpened;
            bagUI.SetActive(bagOpened);
        }

        private void OnBeforeSceneUnloadEvent()
        {
            // //场景卸载是取消高亮选择
            UpdateSlotHightlight(-1);
        }


        /// <summary>
        /// 更新高亮显示
        /// </summary>
        /// <param name="index"></param>
        public void UpdateSlotHightlight(int index)
        {
            foreach (var slot in playerSlots)
            {
                if(slot.isSelected && slot.slotIndex == index)
                {
         
                    slot.slotHightlight.gameObject.SetActive(true);
                }
                else
                {
                    slot.isSelected = false;
                    slot.slotHightlight.gameObject.SetActive(false);
                }
            }
        }

    }
}

