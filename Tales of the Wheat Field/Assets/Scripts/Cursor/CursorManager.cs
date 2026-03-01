using MFarm.CropPlant;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MFarm.Map;
using MFarm.Inventory;
public class CursorManager : MonoBehaviour
{
    // 光标样式（不同物品类型对应不同Sprite）
    public Sprite normal, tool, seed, item, attack;
    private Sprite currentSprite;       // 当前使用的光标Sprite

    // 建造预览相关
    private Image buildImage;           // 家具建造预览图的Image组件
    private Image cursorImage;          // 光标图片的Image组件
    private RectTransform cursorCanvas; // 光标所在的Canvas（UI根节点）

    // 鼠标位置相关
    private Vector3 mouseWorldPos;      // 鼠标在世界空间的坐标
    private Vector3Int mouseGridPos;    // 鼠标所在的网格坐标（Grid系统）

    // 依赖组件
    private Camera mainCamera;          // 主相机（用于屏幕坐标转世界坐标）
    private Grid currentGrid;           // 当前场景的Grid组件（网格管理）
    private ItemManager ItemManager;    // 物品管理器（检测家具放置）

    // 状态控制
    private bool cursorEnable;          // 光标是否启用（选中物品后启用）
    private bool cursorPositionValid;   // 鼠标位置是否有效（能否执行操作）
    private ItemDetails currentItem;    // 当前选中的物品详情
    private Transform PlayerTransform => GameObject.FindWithTag("Player")?.transform; // 玩家位置（简化写法）

 

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;       // 物品选中/取消事件
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent; // 场景卸载前
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadedEvent; // 场景加载后
    }


    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadedEvent;
    }

  

    private void Start()
    {
        // 获取光标Canvas和子组件（光标图片、建造预览图）
        cursorCanvas = GameObject.FindWithTag("CursorCanvas").GetComponent<RectTransform>();
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
        buildImage = cursorCanvas.GetChild(1).GetComponent<Image>();
        buildImage.gameObject.SetActive(false); // 初始隐藏建造预览图

        // 获取物品管理器
        ItemManager = GameObject.FindWithTag("ItemManager").GetComponent<ItemManager>();

        // 初始光标样式为默认
        currentSprite = normal;
        SetCursorImage(normal);

        // 获取主相机
        mainCamera = Camera.main;

    }

    private void Update()
    {
        if (cursorCanvas == null) return; // 空值保护

        // 光标图片跟随鼠标位置
        cursorImage.transform.position = Input.mousePosition;

        // 条件：不在UI上 + 光标启用（选中物品）
        if (!InteractWithUI() && cursorEnable)
        {
            SetCursorImage(currentSprite); // 设置当前选中物品对应的光标样式
            CheckCursorValid();            // 检测鼠标位置是否有效（更新光标颜色）
            CheckPlayerInput();            // 检测鼠标点击（执行操作）
        }
        else
        {
            SetCursorImage(normal);        // 在UI上/光标禁用 → 恢复默认光标
            //buildImage.gameObject.SetActive(false); // 注释掉的代码：隐藏建造预览图
        }

    }

    private void CheckPlayerInput()
    {
        if (Input.GetMouseButtonDown(0) && cursorPositionValid)
        {
            //执行方法
            EventHandler.CallMouseClickedEvent(mouseGridPos, currentItem);
        }
    }


    #region 设置鼠标样式
    /// <summary>
    /// 设置鼠标图片
    /// </summary>
    /// <param name="sprite"></param>
    private void SetCursorImage(Sprite sprite)
    {
        Vector2 spritePixelSize = sprite.rect.size;
        Vector2 spritePixelPivot = sprite.pivot;
        float pivotX = spritePixelPivot.x / spritePixelSize.x;
        float pivotY = spritePixelPivot.y / spritePixelSize.y;
        Vector2 uiNormalizedPivot = new Vector2(pivotX, pivotY);
        cursorImage.sprite = sprite;
        cursorImage.gameObject.GetComponent<RectTransform>().pivot= uiNormalizedPivot; 
        cursorImage.color = new Color(1, 1, 1, 1);
    }
    /// <summary>
    /// 设置鼠标可用
    /// </summary>
    private void SetCursorValid()
    {
        cursorPositionValid = true;
        cursorImage.color = new Color(1, 1, 1, 1);
        //图纸透明度降低
        buildImage.color = new Color(1, 1, 1, 0.8f);
    }
    /// <summary>
    /// 设置鼠标不可用
    /// </summary>
    private void SetCursorInValid()
    {
        cursorPositionValid = false;
        cursorImage.color = new Color(1, 0, 0, 0.4f);
        buildImage.color = new Color(1, 0, 0, 0.5f);
    }
    #endregion

    /// <summary>
    /// 物品选择事件函数
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
       
        if (!isSelected)
        {
            currentItem = null;
            cursorEnable = false;
            currentSprite = normal;
            buildImage.gameObject.SetActive(false);
           
        }
        else//物品被选中才显示其他图片
        {
            currentItem = itemDetails;
          
            //WORKFLOW:添加所有类型对应的图片
            currentSprite = itemDetails.itemType switch
            {
                ItemType.Seed => seed,          // 种子 → 种子光标
                ItemType.Commodity => item,     // 商品 → 物品光标
                ItemType.ChopTool => tool,      // 砍伐工具 → 工具光标
                ItemType.HoeTool => tool,       // 锄头 → 工具光标
                ItemType.WaterTool => tool,     // 水壶 → 工具光标
                ItemType.BreakTool => tool,     // 破坏工具 → 工具光标
                ItemType.ReapTool => tool,      // 收割工具 → 工具光标
                ItemType.Furniture => tool,     // 家具 → 工具光标
                ItemType.CollectTool => tool,   // 收集工具 → 工具光标
                ItemType.SwordTool=> attack,    // 剑 → 攻击光标
                _ => normal

            };
          
           //只有选中才使用
           cursorEnable = true;
            //显示建造物品图片
            if (itemDetails.itemType == ItemType.Furniture)
            {
                buildImage.gameObject.SetActive(true);
                buildImage.sprite = itemDetails.itemOnWorldSprite;
                buildImage.SetNativeSize();
            }
            else
            {
                buildImage.gameObject.SetActive(false);
            }

        }
    }

    private void CheckCursorValid()
    {
        // 1. 屏幕坐标转世界坐标（修正Z轴，避免深度问题）
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
        // 2. 世界坐标转网格坐标
        mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);

        // 3. 获取玩家所在网格坐标
        var playerGridPos = currentGrid.WorldToCell(PlayerTransform.position);
        // 4. 建造预览图跟随鼠标
        buildImage.rectTransform.position = Input.mousePosition;

        // 5. 第一步检测：是否在玩家操作半径内
        if (Mathf.Abs((mouseGridPos.x - playerGridPos.x)) > currentItem.itemUseRadius || Mathf.Abs((mouseGridPos.y - playerGridPos.y)) > currentItem.itemUseRadius)
        {
            SetCursorInValid();
            return;
        }
        if (currentItem.itemType == ItemType.SwordTool)
        {
            SetCursorValid();
            return;
        }
        // 6. 获取鼠标位置的瓦片详情
        TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);
        if (currentTile != null)
        {
            // 7. 获取作物详情（如果有）
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);
            // 8. 获取鼠标位置的作物对象（如果有）
            Crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);

            // 9. 根据物品类型检测有效性
            switch (currentItem.itemType)
            {
                case ItemType.Seed:
                    // 种子：瓦片已锄地 + 未播种 → 有效
                    if (currentTile.daysSinceDug > -1 && currentTile.seedItemID == -1) SetCursorValid();
                    else SetCursorInValid();
                    break;
                case ItemType.Commodity:
                    // 商品：瓦片可掉落物品 + 物品可掉落 → 有效
                    if (currentTile.canDropItem && currentItem.canDropped) SetCursorValid();
                    else SetCursorInValid();
                    break;
                case ItemType.HoeTool:
                    // 锄头：瓦片可锄地 → 有效
                    if (currentTile.canDig) SetCursorValid();
                    else SetCursorInValid();
                    break;
                case ItemType.WaterTool:
                    // 水壶：瓦片已锄地 + 未浇水 → 有效
                    if (currentTile.daysSinceDug > -1 && currentTile.daySinceWatered == -1) SetCursorValid();
                    else SetCursorInValid();
                    break;
                case ItemType.BreakTool:
                case ItemType.ChopTool:
                    // 破坏/砍伐工具：有作物 + 作物可收获 + 工具匹配 → 有效
                    if (crop != null)
                    {
                        if (crop.CanHarvest && crop.cropDetails.CheckToolAvailable(currentItem.itemID))
                        {
                            SetCursorValid();
                        }
                    }
                    else SetCursorInValid();
                    break;
                case ItemType.CollectTool:
                    // 收集工具：有作物详情 + 工具匹配 + 作物成熟 → 有效
                    if (currentCrop != null)
                    {
                        if (currentCrop.CheckToolAvailable(currentItem.itemID))
                        {
                            if (currentTile.growthDays >= currentCrop.TotalGrowthDays) SetCursorValid();
                            else SetCursorInValid();
                        }
                    }
                    else SetCursorInValid();
                    break;
                case ItemType.ReapTool:
                    // 收割工具：范围内有可收割物品 + 工具匹配 → 有效
                    
                    if (GridMapManager.Instance.HaveReapableItemsInRadius(mouseWorldPos, currentItem)) SetCursorValid();
                    else SetCursorInValid();
                    break;
                case ItemType.Furniture:
                    // 家具：瓦片可放置家具 + 物品库存充足 + 网格无家具 → 有效
                    if (currentTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID) && ItemManager.GetisGridFurniture(mouseGridPos)) SetCursorValid();
                    else SetCursorInValid();
                    break;
            }
        }
        else
        {
            // 瓦片详情为空 → 无效
            SetCursorInValid();
        }

    }




    /// <summary>
    /// 是否与UI有互动
    /// </summary>
    /// <returns></returns>
    private bool InteractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnAfterSceneLoadedEvent()
    {
        currentGrid = FindObjectOfType<Grid>();
      
    }

    private void OnBeforeSceneUnloadEvent()
    {
        cursorEnable = false;
    }

}
