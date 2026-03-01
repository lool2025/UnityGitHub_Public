using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetBatton : MonoBehaviour
{
  

    // 启动客户端按钮
    public void OnStartClientBtnClick()
    {
        EventHandler.CallSwitchGameModeEvent(GameMode.MultiPlayerClient);
    }

    // 启动主机（服务器+客户端）按钮
    public void OnStartHostBtnClick()
    {
        EventHandler.CallSwitchGameModeEvent(GameMode.MultiPlayerHost);
    }

    // 关闭网络按钮
    public void OnShutdownNetworkBtnClick()
    {
        EventHandler.CallSwitchGameModeEvent(GameMode.SinglePlayer);
    }
}