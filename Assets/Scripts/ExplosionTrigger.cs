using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
            other.GetComponent<EnemyController>().KillEnemy();

        if (other.gameObject.CompareTag("Player"))
            other.GetComponent<PlayerController2>().KillPlayer();
    }
}
