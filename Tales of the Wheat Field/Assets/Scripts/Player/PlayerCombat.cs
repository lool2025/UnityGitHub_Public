
using UnityEngine;


public class PlayerCombat : MonoBehaviour
{

  
  
    [Header("¹¥»÷·¶Î§¼ì²â")]
    public Transform attackPoint;
    public float weaponRange = 1;
    public LayerMask enemyLayer;

    public void OnEnable()
    {
        EventHandler.PlayerAttack += Attack;
    }
    public void OnDisable()
    {
        EventHandler.PlayerAttack -= Attack;
    }



    public void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, enemyLayer);
        if (hits.Length > 0)
        {
            hits[0].GetComponent<Enemy_Health>().ChangeHealth(-StatsManager.Instance.damage);
            hits[0].GetComponent<Enemy_Knockback>().Knockback(transform, StatsManager.Instance.knockbackForce, StatsManager.Instance.knocjbackTime,StatsManager.Instance.stunTime);
            
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }

}
