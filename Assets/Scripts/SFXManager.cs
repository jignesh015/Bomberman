using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip playerWalk;
    public AudioClip bombExplosion, bombPush, enemyDeath;
    public AudioClip bombPlaced, bombError;
    public AudioClip uiClick, uiHover;
    public AudioClip jewelFound;
    public AudioClip gameWin, gameOver;

    [Header("Audio Sources")]
    public AudioSource uiInteractionAudioSource;
    private AudioSource backgroundMusicSource;
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
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        gameController = GameController.Instance;
        backgroundMusicSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    public void PlayAudio(AudioSource _source, AudioClip _clip, bool _shouldLoop = false, float _volume = 0)
    {
        if (_source == null)
        {
            Debug.LogFormat("<color=red>Audio Source missing</color>");
            return;
        }

        _source.Stop();
        _source.clip = _clip;
        _source.loop = _shouldLoop;
        if (_volume != 0) _source.volume = _volume;
        _source.Play();
    }

    public void StopAudio(AudioSource _source)
    {
        _source.Stop();
    }

    public void ToggleBackgroundMusic(bool _shouldPlay)
    {
        if (_shouldPlay)
        {
            PlayAudio(backgroundMusicSource, backgroundMusic, true);
        }
        else
            StopAudio(backgroundMusicSource);
    }

    public void PlayerWalkingSFX(AudioSource _playerAudioSource)
    {
        if (_playerAudioSource == null)
        {
            Debug.LogFormat("<color=red>Audio Source missing</color>");
            return;
        }

        if (!_playerAudioSource.isPlaying)
        {
            _playerAudioSource.pitch = Random.Range(2f, 2.2f);
            PlayAudio(_playerAudioSource, playerWalk);
        }
    }

    public void PlayButtonClickSFX()
    {
        PlayAudio(uiInteractionAudioSource, uiClick);
    }

    public void PlayButtonHoverSFX()
    {
        PlayAudio(uiInteractionAudioSource, uiHover);
    }
}
