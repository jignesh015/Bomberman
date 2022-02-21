using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeSceneController : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private List<GameObject> buttons;
    [SerializeField] private GameObject controlPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject creditsPanel;


    // Start is called before the first frame update
    void Start()
    {
        //Show Continue button if completed few levels before
        buttons[0].SetActive(PlayerPrefs.HasKey("Level_Completed"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnContinueGamePressed()
    { 
    
    }

    public void OnNewGamePressed()
    {
        int _levelNo = PlayerPrefs.GetInt("Level_Completed");
        if (_levelNo == 0) _levelNo = 1;
        MultiSceneManager.Instance.StartGame(_levelNo);
    }

    public void OnHowToPlayPressed()
    {
        CloseAllCenterPanels();
        howToPlayPanel.SetActive(true);
    }

    public void OnControlsPressed()
    {
        CloseAllCenterPanels();
        controlPanel.SetActive(true);
    }

    public void OnCreditsPressed()
    {
        CloseAllCenterPanels();
        creditsPanel.SetActive(true);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }

    private void CloseAllCenterPanels()
    {
        controlPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }
}
