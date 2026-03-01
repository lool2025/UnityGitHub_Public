using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Inventory
{
    [RequireComponent(typeof(SlotUI))]
    public class ActionBarButton : MonoBehaviour
    {
        public KeyCode key;
        private SlotUI slotUI;

        private bool canuse=true;
        private void Awake()
        {
            slotUI = GetComponent<SlotUI>();
        }

        private void OnEnable()
        {
            EventHandler.UpdateGameStateEvent += OnCallUpdateGameStateEvent;
        }

        private void OnDisable()
        {
            EventHandler.UpdateGameStateEvent += OnCallUpdateGameStateEvent;
        }

        private void OnCallUpdateGameStateEvent(GameState state)
        {
            canuse=state==GameState.GamePlay;
        }

        private void Update()
        {
           
            if(Input.GetKeyDown(key)&& canuse)
            {
                if(slotUI.itemDetails != null)
                {
                    slotUI.isSelected=!slotUI.isSelected;
                    if(slotUI.isSelected )
                    {
                        slotUI.inventoryUI.UpdateSlotHightlight(slotUI.slotIndex);
                    }
                    else
                    {
                        slotUI.inventoryUI.UpdateSlotHightlight(-1);
                    }

                    EventHandler.CallItemSelectedEvent(slotUI.itemDetails,slotUI.isSelected);
                }
            }
        }
    }
}