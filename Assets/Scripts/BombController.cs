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

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (!hasDetonator)
            Explode(explodeTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldTween)
        {
            shouldTween = false;
            transform.DOPunchScale(new Vector3(-0.1f, -0.1f, -0.1f), 0.3f, 1, 0.2f).OnComplete(() => { shouldTween = true; });
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
        Destroy(gameObject, _explodeTime);

        GameController.Instance.Explode(transform.position, _explodeTime);
    }

}
