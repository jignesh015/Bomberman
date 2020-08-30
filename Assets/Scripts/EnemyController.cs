using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector]
    public CharacterController enemy;

    public Renderer[] mainRenderers;
    public enum EnemyType { Normal, Runner1, Runner2, Ghost1, Ghost2, Hitter, MultiLives }
    public List<Vector3> enemyWalkingDirection;

    public EnemyType enemyType;
    public Vector3 walkDirection;
    public float speed, turnSmoothTime = 0.1f;
    public int lives = 1;

    public float startYPos = 0;

    [Header("Variables for adding force")]
    public float force;
    public Vector3 forceDir;
    private Rigidbody rigidbody;

    private bool isDead;
    private float dissolveValue, turnSmoothVelocity;
    private Material[] enemyMat;
    private GameObject trail;

    private GameController gameController;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        trail = transform.GetChild(1).gameObject;
        if (trail != null) trail.SetActive(false);

        rigidbody.isKinematic = true;
        gameController = GameController.Instance;

        enemyMat = new Material[mainRenderers.Length];
        for (int i = 0; i < mainRenderers.Length; i++)
        {
            enemyMat[i] = mainRenderers[i].material;
        }

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
        if (gameController.gameOver) return;

        if (isDead && dissolveValue < 2f)
        {
            dissolveValue += gameController.timeToDissolveEnemy * Time.deltaTime;
            foreach (Material _mat in enemyMat)
            {
                _mat.SetFloat("DissolveAmt", dissolveValue);
            }

            return;
        }

        if (isDead) return;

        if(startYPos == 0) startYPos = transform.position.y;

        //Move forward if path is open
        float targetAngle = Mathf.Atan2(walkDirection.x, walkDirection.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        enemy.Move(walkDirection * speed * Time.deltaTime);
        if (startYPos != 0) enemy.transform.position = new Vector3(enemy.transform.position.x, startYPos, enemy.transform.position.z);
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            //Make character fly!!
            rigidbody.isKinematic = false;
            //rigidbody.AddForce((gameController.mainCamera.transform.position - transform.position) , ForceMode.Impulse);
            rigidbody.AddExplosionForce(force, gameController.lastBombOrigin, gameController.explosionRange + 10, 5, ForceMode.Impulse);
            rigidbody.angularVelocity = new Vector3(2, 2, 2);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

        if (hit.gameObject.CompareTag("Player"))
        {
            walkDirection = enemyWalkingDirection[Random.Range(0, 4)];
            hit.gameObject.GetComponent<PlayerController2>().KillPlayer();
        }

        foreach (string _tag in gameController.tagsEnemyShouldCollideWith)
        {
            if (hit.gameObject.CompareTag(_tag))
            {
                RandomizeDirection();
                break;
            }
        }
    }

    void RandomizeDirection()
    {
        walkDirection = enemyWalkingDirection[Random.Range(0, 4)];
    }

    public void KillEnemy()
    {
        if (isDead) return;

        isDead = true;
        Destroy(gameObject, 2.5f);

        //Spawn trail
        if (trail != null && trail.transform.GetComponent<ParticleSystem>() != null)
        {
            trail.SetActive(true);
        }
    }
}
