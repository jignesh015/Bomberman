using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpPopupUIManager : UIManager
{
    [SerializeField] private Button okBtn;

    // Start is called before the first frame update
    void Start()
    {
        //Assign Camera
        AssignUICamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            okBtn.Select();
            Close();
        }
    }

    public void OnPopupClose()
    {
        if (GameController.Instance != null)
            GameController.Instance.isPopupOpen = false;
    }
}
