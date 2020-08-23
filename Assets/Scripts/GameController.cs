using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject player;
    public bool gameOver;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public GameObject bombHolder;
    public List<GameObject> activeBombs;
    public Dictionary<string, GameObject> activeBombsPositionDict;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public GameObject wallDestroyPrefab, explosionHolder;
    [Range(1, 10)]
    public int explosionRange = 1;
    public float explosionDuration;

    [Header("List of walls position")]
    public Dictionary<string, GameObject> destroyableWallsPositionDict;
    public Dictionary<string, GameObject> nonDestroyableWallsPositionDict;
    public Dictionary<string, GameObject> outerWallsPositionDict;

    [Header("Enemies")]
    public float timeToDissolveEnemy;
    public List<string> tagsEnemyShouldCollideWith;

    private PlayerController2 playerController;

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

    void Start()
    {
        //Get player reference
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController2>();

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

    //Place bomb at player's current location
    public void PlaceBomb()
    {
        GameObject placedBomb = Instantiate(bombPrefab, bombHolder.transform);

        playerController.godMode = true;
        Vector3 playerPos = player.transform.position;
        placedBomb.transform.position = new Vector3(Mathf.Floor(playerPos.x) + 0.5f, placedBomb.transform.position.y, Mathf.Floor(playerPos.z) + 0.5f);
        activeBombs.Add(placedBomb);
    }

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

    #endregion
}
