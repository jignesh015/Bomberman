using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableWall : MonoBehaviour
{
    public bool spawnJewelHere;
    public JewelClass jewelToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        if (jewelToSpawn.jewelSides == 0)
            jewelToSpawn.jewelSides = 4;
    }
}
