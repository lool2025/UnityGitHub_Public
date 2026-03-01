using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : Singleton<NPCManager>
{
    public SceneRouteDataList_SO sceneRouteDate;
    public List<NPCPosition> npcPositionList;

    private Dictionary<string,SceneRoute>sceneRouteDict= new Dictionary<string,SceneRoute>();
  
    protected override void Awake()
    {
        base.Awake();
        InitSceneRouteDict();
    }

    public void OnEnable()
    {
        //新游戏
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    public void OnDisable()
    {
        //新游戏
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }

    private void OnStartNewGameEvent(int obj)
    {
        foreach(var character in npcPositionList)
        {   
          
            character.npc.position=character.position;
            character.npc.GetComponent<NPCMovement>().currentScene = character.startScene;
        }
    }

    private void InitSceneRouteDict()
    {
       
        if (sceneRouteDate.sceneRouteList.Count > 0 )
        {
            foreach(SceneRoute route in sceneRouteDate.sceneRouteList)
            {
                var key=route.fromSceneName+route.gotoSceneName;
               
                if (sceneRouteDict.ContainsKey(key))
                {
                    continue;
                }
                else
                {
                    sceneRouteDict.Add(key,route);
                }

            }
        }
    }
    public SceneRoute GetSceneRoute(string fromSceneName,string gotoSceneName)
    {
       
        if(sceneRouteDict[fromSceneName + gotoSceneName]!=null)
            return sceneRouteDict[fromSceneName+gotoSceneName];
       
        return null;
    }

}
