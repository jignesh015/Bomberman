using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCompleteUI : UIManager
{
    [Header("Button References")]
    [SerializeField] private Button homeBtn;

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
            homeBtn.Select();
            OnHomePressed();
        }
    }

    public void OnHomePressed()
    {
        if (inputProvided) return;
        inputProvided = true;

        MultiSceneManager.Instance.LoadHomeScene();
        Close();
    }
}
