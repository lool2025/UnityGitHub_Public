using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Inventory
{
    public class Box : MonoBehaviour
    {
        public InventoryBag_SO boxBagTemplate;
        public InventoryBag_SO boxBagData;

        public GameObject mouseIcon;
        private bool canOpen =>mouseIcon.GetComponent<Sign>().canOpen;
        private bool isOpen;

        public int index;

        public float rayDistance = 100f;
       

        private void OnEnable()
        {
            if (boxBagData == null)
            {
                boxBagData=Instantiate(boxBagTemplate);
            }

           
        }
       
        public void Update()
        {
          
            if(canOpen&&!isOpen&&Input.GetMouseButtonDown(1))
            {
               
                // 1. 转换鼠标屏幕坐标为世界坐标（2D相机需注意Z轴）
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0; // 2D物体Z轴通常为0，避免深度偏差

                // 2. 创建2D射线（方向无关，因为2D射线是点检测）
                RaycastHit2D hitInfo = Physics2D.Raycast(mouseWorldPos, Vector2.zero, rayDistance);
                // 3. 检测结果
                    if (hitInfo)
                    {
                        // 关键新增：判断命中的碰撞体是否属于当前脚本所在的箱子
                        if (hitInfo.collider.gameObject == this.gameObject|| hitInfo.collider.gameObject==this.transform.GetChild(0).gameObject)
                        {
                           
                            EventHandler.CallBaseBayOpenEvent(SlotType.Box, boxBagData);
                            isOpen = true;
                        }
                        else
                        {
                     
                           
                        }
                    }
                 
               

            }
            if (isOpen && !canOpen)
            {
                EventHandler.CallBaseBayCloseEvent(SlotType.Box,boxBagData);

                isOpen=false;
            }
            if(isOpen && Input.GetKeyDown(KeyCode.Escape))
             {
                EventHandler.CallBaseBayCloseEvent(SlotType.Box, boxBagData);
                isOpen=false;
            }
        }

        private void OnClicked()
        {
           
            EventHandler.CallBaseBayOpenEvent(SlotType.Box, boxBagData);
            isOpen = true;
        }

        public void InitBox(int boxIndex)
        {
            index = boxIndex;
            var key = this.name + index;
            if (InventoryManager.Instance.GetBoxDataList(key) != null)
            {
                boxBagData.itemList = InventoryManager.Instance.GetBoxDataList(key);
            }
            else
            {
                InventoryManager.Instance.AddBoxDataDict(this);
            }
        }

    }
}