using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2 : MonoBehaviour
{
    public bool godMode;
    public CharacterController player;
    public Animator animator;
    public Renderer[] mainRenderers;

    public float speed = 6f, turnSmoothTime = 0.1f, timeToDissolve;
    public bool isDead;

    Vector3 direction;
    float dissolveValue, turnSmoothVelocity;
    List<Material> playerMats;

    // Start is called before the first frame update
    void Start()
    {
        isDead = false;

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
            }
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

    }

    public void KillPlayer()
    {
        if (isDead || godMode) return;

        isDead = true;
        GameController.Instance.gameOver = true;
        animator.SetTrigger("IsDead");
        //animator.SetFloat("Speed", 0);
    }
}
