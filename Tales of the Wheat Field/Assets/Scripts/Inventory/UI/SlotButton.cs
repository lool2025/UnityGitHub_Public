using System.Collections;
using System.Collections.Generic;
using MFarm.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotButton : MonoBehaviour, IPointerExitHandler
{
    public SlotUI SlotUI => GetComponentInParent<SlotUI>();
    public void OnPointerExit(PointerEventData eventData)
    {
        gameObject.SetActive(false);
    }

    

    public void Usageeffect()
    {
        if (SlotUI != null)
        {
            EventHandler.CallusageEffectEvent(SlotUI.itemDetails);
            SlotUI.itemAmount--;
            if (SlotUI.itemAmount > 0)
                SlotUI.UpdateSlot(SlotUI.itemDetails, SlotUI.itemAmount);
            else
                SlotUI.UpdateEmptySlot();
        }
            

    }

}
