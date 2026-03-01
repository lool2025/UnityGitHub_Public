using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace MFarm.Inventory
{
    public class ShowItemTooltip : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        private SlotUI slotUI;

        //查找激活的父级（含自身）中第一个T类型组件
        private InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

       

        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotUI.itemDetails!=null)
            {
                inventoryUI.itemTooltip.gameObject.SetActive(true);
                inventoryUI.itemTooltip.SetupTooltip(slotUI.itemDetails, slotUI.slotType);
                //设置锚点中下
                inventoryUI.itemTooltip.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
                //设置位置在鼠标的上面
               inventoryUI.itemTooltip.transform.position = transform.position + Vector3.up * 60;

                if (slotUI.itemDetails.itemType == ItemType.Furniture)
                {
                    inventoryUI.itemTooltip.resourcePanle.SetActive(true);
                    inventoryUI.itemTooltip.SetipResourcePanel(slotUI.itemDetails.itemID);
                }
                else
                {
                    inventoryUI.itemTooltip.resourcePanle.SetActive(false);
                }

            }
            else
            {
                inventoryUI.itemTooltip.gameObject.SetActive(false);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            inventoryUI.itemTooltip.gameObject.SetActive(false);
        }

    }

}
