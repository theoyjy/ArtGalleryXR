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

    public bool Speaking;

    public bool isMuted;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _cam = Camera.main;
        Speaking = false;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        loudSpeakerMute.gameObject.SetActive(isMuted);
        loudSpeakerLoud.gameObject.SetActive(!isMuted);
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Speaking = !Speaking;
            isMuted = !isMuted;
        }

        if (_cam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        }

    }

    public void isSpeaking(bool isSpeaking)
    {
        Speaking = isSpeaking; 
    }
    public void SetMuteState(bool muteState)
    {
        isMuted = muteState;
        loudSpeakerMute.gameObject.SetActive(isMuted);
        loudSpeakerLoud.gameObject.SetActive(!isMuted);
    }
}
