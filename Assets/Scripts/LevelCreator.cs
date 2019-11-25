using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 offset;

    [Header("Prefabs")]
    public GameObject nonDestroyableWall;
    public GameObject destroyableWall;
    public GameObject outerWall;
    public GameObject groundPlane;

    [Header("PrefabHolders")]
    public GameObject nonDestroyableWallHolder;
    public GameObject destroyableWallHolder;
    public GameObject outerWallHolder;

    [Header("Grid sizes(> 5)")]
    public int gridSizeX;
    public int gridSizeZ;

    [Header("Num of Destroyable Walls")]
    public int noOfDestroyableWalls;

    [Header("Layermasks")]
    public LayerMask layerMask;


}
