using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class CinemachineCameraShaker : MonoBehaviour
{
    private float shakeElapsedTime = 0f;

    // Cinemachine Shake
    public CinemachineVirtualCamera VirtualCamera;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;

    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        //Get gamecontroller reference
        gameController = GameController.Instance;

        // Get Virtual Camera Noise Profile
        if (VirtualCamera != null)
            virtualCameraNoise = VirtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    void Update()
    {
        // If the Cinemachine componet is not set, avoid update
        if (VirtualCamera != null && virtualCameraNoise != null)
        {
            // If Camera Shake effect is still playing
            if (shakeElapsedTime > 0)
            {
                // Set Cinemachine Camera Noise parameters
                virtualCameraNoise.m_AmplitudeGain = gameController.shakeAmplitude;
                virtualCameraNoise.m_FrequencyGain = gameController.shakeFrequency;

                // Update Shake Timer
                shakeElapsedTime -= Time.deltaTime;
            }
            else
            {
                // If Camera Shake effect is over, reset variables
                virtualCameraNoise.m_AmplitudeGain = 0f;
                shakeElapsedTime = 0f;
            }
        }
    }

    public void CinemachineShake()
    {
        shakeElapsedTime = gameController.shakeDuration;
    }
}
