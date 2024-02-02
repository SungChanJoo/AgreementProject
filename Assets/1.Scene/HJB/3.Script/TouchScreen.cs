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
        //Touch 1~10 ���� ����
        if (Input.touchCount>0 && Input.touchCount<=10)
        {
            Debug.Log(Input.touchCount);
            //Touch�� ����ŭ �ݺ�
            for (int i = 0; i < Input.touchCount ; i++)
            {
                //Touch�� ���� �ӽ� ������ ����
                touchTemp[i] = Input.GetTouch(i);
                if (touchTemp[i].phase == TouchPhase.Began)
                {
                    //�� Touch�� Position�� ȭ����� �������� ����
                    touchPosition[i] = Camera.main.ScreenToWorldPoint(touchTemp[i].position);
                    
                }
                //Touch�� �� �� �����ٸ�
                if (touchTemp[i].phase == TouchPhase.Ended)
                {
                    //�� Touch�� Position�� ȭ����� �������� ����                    
                    Instantiate(touch_obj, touchPosition[i], Quaternion.identity);
                    Debug.Log("����");
                }
                
                
            }

        }           

    }
   
}
