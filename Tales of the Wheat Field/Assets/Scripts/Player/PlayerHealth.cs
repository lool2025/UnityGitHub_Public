using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    
    
    private void Update()
    {
      
        // 死亡逻辑
        if (StatsManager.Instance.currentHealth <= 0)
        {
            Debug.Log("死亡");
            gameObject.SetActive(false);
        }
    }

    // 修改血量的方法（对外提供）
    public void ChangeHealthValue(int value)
    {
        Debug.Log(" Mathf.Clamp(StatsManager.Instance.currentHealth + value, 0, StatsManager.Instance.maxHealth)" + Mathf.Clamp(StatsManager.Instance.currentHealth + value, 0, StatsManager.Instance.maxHealth));
        // 限制血量范围，避免负数或超过最大值
        StatsManager.Instance.currentHealth = Mathf.Clamp(StatsManager.Instance.currentHealth - value, 0, StatsManager.Instance.maxHealth);
        
    }
}