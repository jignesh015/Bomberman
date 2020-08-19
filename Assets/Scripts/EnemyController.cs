using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public CharacterController enemy;
    public enum EnemyType { Normal, Runner1, Runner2, Ghost1, Ghost2, Hitter, MultiLives }
    public List<Vector3> enemyWalkingDirection;

    public EnemyType enemyType;
    public Vector3 walkDirection;
    public float speed;
    public int lives = 1;

    public float startYPos = 0;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<CharacterController>();
        enemyWalkingDirection = new List<Vector3>()
        {
            new Vector3(1,0,0), new Vector3(-1,0,0),
            new Vector3(0,0,1), new Vector3(0,0,-1)
        };

        walkDirection = enemyWalkingDirection[Random.Range(0, 4)];
        
    }

    // Update is called once per frame
    void Update()
    {
        if(startYPos == 0) startYPos = transform.position.y;

        //Enemy movement logic
        if (enemyType == EnemyType.Normal)
        {
            //Move forward if path is open
            enemy.Move(walkDirection * speed * Time.deltaTime);
            if (startYPos != 0) enemy.transform.position = new Vector3(enemy.transform.position.x, startYPos, enemy.transform.position.z);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Destroyable") || hit.gameObject.CompareTag("NonDestroyable")
            || hit.gameObject.CompareTag("OuterWall") || hit.gameObject.CompareTag("Bomb") || hit.gameObject.CompareTag("Enemy"))
        {
            walkDirection = enemyWalkingDirection[Random.Range(0, 4)];
        }
    }
}
