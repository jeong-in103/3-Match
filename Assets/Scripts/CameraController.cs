using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 디바이스별 해상도 대응
public class CameraCntroller : MonoBehaviour
{
    Camera targetCamera;
    
    [SerializeField] float boardUnit = 4.6f;
    [SerializeField] private GameObject bg1;
    [SerializeField] private GameObject bg2;
    [SerializeField] private GameObject bg3;

    private void Awake()
    {
        targetCamera = Camera.main;
    }

    private void Start()
    {
        float originSize = targetCamera.orthographicSize;
        //넓이가 boardUnit을 출력할 수 있도록 카메라 size 계산
        targetCamera.orthographicSize = boardUnit / targetCamera.aspect;

        float ratio = targetCamera.orthographicSize / originSize;
        bg1.transform.localScale *= ratio;
        bg2.transform.localScale *= ratio;
        bg3.transform.localScale *= ratio;
    }
}