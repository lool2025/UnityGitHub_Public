using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : NetworkBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public EnemyDataList_SO enemyDataList_SO;
    public Transform Fatherobject;

    [Header("作弊设置")]
    [SerializeField] private bool enableCheats = true;
    [SerializeField] private KeyCode spawnEnemyKey = KeyCode.H;
    [SerializeField] private int cheatEnemyId = 1001;
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private bool spawnMultiple = false;
    [SerializeField] private int multipleCount = 5;

    private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer && !IsHost)
        {
            enabled = false;
        }

        FindPlayer();
    }

    private void Update()
    {
        if (enableCheats && (IsServer || IsHost))
        {
            CheckCheatInput();
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void CheckCheatInput()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        if (Input.GetKeyDown(spawnEnemyKey))
        {
            if (spawnMultiple)
            {
                SpawnMultipleEnemiesAroundPlayerServerRpc();
            }
            else
            {
                SpawnEnemyInFrontOfPlayerServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnEnemyInFrontOfPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        if (playerTransform == null) return;

        Vector3 spawnPos = playerTransform.position + playerTransform.forward * spawnDistance;
        spawnPos.y = 0;

        GameObject enemy = CreateEnemyNetwork(cheatEnemyId, spawnPos);

        if (enemy != null)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            Debug.Log($"玩家 {clientId} 在位置 {spawnPos} 生成了怪物");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnMultipleEnemiesAroundPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        if (playerTransform == null) return;

        for (int i = 0; i < multipleCount; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(spawnDistance * 0.5f, spawnDistance * 1.5f);

            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
            Vector3 spawnPos = playerTransform.position + offset;

            CreateEnemyNetwork(cheatEnemyId, spawnPos);
        }

        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"玩家 {clientId} 生成了 {multipleCount} 个怪物");
    }

    public GameObject CreateEnemyNetwork(int id, Vector3 spawnPos)
    {
        if (!IsServer && !IsHost)
        {
            Debug.LogError("只有服务器可以生成网络怪物！");
            return null;
        }

        EventDetails enemyConfig = GetItemDetails(id);
        if (enemyConfig == null || enemyConfig.eventObject == null)
        {
            Debug.LogError($"找不到ID为{id}的怪物配置或预制体为空！");
            return null;
        }

        NetworkObject prefabNetObj = enemyConfig.eventObject.GetComponent<NetworkObject>();
        if (prefabNetObj == null)
        {
            Debug.LogError($"怪物预制体 {enemyConfig.eventName} 缺少 NetworkObject 组件！");
            return null;
        }

        Transform parentTransform = GetParentTransform();
        GameObject enemyObj = Instantiate(enemyConfig.eventObject, spawnPos, Quaternion.identity, parentTransform);

        // 设置InitEnemy的ID
        InitEnemy initEnemy = enemyObj.GetComponent<InitEnemy>();
        if (initEnemy != null)
        {
            initEnemy.enemyId = id;
        }

        NetworkObject netObj = enemyObj.GetComponent<NetworkObject>();
        netObj.Spawn();

        return enemyObj;
    }

    public GameObject CreateEnemy(int id, Vector3 spawnPos)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            return CreateEnemyNetwork(id, spawnPos);
        }

        EventDetails enemyConfig = GetItemDetails(id);
        if (enemyConfig == null || enemyConfig.eventObject == null) return null;

        Transform parentTransform = GetParentTransform();
        GameObject enemyObj = Instantiate(enemyConfig.eventObject, spawnPos, Quaternion.identity, parentTransform);

        return enemyObj;
    }

    private Transform GetParentTransform()
    {
        if (Fatherobject != null)
        {
            Transform sceneParent = Fatherobject.Find(SceneManager.GetActiveScene().name);
            if (sceneParent != null)
            {
                return sceneParent;
            }
        }
        return null;
    }

    public EventDetails GetItemDetails(int ID)
    {
        if (enemyDataList_SO == null || enemyDataList_SO.eventDetailsList == null)
        {
            Debug.LogError("EnemyDataList_SO 或其 eventDetailsList 未赋值！");
            return null;
        }

        return enemyDataList_SO.eventDetailsList.Find(i => i.eventID == ID);
    }
}