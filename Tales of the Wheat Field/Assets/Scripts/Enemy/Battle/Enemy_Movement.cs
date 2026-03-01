using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Enemy_Movement : NetworkBehaviour
{
    [Header("速度")]
    public float speed;
    [Header("攻击范围")]
    public float attackingRange;
    [Header("追击范围")]
    public float rangeofPursuit;
    [Header("攻击冷却")]
    public float attackCooldown;
    [Header("延迟攻击")]
    public float delayAttackTime;

    // 检查范围参数
    public Transform detectionPoint;
    public LayerMask playerLayer;

    // 网络变量 - 用于同步怪物状态
    private NetworkVariable<EnemyState> networkEnemyState = new NetworkVariable<EnemyState>(
        EnemyState.Idle,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>(
        Vector2.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<int> networkFacingDirection = new NetworkVariable<int>(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // 本地变量
    private float attackCooldownTimer = 0;
    private int scalingXValue = 1;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private float AttackingX;
    private float AttackingY;

    // 平滑移动相关
    private Vector2 targetPosition;
    private float smoothTime = 0.1f;
    private Vector2 velocity = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // 服务器端初始化
            networkEnemyState.Value = EnemyState.Idle;
            networkPosition.Value = transform.position;
            networkFacingDirection.Value = scalingXValue;
        }
        else
        {
            // 客户端禁用物理模拟
            rb.simulated = false;
            rb.isKinematic = true;
        }

        // 监听状态变化
        networkEnemyState.OnValueChanged += OnEnemyStateChanged;
        networkPosition.OnValueChanged += OnPositionChanged;
        networkFacingDirection.OnValueChanged += OnFacingDirectionChanged;

        // 初始化状态
        ChangeStart(networkEnemyState.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // 取消监听
        networkEnemyState.OnValueChanged -= OnEnemyStateChanged;
        networkPosition.OnValueChanged -= OnPositionChanged;
        networkFacingDirection.OnValueChanged -= OnFacingDirectionChanged;
    }

    private void Update()
    {
        if (IsServer)
        {
            // 服务器运行完整逻辑
            ServerUpdate();
        }
        else
        {
            // 客户端只做平滑插值
            ClientUpdate();
        }
    }

    #region 服务器逻辑

    private void ServerUpdate()
    {
        if (networkEnemyState.Value != EnemyState.Knocback)
        {
            CheckForPlayer();

            if (attackCooldownTimer > 0)
            {
                attackCooldownTimer -= Time.deltaTime;
            }

            if (networkEnemyState.Value == EnemyState.Run)
            {
                ChaseServerRpc();
            }
            else if (networkEnemyState.Value == EnemyState.Attacking)
            {
                rb.velocity = Vector3.zero;
            }
        }

        // 同步位置到网络变量
        networkPosition.Value = transform.position;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChaseServerRpc()
    {
        if (player == null) return;

        // 朝向逻辑
        if ((player.position.x > transform.position.x && scalingXValue == -1) ||
            (player.position.x < transform.position.x && scalingXValue == 1))
        {
            FlipServerRpc();
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;

        // 同步朝向
        networkFacingDirection.Value = scalingXValue;
    }
    /// <summary>
    /// 更改朝向
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void FlipServerRpc()
    {
        scalingXValue *= -1;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * scalingXValue,
            transform.localScale.y,
            transform.localScale.z
        );

        // 同步朝向
        networkFacingDirection.Value = scalingXValue;
    }
    /// <summary>
    /// 检测player
    /// </summary>
    private void CheckForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, rangeofPursuit, playerLayer);

        if (hits.Length > 0)
        {
            player = hits[0].transform;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackingRange && attackCooldownTimer <= 0)
            {
                // 进入攻击状态
                attackCooldownTimer = attackCooldown;
                StartCoroutine(DelayAttack(delayAttackTime));
            }
            else if (distanceToPlayer > attackingRange &&
                     anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                ChangeStart(EnemyState.Run);
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            ChangeStart(EnemyState.Idle);
        }
    }

    [ClientRpc]
    private void SendAttackAnimationClientRpc(Vector2 direction)
    {
       
        if (direction.x < 0)
        {
            anim.SetFloat("AttackingX", direction.x * -1);
            anim.SetFloat("AttackingY", direction.y);
        }
        else
        {
            anim.SetFloat("AttackingX", direction.x);
            anim.SetFloat("AttackingY", direction.y);
        }

        // 可选：根据方向设置朝向
        if (direction.x < 0 && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(
                -transform.localScale.x,
                transform.localScale.y,
                transform.localScale.z
            );
        }
        else if (direction.x > 0 && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(
                -transform.localScale.x,
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }


    private IEnumerator DelayAttack(float delayAttackTime)
    {
        yield return new WaitForSeconds(delayAttackTime);
        ChangeStart(EnemyState.Attacking);
    }

    public void ChangeStart(EnemyState newState)
    {
        if (!IsServer) return; // 只有服务器能改变状态

        // 退出动画
        ExitAnimation(networkEnemyState.Value);

        // 设置新状态
        networkEnemyState.Value = newState;

        // 进入动画
        EnterAnimation(newState);
    }

    private void ExitAnimation(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                anim.SetBool("isIdle", false);
                break;
            case EnemyState.Run:
                anim.SetBool("isRun", false);
                break;
            case EnemyState.Attacking:
                anim.SetBool("isAttacking", false);
                break;
        }
    }

    private void EnterAnimation(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                anim.SetBool("isIdle", true);
                break;
            case EnemyState.Run:
                anim.SetBool("isRun", true);
                break;
            case EnemyState.Attacking:
                anim.SetBool("isAttacking", true);
                AttackAnimation();
                break;
        }
    }

    private void AttackAnimation()
    {
        if (!IsServer) return;

        if (player == null) return;

        rb.velocity = Vector3.zero;
        Vector2 direction1 = (player.position - transform.position).normalized;

        // 朝向逻辑
        if (player.position.x < transform.position.x && scalingXValue == 1)
        {
            FlipServerRpc();
        }
        else if (player.position.x > transform.position.x && scalingXValue == -1)
        {
            FlipServerRpc();
        }

        // 动画参数（可以通过ClientRpc同步）
        SendAttackAnimationClientRpc(direction1);
    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector2.zero;
            ChangeStart(EnemyState.Idle);
        }
    }

    #endregion

    #region 客户端逻辑

    private void ClientUpdate()
    {
        // 平滑移动到服务器同步的位置
        transform.position = Vector2.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    private void OnEnemyStateChanged(EnemyState oldValue, EnemyState newValue)
    {
        if (!IsServer) // 客户端响应状态变化
        {
            // 退出旧动画
            ExitAnimation(oldValue);
            // 进入新动画
            EnterAnimation(newValue);
        }
    }

    private void OnPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        if (!IsServer) // 客户端更新目标位置
        {
            targetPosition = newValue;
        }
    }

    private void OnFacingDirectionChanged(int oldValue, int newValue)
    {
        if (!IsServer) // 客户端更新朝向
        {
            scalingXValue = newValue;
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * scalingXValue,
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    #endregion

    #region 击退逻辑

    [ServerRpc(RequireOwnership = false)]
    public void KnockbackServerRpc(Vector2 knockbackDirection, float knockbackForce, float stunTime)
    {
        if (!IsServer) return;

        StartCoroutine(KnockbackCoroutine(knockbackDirection, knockbackForce, stunTime));
    }

    private IEnumerator KnockbackCoroutine(Vector2 knockbackDirection, float knockbackForce, float stunTime)
    {
        EnemyState previousState = networkEnemyState.Value;
        networkEnemyState.Value = EnemyState.Knocback;

        rb.velocity = knockbackDirection * knockbackForce;

        yield return new WaitForSeconds(stunTime);

        rb.velocity = Vector2.zero;
        networkEnemyState.Value = previousState;
    }

    #endregion
}

