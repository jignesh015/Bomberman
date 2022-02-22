using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewelSpawnerController : MonoBehaviour
{
    [Header("Jewel Prefabs")]
    [SerializeField] private GameObject fourSidesPrefab;
    [SerializeField] private GameObject fiveSidesPrefab;
    [SerializeField] private GameObject sixSidesPrefab;
    [SerializeField] private GameObject sevenSidesPrefab;
    [SerializeField] private GameObject eightSidesPrefab;
    [SerializeField] private GameObject nineSidesPrefab;

    [Header("Jewel Materials")]
    [SerializeField] private Material whiteMat;
    [SerializeField] private Material blueMat;
    [SerializeField] private Material redMat;
    [SerializeField] private Material orangeMat;
    [SerializeField] private Material greenMat;
    [SerializeField] private Material purpleMat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckForJewelSpawn(DestroyableWall _destroyedWall)
    {
        if (_destroyedWall.spawnJewelHere)
        {
            JewelClass _jewelToSpawn = _destroyedWall.jewelToSpawn;
            //Instantiate Jewel
            Jewel _spawnedJewel = InstantiateJewel(_jewelToSpawn.jewelSides);
            //Assign position
            _spawnedJewel.transform.position = _destroyedWall.transform.position;
            //Assign parent
            _spawnedJewel.transform.parent = transform;
            //Assign material
            AssignJewelMaterial(_spawnedJewel, _jewelToSpawn.jewelColor);
            //Assign other parameters
            _spawnedJewel.jewelColor = _jewelToSpawn.jewelColor;
            _spawnedJewel.isReal = _jewelToSpawn.isReal;
        }
    }

    private Jewel InstantiateJewel(int _sides)
    {
        Jewel _spawnedJewel;
        switch (_sides)
        {
            case 4:
                _spawnedJewel = Instantiate(fourSidesPrefab).GetComponent<Jewel>();
                break;
            case 5:
                _spawnedJewel = Instantiate(fiveSidesPrefab).GetComponent<Jewel>();
                break;
            case 6:
                _spawnedJewel = Instantiate(sixSidesPrefab).GetComponent<Jewel>();
                break;
            case 7:
                _spawnedJewel = Instantiate(sevenSidesPrefab).GetComponent<Jewel>();
                break;
            case 8:
                _spawnedJewel = Instantiate(eightSidesPrefab).GetComponent<Jewel>();
                break;
            case 9:
                _spawnedJewel = Instantiate(nineSidesPrefab).GetComponent<Jewel>();
                break;
            default:
                _spawnedJewel = Instantiate(fourSidesPrefab).GetComponent<Jewel>();
                break;
        }
        _spawnedJewel.jewelSides = _sides;
        return _spawnedJewel;
    }

    private void AssignJewelMaterial(Jewel _spawnedJewel, JewelColor _color)
    {
        MeshRenderer _renderer = _spawnedJewel.GetComponentInChildren<MeshRenderer>();
        if (_renderer == null) return;
        switch (_color)
        {
            case JewelColor.White:
                _renderer.material = whiteMat;
                break;
            case JewelColor.Blue:
                _renderer.material = blueMat;
                break;
            case JewelColor.Red:
                _renderer.material = redMat;
                break;
            case JewelColor.Orange:
                _renderer.material = orangeMat;
                break;
            case JewelColor.Green:
                _renderer.material = greenMat;
                break;
            case JewelColor.Purple:
                _renderer.material = purpleMat;
                break;
            default:
                _renderer.material = whiteMat;
                break;
        }
    }
}

public enum JewelType
{
    Fake, Diamond, Ruby, Emerald, Sapphire
}

public enum JewelColor
{
    White, Blue, Red, Orange, Green, Purple
}

[Serializable]
public class JewelClass
{
    public bool isReal;
    public int jewelSides = 4;
    public JewelColor jewelColor;
}