using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombsAvailableDisplayController : MonoBehaviour
{
    [Header("Bomb images")]
    [SerializeField] private List<Image> bombImages;

    [Header("Sprites")]
    [SerializeField] private Sprite bombAvailableSprite;
    [SerializeField] private Sprite bombUnAvailableSprite;

    private Animator alertAnimator;

    // Start is called before the first frame update
    void Start()
    {
        alertAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateBombAvailableDisplay(int _liveBombCount, int _maxBombCount)
    {
        int _bombsAvailable = _maxBombCount - _liveBombCount;
        for (int i = 0; i < bombImages.Count; i++)
        {
            //bombCountHolder.GetChild(i).gameObject.SetActive(i < _bombsAvailable);
            bombImages[i].gameObject.SetActive(i < _maxBombCount);
            bombImages[i].sprite = (i < _bombsAvailable) ? bombAvailableSprite : bombUnAvailableSprite;
        }
    }

    public void AlertUser()
    {
        alertAnimator.SetTrigger("Play");
    }
}
