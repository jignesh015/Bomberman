﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public GameObject bombHolder;
    public List<GameObject> activeBombs;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public GameObject explosionHolder;
    [Range(1, 10)]
    public int explosionRange = 1;

    [Header("List of walls position")]
    public Dictionary<string, GameObject> destroyableWallsPositionDict;
    public Dictionary<string, GameObject> nonDestroyableWallsPositionDict;
    public Dictionary<string, GameObject> outerWallsPositionDict;

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

        //Initialize list for storing active bombs
        activeBombs = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceBomb();
        }

#endif
    }

    //Place bomb at player's current location
    public void PlaceBomb()
    {
        GameObject placedBomb = Instantiate(bombPrefab, bombHolder.transform);

        Vector3 playerPos = player.transform.position;
        placedBomb.transform.position = new Vector3(Mathf.Floor(playerPos.x) + 0.5f, placedBomb.transform.position.y, Mathf.Floor(playerPos.z) + 0.5f);
        activeBombs.Add(placedBomb);
    }

    #region "Store walls position to a list"
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

    #endregion
}