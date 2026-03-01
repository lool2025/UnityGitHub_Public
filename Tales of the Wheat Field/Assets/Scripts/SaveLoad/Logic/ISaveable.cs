using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.Save 
{
    public interface ISaveable
    {
        string GUID { get; }
        void RegisterSaveable()
        {
            SaveLoadManager.Instance.RegisterSaveable(this);
        }
        GameSaveData GenerateSaveData();

        void RestoreData(GameSaveData saveData);
    }
}


