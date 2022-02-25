using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.Rendering;

public class GameController : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;
    public CinemachineVirtualCamera _cinemachineCamera;

    [HideInInspector] public GameObject player;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public GameObject bombHolder;
    public List<string> tagsBombShouldCollideWith;
    public List<GameObject> activeBombs;
    [HideInInspector]
    public Dictionary<string, GameObject> activeBombsPositionDict;
    public int maxBombCount;

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

    [Header("Walls Transparent Mat")]
    public Material destroyableWallTransparentMat;
    public Material nonDestroyableWallTransparentMat;
    public Material outerWallTransparentMat;

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
    public BombsAvailableDisplayController bombsAvailableDisplayController;
    [HideInInspector] public PlayerController2 playerController;
    [HideInInspector] public LevelManager level;

    [Header("UI References")]
    public GameObject gameHUD;
    public Text levelText;
    public Text scoreText;
    public Text jewelsCollectedText;
    public Text explosionRangeText;
    public Text timerText;

    [HideInInspector] public bool isPopupOpen;
    [HideInInspector] public bool isLevelComplete;

    //Timer
    private float timeRemaining;
    private bool timerIsRunning = false;
    private bool timerAnimationActive = false;

    private int ogCamCullingMask;
    private int liveBombCount;

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
        //Start background SFX
        SFXManager.Instance.ToggleBackgroundMusic(true);

        //Assign culling mask
        ogCamCullingMask = mainCamera.cullingMask;

        //Load level
        yield return StartCoroutine(LoadLevel(PlayerPrefs.GetInt("Last_Selected_Level")));
        yield return null;

        //Set explosion range
        SetExplosionRange(level.explosionRange);

        //Set max bomb count
        maxBombCount = level.maxBombCount;
        liveBombCount = 0;

        //Get player reference
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController2>();

        //Assign Camera Bounds and ortographic size
        mainCamera.orthographicSize = level.cameraSize;
        _cinemachineCamera.m_Lens.OrthographicSize = level.cameraSize;
        cinemachineConfiner.m_BoundingVolume = level.cameraBoundingBox;

        //Assign player to virtual camera
        VirtualCamera.Follow = player.transform;
        VirtualCamera.LookAt = player.transform;

        //Initialize list for storing active bombs
        activeBombs = new List<GameObject>();

        //Get all non-destroyable and outer wall positions
        GetNonDestroyableWallsPosition();

        //Reset HUD
        DisplayScore(0, 0, 0);
        DisplayLevelNumber();
        DisplayExplosionRange(explosionRange);
        bombsAvailableDisplayController.UpdateBombAvailableDisplay(liveBombCount, maxBombCount);

        //Reset Timer
        timeRemaining = level.timeLimit;
        timerIsRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (level == null)
            return;

        if (gameOverController.isGameOver || isLevelComplete)
        {
            gameHUD.SetActive(false);
            jewelSpawner.gameObject.SetActive(false);
            return;
        }

        gameHUD.SetActive(!isPopupOpen);
        jewelSpawner.gameObject.SetActive(!isPopupOpen);

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
    }
    #endregion

    #region "Player's functions and power-ups"
    //Place bomb at player's current location
    public void PlaceBomb()
    {
        SFXManager _sfx = SFXManager.Instance;

        //Place bomb only if live bombs are less than the max bomb count 
        if (liveBombCount >= maxBombCount)
        {
            //Play error SFX
            _sfx.PlayAudio(playerController.playerAudioSource, _sfx.bombError);

            //Alert user
            bombsAvailableDisplayController.AlertUser();

            return;
        }

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
        UpdateLiveBombCount(0);
    }

    public void SetExplosionRange(int _explosionRange) => explosionRange = _explosionRange <= 
        maxExplosionRange ? _explosionRange : maxExplosionRange;

    /// <summary>
    /// Update live bomb count based on given status
    /// | 0 = Bomb Placed | 1 = Bomb exploded |
    /// </summary>
    /// <param name="_status"></param>
    public void UpdateLiveBombCount(int _status)
    {
        switch (_status)
        {
            case 0:
                //Bomb placed
                liveBombCount++;
                break;
            case 1:
                //Bomb exploded
                liveBombCount--;
                break;
        }
        bombsAvailableDisplayController.UpdateBombAvailableDisplay(liveBombCount, maxBombCount);
    }

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
            //Check if player has enough score
            if (!jewelCollectionController.HasEnoughScore())
            {
                //If doesn't have enough score, its game over
                gameOverController.OnGameOver(GameOverReason.Score, 1f);
                return;
            }

            //Level Complete
            isLevelComplete = true;
            PlayerPrefs.SetInt("Level_Completed", level.levelNumber);

            //Play Game win SFX
            SFXManager _sfx = SFXManager.Instance;
            _sfx.PlayAudio(_sfx.uiInteractionAudioSource, _sfx.gameWin);

            //Stop background music
            _sfx.ToggleBackgroundMusic(false);

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
    public void DisplayLevelNumber()
    {
        levelText.text = string.Format("Level : {0}", level.levelNumber);
    }

    public void DisplayScore(int _gameScore, int _jewelsFoundCount, float _delay = 1.5f)
    {
        StartCoroutine(DisplayScoreAsync(_gameScore,_jewelsFoundCount,_delay));
    }

    private IEnumerator DisplayScoreAsync(int _gameScore, int _jewelsFoundCount, float _delay = 1.5f)
    {
        yield return new WaitForSeconds(_delay);
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

    public void DisplayExplosionRange(int _range)
    {
        explosionRangeText.text = string.Format("Range : x{0}", _range);
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