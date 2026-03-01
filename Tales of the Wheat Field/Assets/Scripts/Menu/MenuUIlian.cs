using System.Collections;
using System.Collections.Generic;
using MFarm.Save;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIlian : MonoBehaviour
{
    private DataSlot currentData;
    private Button currentButton;
    private int Index => transform.GetSiblingIndex();

    private void Awake()
    {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(LoadGameData);
    }

    private void OnEnable()
    {
        SetupSlotUI();
    }

    private void SetupSlotUI()
    {
        // 安全判断，防止越界
        if (SaveLoadManager.Instance != null &&
            SaveLoadManager.Instance.dataSlots != null &&
            Index >= 0 && Index < SaveLoadManager.Instance.dataSlots.Count)
        {
            currentData = SaveLoadManager.Instance.dataSlots[Index];
        }
        else
        {
            currentData = null;
        }
    }

    private void LoadGameData()
    {
        // 1. 先安全加载存档
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("SaveLoadManager 不存在！");
            return;
        }

        if (currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else
        {
            EventHandler.CallStartNewGameEvent(Index);
        }

        // 2. 安全启动客户端（关键：防止空引用、重复启动）
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager 不存在！请确保场景里有 NetworkManager");
            return;
        }

        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("已经是客户端/服务器，不再重复启动");
            return;
        }

        bool result = NetworkManager.Singleton.StartClient();
        if (result)
        {
            Debug.Log("客户端启动成功，正在尝试连接服务器...");
        }
        else
        {
            Debug.LogError("客户端启动失败！");
        }
    }
}