using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private GameObject menuCanvas;
    public GameObject menuPrefab;

    public Button settingsBtn;
    public GameObject pausePanel;
    public Slider volumeSlider;
    public GameObject StatsPanel;

    public void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += OnAfterSceneLoadEvent;
    }

    public void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= OnAfterSceneLoadEvent;
    }

    public void Awake()
    {
        settingsBtn.onClick.AddListener(TogglePausePanel);
        if (AudioManager.Instance != null)
            volumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
    }

    public void Update()
    {
        if (Input.GetButtonDown("OpenStatsPanel"))
        {
            StatsPanel.GetComponent<StatsUI>(). openUI();
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            StatsManager.Instance.experienceAdd(100);
        }
    }

    private void Start()
    {
        menuCanvas = GameObject.FindWithTag("MenuCanvas");
        if (menuCanvas == null)
            Debug.Log("Пе");
        Instantiate(menuPrefab, menuCanvas.transform);
    }

    private void OnAfterSceneLoadEvent()
    {
        if (menuCanvas.transform.childCount > 0)
        {
            Destroy(menuCanvas.transform.GetChild(0).gameObject);
        }
    }

    private void TogglePausePanel()
    {
        bool isOpen = pausePanel.activeInHierarchy;

        if (isOpen)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1.0f;
        }
        else
        {
            System.GC.Collect();
            pausePanel.SetActive(true);
            Time.timeScale = 0.0f;
        }
    }

    public void ReturnMenuCanvas()
    {
        Time.timeScale = 1.0f;
        StartCoroutine(BackToMenu());
    }

    private IEnumerator BackToMenu()
    {
        pausePanel.SetActive(false);
        EventHandler.CallEndGameEvent();
        yield return new WaitForSeconds(1f);
        Instantiate(menuPrefab, menuCanvas.transform);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenMenu()
    {
        Instantiate(menuPrefab, menuCanvas.transform);
    }

    public void Text1()
    {
        Debug.Log("123");
    }
}