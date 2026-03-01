using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory 
{
    public class ItemPickUp : MonoBehaviour
    {
        /// <summary>
        /// 物品捡起
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerEnter2D(Collider2D other)
        {
            Item item = other.GetComponent<Item>();

            if (item != null)
            {   //判断是否能拾取
                if (item.itemDetails.canPickedup)
                {
                    
                    //拾取物品添加到背包
                    InventoryManager.Instance.AddItem(item, true);
                    //音效
                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);
                }
            }
        }
    }
}


