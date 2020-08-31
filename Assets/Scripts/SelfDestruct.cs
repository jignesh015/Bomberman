using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float destructAfter;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("RemoveCollisionDetection", 0.2f);
        Destroy(gameObject, destructAfter);
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.GetComponent<EnemyController>() != null &&
            Vector3.Distance(transform.position, other.transform.position) < 0.5f) 
            other.GetComponent<EnemyController>().KillEnemy();
        else if (other.GetComponent<PlayerController2>() != null &&
            Vector3.Distance(transform.position, other.transform.position) < 0.5f)
            other.GetComponent<PlayerController2>().KillPlayer();
    }

    void RemoveCollisionDetection()
    {
        ParticleSystem _ps = GetComponent<ParticleSystem>();
        ParticleSystem.CollisionModule collisionModule = _ps.collision;
        collisionModule.enabled = false;
    }
}
