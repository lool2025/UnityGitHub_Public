using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
namespace MFarm.Save
{
    public class SaveLoadManager : Singleton<SaveLoadManager>
    {

        private List<ISaveable> saveableList = new List<ISaveable>();

        public List<DataSlot> dataSlots = new List<DataSlot>(new DataSlot[3]);

        private string jsonFolder;

        private int currentDataIndex;

        public void Awake()
        {
            base.Awake();
            jsonFolder = Application.persistentDataPath + "/SAVE DATA/";
            ReadSaveData();
        }

        public void Update()
        {
            //if (Input.GetKeyDown(KeyCode.M))
            //{
            //    Save(currentDataIndex);
            //}
            //if (Input.GetKeyDown(KeyCode.N))
            //{
            //    Load(currentDataIndex);
            //}
        }

        public void OnEnable()
        {
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        public void OnDisable()
        {
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            Save(currentDataIndex);
        }

        private void OnStartNewGameEvent(int index)
        {
            currentDataIndex = index;

        }

        /// <summary>
        /// 可存档对象加入管理器的列表
        /// </summary>
        /// <param name="saveable"></param>
        public void RegisterSaveable(ISaveable saveable)
        {
            if(!saveableList.Contains(saveable))
            {
                saveableList.Add(saveable);
            }
        }


        private void ReadSaveData()
        {
            if (Directory.Exists(jsonFolder))
            {
                for(int i = 0; i < dataSlots.Count; i++)
                {
                    var resultPath = jsonFolder + "data" + i + ".json";
                    if (File.Exists(resultPath))
                    {
                        var stringData=File.ReadAllText(resultPath);
                        var jsonData=JsonConvert.DeserializeObject<DataSlot>(stringData);
                        dataSlots[i]=jsonData;
                    }
                }
            }
        }



        public void Save(int index)
        {
            DataSlot data=new DataSlot();
            foreach(var saveable in saveableList)
            {
                data.dataDict.Add(saveable.GUID, saveable.GenerateSaveData());
            }
            dataSlots[index]=data;

            var resultPath = jsonFolder + "data" + index + ".json";
            var jsonData = JsonConvert.SerializeObject(dataSlots[index],Formatting.Indented);
            Debug.Log("data" + index + ".json");
            if(!File.Exists(resultPath))
            {
                Directory.CreateDirectory(jsonFolder);
            }
            File.WriteAllText(resultPath, jsonData);

        }

        public void Load(int index)
        {
            currentDataIndex=index;
            var resultPath = jsonFolder + "data" + index + ".json";

            var stringData=File.ReadAllText(resultPath);

            var jsonData=JsonConvert.DeserializeObject<DataSlot>(stringData);
            Debug.Log("data" + index + ".json");
            foreach (var saveable in saveableList)
            {
                saveable.RestoreData(jsonData.dataDict[saveable.GUID]);
            }
        }

    }
}