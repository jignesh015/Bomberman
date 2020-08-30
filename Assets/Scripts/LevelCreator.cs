using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 offset;

    [Header("Wall Prefabs")]
    public GameObject nonDestroyableWall;
    public GameObject destroyableWall;
    public GameObject outerWall;
    public GameObject groundPlane, cameraBoundingBox;

    [Header("PrefabHolders")]
    public GameObject nonDestroyableWallHolder;
    public GameObject destroyableWallHolder;
    public GameObject outerWallHolder;

    [Header("Grid sizes(> 5)")]
    public int gridSizeX;
    public int gridSizeZ;

    [Header("Num of Destroyable Walls")]
    public int noOfDestroyableWalls;

    [Header("Enemy Creation")]
    public Vector3 enemyOffset;
    public List<GameObject> enemies;
    public GameObject enemyHolder;

    [Header("Num of Enemies")]
    public List<int> noOfEachEnemies;

    [Header("Player Placement")]
    public GameObject player;
    public Vector3 playerOffset;
    
    [Header("Layermasks")]
    public LayerMask layerMask;

}
