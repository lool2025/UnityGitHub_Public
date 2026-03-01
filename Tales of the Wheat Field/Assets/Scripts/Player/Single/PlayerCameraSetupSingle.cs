using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerCameraSetupSingle : MonoBehaviour
{
    public CinemachineVirtualCamera VirtualCamera;
    public void Start()
    {
        VirtualCamera = GameObject.FindWithTag("CM vcam").GetComponent<CinemachineVirtualCamera>();
    }
    public void OnEnable()
    {
        OnCinemachineVirtualCamera();
    }
    // 当这个NetworkObject在本地生成时调用
    public  void OnCinemachineVirtualCamera()
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
