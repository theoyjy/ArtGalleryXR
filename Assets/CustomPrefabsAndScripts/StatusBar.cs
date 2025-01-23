using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class StatusBar : MonoBehaviour
{

    private Camera _cam;

    [SerializeField] private Image loudSpearker;
    [SerializeField] private GameObject test;

    private bool Speaking;

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
        loudSpearker.gameObject.SetActive(Speaking);
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Speaking = !Speaking;
        }

        if(Input.GetKeyDown(KeyCode.M))
        {
            //Debug.Log(_cam.transform.position);
            //Debug.Log(test.transform.rotation);
            Debug.Log(transform.eulerAngles);
        }

    }

    public void isSpeaking(bool isSpeaking)
    {
        Speaking = isSpeaking; 
    }
}
