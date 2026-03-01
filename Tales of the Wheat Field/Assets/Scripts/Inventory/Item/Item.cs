using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFarm.CropPlant;
namespace MFarm.Inventory 
{
    public class Item : MonoBehaviour
    {
        public int itemID;

        private Animator anim;

        private SpriteRenderer spriteRenderer;

        public ItemDetails itemDetails;

        private BoxCollider2D coll;

        private void Awake()
        {
            anim=GetComponent<Animator>();

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            coll = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if (itemID != 0) { 
                Init(itemID);
            }
        }

        public void Init(int ID)
        {
            this.itemID = ID;

            //Inventory获取当前数据
            itemDetails=InventoryManager.Instance.GetItemDetails(itemID);

            if(itemDetails != null)
            {
                spriteRenderer.sprite=itemDetails.itemOnWorldSprite!=null ? itemDetails.itemOnWorldSprite :itemDetails.itemIcon;

                //修改碰撞体大小
                Vector2 newSize = new Vector2(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y);

                coll.size = newSize;
                coll.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);


            }
            if(itemDetails.itemType==ItemType.ReapablsScenery)
            {
                gameObject.AddComponent<ReapItem>();
                gameObject.GetComponent<ReapItem>().InitCropData(itemDetails.itemID);
                gameObject.AddComponent<ItemInteractive>();
            }
        }

        public void Playanimation()
        {
            if (anim != null) 
            {
                Debug.Log("播放");
                anim.SetBool("isPlay",true);
            }
        }


    }
}


