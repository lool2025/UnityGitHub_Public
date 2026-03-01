using System.Collections;
using System.Collections.Generic;
using MFarm.Dialogue;
using UnityEngine;
using UnityEngine.Playables;
[System.Serializable]
public class DialogueBehaviour : PlayableBehaviour
{
    //获得当前的播放器
    private PlayableDirector director;

    public DialoguePiece dialoguePiece;

   


    public override void OnPlayableCreate(Playable playable)
    {
        director=(playable.GetGraph().GetResolver()as PlayableDirector);
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (dialoguePiece.isContinue != false)
        {
            Debug.Log("转换1");
            EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
        }
        else
        {
            Debug.Log("转换");
            EventHandler.CallUpdateGameStateEvent(GameState.Pause);

        }
        //对话开始呼叫ui
        EventHandler.CallShowDialogueEvent(dialoguePiece);

        if (Application.isPlaying)
        {
            if(dialoguePiece.hasToPause)
            {
                //暂停timeline
                TimelineManager.Instance.PauseTimeline(director);
              
            }
            else
            {
                EventHandler.CallShowDialogueEvent(null);
              
            }
        }

    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
       if(Application.isPlaying) 
            TimelineManager.Instance.IsDone=dialoguePiece.isDone;
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        EventHandler.CallShowDialogueEvent(null);
    }

    public override void OnGraphStart(Playable playable)
    {
        Debug.Log("有继续");
      //  EventHandler.CallUpdateGameStateEvent(GameState.Pause);
    }

    public override void OnGraphStop(Playable playable)
    {
        Debug.Log("有继续111");
       // EventHandler.CallUpdateGameStateEvent(GameState.GamePlay);
    }

}
