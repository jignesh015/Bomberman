using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2 : MonoBehaviour
{
    [HideInInspector]
    public bool indestructible = false, ghostMode = false;
    public CharacterController player;
    public Animator animator;
    public Renderer[] mainRenderers;
    public ParticleSystem trailEffect;

    public float speed = 6f, turnSmoothTime = 0.1f, timeToDissolve;
    public bool isDead;

    [Header("FOR DEBUGGING PURPOSE ONLY")]
    public bool godMode;
    public bool enableGhostMode;

    Vector3 direction;
    float dissolveValue, turnSmoothVelocity, startYPos;
    List<Material> playerMats;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        isDead = false;
        gameController = GameController.Instance;

        playerMats = new List<Material>();
        for (int i = 0; i < mainRenderers.Length; i++)
        {
            foreach (Material _mat in mainRenderers[i].sharedMaterials)
            {
                _mat.SetFloat("DissolveAmt", 0);
                playerMats.Add(_mat);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (startYPos == 0) startYPos = transform.position.y;
        //trailEffect.gameObject.SetActive(false);

        if (!isDead)
        {
            direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

            animator.SetFloat("Speed", direction.magnitude);
            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                player.Move(direction * speed * Time.deltaTime);
                if (startYPos != 0) player.transform.position = new Vector3(player.transform.position.x, startYPos, player.transform.position.z);

                //Play trail effect
                trailEffect.Play();
            }
            else trailEffect.Stop();
        }

        if (isDead && dissolveValue < 2f)
        {
            dissolveValue += timeToDissolve * Time.deltaTime;
            foreach (Material _mat in playerMats)
            {
                _mat.SetFloat("DissolveAmt", dissolveValue);
            }
            return;
        }

        if (isDead && dissolveValue > 2)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            foreach (Material _mat in playerMats)
            {
                _mat.SetFloat("DissolveAmt", 0);
            }
        }

        //For debugging purpose
        if (enableGhostMode && !ghostMode) gameController.TogglePlayerGhostMode(true);

    }

    public void KillPlayer()
    {
        if (isDead || indestructible || godMode) return;

        isDead = true;
        GameController.Instance.gameOver = true;
        animator.SetTrigger("IsDead");
        //animator.SetFloat("Speed", 0);
    }
}
