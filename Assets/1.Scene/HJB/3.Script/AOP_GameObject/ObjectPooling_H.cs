using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling_H : MonoBehaviour
{

    [SerializeField] private GameObject cube_Obj;
    [SerializeField] private GameObject[] poolPosition;
    
    private List<GameObject> cubePool = new List<GameObject>();

    [SerializeField]private AOP_Manager aopManager;
    
    public float answer;
       
    
    //Start ��ư �̺�Ʈ�� �ݹ�Ǹ� ����
    public void ObjectPooling()
    {
        //���� ������Ʈ�� ������ŭ ���� �� Pool�� ���
        for (int i = 0; i < poolPosition.Length; i++)
        {
            GameObject cube = Instantiate(cube_Obj);
            cube.transform.position = poolPosition[i].transform.position;
            cube.SetActive(false);
            cubePool.Add(cube);
        }
        CubeStart();
    }
    private void Update()
    {        
        Click_Obj();
    }
    //Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began
    private void Click_Obj()
    {        
        if (Input.GetMouseButtonDown(0)||
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {                
                hit.collider.gameObject.SetActive(false);
                Answer_Check(hit.collider.gameObject);                
            }
        }
    }

    
    private void CubeStart()
    {
        int randomResult = Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            cubePool[i].SetActive(true);
            MovingCube movingcube = cubePool[i].GetComponent<MovingCube>();
            aopManager.SplitLevelAndStep();
            int firstNum = aopManager.first_num;
            int secondNum = aopManager.second_num;
            char _operator = aopManager._Operator;
            movingcube.result = aopManager.result;            
            if (i.Equals(randomResult))
            {
                TakeResult(movingcube.result);
            }
            
            //���⿡ ���� �Ҵ�
            movingcube.Start_Obj(firstNum,_operator , secondNum);
        }        
    }
    private void  Next_Result()
    {
        int random_Answer = Random.Range(0, 2);
        
        if (random_Answer.Equals(1))
        {
            for (int i = 0; i < 3; i++)
            {
                if (cubePool[i].activeSelf)
                {
                    MovingCube movingCube = cubePool[i].GetComponent<MovingCube>();
                    TakeResult(movingCube.result);
                    return;
                }
            }
        }
        else
        {
            for (int i = 2; i > -1; i--)
            {
                if (cubePool[i].activeSelf)
                {
                    MovingCube movingCube = cubePool[i].GetComponent<MovingCube>();
                    TakeResult(movingCube.result);
                    return;
                }
            }
        }
        
        CubeStart();        
    }
    private void Answer_Check(GameObject cube)
    {
        MovingCube movingCube = cube.GetComponent<MovingCube>();
        
        if (movingCube.result.Equals(answer))
        {
            Next_Result();
        }
        else
        {
            Debug.Log("Ʋ�Ƚ��ϴ�.");
            //�ð� ����
        }        
    }

    private float TakeResult(float result)
    {
        answer = result;
        aopManager.Show_Result(answer);
        return result;
    }
    
}

