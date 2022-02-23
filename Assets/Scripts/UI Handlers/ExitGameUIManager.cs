using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ExitGameUIManager : UIManager
{
    [SerializeField] private Button noBtn;
    [SerializeField] private Button yesBtn;

    private bool inputProvided;

    // Start is called before the first frame update
    void Start()
    {
        //Assign Camera
        AssignUICamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            noBtn.Select();
            OnNoPressed();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            yesBtn.Select();
            OnYesPressed();
        }
    }

    public void OnNoPressed()
    {
        if (inputProvided) return;
        inputProvided = true;

        Close();
    }

    public void OnYesPressed()
    {
        if (inputProvided) return;
        inputProvided = true;

        //Open Home Scene
        MultiSceneManager.Instance.LoadHomeScene();

        Close();
    }

    public void OnPopupClose()
    {
        if(GameController.Instance != null)
            GameController.Instance.isPopupOpen = false;
    }
}
