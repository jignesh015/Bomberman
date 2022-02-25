using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JewelCollectionController : MonoBehaviour
{
    public int jewelsCollectedCount;
    public int jewelScore;

    public int fakeJewelPenalty = -200;
    public int diamondScore = 100;
    public int rubyScore = 200;
    public int emeraldScore = 300;
    public int sapphireScore = 500;

    public List<Jewel> jewelsCollectedList;

    public Animator jewelScoreAnimator;
    public Text jewelScoreAnimatorText;
    public Color addScoreColor;
    public Color subtractScoreColor;

    private int jewelsFound;

    // Start is called before the first frame update
    void Start()
    {
        jewelsCollectedList = new List<Jewel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionWithJewel(Jewel _jewel)
    {
        jewelsFound++;
        _jewel.gameObject.SetActive(false);
        SFXManager.Instance.PlayAudio(SFXManager.Instance.uiInteractionAudioSource,
            SFXManager.Instance.jewelFound);
        MultiSceneManager.Instance.OpenCanvas<CollectJewelUIManager>("UI/CollectJewelUI", popup => {
            GameController.Instance.isPopupOpen = true;
            popup.OnPopupOpen(_jewel, OnJewelCollected, OnJewelDiscarded);
        });
    }

    public void OnJewelCollected(Jewel _jewel)
    {
        jewelsCollectedList.Add(_jewel);
        CalculateScore(_jewel);

        GameController.Instance.DisplayScore(jewelScore, jewelsFound);
        GameController.Instance.CheckForLevelComplete(jewelScore, jewelsFound, jewelsCollectedList);

        GameController.Instance.isPopupOpen = false;
    }

    public void OnJewelDiscarded(Jewel _jewel)
    {
        GameController.Instance.isPopupOpen = false;
        GameController.Instance.DisplayScore(jewelScore, jewelsFound);
        GameController.Instance.CheckForLevelComplete(jewelScore, jewelsFound, jewelsCollectedList);

        //if (_jewel == null || _jewel.gameObject == null) return;
        //Destroy(_jewel.gameObject);
    }

    public bool HasEnoughScore()
    {
        return jewelScore > 0;
    }

    private void CalculateScore(Jewel _jewel)
    {
        if (_jewel.isReal)
        {
            int _sides = _jewel.jewelSides;
            int _scoreToAdd = 0;
            JewelColor _color = _jewel.jewelColor;
            if ((_sides == 4 || _sides == 5)
                && (_color == JewelColor.White || _color == JewelColor.Blue))
            {
                //DIAMOND
                _scoreToAdd = diamondScore;
            }
            else if ((_sides == 5 || _sides == 7) 
                && (_color == JewelColor.Red || _color == JewelColor.Orange))
            {
                //RUBY
                _scoreToAdd = rubyScore;
            }
            else if ((_sides == 7 || _sides == 9)
                && (_color == JewelColor.Green))
            {
                //EMERALD
                _scoreToAdd = emeraldScore;
            }
            else if ((_sides == 6 || _sides == 8)
                && (_color == JewelColor.Blue || _color == JewelColor.Purple))
            {
                //SAPPHIRE
                _scoreToAdd = sapphireScore;
            }
            jewelScore += _scoreToAdd;

            //Show Jewel animation
            ShowJewelScoreAnimation(_scoreToAdd);
        }
        else
        {
            jewelScore += fakeJewelPenalty;
            //Show Jewel animation
            ShowJewelScoreAnimation(fakeJewelPenalty);
        }
    }

    private void ShowJewelScoreAnimation(int _scoreToDisplay)
    {
        StartCoroutine(ShowJewelScoreAnimationAsync(_scoreToDisplay));
    }

    private IEnumerator ShowJewelScoreAnimationAsync(int _scoreToDisplay)
    {
        yield return new WaitForSeconds(0.3f);
        bool isPositive = _scoreToDisplay > 0;
        jewelScoreAnimatorText.text = string.Format("{0}{1}", (isPositive ? "+" : ""), _scoreToDisplay);
        jewelScoreAnimatorText.color = isPositive ? addScoreColor : subtractScoreColor;
        jewelScoreAnimator.SetTrigger("Play");
    }
}
