using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;

    [Header("Global Variables")]
    public int numOfLevels;

    private static MultiSceneManager _instance;

    public static MultiSceneManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        loadingScreen.SetActive(true);
        LoadHomeScene();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleLoadingScreen(bool _toggleOn)
    {
        loadingScreen.SetActive(_toggleOn);
    }

    public void LoadHomeScene()
    {
        StartCoroutine(LoadHomeSceneAsync());
    }

    private IEnumerator LoadHomeSceneAsync()
    {
        ToggleLoadingScreen(true);
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(UnloadAllSceneAsync());
        yield return StartCoroutine(LoadSceneAdditive("Home"));
        ToggleLoadingScreen(false);
    }

    public void StartGame(int _levelNo)
    {
        StartCoroutine(StartGameAsync(_levelNo));
    }

    private IEnumerator StartGameAsync(int _levelNo)
    {
        PlayerPrefs.SetInt("Last_Selected_Level", _levelNo);
        ToggleLoadingScreen(true);
        yield return StartCoroutine(UnloadAllSceneAsync());
        yield return StartCoroutine(LoadSceneAdditive("GameScene"));
        ToggleLoadingScreen(false);
    }

    #region "UI Canvas Handlers"
    public void OpenCanvas<T>(string canvasName, Action<T> onOpened = null) where T : UIManager
    {
        if (FindObjectOfType<T>() == null)
            StartCoroutine(OpenCanvasAsync(canvasName, onOpened));
    }

    private IEnumerator OpenCanvasAsync<T>(string canvasName, Action<T> onOpened) where T : UIManager
    {
        var request = Resources.LoadAsync<GameObject>(canvasName);
        while (!request.isDone)
        {
            yield return null;
        }

        var canvasObj = Instantiate(request.asset) as GameObject;
        if (canvasObj != null && onOpened != null)
        {
            onOpened(canvasObj.GetComponent<T>());
        }
    }
    #endregion

    #region "Scene Load/Unload"
    private IEnumerator LoadSceneAdditive(string _sceneName)
    {
        Scene _scene = SceneManager.GetSceneByName(_sceneName);
        if (_scene == null) yield break;
        if (_scene.isLoaded) yield break;

        AsyncOperation asyncAROperation = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
        asyncAROperation.allowSceneActivation = false;
        while (!asyncAROperation.isDone)
        {
            if (asyncAROperation.progress >= 0.9f)
            {
                asyncAROperation.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    private IEnumerator UnloadScene(string _sceneName)
    {
        Scene _scene = SceneManager.GetSceneByName(_sceneName);
        if (_scene != null && _scene.isLoaded)
        {
            AsyncOperation unloadAsync = SceneManager.UnloadSceneAsync(_sceneName, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
            while (!unloadAsync.isDone)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator UnloadAllSceneAsync(List<string> _skipScenes = null)
    {
        //Get all loaded scenes
        int countLoaded = SceneManager.sceneCount;
        Scene[] loadedScenes = new Scene[countLoaded];
        List<string> _scenesToSkip = new List<string> { "Index" };
        if (_skipScenes != null) _scenesToSkip.AddRange(_skipScenes);

        for (int i = 0; i < countLoaded; i++)
        {
            loadedScenes[i] = SceneManager.GetSceneAt(i);
        }
        foreach (Scene _scene in loadedScenes)
        {
            if (_scene != null && _scene.isLoaded && !_scenesToSkip.Contains(_scene.name))
            {
                yield return StartCoroutine(UnloadScene(_scene.name));
            }
        }
        yield return null;
    }
    #endregion

}
