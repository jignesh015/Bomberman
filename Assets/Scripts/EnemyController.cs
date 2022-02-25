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

    [Header("Variables for adding force")]
    public float force;
    public Vector3 forceDir;
    private Rigidbody enemyRigidbody;

    [Header("Variables for randomizing movement")]
    public float minRandomDistance;
    public float maxRandomDistance;


    #region "Private variables"
    private bool isDead, changeDirectionRandomly;
    private float startYPos = 0,dissolveValue, turnSmoothVelocity, randomWalkDistance;
    private Material[] enemyMat;
    private GameObject trail;

    [SerializeField]
    private Vector2 currentGridPos, gridPosAtDirectionChange;
    private Vector3 currentEnemyPos;
    
    private AudioSource enemyAudioSource;
    #endregion


    private GameController gameController;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<CharacterController>();
        enemyRigidbody = GetComponent<Rigidbody>();
        enemyAudioSource = GetComponent<AudioSource>();
        trail = transform.Find("Trail")?.gameObject;
        if (trail != null) trail.SetActive(false);

        enemyRigidbody.isKinematic = true;
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
        if (gameController.gameOverController.isGameOver 
            || gameController.isLevelComplete 
            || gameController.isPopupOpen) 
            return;

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
        currentEnemyPos = enemy.transform.position;


        //Change walk direction after random intervals
        currentGridPos = new Vector2(Mathf.Floor(currentEnemyPos.x) + 0.5f,Mathf.Floor(currentEnemyPos.z) + 0.5f);
        if (Vector2.Distance(currentGridPos, gridPosAtDirectionChange) > randomWalkDistance && !changeDirectionRandomly)
            changeDirectionRandomly = true;

        if (changeDirectionRandomly)
        {
            if (!(Mathf.Floor(currentEnemyPos.x) % 2 == 0 && Mathf.Floor(currentEnemyPos.z) % 2 == 0) 
                && Vector2.Distance(currentGridPos, new Vector2(currentEnemyPos.x, currentEnemyPos.z)) < 0.05f)
                RandomizeDirection();
        }

        
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            //Make character fly!!
            enemyRigidbody.isKinematic = false;
            enemyRigidbody.AddExplosionForce(force, gameController.lastBombOrigin, gameController.explosionRange + 10, 5, ForceMode.Impulse);
            enemyRigidbody.angularVelocity = new Vector3(2, 2, 2);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

        if (hit.gameObject.CompareTag("Player"))
        {
            RandomizeDirection();
            hit.gameObject.GetComponent<PlayerController2>().KillPlayer();
            return;
        }

        if (enemyType == EnemyType.Hitter && hit.gameObject.CompareTag("Bomb"))
        {
            //Push bomb
            hit.gameObject.GetComponent<BombController>().PushBomb(walkDirection);

            //Go to opposite direction
            walkDirection *= -1;
            return;

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
        changeDirectionRandomly = false;
        randomWalkDistance = Random.Range(minRandomDistance, maxRandomDistance);
        walkDirection = enemyWalkingDirection[Random.Range(0, 4)];
        gridPosAtDirectionChange = new Vector2(Mathf.Floor(currentEnemyPos.x) + 0.5f, Mathf.Floor(currentEnemyPos.z) + 0.5f);
    }

    public void KillEnemy()
    {
        if (isDead) return;

        isDead = true;
        Destroy(gameObject, 2.5f);

        //Play enemy dead sfx
        SFXManager.Instance.PlayAudio(enemyAudioSource, SFXManager.Instance.enemyDeath);

        //Spawn trail
        if (trail != null && trail.transform.GetComponent<ParticleSystem>() != null)
        {
            trail.SetActive(true);
        }
    }

    public void HitBomb(GameObject _bomb, Vector3 _hitDirection)
    {
        Rigidbody _rb = _bomb.GetComponent<Rigidbody>();
        if (!_rb) return;

        _rb.isKinematic = false;
        _rb.AddForce(_hitDirection * 200f);
    }
}
