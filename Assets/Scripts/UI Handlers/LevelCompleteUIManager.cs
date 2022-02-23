using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteUIManager : UIManager
{
    [Header("Text References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text realJewelsCollectedText;

    [Header("Button References")]
    [SerializeField] private Button nextLevelBtn;
    [SerializeField] private Button replayBtn;
    [SerializeField] private Button quitBtn;

    private bool inputProvided;
    private LevelManager level;

    // Start is called before the first frame update
    void Start()
    {
        //Assign Camera
        AssignUICamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            replayBtn.Select();
            OnReplayPressed();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            nextLevelBtn.Select();
            OnNextLevelPressed();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            quitBtn.Select();
            OnQuitPressed();
        }
    }

    public void OnPopupOpen(int _gameScore, List<Jewel> _jewelsCollected, LevelManager _level)
    {
        level = _level;

        //Set Score
        scoreText.text = string.Format("Score : {0}", _gameScore);

        //Set real jewels collected
        int _realJewelsCollected = _jewelsCollected.FindAll(j => j.isReal).Count;
        int _realJewelsInLevel = _level.jewelsToSpawn.FindAll(j => j.isReal).Count;
        realJewelsCollectedText.text = string.Format("Real Jewels Collected : {0}/{1}",
            _realJewelsCollected, _realJewelsInLevel);
    }

    public void OnReplayPressed()
    {
        if (inputProvided) return;
        inputProvided = true;

        MultiSceneManager.Instance.StartGame(level.levelNumber);
        Close();
    }

    public void OnNextLevelPressed()
    {
        if (inputProvided) return;
        inputProvided = true;

        MultiSceneManager.Instance.StartGame(level.levelNumber + 1);
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
