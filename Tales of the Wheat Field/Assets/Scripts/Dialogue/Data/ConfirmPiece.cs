using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ConfirmPiece 
{
    [Header("对话详情")]
    public Sprite faceImage;
    public string name;
    
    [TextArea]
    public string dialogueText;
          
    public bool isTransmitting;
    [Header("目标场景")] public string sceneToGo;
    [Header("目标地点")] public Vector3 positionToGo;

}
