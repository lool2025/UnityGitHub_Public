using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "EnemyDataList_SO", menuName = "Enemy/EnemyDataList_SO")]
public class EnemyDataList_SO : ScriptableObject
{
    public List<EventDetails> eventDetailsList;
}
