using System.Collections;
using MFarm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace MFarm.Transition
{
    public class TransitionManager : Singleton<TransitionManager>,ISaveable
    {
        public string startSceneName = string.Empty;

        private CanvasGroup fadeCanvasGroup;

        private bool isFade;

        public string GUID => GetComponent<DataGUID>().guid;


        protected override void Awake()
        {

            base.Awake ();
            SceneManager.LoadScene("UI",LoadSceneMode.Additive);
        }

   
        public void Start()
        {
            ISaveable saveable = this;
            saveable.RegisterSaveable();

            fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
          
        }

        private void OnEnable()
        {

            EventHandler.TransitionEvent += OnTransitionEvent;
            //新游戏
            EventHandler.StartNewGameEvent += OnStartNewGameEvent;
            EventHandler.EndGameEvent += OnEndGameEvent;
        }

        private void OnDisable()
        {

            EventHandler.TransitionEvent -= OnTransitionEvent;
            //新游戏
            EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
            EventHandler.EndGameEvent -= OnEndGameEvent;
        }

        private void OnEndGameEvent()
        {
            StartCoroutine(UnloadScene());
        }

        private void OnStartNewGameEvent(int obj)
        {
            StartCoroutine(LoadSaveDataScene(startSceneName));
        }


        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="sceneName">目标场景</param>
        /// <param name="targetPosition">目标位置</param>
        /// <returns></returns>
        private IEnumerator Transition(string sceneName, Vector3 targetPosition)
        {
            EventHandler.CallBeforeSceneUnloadEvent();

            yield return Fade(1);
            //卸载场景
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            //加载场景
            yield return LoadSceneSetActive(sceneName);
            //移动人物
            EventHandler.CallMoveToPosition(targetPosition);
           
            EventHandler.CallAfterSceneLoadedEvent();
            yield return Fade(0);
        }


        /// <summary>
        /// 加载场景并设置为激活
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <returns></returns>
        private IEnumerator LoadSceneSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

            SceneManager.SetActiveScene(newScene);

            EventHandler.CallAfterSceneLoadedEvent();

        }

      

        private void OnTransitionEvent(string sceneToGo, Vector3 targetPosition)
        {
            if(!isFade) 
                StartCoroutine(Transition(sceneToGo, targetPosition));
        }
        /// <summary>
        /// 实现切换场景的淡入淡出的效果
        /// </summary>
        /// <param name="targetAlpha">1是黑2是透明</param>
        /// <returns></returns>
        private IEnumerator Fade(float targetAlpha)
        {
            //表示正在淡入淡出
            isFade = true;
            //将UI的点击事件取消
            fadeCanvasGroup.blocksRaycasts = true;
            //根据他们的差值除以固定时间得出速度
            float speed=Mathf.Abs(fadeCanvasGroup.alpha-targetAlpha)/Settings.fadeDuration;

            while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
            {
                fadeCanvasGroup.alpha=Mathf.MoveTowards(fadeCanvasGroup.alpha,targetAlpha,speed*Time.deltaTime);
                yield return null;
            }
            fadeCanvasGroup.blocksRaycasts = false;
            isFade = false;
        }

        private IEnumerator LoadSaveDataScene(string sceneName)
        {
            yield return Fade(1f);
            if (SceneManager.GetActiveScene().name != "PersistentScene")
            {
                EventHandler.CallBeforeSceneUnloadEvent();
                yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            }

            yield return LoadSceneSetActive(sceneName);
            EventHandler.CallAfterSceneLoadedEvent();
            yield return Fade(0);

        }


        private IEnumerator UnloadScene()
        {
            EventHandler.CallBeforeSceneUnloadEvent();
            yield return Fade(1f);
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            yield return Fade(0);
        }


        public GameSaveData GenerateSaveData()
        {
            GameSaveData saveData = new GameSaveData();
            saveData.dataSceneName=SceneManager.GetActiveScene().name;

            return saveData;
        }

        public void RestoreData(GameSaveData saveData)
        {
           StartCoroutine(LoadSaveDataScene(saveData.dataSceneName));
        }
    }

  
}
