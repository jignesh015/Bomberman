using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectJewelUIManager : UIManager
{
    [Header("Prefabs")]
    [SerializeField] private GameObject fourSidesUIPrefab;
    [SerializeField] private GameObject fiveSidesUIPrefab;
    [SerializeField] private GameObject sixSidesUIPrefab;
    [SerializeField] private GameObject sevenSidesUIPrefab;
    [SerializeField] private GameObject eightSidesUIPrefab;
    [SerializeField] private GameObject nineSidesUIPrefab;

    [Header("Jewel Materials")]
    [SerializeField] private Material whiteMat;
    [SerializeField] private Material blueMat;
    [SerializeField] private Material redMat;
    [SerializeField] private Material orangeMat;
    [SerializeField] private Material greenMat;
    [SerializeField] private Material purpleMat;

    [SerializeField] private Transform jewelHolder;

    [SerializeField] private Text colorInfoText;
    [SerializeField] private Text sidesInfoText;

    [SerializeField] private Button collectBtn;
    [SerializeField] private Button discardBtn;

    private bool inputProvided;

    private Jewel jewel;
    private Action<Jewel> OnCollectedCallback;
    private Action<Jewel> OnDiscardedCallback;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            collectBtn.Select();
            OnCollectPressed();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            discardBtn.Select();
            OnDiscardPressed();
        }
    }

    public void OnPopupOpen(Jewel _jewel, Action<Jewel> _onCollectedCallback, Action<Jewel> _onDiscardedCallback)
    {
        jewel = _jewel;
        OnCollectedCallback = _onCollectedCallback;
        OnDiscardedCallback = _onDiscardedCallback;

        //Instantiate jewel ui prefab
        GameObject _spawnedJewel;
        switch (_jewel.jewelSides)
        {
            case 4:
                _spawnedJewel = Instantiate(fourSidesUIPrefab, jewelHolder);
                break;
            case 5:
                _spawnedJewel = Instantiate(fiveSidesUIPrefab, jewelHolder);
                break;
            case 6:
                _spawnedJewel = Instantiate(sixSidesUIPrefab, jewelHolder);
                break;
            case 7:
                _spawnedJewel = Instantiate(sevenSidesUIPrefab, jewelHolder);
                break;
            case 8:
                _spawnedJewel = Instantiate(eightSidesUIPrefab, jewelHolder);
                break;
            case 9:
                _spawnedJewel = Instantiate(nineSidesUIPrefab, jewelHolder);
                break;
            default:
                _spawnedJewel = Instantiate(fourSidesUIPrefab, jewelHolder);
                break;
        }

        //Assign proper material
        MeshRenderer _renderer = _spawnedJewel.GetComponentInChildren<MeshRenderer>();
        if (_renderer == null) return;
        switch (_jewel.jewelColor)
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

        //Assign Info
        colorInfoText.text = "Color: " + _jewel.jewelColor.ToString();
        sidesInfoText.text = "Sides: " + _jewel.jewelSides.ToString();

        //Assign Camera
        AssignUICamera();
    }

    public void OnCollectPressed()
    {
        if (inputProvided) return;
        inputProvided = true;

        OnCollectedCallback(jewel);
        Close();
    }

    public void OnDiscardPressed()
    {
        if (inputProvided) return;
        inputProvided = true;

        OnDiscardedCallback(jewel);
        Close();
    }
}