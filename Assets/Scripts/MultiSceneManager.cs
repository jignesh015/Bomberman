using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingScreen;

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
        yield return StartCoroutine(LoadSceneAdditive("Home"));
        ToggleLoadingScreen(false);
    }

    public void StartGame(int _levelNo)
    {
        StartCoroutine(StartGameAsync(_levelNo));
    }

    private IEnumerator StartGameAsync(int _levelNo)
    {
        string _levelName = string.Format("Level {0}", _levelNo);
        ToggleLoadingScreen(true);
        yield return StartCoroutine(UnloadScene("Home"));
        yield return StartCoroutine(LoadSceneAdditive(_levelName));
        ToggleLoadingScreen(false);
    }

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


}
