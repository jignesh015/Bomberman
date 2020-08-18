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
    private bool isRigid = false;
    private List<GameObject> explosionTrails;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        explosionTrails = new List<GameObject>();
        if (!hasDetonator)
            Explode(explodeTime);
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
            }
        }
    }

    public void Explode()
    {
        Destroy(gameObject);
    }

    public void Explode(float _explodeTime)
    {
        //Start bomb explosion at bomb origin after explodeTime
        StartCoroutine(DoExplosion(transform.position, _explodeTime));
    }

    public IEnumerator DoExplosion(Vector3 _bombOrigin, float _explodeTime)
    {
        yield return new WaitForSeconds(_explodeTime);

        //Hide bomb 
        GetComponent<BoxCollider>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
        Invoke("DestroyTrails", GameController.Instance.explosionDuration);
        Destroy(gameObject, GameController.Instance.explosionDuration);

        GameObject explosion = GetExplosionGameObject();
        explosionTrails.Add(explosion);
        explosion.transform.position = new Vector3(_bombOrigin.x, explosion.transform.position.y, _bombOrigin.z);

        //Do trail explosion
        StartCoroutine(DoTrailExplosion(_bombOrigin));

        //Destroy walls near explosion
    }

    private GameObject GetExplosionGameObject()
    {
        return Instantiate(GameController.Instance.explosionPrefab, GameController.Instance.explosionHolder.transform);
    }

    //private IEnumerator DoTrailExplosion(Vector3 _bombOrigin)
    //{
    //    Dictionary<string, GameObject> destroyableWallsPosition = GameController.Instance.GetDestroyableWallsPosition();

    //    int explosionRange = GameController.Instance.explosionRange;
    //    for (int i = 1; i <= explosionRange; i++)
    //    {
    //        //X+1
    //        GameObject explosionX1 = GetExplosionGameObject();
    //        explosionX1.transform.position = new Vector3(_bombOrigin.x + i, explosionX1.transform.position.y, _bombOrigin.z);
    //        string positionStringX1 = "X" + explosionX1.transform.position.x + "Z" + explosionX1.transform.position.z;
    //        if (destroyableWallsPosition.ContainsKey(positionStringX1))
    //        {
    //            Destroy(destroyableWallsPosition[positionStringX1]);
    //        }

    //        //X-1
    //        GameObject explosionX2 = GetExplosionGameObject();
    //        explosionX2.transform.position = new Vector3(_bombOrigin.x - i, explosionX2.transform.position.y, _bombOrigin.z);
    //        string positionStringX2 = "X" + explosionX2.transform.position.x + "Z" + explosionX2.transform.position.z;
    //        if (destroyableWallsPosition.ContainsKey(positionStringX2))
    //        {
    //            Destroy(destroyableWallsPosition[positionStringX2]);
    //        }

    //        //Z+1
    //        GameObject explosionZ1 = GetExplosionGameObject();
    //        explosionZ1.transform.position = new Vector3(_bombOrigin.x, explosionZ1.transform.position.y, _bombOrigin.z + i);
    //        string positionStringZ1 = "X" + explosionZ1.transform.position.x + "Z" + explosionZ1.transform.position.z;
    //        if (destroyableWallsPosition.ContainsKey(positionStringZ1))
    //        {
    //            Destroy(destroyableWallsPosition[positionStringZ1]);
    //        }

    //        //Z-1
    //        GameObject explosionZ2 = GetExplosionGameObject();
    //        explosionZ2.transform.position = new Vector3(_bombOrigin.x, explosionZ2.transform.position.y, _bombOrigin.z - i);
    //        string positionStringZ2 = "X" + explosionZ2.transform.position.x + "Z" + explosionZ2.transform.position.z;
    //        if (destroyableWallsPosition.ContainsKey(positionStringZ2))
    //        {
    //            Destroy(destroyableWallsPosition[positionStringZ2]);
    //        }
    //    }

    //    yield return explosionRange;
    //}

    private IEnumerator DoTrailExplosion(Vector3 _bombOrigin)
    {
        Dictionary<string, GameObject> destroyableWallsPosition = GameController.Instance.GetDestroyableWallsPosition();
        Dictionary<string, GameObject> nonDestroyableWallsPosition = GameController.Instance.nonDestroyableWallsPositionDict;
        Dictionary<string, GameObject> outerWallsPosition = GameController.Instance.outerWallsPositionDict;
        bool stopX1 = false, stopX2 = false, stopZ1 = false, stopZ2 = false;

        int explosionRange = GameController.Instance.explosionRange;
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
                    || outerWallsPosition.ContainsKey(positionStringX1))
                {
                    explosionX1.SetActive(false);
                    if(destroyableWallsPosition.ContainsKey(positionStringX1)) Destroy(destroyableWallsPosition[positionStringX1]);
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
                    || outerWallsPosition.ContainsKey(positionStringX2))
                {
                    explosionX2.SetActive(false);
                    if (destroyableWallsPosition.ContainsKey(positionStringX2)) Destroy(destroyableWallsPosition[positionStringX2]);
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
                    || outerWallsPosition.ContainsKey(positionStringZ1))
                {
                    explosionZ1.SetActive(false);
                    if (destroyableWallsPosition.ContainsKey(positionStringZ1)) Destroy(destroyableWallsPosition[positionStringZ1]);
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
                    || outerWallsPosition.ContainsKey(positionStringZ2))
                {
                    explosionZ2.SetActive(false);
                    if (destroyableWallsPosition.ContainsKey(positionStringZ2)) Destroy(destroyableWallsPosition[positionStringZ2]);
                    stopZ2 = true;
                }
            }

            yield return new WaitForSeconds(0.05f);
        }

        yield return explosionRange;
    }

    private void DestroyTrails()
    {
        foreach (GameObject _obj in explosionTrails) { Destroy(_obj); }
    }
}
