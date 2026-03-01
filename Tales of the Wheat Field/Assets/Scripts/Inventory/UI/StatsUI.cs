using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class StatsUI : MonoBehaviour
{
    public GameObject[] StatsSlots;
    public GameObject ExperienceSlider;

    private bool isopenUI;
    private void Awake()
    {
        updataAttribute();
        updataLevel();
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.L))
            StatsManager.Instance.experienceAdd(100);
    }

    public void OnEnable()
    {
        EventHandler.UpgradeEvent += openUpgradeButton;
        EventHandler.UpdataStatsUIEvent += OnUpdataStatsUIEvent;
    }

    public void OnDisable()
    {
        EventHandler.UpgradeEvent += openUpgradeButton;
        EventHandler.UpdataStatsUIEvent += OnUpdataStatsUIEvent;
    }

    private void OnUpdataStatsUIEvent()
    {
        updataAttribute();
        updataLevel();

    }

    /// <summary>
    /// 属性更新
    /// </summary>
    /// <param name="statsManager"></param>
    public void updataAttribute()
    {
        StatsSlots[0].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = AttributeState.生命+":"+ StatsManager.Instance.maxHealth;
        StatsSlots[1].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = AttributeState.攻击 + ":" + StatsManager.Instance.damage;
        StatsSlots[2].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = AttributeState.防御 + ":" + StatsManager.Instance.defenseValue;
        StatsSlots[3].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = AttributeState.速度 + ":" + StatsManager.Instance.speed;
    }
    /// <summary>
    /// 经验值更新
    /// </summary>
    public void updataLevel()
    {

        ExperienceSlider.GetComponent<Slider>().value = StatsManager.Instance.Proportionexperience();
        ExperienceSlider.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = AttributeState.等级+ ":" + StatsManager.Instance.level+" ("+ StatsManager.Instance.Proportionexperience()*100 + "%)"+"     属性点:"+ StatsManager.Instance.upgrade;
    }
    /// <summary>
    /// 升级按钮显示
    /// </summary>
    public void openUpgradeButton()
    {
        if (StatsManager.Instance.upgrade > 0)
        {
            StatsSlots[0].transform.GetChild(1).gameObject.SetActive(true);
            StatsSlots[1].transform.GetChild(1).gameObject.SetActive(true);
            StatsSlots[2].transform.GetChild(1).gameObject.SetActive(true);
            StatsSlots[3].transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            StatsSlots[0].transform.GetChild(1).gameObject.SetActive(false);
            StatsSlots[1].transform.GetChild(1).gameObject.SetActive(false);
            StatsSlots[2].transform.GetChild(1).gameObject.SetActive(false);
            StatsSlots[3].transform.GetChild(1).gameObject.SetActive(false);
        }
        updataLevel();

    }

    public void AddMaxHealth()
    {
        StatsManager.Instance.AddMaxHealth(Settings.maxHealthGrowth);
        StatsManager.Instance.upgrade--;
        openUpgradeButton();
        updataAttribute();


    }
    public void Adddamage()
    {
        StatsManager.Instance.Adddamage(Settings.damageGrowth);
        StatsManager.Instance.upgrade--;
        openUpgradeButton();
        updataAttribute();
    }
    public void AddDefenseValue()
    {
        StatsManager.Instance.AddDefenseValue(Settings.defenseValueGrowth);
        StatsManager.Instance.upgrade--;
        openUpgradeButton();
        updataAttribute();
    }
    public void AdjustSpeed()
    {
        StatsManager.Instance.AdjustSpeed(Settings.speedGrowth);
        StatsManager.Instance.upgrade--;
        openUpgradeButton();
        updataAttribute();
    }





    public void openUI()
    {
        if (isopenUI)
        {
            gameObject.SetActive(false);
            EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
            isopenUI = false;
        }
        else
        {
            gameObject.SetActive(true);
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);
            isopenUI = true;
        }
    }

}
