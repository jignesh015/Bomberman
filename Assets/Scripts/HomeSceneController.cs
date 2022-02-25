using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        //Start background SFX
        SFXManager.Instance.ToggleBackgroundMusic(true);

        //Show Continue button if completed a level before and hasn't completed the game
        buttons[0].SetActive(PlayerPrefs.HasKey("Level_Completed") && !PlayerPrefs.HasKey("Game_Completed"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnContinueGamePressed()
    {
        int _levelNo = PlayerPrefs.GetInt("Level_Completed");
        MultiSceneManager.Instance.StartGame(_levelNo + 1);
    }

    public void OnNewGamePressed()
    {
        PlayerPrefs.DeleteKey("Game_Completed");
        PlayerPrefs.DeleteKey("Level_Completed");
        MultiSceneManager.Instance.StartGame(1);
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

    public void PlayButtonClickSFX()
    {
        SFXManager.Instance.PlayButtonClickSFX();
    }

    public void PlayButtonHoverSFX()
    {
        SFXManager.Instance.PlayButtonHoverSFX();
    }
}
