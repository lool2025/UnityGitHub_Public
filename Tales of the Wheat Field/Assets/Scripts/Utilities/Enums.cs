using UnityEditor;

public enum ItemType
{
    Seed,Commodity,Furniture,
    HoeTool,ChopTool,BreakTool,ReapTool,WaterTool,CollectTool, SwordTool,
    ReapablsScenery
}

public enum SlotType
{ 
    Bag,Box,Shop
}


public enum InventoryLocation
{
    Player,Box
}
/// <summary>
/// 触发器类型
/// </summary>
public enum TriggerType2D { Door, Pickup, NPC, Chest }

public enum PartType
{
    None,    // 无类型（默认值）
    Carry,   // 可携带物品（如背包、箱子）
    Hoe,     // 锄头类工具（用于耕种）
    Break,    // 破坏工具（如锤子、镐子）
    Water,//水
    Collect,//采集（）
    Chop,//斧头
    Reap,
    Sword//剑
}

public enum PartName
{
    Body,   // 身体部位
    Hair,   // 头发部位
    Arm,    // 手臂部位
    Tool    // 工具部位
}

public enum Season
{
    春天,夏天,秋天,冬天
}

public enum GridType
{
    Diggable,DropItem,PlaceFurniture,NPCObstacle
}

public enum SoundName
{
    none, FootStepSoft, FootStepHard,
    Axe, Pickaxe, Hoe, Reap, Water, Basket, Chop,
    Pickup, Plant, TreeFalling, Rustle,
    AmbientCountryside1, AmbientCountryside2, MusicCalm1, MusicCalm2, MusicCalm3, MusicCalm4, MusicCalm5, MusicCalm6, AmbientIndoor1
}

public enum ParticaleEffectType
{
    None,LeavesFalling01, LeavesFalling02,Rock,ReapableScenery
}
/// <summary>
/// 游戏的状态，开启和关闭
/// </summary>
public enum GameState
{
    GamePlay,Pause
}

public enum LightShift
{
    Morning, Night
}

public enum EnemyState 
{
    Idle,Run,Attacking,Knocback
}

public enum AttributeState
{
    生命,攻击,防御,速度,等级
}
public enum Attribute
{
    currentHealth, maxHealth, damage, defenseValue, speed, experience, upgrade,money
}









