using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Vector3 playerStartPosition = new Vector3(0, 0, 0);

    // 标记：是否已为主机生成过玩家（避免重复生成）
    private bool _hasSpawnedHostPlayer = false;

    private void Start()
    {
        // 关键修复1：主机启动后主动生成本地玩家（不依赖连接回调）
        if (NetworkManager.Singleton != null)
        {
            // 监听NetworkManager启动事件
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // 服务器（主机）启动时触发 → 主动生成本机玩家
    private void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsHost && !_hasSpawnedHostPlayer)
        {
            SpawnLocalHostPlayer();
            _hasSpawnedHostPlayer = true;
        }
    }

    // 客户端连接时触发 → 为新客户端生成玩家
    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("非服务器端尝试生成玩家，已阻止");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("玩家预制体未赋值！请在Inspector面板中指定", this);
            return;
        }

        // 关键修复2：区分“主机自身”和“其他客户端”
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // 主机自身：仅当未生成过玩家时才生成
            if (!_hasSpawnedHostPlayer)
            {
                SpawnLocalHostPlayer();
                _hasSpawnedHostPlayer = true;
            }
            return;
        }

        // 其他客户端：正常生成玩家
        SpawnPlayerForClient(clientId);
    }

    // 为普通客户端生成玩家
    private void SpawnPlayerForClient(ulong clientId)
    {
        GameObject player = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        NetworkObject networkObj = player.GetComponent<NetworkObject>();

        if (networkObj == null)
        {
            Debug.LogError("玩家预制体缺少 NetworkObject 组件！", player);
            Destroy(player);
            return;
        }

        // 关键：SpawnAsPlayerObject 第二个参数设为 true → 强制设置所有权
        networkObj.SpawnAsPlayerObject(clientId, true);
        Debug.Log($"已为客户端 {clientId} 生成玩家对象");
    }

    // 为主机生成本地玩家
    private void SpawnLocalHostPlayer()
    {
        GameObject hostPlayer = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        NetworkObject hostNetworkObj = hostPlayer.GetComponent<NetworkObject>();

        if (hostNetworkObj == null)
        {
            Debug.LogError("玩家预制体缺少 NetworkObject 组件！", hostPlayer);
            Destroy(hostPlayer);
            return;
        }

        hostNetworkObj.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
        _hasSpawnedHostPlayer = true;
        Debug.Log("已为主机生成本地玩家对象");
    }

    // 可选：手动触发生成（比如按钮点击后调用）
    public void ForceSpawnHostPlayer()
    {
        if (NetworkManager.Singleton.IsHost && !_hasSpawnedHostPlayer)
        {
            SpawnLocalHostPlayer();
        }
    }
}