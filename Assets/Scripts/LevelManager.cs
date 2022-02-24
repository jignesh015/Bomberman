using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int levelNumber;
    public float cameraSize;

    public int timeLimit = 90; //in seconds
    public int explosionRange = 1;
    public int maxBombCount = 2;

    public int numOfRealJewels;
    public List<JewelClass> jewelsToSpawn;

    [Header("References")]
    public Collider cameraBoundingBox;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
