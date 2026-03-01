using System.Collections;
using System.Collections.Generic;
using MFarm.Save;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour
{
    public Text dataTime, dataScene;
    private Button currentButton;

    private DataSlot currentData;

    private int Index=>transform.GetSiblingIndex();

    private void Awake()
    {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(LoadGameData);
    }

    private void OnEnable()
    {
        SetupSlotUI();
    }


    private void SetupSlotUI()
    {
        currentData=SaveLoadManager.Instance.dataSlots[Index];

        if (currentData != null)
        {
            dataTime.text = currentData.DataTime;
            dataScene.text = currentData.DataScene;
        }
        else
        {
            dataTime.text = "时间还未开始流动";
            dataScene.text = "空间还未开始运动";
        }
    }


    private void LoadGameData()
    {
        if(currentData != null)
        {
            SaveLoadManager.Instance.Load(Index);
        }
        else
        {
         
            EventHandler.CallStartNewGameEvent(Index);
        }
    }

}
