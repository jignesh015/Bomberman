using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip playerWalk;
    public AudioClip bombExplosion, bombPush, enemyDeath;

    private AudioSource audioSource;
    private GameController gameController;

    private static SFXManager _instance;
    public static SFXManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        gameController = GameController.Instance;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    public void PlaySFX(AudioSource _source, AudioClip _clip)
    {
        _source.Stop();
        _source.clip = _clip;
        _source.Play();
    }

    public void PlayerWalkingSFX()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.pitch = Random.Range(2f, 2.2f);
            PlaySFX(audioSource, playerWalk);
        }
    }
}
