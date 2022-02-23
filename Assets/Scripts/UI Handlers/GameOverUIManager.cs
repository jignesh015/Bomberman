using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUIManager : UIManager
{
    [SerializeField] private Text reasonText;

    [SerializeField] private Button replayBtn;
    [SerializeField] private Button quitBtn;

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
            replayBtn.Select();
            OnReplayPressed();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            quitBtn.Select();
            OnQuitPressed();
        }
    }

    public void OnPopupOpen(GameOverReason _reason)
    {
        switch (_reason)
        {
            case GameOverReason.Dead:
                reasonText.text = "You Died!";
                break;
            case GameOverReason.Time:
                reasonText.text = "You ran out of time!";
                break;
            case GameOverReason.Score:
                reasonText.text = "You didn't collect enough real jewels!";
                break;
        }
    }

    public void OnReplayPressed()
    {
        if (inputProvided) return;
        inputProvided = true;

        MultiSceneManager.Instance.StartGame(GameController.Instance.level.levelNumber);
        Close();
    }

    public void OnQuitPressed()
    {
        if (inputProvided) return;
        inputProvided = true;

        MultiSceneManager.Instance.LoadHomeScene();
        Close();
    }
}