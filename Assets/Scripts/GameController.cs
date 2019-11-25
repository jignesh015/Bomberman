using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject player;
    public GameObject bombPrefab;
    public GameObject bombHolder;

    List<GameObject> activeBombs;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        activeBombs = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaceBomb()
    {
        GameObject placedBomb = Instantiate(bombPrefab, bombHolder.transform);

        Vector3 playerPos = player.transform.position;
        placedBomb.transform.position = new Vector3(Mathf.Floor(playerPos.x) + 0.5f, placedBomb.transform.position.y, Mathf.Floor(playerPos.z) + 0.5f);
        activeBombs.Add(placedBomb);
    }
}
