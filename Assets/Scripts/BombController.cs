using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BombController : MonoBehaviour
{
    public bool hasDetonator;
    public float explodeTime = 4f;
    private Transform player;
    private bool shouldTween = true;
    private bool isRigid = false, shouldPush = false;
    private List<GameObject> explosionTrails;
    private ParticleSystem trailEffect;

    private float bombPlaceTime;
    private Vector3 pushToPos;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameController.Instance;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        trailEffect = transform.GetChild(1).GetComponent<ParticleSystem>();
        explosionTrails = new List<GameObject>();
        bombPlaceTime = Time.time;

        if (!hasDetonator) Explode(explodeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (string _tag in gameController.tagsBombShouldCollideWith)
        {
            if (collision.gameObject.CompareTag(_tag)) StopPushingBomb();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldTween)
        {
            shouldTween = false;
            transform.DOPunchScale(new Vector3(-0.1f, -0.1f, -0.1f), 0.3f, 1, 0.2f).OnComplete(() => {shouldTween = true;});
        }

        if (!isRigid)
        {
            if (Vector3.Distance(player.position, transform.position) > 1)
            {
                isRigid = true;
                GetComponent<BoxCollider>().enabled = true;
                player.GetComponent<PlayerController2>().indestructible = false;
            }
            if (Time.time - bombPlaceTime > 0.75f) player.GetComponent<PlayerController2>().indestructible = false;
        }

        if (shouldPush)
        {
            transform.position = Vector3.Lerp(transform.position, pushToPos, Time.deltaTime * 5f);
            if (Vector3.Distance(transform.position, pushToPos) < 0.1f) StopPushingBomb();
        }

    }

    public void Explode()
    {
        //Start bomb explosion at bomb origin right now!
        StartCoroutine(DoExplosion(transform.position, 0));
    }

    public void Explode(float _explodeTime)
    {
        //Start bomb explosion at bomb origin after explodeTime
        StartCoroutine(DoExplosion(transform.position, _explodeTime));
    }

    public IEnumerator DoExplosion(Vector3 _bombOrigin, float _explodeTime)
    {
        yield return new WaitForSeconds(_explodeTime);

        //Update bomb origin
        _bombOrigin = new Vector3(Mathf.Floor(transform.position.x) + 0.5f, 
            0, Mathf.Floor(transform.position.z) + 0.5f);

        //Hide bomb 
        GetComponent<BoxCollider>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
        Invoke("DestroyTrails", gameController.explosionDuration);
        Destroy(gameObject, gameController.explosionDuration);
        //gameController.activeBombs.RemoveAt(0);
        gameController.activeBombs.Remove(gameObject);
        gameController.ShakeCamera();
        gameController.lastBombOrigin = _bombOrigin;

        GameObject explosion = GetExplosionGameObject();
        explosionTrails.Add(explosion);
        explosion.transform.position = new Vector3(_bombOrigin.x, explosion.transform.position.y, _bombOrigin.z);

        //Do trail explosion
        StartCoroutine(DoTrailExplosion(_bombOrigin));

        //Update live bomb count
        GameController.Instance.UpdateLiveBombCount(1);
    }

    public void CancelPreviousDetonationAndDoExplosion()
    {
        StopAllCoroutines();
        Explode();
    }

    private GameObject GetExplosionGameObject()
    {
        return Instantiate(gameController.explosionPrefab, gameController.explosionHolder.transform);
    }

    private IEnumerator DoTrailExplosion(Vector3 _bombOrigin)
    {
        Dictionary<string, GameObject> destroyableWallsPosition = gameController.GetDestroyableWallsPosition();
        Dictionary<string, GameObject> nonDestroyableWallsPosition = gameController.nonDestroyableWallsPositionDict;
        Dictionary<string, GameObject> outerWallsPosition = gameController.outerWallsPositionDict;
        Dictionary<string, GameObject> activeBombPosition = gameController.GetActiveBombsPosition();
        bool stopX1 = false, stopX2 = false, stopZ1 = false, stopZ2 = false;

        int explosionRange = gameController.explosionRange;
        for (int i = 1; i <= explosionRange; i++)
        {
            //X+1
            if (!stopX1)
            {
                GameObject explosionX1 = GetExplosionGameObject();
                explosionTrails.Add(explosionX1);
                explosionX1.transform.position = new Vector3(_bombOrigin.x + i, explosionX1.transform.position.y, _bombOrigin.z);
                string positionStringX1 = "X" + explosionX1.transform.position.x + "Z" + explosionX1.transform.position.z;
                if (destroyableWallsPosition.ContainsKey(positionStringX1) || nonDestroyableWallsPosition.ContainsKey(positionStringX1) 
                    || outerWallsPosition.ContainsKey(positionStringX1) || activeBombPosition.ContainsKey(positionStringX1))
                {
                    explosionX1.SetActive(false);
                    if(destroyableWallsPosition.ContainsKey(positionStringX1)) DestroyWalls(destroyableWallsPosition[positionStringX1]);
                    if (activeBombPosition.ContainsKey(positionStringX1)) 
                        activeBombPosition[positionStringX1].GetComponent<BombController>().CancelPreviousDetonationAndDoExplosion();
                    stopX1 = true;
                }
            }


            //X-1
            if (!stopX2)
            {
                GameObject explosionX2 = GetExplosionGameObject();
                explosionTrails.Add(explosionX2);
                explosionX2.transform.position = new Vector3(_bombOrigin.x - i, explosionX2.transform.position.y, _bombOrigin.z);
                string positionStringX2 = "X" + explosionX2.transform.position.x + "Z" + explosionX2.transform.position.z;
                if (destroyableWallsPosition.ContainsKey(positionStringX2) || nonDestroyableWallsPosition.ContainsKey(positionStringX2)
                    || outerWallsPosition.ContainsKey(positionStringX2) || activeBombPosition.ContainsKey(positionStringX2))
                {
                    explosionX2.SetActive(false);
                    if (destroyableWallsPosition.ContainsKey(positionStringX2)) DestroyWalls(destroyableWallsPosition[positionStringX2]);
                    if (activeBombPosition.ContainsKey(positionStringX2))
                        activeBombPosition[positionStringX2].GetComponent<BombController>().CancelPreviousDetonationAndDoExplosion();
                    stopX2 = true;
                }
            }


            //Z+1
            if (!stopZ1)
            {
                GameObject explosionZ1 = GetExplosionGameObject();
                explosionTrails.Add(explosionZ1);
                explosionZ1.transform.position = new Vector3(_bombOrigin.x, explosionZ1.transform.position.y, _bombOrigin.z + i);
                string positionStringZ1 = "X" + explosionZ1.transform.position.x + "Z" + explosionZ1.transform.position.z;
                if (destroyableWallsPosition.ContainsKey(positionStringZ1) || nonDestroyableWallsPosition.ContainsKey(positionStringZ1)
                    || outerWallsPosition.ContainsKey(positionStringZ1) || activeBombPosition.ContainsKey(positionStringZ1))
                {
                    explosionZ1.SetActive(false);
                    if (destroyableWallsPosition.ContainsKey(positionStringZ1)) DestroyWalls(destroyableWallsPosition[positionStringZ1]);
                    if (activeBombPosition.ContainsKey(positionStringZ1)) 
                        activeBombPosition[positionStringZ1].GetComponent<BombController>().CancelPreviousDetonationAndDoExplosion();
                    stopZ1 = true;
                }
            }


            //Z-1
            if (!stopZ2)
            {
                GameObject explosionZ2 = GetExplosionGameObject();
                explosionTrails.Add(explosionZ2);
                explosionZ2.transform.position = new Vector3(_bombOrigin.x, explosionZ2.transform.position.y, _bombOrigin.z - i);
                string positionStringZ2 = "X" + explosionZ2.transform.position.x + "Z" + explosionZ2.transform.position.z;
                if (destroyableWallsPosition.ContainsKey(positionStringZ2) || nonDestroyableWallsPosition.ContainsKey(positionStringZ2)
                    || outerWallsPosition.ContainsKey(positionStringZ2) || activeBombPosition.ContainsKey(positionStringZ2))
                {
                    explosionZ2.SetActive(false);
                    if (destroyableWallsPosition.ContainsKey(positionStringZ2)) DestroyWalls(destroyableWallsPosition[positionStringZ2]);
                    if (activeBombPosition.ContainsKey(positionStringZ2)) 
                        activeBombPosition[positionStringZ2].GetComponent<BombController>().CancelPreviousDetonationAndDoExplosion();
                    stopZ2 = true;
                }
            }

            yield return new WaitForSeconds(0.05f);
        }

        yield return explosionRange;
    }

    private void DestroyWalls(GameObject _wallToDestroy)
    {
        if (_wallToDestroy == null) return;

        //Check if we can spawn jewel at this location
        gameController.jewelSpawner.CheckForJewelSpawn(new Vector2(_wallToDestroy.transform.position.x, 
            _wallToDestroy.transform.position.z));

        GameObject _wallDestroyEffect = Instantiate(gameController.wallDestroyPrefab, gameController.explosionHolder.transform);
        _wallDestroyEffect.transform.position = _wallToDestroy.transform.position;
        Destroy(_wallToDestroy);
    }

    private void DestroyTrails()
    {
        foreach (GameObject _obj in explosionTrails) { Destroy(_obj); }
    }

    public void PushBomb(Vector3 _direction)
    {
        GetComponent<Rigidbody>().isKinematic = false;
        shouldPush = true;
        pushToPos = new Vector3(transform.position.x + (_direction.x * 2),
            transform.position.y, transform.position.z + (_direction.z * 2));

        if(trailEffect) trailEffect.Play();
    }

    private void StopPushingBomb()
    {
        shouldPush = false;
        transform.position = new Vector3(Mathf.Floor(transform.position.x) + 0.5f,
            0, Mathf.Floor(transform.position.z) + 0.5f);
        GetComponent<Rigidbody>().isKinematic = true;
        if(trailEffect) trailEffect.Stop();
    }
}
