using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverController : MonoBehaviour
{
    public bool isGameOver;

    private void Start()
    {
        CloseAllOtherPopups();
    }

    public void OnGameOver(GameOverReason _reason, float _delay)
    {
        StartCoroutine(OnGameOverAsync(_reason, _delay));
    }

    private IEnumerator OnGameOverAsync(GameOverReason _reason, float _delay)
    {
        isGameOver = true;

        yield return new WaitForSeconds(_delay);

        //Show game over popup
        MultiSceneManager.Instance.OpenCanvas<GameOverUIManager>("UI/GameOverUI", popup => {
            popup.OnPopupOpen(_reason);
            GameController.Instance.isPopupOpen = true;
        });
    }

    private void CloseAllOtherPopups()
    {
        if (FindObjectOfType<CollectJewelUIManager>() != null)
        {
            DestroyImmediate(FindObjectOfType<CollectJewelUIManager>().gameObject);
        }
        if (FindObjectOfType<ExitGameUIManager>() != null)
        {
            DestroyImmediate(FindObjectOfType<ExitGameUIManager>().gameObject);
        }
        if (FindObjectOfType<HelpPopupUIManager>() != null)
        {
            DestroyImmediate(FindObjectOfType<HelpPopupUIManager>().gameObject);
        }
    }
}

public enum GameOverReason
{
    Dead, Time, Score
}
