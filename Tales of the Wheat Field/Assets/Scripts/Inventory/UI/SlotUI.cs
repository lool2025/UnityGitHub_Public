
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace MFarm.Inventory 
{

    public class SlotUI : MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        [Header("组件获取")]
        [SerializeField] private Image slotImage;        // 物品图标
        [SerializeField] private TextMeshProUGUI amountText;  // 物品数量文本
        public Image slotHightlight;   // 选中高亮图像
        [SerializeField] private Button button;          // 交互按钮
        public Button effectButton;   // 使用按键

        [Header("格子类型")]
        public SlotType slotType;     // 槽位类型（如背包、装备等）
        public ItemDetails itemDetails; // 物品详情数据
        public int itemAmount;        // 物品数量
        public bool isSelected;       // 是否被选中

        public int slotIndex;

        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player
                };
            }
        }


        public InventoryUI inventoryUI=>GetComponentInParent<InventoryUI>();

        public void Start()
        {
            isSelected = false;
            if (itemDetails==null)
            {
                UpdateEmptySlot();
            }

           
        }


        /// <summary>
        /// 更新格子UI和信息
        /// </summary>
        /// <param name="item">itemDetails</param>
        /// <param name="amount">持有数量</param>
        public void UpdateSlot(ItemDetails item, int amount)
        {

            slotImage.enabled =true;
            itemDetails = item;
            slotImage.sprite = item.itemIcon;
            itemAmount = amount;
            amountText.text = amount.ToString();
            button.interactable = true;
        }



        /// <summary>
        /// 将Slot更新为空
        /// </summary>
        public void UpdateEmptySlot()
        {
            if (isSelected)
            {
                isSelected = false;

                inventoryUI.UpdateSlotHightlight(-1);
                //通知物品被选中的状态和信息
                EventHandler.CallItemSelectedEvent(itemDetails,isSelected);
            }
            itemDetails=null;
            slotImage.enabled = false;
            amountText.text = string.Empty;
            button.interactable = false;
        }
        //点击快捷栏的方法
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right&& itemDetails.canEffect&& Location== InventoryLocation.Player)
            {
                effectButton.gameObject.SetActive(true);
                Debug.Log("测试567");
            }
            // 正确的调用方式
            EventHandler.CallPlayerAnimNone();
            if ((itemDetails==null)) return;

            isSelected =!isSelected;
            inventoryUI.UpdateSlotHightlight(slotIndex);
            
            if (slotType == SlotType.Bag)
            {   //通知物品被选中的状态和信息
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
                
            }
        }
        /// <summary>
        /// 按住物体并开始拖动的瞬间触发（拖拽的起点）
        /// </summary>
        /// <param name="eventData"></param>
        public void OnBeginDrag(PointerEventData eventData)
        {
          
            if (itemAmount != 0)
            {
              
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItem.sprite=slotImage.sprite;
                inventoryUI.dragItem.SetNativeSize();

                isSelected = true;
                inventoryUI.UpdateSlotHightlight(slotIndex);
            }
        }
        /// <summary>
        /// 拖拽过程中持续触发（每帧），用于处理物体跟随鼠标移动
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.transform.position=Input.mousePosition;
        }


        /// <summary>
        /// 松开鼠标 / 结束触摸时触发（拖拽的终点），处理拖拽结束后的逻辑
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled=false;

            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>()== null)
                    return;
                var targetSlot=eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                int targetIndex=targetSlot.slotIndex;
          
                //在player自身背包范围内交换
                if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex,targetIndex);
                }
                else if(slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)//买
                {
         
                    EventHandler.CallShowTradeUI(itemDetails, false);
                }
                else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)//卖
                {
                    
                    EventHandler.CallShowTradeUI(itemDetails,true);
                }else if (slotType != SlotType.Shop&& targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                {
                   
                    //跨背包交换
                    InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
                }else if(slotType == SlotType.Box && targetSlot.slotType == SlotType.Box)
                {
                    InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
                }
               
                itemAmount = 0;
                //将所有高亮显示取消
                inventoryUI.UpdateSlotHightlight(-1);

            }
            //else//测试物品人在地上
            //{
            //    if (itemDetails.canDropped)
            //    {

            //        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

            //        EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
            //    }
               
            //}
        }
    }

}
