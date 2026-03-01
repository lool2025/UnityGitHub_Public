using Unity.Netcode;
using UnityEngine;

public class NetworkManagerSetup : MonoBehaviour
{
    [Header("怪物预制体")]
    [SerializeField] private GameObject[] enemyPrefabs;

    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            RegisterEnemyPrefabs();
        }
    }

    private void RegisterEnemyPrefabs()
    {
        Debug.Log("开始注册怪物预制体到 NetworkPrefabHandler...");

        var networkManager = NetworkManager.Singleton;
        var prefabHandler = networkManager.PrefabHandler;

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab == null) continue;

            if (prefab.GetComponent<NetworkObject>() == null)
            {
                Debug.LogError($"预制体 {prefab.name} 没有 NetworkObject 组件！");
                continue;
            }

            // 使用 PrefabHandler 添加网络预制体
            // 注意：这会自动添加到 NetworkPrefabs 列表
            prefabHandler.AddNetworkPrefab(prefab);
            Debug.Log($"成功注册预制体: {prefab.name}");
        }

        Debug.Log("怪物预制体注册完成！");
    }
}