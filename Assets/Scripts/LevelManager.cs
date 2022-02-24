using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int levelNumber;
    public float cameraSize;

    public int timeLimit = 90; //in seconds
    public int numOfRealJewels;

    public List<JewelClass> jewelsToSpawn;

    [Header("Score Goals")]
    public int totalScore;
    public int star1Score;
    public int star2Score;
    public int star3Score;

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
