using System.Collections;
using System.Collections.Generic;
using MFarm.Transition;
using UnityEngine;
namespace MFarm.Save
{
    /// <summary>
    /// 进度条，String是guid
    /// </summary>
    public class DataSlot
    {
        public Dictionary<string,GameSaveData> dataDict=new Dictionary<string,GameSaveData>();


        public string DataTime
        {
            get
            {
                var key=TimeManager.Instance.GUID;

                if (dataDict.ContainsKey(key))
                {
                    var timeData=dataDict[key];
                    return timeData.timeDict["gameYear"] + "年/" + (Season)timeData.timeDict["gameSeason"] + "/" + timeData.timeDict["gameMonth"] + "月/" + timeData.timeDict["gameDay"] + "日/";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string DataScene
        {
            get
            {
                var key=TransitionManager.Instance.GUID;
               
                if (dataDict.ContainsKey(key))
                {
                    
                    var transitionData=dataDict[key];
                    return transitionData.dataSceneName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

    }

   

}

