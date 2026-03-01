using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 表示物品中的一个属性条目。
/// </summary>
/// <remarks>
/// 轻量级数据结构，用于存储物品的属性
/// </remarks>
[System.Serializable]
public class ItemDetails
{
    // 物品唯一ID（建议设为不可在Inspector编辑，避免重复）
    [Tooltip("物品唯一标识ID，不可重复")]
    public int itemID;

    // 物品基础信息
    [Tooltip("物品名称（UI显示用）")]
    public string itemName;
    [Tooltip("物品类型")]
    public ItemType itemType;
    [Tooltip("物品描述（悬浮提示用）")]
    [TextArea(2, 4)] // 多行文本框，方便编辑长描述
    public string itemDescription;

    // 视觉表现
    [Tooltip("UI图标（背包/商店用）")]
    public Sprite itemIcon;
    [Tooltip("世界中显示的精灵（场景物品用）")]
    public Sprite itemOnWorldSprite;

    // 交互规则
    [Tooltip("是否可拾取到背包")]
    public bool canPickedup = true; // 默认可拾取
    [Tooltip("是否可从背包丢弃到场景")]
    public bool canDropped = true;  // 默认可丢弃
    [Tooltip("是否可携带（比如大件家具不可携带）")]
    public bool canCarried = true;  // 默认可携带
   

    [Tooltip("物品使用范围半径（格子/米）")]
    public int itemUseRadius = 0;   // 默认无范围

    // 经济属性
    [Tooltip("商店购买价格")]
    public int itemPrice = 0;       // 默认无价格
    [Tooltip("出售折扣率（0~1）")]
    [Range(0, 1)]
    public float sellPercentage = 0.5f; // 默认5折出售

    [Tooltip("是否有特殊效果（比如肉可以恢复）")]
    public bool canEffect = false;  // 默认可携带
    [Tooltip("是否添加")]
    public bool isIncrease;
    [Tooltip("是否时限")]
    public bool istimeLimit;
    [Tooltip("物品属性")]
    public Attribute attribute;
    [Tooltip("属性值")]
    public int AddValue;
   
    [Tooltip("时间大小")]
    public float timeSize; 


}

/// <summary>
/// 表示怪物属性
/// </summary>
[System.Serializable]
public class EventDetails
{
    [Tooltip("怪物id")]
    public int eventID;
    [Tooltip("怪物名称")]
    public string eventName;
    [Tooltip("怪物预制体")]
    public GameObject eventObject;

    [Tooltip("当前生命")]
    public int currentHealth;
    [Tooltip("最大生命")]
    public int maxHealth;
    [Tooltip("攻击力")]
    public int attackPower;
    [Tooltip("速度")]
    public float speed;
    [Tooltip("攻击范围")]
    public float attackingRange;
    [Tooltip("追击范围")]
    public float rangeofPursuit ;
    [Tooltip("攻击冷却")]
    public float attackCooldown ;
    [Tooltip("延迟攻击")]
    public float delayAttackTime;

    [Tooltip("反击力")]
    public float knockbackForce;
    [Tooltip("击退时间")]
    public float stunTime;
   
    [Tooltip("掉落经验值")]
    public int experience;
    [Tooltip("战利品")]
    public int[] spoils;
    [Tooltip("战利品数量大")]
    public int[] Maxspoils;
    [Tooltip("战利品数量小")]
    public int[] Minspoils;
}


public class ItemEffect 
{
    [Tooltip("物品唯一标识ID，不可重复")]
    public int itemID;
   

}




/// <summary>
/// 表示玩家背包中的一个物品条目。
/// 设计为结构体以实现高效的数据复制（值类型语义）。
/// </summary>
/// <remarks>
/// 轻量级数据结构，用于存储物品ID和数量，常用于背包系统、物品管理等场景。
/// </remarks>
[System.Serializable]
public struct InventoryItem
{
    public int itemID;

    public int itemAmount;
  
}
[System.Serializable]
public class AnimatorType
{
    public PartType partType;

    public PartName partName;
    /// <summary>
    /// 覆盖原始动画控制器中的特定动画剪辑
    /// </summary>
    public AnimatorOverrideController overrideController;
}
[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;
    public SerializableVector3(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }


    public Vector3 ToVector3()
    {  
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int(Vector2Int pos)
    {
        return new Vector2Int((int)x, (int)y);
    }
    // 【扩展】直接转换为Vector3Int（更贴合你的最终需求）
    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
    }


}

[System.Serializable]
public class SceneItem
{
    public int itemID;
    public SerializableVector3 position;
}
/// <summary>
/// 蓝图存储信息
/// </summary>
[System.Serializable]
public class SceneFurniture
{
    public int itemID;
    public SerializableVector3 position;
    public int boxIndex;
}



[System.Serializable]
/// <summary>
/// 表示地图中单个瓦片的属性配置
/// </summary>
public class TileProperty
{
    /// <summary>
    /// 瓦片在网格中的坐标位置
    /// </summary>
    public Vector2Int tileCoordinate;

    /// <summary>
    /// 瓦片属性的类型
    /// </summary>
    public GridType gridType;

    /// <summary>
    /// 与属性类型关联的布尔值
    /// 具体含义取决于gridType：
    /// - 当gridType为Diggable时，表示是否可挖掘
    /// - 当gridType为DropItem时，表示是否可放置物品
    /// - 当gridType为PlaceFurniture时，表示是否可放置家具
    /// - 当gridType为NPCObstacle时，表示是否阻挡NPC移动
    /// </summary>
    public bool boolTypeValue;
}



/// <summary>
/// 存储游戏地图中单个瓦片的详细信息
/// </summary>
[System.Serializable]
public class TileDetails
{
    /// <summary>
    /// 瓦片在网格中的X坐标位置
    /// </summary>
    public int gridX;

    /// <summary>
    /// 瓦片在网格中的Y坐标位置
    /// </summary>
    public int gridY;

    /// <summary>
    /// 是否可以对该瓦片执行挖掘操作
    /// </summary>
    public bool canDig;

    /// <summary>
    /// 是否可以在该瓦片上放置物品
    /// </summary>
    public bool canDropItem;

    /// <summary>
    /// 是否可以在该瓦片上放置家具
    /// </summary>
    public bool canPlaceFurniture;

    /// <summary>
    /// 是否阻挡NPC移动（true表示NPC无法穿过）
    /// </summary>
    public bool isNPCObstacle;

    /// <summary>
    /// 自瓦片被挖掘以来的天数
    /// 值为-1表示该瓦片尚未被挖掘
    /// </summary>
    public int daysSinceDug=-1;

    /// <summary>
    /// 自瓦片上次浇水以来的天数
    /// 值为-1表示该瓦片尚未浇水
    /// </summary>
    public int daySinceWatered=-1;

    /// <summary>
    /// 当前种植在该瓦片上的种子物品ID
    /// 值为-1表示该瓦片未种植种子
    /// </summary>
    public int seedItemID=-1;

    /// <summary>
    /// 种子在该瓦片上的生长天数
    /// </summary>
    public int growthDays;

    /// <summary>
    /// 该瓦片自上次收获以来的天数
    /// 值为-1表示该瓦片尚未收获过作物
    /// </summary>
    public int daySinceLastHarvest=-1;
}
[System.Serializable]
public class NPCPosition
{
    public Transform npc;

    public string startScene;

    public Vector3 position;
}
[System.Serializable]
public class SceneRoute
{
    public string fromSceneName;
    public string gotoSceneName;
    public List<ScenePath> scenePathList;

    public static implicit operator SceneRoute(ScenePath v)
    {
        throw new NotImplementedException();
    }
}

[System.Serializable]
public class ScenePath
{
    public string sceneName;
    public Vector2Int fromGridCell;
    public Vector2Int gotoGridCell;
}
