using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchScreen : MonoBehaviour
{
    private Touch[] touchTemp = new Touch[10];
    private Vector3[] touchPosition = new Vector3[10];
    [SerializeField] private GameObject touch_obj;
    
    private void Start()
    {
        
    }
    private void Update()
    {
        //Touch 1~10 가지 제한
        if (Input.touchCount>0 && Input.touchCount<=10)
        {
            Debug.Log(Input.touchCount);
            //Touch한 수만큼 반복
            for (int i = 0; i < Input.touchCount ; i++)
            {
                //Touch한 것을 임시 변수에 저장
                touchTemp[i] = Input.GetTouch(i);
                if (touchTemp[i].phase == TouchPhase.Began)
                {
                    //각 Touch의 Position를 화면상의 기준으로 저장
                    touchPosition[i] = Camera.main.ScreenToWorldPoint(touchTemp[i].position);
                    
                }
                //Touch가 된 후 끝났다면
                if (touchTemp[i].phase == TouchPhase.Ended)
                {
                    //각 Touch의 Position를 화면상의 기준으로 저장                    
                    Instantiate(touch_obj, touchPosition[i], Quaternion.identity);
                    Debug.Log("생성");
                }
                
                
            }

        }           

    }
   
}
