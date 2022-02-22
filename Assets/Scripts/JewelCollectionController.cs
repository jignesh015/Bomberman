using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewelCollectionController : MonoBehaviour
{
    public int jewelsCollectedCount;
    public int jewelScore;

    public int fakeJewelPenalty = -100;
    public int diamondScore = 100;
    public int rubyScore = 200;
    public int emeraldScore = 300;
    public int sapphireScore = 500;


    public List<Jewel> jewelsList;

    // Start is called before the first frame update
    void Start()
    {
        jewelsList = new List<Jewel>();
        GameController.Instance.DisplayScore(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionWithJewel(Jewel _jewel)
    {
        _jewel.gameObject.SetActive(false);
        MultiSceneManager.Instance.OpenCanvas<CollectJewelUIManager>("UI/CollectJewelUI", popup => {
            GameController.Instance.isPopupOpen = true;
            popup.OnPopupOpen(_jewel, OnJewelCollected, OnJewelDiscarded);
        });
    }

    public void OnJewelCollected(Jewel _jewel)
    {
        jewelsList.Add(_jewel);
        CalculateScore(_jewel);

        GameController.Instance.DisplayScore(jewelScore, jewelsList.Count);
        GameController.Instance.isPopupOpen = false;
    }

    public void OnJewelDiscarded(Jewel _jewel)
    {
        GameController.Instance.isPopupOpen = false;
        if (_jewel == null || _jewel.gameObject == null) return;
        Destroy(_jewel.gameObject);
    }

    private void CalculateScore(Jewel _jewel)
    {
        if (_jewel.isReal)
        {
            int _sides = _jewel.jewelSides;
            JewelColor _color = _jewel.jewelColor;
            if ((_sides == 4 || _sides == 5)
                && (_color == JewelColor.White || _color == JewelColor.Blue))
            {
                //DIAMOND
                jewelScore += diamondScore;
            }
            else if ((_sides == 5 || _sides == 7) 
                && (_color == JewelColor.Red || _color == JewelColor.Orange))
            {
                //RUBY
                jewelScore += rubyScore;
            }
            else if ((_sides == 7 || _sides == 9)
                && (_color == JewelColor.Green))
            {
                //EMERALD
                jewelScore += emeraldScore;
            }
            else if ((_sides == 6 || _sides == 8)
                && (_color == JewelColor.Blue || _color == JewelColor.Purple))
            {
                //SAPPHIRE
                jewelScore += sapphireScore;
            }
        }
        else
        {
            jewelScore += fakeJewelPenalty; 
        }
    }
}
