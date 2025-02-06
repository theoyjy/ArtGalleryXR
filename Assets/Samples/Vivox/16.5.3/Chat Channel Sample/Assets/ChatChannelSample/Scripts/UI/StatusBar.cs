using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class StatusBar : MonoBehaviour
{
    private Camera _cam;
    [SerializeField] private Image loudSpeakerMute;
    [SerializeField] private Image loudSpeakerLoud;
    [SerializeField] private GameObject test;

    // This StatusBar represents the local user¡¯s mute state.
    // Remove the ¡°Speaking¡± toggle test variable because it was only used for testing.
    private bool isMuted;

    public static StatusBar Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        _cam = Camera.main;
        // Optionally, initialize the icon based on the current mute state.
        SetMuteState(isMuted);
    }

    void Update()
    {
        // *** FIX 2 ***
        // Remove test code that toggles the icon with the spacebar.
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    // This was just for testing and does not update the actual Vivox UI state.
        //    isMuted = !isMuted;
        //    SetMuteState(isMuted);
        //}

        if (_cam != null)
        {
            // Make the status bar face the camera.
            transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        }
    }

    /// <summary>
    /// Call this method (from the RosterItem or elsewhere) to update the local user¡¯s mute icon.
    /// </summary>
    public void SetMuteState(bool muteState)
    {
        isMuted = muteState;
        loudSpeakerMute.gameObject.SetActive(isMuted);
        loudSpeakerLoud.gameObject.SetActive(!isMuted);
    }
}
