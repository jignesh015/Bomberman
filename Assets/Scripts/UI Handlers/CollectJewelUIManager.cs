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
        switch (_jewel.jewelSides)
        {
            case 4:
                Instantiate(fourSidesUIPrefab, jewelHolder);
                break;
            case 5:
                Instantiate(fiveSidesUIPrefab, jewelHolder);
                break;
            case 6:
                Instantiate(sixSidesUIPrefab, jewelHolder);
                break;
            case 7:
                Instantiate(sevenSidesUIPrefab, jewelHolder);
                break;
            case 8:
                Instantiate(eightSidesUIPrefab, jewelHolder);
                break;
            case 9:
                Instantiate(nineSidesUIPrefab, jewelHolder);
                break;
            default:
                Instantiate(fourSidesUIPrefab, jewelHolder);
                break;
        }

        //Assign Info
        colorInfoText.text = "Color: " + _jewel.jewelColor.ToString();
        sidesInfoText.text = "Sides: " + _jewel.jewelSides.ToString();

        //Assign Camera
        Canvas _canvas = GetComponent<Canvas>();
        _canvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _canvas.planeDistance = 5;
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