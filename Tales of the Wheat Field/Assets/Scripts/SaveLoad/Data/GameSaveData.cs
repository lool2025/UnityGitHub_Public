using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Save
{
    [System.Serializable]
    public class GameSaveData
    {

        public string dataSceneName;
        /// <summary>
        /// 存储人物坐标，string人物名字
        /// </summary>
        public Dictionary<string, SerializableVector3> characterPosDict;

        public Dictionary<string, List<SceneItem>> sceneItemDict ;
        /// <summary>
        /// 记录场景家具
        /// </summary>
        public Dictionary<string, List<SceneFurniture>> sceneFurnitureDict ;

        /// <summary>
        /// 场景名字+坐标和对应的瓦片信息
        /// </summary>
        public Dictionary<string, TileDetails> tileDetailsDict;
        /// <summary>
        /// 场景是否第一次加载
        /// </summary>
        public Dictionary<string, bool> firstLoadDict ;
        /// <summary>
        /// 背包数据
        /// </summary>
        public Dictionary<string, List<InventoryItem>> inventoryDict;

        public Dictionary<string, int> timeDict;

        public int playerMoney;

        //NPC
        public string targetScene;
        public bool interactable;
        public int animationInstanceID;
    }
}
