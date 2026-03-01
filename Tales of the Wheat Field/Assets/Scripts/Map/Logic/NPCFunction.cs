using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFunction : MonoBehaviour
{
    public InventoryBag_SO shopData;
    /// <summary>
    /// 是否打开背包
    /// </summary>
    private bool isOpen;

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            //关闭背包
            CloseShop();
        }
    }
    public void OpenShop()
    {
        isOpen = true;
        EventHandler.CallBaseBayOpenEvent(SlotType.Shop,shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }
    public void CloseShop()
    {
        isOpen = false;
        EventHandler.CallBaseBayCloseEvent(SlotType.Shop,shopData);
        EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
    }
}
