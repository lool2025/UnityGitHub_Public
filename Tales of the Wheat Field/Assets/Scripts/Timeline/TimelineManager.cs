using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : Singleton<TimelineManager>
{
    public PlayableDirector startDirector;

    private PlayableDirector currentDirector;

    private bool isDone;

    public GameObject TimelinePanel;

    public bool IsDone { set =>isDone = value; }

    private bool isPause;

    protected override void Awake()
    {
        base.Awake();
        currentDirector=startDirector;
    }

    private void Update()
    {
        if (isPause&&Input.GetKeyDown(KeyCode.Space))
        {
            isPause = false;
            currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
    }

    public void OnEnable()
    {
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }
    public void OnDisable()
    {
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }
    private void OnStartNewGameEvent(int obj)
    {
        currentDirector.Stop();
        currentDirector = FindObjectOfType<PlayableDirector>();
        TimelinePanel.SetActive(true);
        if (currentDirector != null)
        {
            currentDirector.Play();
        }
    }

    public void PauseTimeline(PlayableDirector director)
    {
        currentDirector=director;

        currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);

        isPause=true;
    }
   
}
