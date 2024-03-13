using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EAG_LookAtCamera : MonoBehaviour
{
    [SerializeField] private EAG_Manager eag_Manager;
    [SerializeField] private float speed_X;
    [SerializeField] private float speed_Y; 
    [SerializeField]float amplitude = 1.0f; //위아래 운동의 진폭 조절을 위한 변수
    
    [SerializeField] bool randomCheck = false;
    private void Start()
    {
        if (randomCheck)
        {
            speed_X = Random.Range(5f, 12f);
            speed_Y = Random.Range(1f, 10f);
        }
    }
    private void FixedUpdate()
    {
        transform.LookAt(Camera.main.transform);

        Moving();
    }

    private void Moving()
    {
        if (eag_Manager.isStop)
        {
            return;
        }
        transform.position += new Vector3(speed_X,Mathf.Sin(Time.time*speed_Y)*amplitude)*Time.deltaTime;
        if (transform.position.x>110f)
        {
            transform.position = new Vector3(-100, transform.position.y, transform.position.y);
        }
        
    }

    
}
