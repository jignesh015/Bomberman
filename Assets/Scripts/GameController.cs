using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public GameObject bombHolder;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public GameObject explosionHolder;

    public List<GameObject> activeBombs;

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
        player = GameObject.FindGameObjectWithTag("Player");

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

    public void PlaceBomb()
    {
        GameObject placedBomb = Instantiate(bombPrefab, bombHolder.transform);

        Vector3 playerPos = player.transform.position;
        placedBomb.transform.position = new Vector3(Mathf.Floor(playerPos.x) + 0.5f, placedBomb.transform.position.y, Mathf.Floor(playerPos.z) + 0.5f);
        activeBombs.Add(placedBomb);
    }

    public void Explode(Vector3 bombOrigin, float _explodeTime)
    {
        StartCoroutine(DoExplosion(bombOrigin, _explodeTime));
    }

    public IEnumerator DoExplosion(Vector3 bombOrigin, float _explodeTime)
    {
        Debug.Log("Do explosion");
        yield return new WaitForSeconds(_explodeTime);

        GameObject explosion = Instantiate(explosionPrefab, explosionHolder.transform);
        explosion.transform.position = new Vector3(bombOrigin.x, explosion.transform.position.y, bombOrigin.z);
        Debug.Log("Explosion done");
    }
}
