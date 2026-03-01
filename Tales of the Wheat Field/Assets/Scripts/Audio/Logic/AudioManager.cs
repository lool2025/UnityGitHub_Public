using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
public class AudioManager : Singleton<AudioManager>
{
    [Header("音乐数据库")]
    public  SceneSoundList_SO SceneSoundData;
    public SoundDetailsList_SO SoundDetailsData;
    [Header("Audio Source")]
    public AudioSource ambientSource;
    public AudioSource gameSource;

    private Coroutine soundRoutine;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Snapshots")]
    public AudioMixerSnapshot normalSnapShot;
    public AudioMixerSnapshot ambientSnapShot;
    public AudioMixerSnapshot muteSnapShot;

    public float MusicStartSecond=>Random.Range(5f, 10f);

    private float musicTransitionSecond = 8f;

    public void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;

        EventHandler.PlaySoundEvent += OnPlaySoundEvent;

        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    public void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
        EventHandler.PlaySoundEvent -= OnPlaySoundEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void OnEndGameEvent()
    {
        if(soundRoutine != null)
            StopCoroutine(soundRoutine);
        muteSnapShot.TransitionTo(1f);
    }

    private void OnPlaySoundEvent(SoundName name)
    {
        var soundDetails= SoundDetailsData.GetSoundDetails(name);
        if(soundDetails != null)
        {
            EventHandler.CallInitSoundEffect(soundDetails);
        }
    }

    private void OnAfterSceneLoadEvent()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneSoundItem sceneSound=SceneSoundData.GetsceneSoundItem(currentScene);
        if (sceneSound == null)
            return;
        SoundDetails ambient = SoundDetailsData.GetSoundDetails(sceneSound.ambient);
        SoundDetails music= SoundDetailsData.GetSoundDetails(sceneSound.music);

        if (soundRoutine != null)
            StopCoroutine(soundRoutine);
        soundRoutine = StartCoroutine(PlaySoundRoutine(music, ambient));

    }

    private IEnumerator PlaySoundRoutine(SoundDetails music,SoundDetails ambient)
    {
        if(music != null&& ambient != null)
        {
            PlayAmbientClip(ambient, 1f);
            //暂停几秒播放音乐
            yield return new WaitForSeconds(MusicStartSecond);
            PlayMusicClip(music,musicTransitionSecond);
        }
    }


    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="soundDetails"></param>
    private void PlayMusicClip(SoundDetails soundDetails, float transitionTime)
    {
        audioMixer.SetFloat("MusicVolume", ConvertSoundVolume(soundDetails.soundVolume));
        gameSource.clip=soundDetails.soundClip;
      
        if (gameSource.isActiveAndEnabled)
        {
            gameSource.Play();
          
            
        }
        normalSnapShot.TransitionTo(transitionTime);

    }
    /// <summary>
    /// 播放环境音效
    /// </summary>
    /// <param name="soundDetails"></param>
    private void PlayAmbientClip(SoundDetails soundDetails,float transitionTime)
    {
        audioMixer.SetFloat("AmbientVolume", ConvertSoundVolume(soundDetails.soundVolume));
        ambientSource.clip = soundDetails.soundClip;
       
        if (ambientSource.isActiveAndEnabled)
        {
       
            ambientSource.Play();
        }
        ambientSnapShot.TransitionTo(transitionTime);
    }

    private float ConvertSoundVolume(float amount)
    {
        amount = Mathf.Clamp01(amount);
        if (Mathf.Approximately(amount, 0f)) return -80f;
        return Mathf.Log10(amount) * 20f; // 正确的分贝转换公式
    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", (value + 100 - 80));
    }
}
