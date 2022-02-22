using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.Rendering;

public class GameController : MonoBehaviour
{
    [HideInInspector] public GameObject player, mainCamera;
    public bool gameOver;

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
    private LevelManager level;
    private PlayerController2 playerController;
    public JewelSpawnerController jewelSpawner;
    public CinemachineConfiner cinemachineConfiner;

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
        //Load level
        yield return StartCoroutine(LoadLevel(PlayerPrefs.GetInt("Last_Selected_Level")));

        //Get player reference
        player = GameObject.FindGameObjectWithTag("Player");
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
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
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!playerController.isDead)
                PlaceBomb();
            else
            {
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            }
        }

#endif
    }

    #region "Level loading"
    private IEnumerator LoadLevel(int _levelNo)
    {
        _levelNo = _levelNo == 0 ? 1 : _levelNo;
        PlayerPrefs.SetInt("Last_Selected_Level", _levelNo);
        string levelPrefabName = string.Format("Levels/Level {0}", _levelNo);
        Debug.LogFormat("Load level at: {0}", levelPrefabName);
        var request = Resources.LoadAsync<GameObject>(levelPrefabName);
        while (!request.isDone)
        {
            yield return null;
        }

        var levelObj = Instantiate(request.asset) as GameObject;
        level = levelObj.GetComponent<LevelManager>();
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
}
