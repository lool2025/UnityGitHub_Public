
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class SwitchBounds : MonoBehaviour
{

   
 

    // （可选）网络场景中监听玩家生成事件，确保玩家生成后再设置相机
    private void OnEnable()
    {
       
        EventHandler.AfterSceneLoadEvent += SwitchConfinerShape;
    }

    private void OnDisable()
    {
       
        EventHandler.AfterSceneLoadEvent -= SwitchConfinerShape;
    }

   



 
    private void SwitchConfinerShape()
    {
        //搜索场景中标签是BoundsConfiner的并获取PolygonCollider2D
        
       
        GameObject[] confinerObjects = GameObject.FindGameObjectsWithTag("BoundsConfiner");
        
        GameObject confiner1 = confinerObjects[0];
        PolygonCollider2D confinerShape = confiner1.GetComponent<PolygonCollider2D>();

        CinemachineConfiner confiner=GetComponent<CinemachineConfiner>();
        //赋值
        confiner.m_BoundingShape2D = confinerShape;
        //每次切换场景要调用这个函数，清楚缓存
        confiner.InvalidatePathCache();
    }



    
}
