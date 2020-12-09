﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    private Transform targetTransform;
    private Transform xform;
    private Vector3 offset = new Vector3(0f,1f,-3f);
    private Camera cam;

    private void FollowTarget()
    {
        xform.position = targetTransform.position + offset;
    }

    private void LoadGameCamera(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game") { return; }
        xform = transform;
           
        targetTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        cam = Camera.main;
        cam.clearFlags = CameraClearFlags.Skybox;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += LoadGameCamera;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= LoadGameCamera;
    }


    private void Update()
    {
        if (targetTransform == null) {
            Debug.Log("1");
            return; }
        FollowTarget();
    }
}
