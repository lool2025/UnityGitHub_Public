using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "LightPattenList_SO",menuName ="Light/Light Patten")]
public class LightPattenList_SO : ScriptableObject
{
    public List<LightDetails> lightPatternList;
    /// <summary>
    /// 根据季节和周期返回灯光情况
    /// </summary>
    /// <param name="season">季节</param>
    /// <param name="lightShift">周期</param>
    /// <returns></returns>
    public LightDetails GetLightDetails(Season season, LightShift lightShift)
    {
        return lightPatternList.Find(l => l.season == season&& l.lightShift == lightShift);
    }
}
[System.Serializable]
public class LightDetails
{
    public Season season;

    public LightShift lightShift;

    public Color lightColor;

    public float lightAmount;
}
