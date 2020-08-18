using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyType { Normal, Runner1, Runner2, Ghost1, Ghost2, Hitter, MultiLives }

    public EnemyType enemyType;
    public float speed;
    public int lives = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Enemy movement logic
        if (enemyType == EnemyType.Normal)
        { 
            
        }
    }
}
