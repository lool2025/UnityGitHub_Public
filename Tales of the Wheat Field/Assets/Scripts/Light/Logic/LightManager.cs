using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    private LightControl[] sceneLights;

    private LightShift currentlightShift;

    private Season currentSeason;

    private float timeDifference=Settings.lightChangeDuration;
    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
        EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;
        //新游戏
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
        EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;
        //新游戏
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
       currentlightShift=LightShift.Morning;
    }

    private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
    {
        currentSeason = season;
        this.timeDifference = timeDifference;

        if (currentlightShift != lightShift)
        {
            currentlightShift = lightShift;

            // 添加空值检查
            if (sceneLights == null)
            {
                Debug.LogWarning("sceneLights is null in OnLightShiftChangeEvent. Attempting to find LightControl objects...");
                sceneLights = FindObjectsOfType<LightControl>();

                if (sceneLights == null || sceneLights.Length == 0)
                {
                    Debug.LogError("No LightControl objects found after scene load!");
                    return;
                }
            }

            foreach (LightControl lightControl in sceneLights)
            {
                // 添加空值检查 - 这应该是你第43行的代码
                if (lightControl != null)
                {
                    // 改变灯光
                    lightControl.ChangeLightShift(currentSeason, currentlightShift, timeDifference);
                }
                else
                {
                    Debug.LogWarning("Null LightControl found in sceneLights array");
                }
            }
        }
    }

    private void OnAfterSceneLoadEvent()
    {
        sceneLights =FindObjectsOfType<LightControl>();

        foreach(LightControl lightControl in sceneLights)
        {
          
            //改变灯光
            lightControl.ChangeLightShift(currentSeason, currentlightShift, timeDifference);
        }

    }
}
