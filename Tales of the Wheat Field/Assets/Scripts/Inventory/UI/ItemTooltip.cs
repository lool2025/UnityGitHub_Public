using System.Collections;
using System.Collections.Generic;
using MFarm.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    [SerializeField] private TextMeshProUGUI typeText;

    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private TextMeshProUGUI valueText;

    [SerializeField] private GameObject bottomPart;

    [Header("建造")]
    public GameObject resourcePanle;
    [SerializeField] private Image[] resourceItem;


    public void SetupTooltip(ItemDetails itemDetails,SlotType slotType)
    {
        nameText.text=itemDetails.itemName;

        typeText.text= GetItemType(itemDetails.itemType);

        descriptionText.text = itemDetails.itemDescription;

        if (itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.Commodity || itemDetails.itemType == ItemType.Furniture)
        {

            bottomPart.SetActive(true);

            var price = itemDetails.itemPrice;

            if (slotType == SlotType.Bag)
            {
                price = (int)(price * itemDetails.sellPercentage);
            }
            valueText.text = price.ToString();
        }
        else 
        { 
            bottomPart.SetActive(false);
        }
        //强制渲染窗口
       LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

    }
    /// <summary>
    /// 将类型改为中文
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    private string GetItemType(ItemType itemType)
    {
        return itemType switch
        {

            ItemType.Seed => "种子",
            ItemType.Commodity => "商品",
            ItemType.Furniture => "家具",
            ItemType.BreakTool => "工具",
            ItemType.ChopTool => "工具",
            ItemType.CollectTool => "工具",
            ItemType.HoeTool => "工具",
            ItemType.ReapTool => "工具",
            ItemType.WaterTool => "工具",
            _ => "无"
        };
    }
    /// <summary>
    /// 获取SO里面的数据更新UI
    /// </summary>
    /// <param name="bluePrintDetails"></param>
    public void SetipResourcePanel(int ID)
    {
        var bluePrintDetails=InventoryManager.Instance.bluePrintData.GetBluePrintDetails(ID);
        for (int i = 0;i< resourceItem.Length; i++)
        {
            if (i <bluePrintDetails.resourceItem.Length )
            {
                var item= bluePrintDetails.resourceItem[i];
                resourceItem[i].gameObject.SetActive(true);
                resourceItem[i].sprite = InventoryManager.Instance.GetItemDetails(item.itemID).itemIcon;
                resourceItem[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=item.itemAmount.ToString();
            }
            else
            {
                resourceItem[i].gameObject.SetActive(false);
            }
        }
    }
}
