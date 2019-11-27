using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : MonoBehaviour
{
    public Rigidbody playerRigidBody;
    public float speed = 1f;
    public bool isDown = false;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            ButtonClickHandler(true);
        }

        if (isDown)
        {
            playerRigidBody.velocity = new Vector3(CrossPlatformInputManager.GetAxis("Horizontal") * speed + Input.GetAxis("Horizontal") * speed, 0,
            CrossPlatformInputManager.GetAxis("Vertical") * speed + Input.GetAxis("Vertical") * speed);
        }
        else
        {
            playerRigidBody.velocity = Vector3.zero;
        }
        
    }

    public void ButtonClickHandler(bool _isDown)
    {
        isDown = _isDown;
    }
}
