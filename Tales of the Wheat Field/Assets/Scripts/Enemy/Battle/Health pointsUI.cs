using TMPro;
using UnityEngine;

public class HealthpointsUI : MonoBehaviour
{
   
    public TMP_Text HealthText;

    private void Start()
    {
        // 空值检查：防止TMP组件未赋值
        if (HealthText == null)
        {
            return;
        }

        // 初始化UI文本（从单例读取数据）
        UpdateHealthUI();
    }

    private void Update()
    {
        // 实时更新UI（每一帧从单例读取最新血量）
        UpdateHealthUI();
    }

    // 封装UI更新逻辑，集中做空值检查
    private void UpdateHealthUI()
    {
        // 检查单例是否存在
        if (StatsManager.Instance == null)
        {
            return;
        }

        // 从单例读取实时血量并更新TMP
        HealthText.text = $"HP: {StatsManager.Instance.currentHealth}/{StatsManager.Instance.maxHealth}";
    }
}