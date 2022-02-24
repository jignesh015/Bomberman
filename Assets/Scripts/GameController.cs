using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.Rendering;

public class GameController : MonoBehaviour
{
    public bool gameOver;
    public Camera mainCamera;
    [HideInInspector] public GameObject player;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public GameObject bombHolder;
    public List<string> tagsBombShouldCollideWith;
    public List<GameObject> activeBombs;
    [HideInInspector]
    public Dictionary<string, GameObject> activeBombsPositionDict;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public GameObject wallDestroyPrefab, explosionHolder;
    [Range(1, 10)]
    public int explosionRange = 1;
    public int maxExplosionRange = 10;
    public float explosionDuration;
    public Vector3 lastBombOrigin;

    [Header("List of walls position")]
    public Dictionary<string, GameObject> destroyableWallsPositionDict;
    public Dictionary<string, GameObject> nonDestroyableWallsPositionDict;
    public Dictionary<string, GameObject> outerWallsPositionDict;

    [Header("Enemies")]
    public float timeToDissolveEnemy;
    public List<string> tagsEnemyShouldCollideWith;

    [Header("Camera properties")]
    public float shakeDuration = 0.3f;
    public float shakeAmplitude = 1.2f, shakeFrequency = 2.0f;
    public CinemachineVirtualCamera VirtualCamera;
    public CinemachineCameraShaker cameraShaker;
    public Vector2 cameraMinClampOffset, cameraMaxClampOffset;

    [Header("Script References")]
    public JewelSpawnerController jewelSpawner;
    public JewelCollectionController jewelCollectionController;
    public CinemachineConfiner cinemachineConfiner;
    public GameOverController gameOverController;
    [HideInInspector] public PlayerController2 playerController;
    [HideInInspector] public LevelManager level;

    [Header("UI References")]
    public GameObject gameHUD;
    public Text scoreText;
    public Text jewelsCollectedText;
    public Text timerText;

    [HideInInspector] public bool isPopupOpen;
    [HideInInspector] public bool isLevelComplete;

    //Timer
    private float timeRemaining;
    private bool timerIsRunning = false;
    private bool timerAnimationActive = false;

    private int ogCamCullingMask;


    private static GameController _instance;
    public static GameController Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    IEnumerator Start()
    {
        //Assign culling mask
        ogCamCullingMask = mainCamera.cullingMask;

        //Load level
        yield return StartCoroutine(LoadLevel(PlayerPrefs.GetInt("Last_Selected_Level")));

        //Reset Score
        DisplayScore(0,0);

        //Get player reference
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController2>();

        //Assign Camera Bounds
        cinemachineConfiner.m_BoundingVolume = level.cameraBoundingBox;

        //Assign player to virtual camera
        VirtualCamera.Follow = player.transform;
        VirtualCamera.LookAt = player.transform;

        //Initialize list for storing active bombs
        activeBombs = new List<GameObject>();

        //Get all non-destroyable and outer wall positions
        GetNonDestroyableWallsPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (level == null)
            return;

        if (gameOverController.isGameOver || isLevelComplete)
        {
            gameHUD.SetActive(false);
            return;
        }

        gameHUD.SetActive(!isPopupOpen);

        mainCamera.cullingMask = isPopupOpen ? (ogCamCullingMask & ~(1 << 13)) : ogCamCullingMask;

        if (isPopupOpen) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!playerController.isDead)
                PlaceBomb();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnHomeButtonPressed();
        }

        //Timer
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTimer(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
                DisplayTimer(timeRemaining);
                gameOverController.OnGameOver(GameOverReason.Time, 0.2f);
            }
        }
    }

    #region "Level loading"
    private IEnumerator LoadLevel(int _levelNo)
    {
        _levelNo = _levelNo == 0 ? 1 : _levelNo;
        PlayerPrefs.SetInt("Last_Selected_Level", _levelNo);
        string levelPrefabName = string.Format("Levels/Level {0}", _levelNo);
        var request = Resources.LoadAsync<GameObject>(levelPrefabName);
        while (!request.isDone)
        {
            yield return null;
        }

        var levelObj = Instantiate(request.asset) as GameObject;
        levelObj.transform.parent = transform;
        level = levelObj.GetComponent<LevelManager>();
        timeRemaining = level.timeLimit;
        timerIsRunning = true;
    }
    #endregion

    #region "Player's functions and power-ups"
    //Place bomb at player's current location
    public void PlaceBomb()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 _posToPlaceBomb = new Vector3(Mathf.Floor(playerPos.x) + 0.5f, 0, Mathf.Floor(playerPos.z) + 0.5f);
        if (GetActiveBombsPosition().ContainsKey("X" + _posToPlaceBomb.x.ToString() + "Z" + _posToPlaceBomb.z.ToString())
            || GetDestroyableWallsPosition().ContainsKey("X" + _posToPlaceBomb.x.ToString() + "Z" + _posToPlaceBomb.z.ToString()))
        {
            Debug.Log("Can't place bomb at this location");
            return;
        }
        
        playerController.indestructible = true;
        GameObject placedBomb = Instantiate(bombPrefab, bombHolder.transform);
        placedBomb.transform.position = new Vector3(_posToPlaceBomb.x, placedBomb.transform.position.y, _posToPlaceBomb.z);
        activeBombs.Add(placedBomb);
    }

    public void SetExplosionRange(int _explosionRange) => explosionRange = _explosionRange <= 
        maxExplosionRange ? _explosionRange : maxExplosionRange;

    public void TogglePlayerGhostMode(bool _ghostMode)
    {
        playerController.ghostMode = _ghostMode;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("DestroyableWall"), _ghostMode);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Bomb"), _ghostMode);
    }

    public void ShakeCamera()
    {
        cameraShaker.CinemachineShake();
    }

    public void CheckForLevelComplete(int _gameScore,int _jewelsFoundCount, List<Jewel> _jewelsCollected)
    {
        if (_jewelsFoundCount == level.jewelsToSpawn.Count)
        {
            //Level Complete
            isLevelComplete = true;
            PlayerPrefs.SetInt("Level_Completed", level.levelNumber);

            //Check for game complete
            if (level.levelNumber == MultiSceneManager.Instance.numOfLevels)
            {
                PlayerPrefs.SetInt("Game_Completed", level.levelNumber);
                //Open Game Complete UI
                StartCoroutine(OpenGameCompleteUI());
            }
            else
            { 
                //Open Level Complete UI
                StartCoroutine(OpenLevelCompleteUI(_gameScore, _jewelsCollected));
            }

        }
    }

    private IEnumerator OpenLevelCompleteUI(int _gameScore, List<Jewel> _jewelsCollected)
    {
        yield return new WaitForSeconds(1f);

        MultiSceneManager.Instance.OpenCanvas<LevelCompleteUIManager>("UI/LevelCompleteUI", popup => {
            popup.OnPopupOpen(_gameScore, _jewelsCollected, level);
        });
    }

    private IEnumerator OpenGameCompleteUI()
    {
        yield return new WaitForSeconds(1f);

        MultiSceneManager.Instance.OpenCanvas<LevelCompleteUIManager>("UI/GameCompleteUI");
    }
    #endregion

    #region "Store walls and bombs position in a list"
    public Dictionary<string, GameObject> GetDestroyableWallsPosition()
    {
        destroyableWallsPositionDict = new Dictionary<string, GameObject>();
        GameObject[] destroyableWalls = GameObject.FindGameObjectsWithTag("Destroyable");
        foreach (GameObject wall in destroyableWalls)
        {
            string positionString = "X" + wall.transform.position.x.ToString() + "Z" + wall.transform.position.z.ToString();
            destroyableWallsPositionDict.Add(positionString, wall);
        }

        return destroyableWallsPositionDict;
    }

    public void GetNonDestroyableWallsPosition()
    {
        nonDestroyableWallsPositionDict = new Dictionary<string, GameObject>();
        outerWallsPositionDict = new Dictionary<string, GameObject>();
        GameObject[] nonDestroyableWalls = GameObject.FindGameObjectsWithTag("NonDestroyable");
        GameObject[] outerWalls = GameObject.FindGameObjectsWithTag("OuterWall");
        foreach (GameObject wall in nonDestroyableWalls)
        {
            string positionString = "X" + wall.transform.position.x.ToString() + "Z" + wall.transform.position.z.ToString();
            nonDestroyableWallsPositionDict.Add(positionString, wall);
        }
        foreach (GameObject wall in outerWalls)
        {
            string positionString = "X" + wall.transform.position.x.ToString() + "Z" + wall.transform.position.z.ToString();
            outerWallsPositionDict.Add(positionString, wall);
        }
    }

    public Dictionary<string,GameObject> GetActiveBombsPosition()
    {
        activeBombsPositionDict = new Dictionary<string, GameObject>();
        foreach (GameObject _bomb in activeBombs)
        {
            string positionString = "X" + _bomb.transform.position.x.ToString() + "Z" + _bomb.transform.position.z.ToString();
            activeBombsPositionDict.Add(positionString, _bomb);
        }

        return activeBombsPositionDict;
    }

    public Vector2 GetGridSize()
    {
        GameObject[] outerWalls = GameObject.FindGameObjectsWithTag("OuterWall");
        float posX = 0, posZ = 0;

        foreach (GameObject wall in outerWalls)
        {
            posX = posX < wall.transform.position.x ? wall.transform.position.x : posX;
            posZ = posZ < wall.transform.position.x ? wall.transform.position.z : posZ;
        }
        Vector2 _gridSize = new Vector2(posX,posZ);
        return _gridSize;
    }

    #endregion

    #region "HUD Display Management"
    public void DisplayScore(int _gameScore, int _jewelsFoundCount)
    {
        scoreText.text = string.Format("Score : {0}", _gameScore);
        jewelsCollectedText.text = string.Format("Jewels Found : {0}/{1}", _jewelsFoundCount, level.jewelsToSpawn.Count);
    }

    public void DisplayTimer(float _timeRemaining)
    {
        if (_timeRemaining < 11 && !timerAnimationActive)
        {
            timerAnimationActive = true;
            timerText.GetComponent<Animator>().SetTrigger("Play");
        }
        float minutes = Mathf.FloorToInt(_timeRemaining / 60);
        float seconds = Mathf.FloorToInt(_timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    #endregion

    #region "On UI Interaction"
    public void OnHelpButtonPressed()
    {
        if (isPopupOpen) return;

        MultiSceneManager.Instance.OpenCanvas<HelpPopupUIManager>("UI/HelpPopupUI", popup => {
            isPopupOpen = true;
        });
    }

    public void OnHomeButtonPressed()
    {
        if (isPopupOpen) return;

        MultiSceneManager.Instance.OpenCanvas<ExitGameUIManager>("UI/ExitGameUI", popup => {
            isPopupOpen = true;
        });
    }
    #endregion
}