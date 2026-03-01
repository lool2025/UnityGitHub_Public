using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// 精简版玩家切换管理器（支持Host和Client）
/// </summary>
public class PlayerSwitchManager : MonoBehaviour
{
    [Header("玩家配置")]
    public GameObject scenePlayer;           // 场景中的单机玩家
    public GameObject networkPlayerPrefab;   // 联机玩家预制体

    [Header("NetworkManager")]
    public NetworkManager networkManager;

    [Header("网络配置")]
    public ushort networkPort = 2356;
    public string serverIP = "127.0.0.1";    // 服务器IP地址

    [Header("生成配置")]
    [Tooltip("范围")]
    public float spawnRadius = 2f;           // 生成半径

    private GameObject currentNetworkPlayer;
    private Vector3 lastPlayerPosition;      // 最后记录的位置
    private bool isHost = false;              // 标记是否是主机

    private void Awake()
    {
        if (networkManager == null)
            networkManager = FindObjectOfType<NetworkManager>();
    }

    private void OnEnable()
    {
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback += OnClientConnected;
            networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
        EventHandler.SwitchGameModeEvent += OnSwitchGameModeEvent;
    }

    private void OnDisable()
    {
        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        EventHandler.SwitchGameModeEvent -= OnSwitchGameModeEvent;
    }

    /// <summary>
    /// 事件回调 - 处理模式切换
    /// </summary>
    private void OnSwitchGameModeEvent(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.SinglePlayer:
                ExitToSinglePlayer();
                break;
            case GameMode.MultiPlayerHost:
                StartHost();
                break;
            case GameMode.MultiPlayerClient:
                StartClient();
                break;
        }
    }

    /// <summary>
    /// 创建主机（自己当房主）
    /// </summary>
    public void StartHost()
    {
        if (scenePlayer == null || networkPlayerPrefab == null)
        {
            Debug.LogError("玩家配置不完整！");
            return;
        }

        isHost = true;
        // 记录位置并隐藏单机玩家
        lastPlayerPosition = scenePlayer.transform.position;
        scenePlayer.SetActive(false);

        // 启动Host
        networkManager.StartHost();
        Debug.Log($"Host启动，端口：{networkPort}");
    }

    /// <summary>
    /// 作为客户端加入
    /// </summary>
    public void StartClient()
    {
        StartClient(serverIP);
    }

    /// <summary>
    /// 作为客户端加入（指定IP）
    /// </summary>
    public void StartClient(string ipAddress)
    {
        if (scenePlayer == null || networkPlayerPrefab == null)
        {
            Debug.LogError("玩家配置不完整！");
            return;
        }

        isHost = false;
        // 记录位置并隐藏单机玩家（注意：这里记录的是单机玩家的位置，但客户端不会用这个位置）
        lastPlayerPosition = scenePlayer.transform.position;
        scenePlayer.SetActive(false);

        // 配置客户端连接
        var transport = networkManager.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(ipAddress, networkPort);
        }

        // 启动客户端
        networkManager.StartClient();
        Debug.Log($"客户端启动，连接至：{ipAddress}:{networkPort}");
    }

    /// <summary>
    /// 退出联机，返回单机
    /// </summary>
    public void ExitToSinglePlayer()
    {
        // 关闭网络
        if (networkManager.IsListening)
            networkManager.Shutdown();

        // 销毁联机玩家
        if (currentNetworkPlayer != null)
        {
            if (currentNetworkPlayer.GetComponent<NetworkObject>() != null)
            {
                var netObj = currentNetworkPlayer.GetComponent<NetworkObject>();
                if (netObj.IsSpawned)
                    netObj.Despawn();
            }
            Destroy(currentNetworkPlayer);
            currentNetworkPlayer = null;
        }

        // 恢复单机玩家 - 使用最后记录的联机玩家位置
        if (scenePlayer != null)
        {
            scenePlayer.transform.position = lastPlayerPosition;
            scenePlayer.SetActive(true);
            Debug.Log($"单机玩家恢复到位置：{lastPlayerPosition}");
        }

        Debug.Log("已退出联机，恢复单机模式");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != networkManager.LocalClientId)
            return;

        // 查找生成的联机玩家
        var networkObjects = FindObjectsOfType<NetworkObject>();
        foreach (var netObj in networkObjects)
        {
            if (netObj.IsLocalPlayer)
            {
                currentNetworkPlayer = netObj.gameObject;

                // 根据身份设置位置
                if (isHost)
                {
                    // 主机：使用之前记录的单机玩家位置
                    currentNetworkPlayer.transform.position = lastPlayerPosition;
                    Debug.Log($"主机玩家生成在：{lastPlayerPosition}");
                }
                else
                {
                    // 客户端：在主机玩家附近随机位置生成
                    SetClientSpawnPosition();
                }

                // 更新最后位置记录（用于退出联机时恢复）
                lastPlayerPosition = currentNetworkPlayer.transform.position;

                Debug.Log($"联机玩家已生成，ClientId：{clientId}，位置：{currentNetworkPlayer.transform.position}");
                break;
            }
        }
    }

    /// <summary>
    /// 设置客户端生成位置（在主机附近）
    /// </summary>
    private void SetClientSpawnPosition()
    {
        // 查找主机玩家的位置
        Vector3 hostPosition = FindHostPlayerPosition();

        // 在主机附近随机生成
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = hostPosition + new Vector3(randomCircle.x, randomCircle.y, 0);

        currentNetworkPlayer.transform.position = spawnPosition;
        Debug.Log($"客户端在主机附近生成，主机位置：{hostPosition}，客户端位置：{spawnPosition}");
    }

    /// <summary>
    /// 查找主机玩家的位置
    /// </summary>
    private Vector3 FindHostPlayerPosition()
    {
        // 如果是主机自己，返回自己的位置
        if (isHost && currentNetworkPlayer != null)
            return currentNetworkPlayer.transform.position;

        // 查找主机玩家的NetworkObject
        var networkObjects = FindObjectsOfType<NetworkObject>();
        foreach (var netObj in networkObjects)
        {
            // 主机玩家通常是第一个连接的，或者是拥有Server权限的
            if (netObj.OwnerClientId == 0 && netObj.IsPlayerObject) // OwnerClientId 0 通常是主机
            {
                return netObj.transform.position;
            }
        }

        return Vector3.zero;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId != networkManager.LocalClientId)
            return;

        Debug.Log($"与服务器断开连接，ClientId：{clientId}");
        ExitToSinglePlayer();
    }
}

/// <summary>
/// 游戏模式枚举
/// </summary>
public enum GameMode
{
    SinglePlayer,
    MultiPlayer,
    MultiPlayerHost,
    MultiPlayerClient
}