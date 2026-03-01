using Unity.Netcode;
using Cinemachine;
using UnityEngine;

// 将此脚本挂载到你的玩家预制体上
public class PlayerCameraSetup : NetworkBehaviour
{
    // 在Inspector中拖入你的虚拟相机
    public CinemachineVirtualCamera VirtualCamera;
    public void Start()
    {
        VirtualCamera= GameObject.FindWithTag("CM vcam").GetComponent<CinemachineVirtualCamera>();
    }
    // 当这个NetworkObject在本地生成时调用
    public override void OnNetworkSpawn()
    {
        // 只在本地玩家上执行相机设置
        if (IsLocalPlayer)
        {
                // 如果忘记赋值，可以自动查找场景中的虚拟相机
                VirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
                if (VirtualCamera != null)
                {
                    VirtualCamera.Follow = this.transform;
                    VirtualCamera.LookAt = this.transform;
                }
            
        }
    }
}