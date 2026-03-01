using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Knockback : MonoBehaviour
{
    private Rigidbody2D rb;
    private Enemy_Movement enemy_Movement;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy_Movement = GetComponent<Enemy_Movement>();
    }

    public void Knockback(Transform playerTransform,float knockbackForce,float knocjbackTime,float stunTime)
    {
        enemy_Movement.ChangeStart(EnemyState.Knocback);
        Vector2 direction=(transform.position=playerTransform.position).normalized;
        rb.velocity = direction*knockbackForce;

        StartCoroutine(StunTimer(knocjbackTime, stunTime));
    }

    private IEnumerator StunTimer(float knocjbackTime, float stunTime)
    {
        yield return new WaitForSeconds(knocjbackTime);
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(stunTime);
        enemy_Movement.ChangeStart(EnemyState.Idle);
    }

}
