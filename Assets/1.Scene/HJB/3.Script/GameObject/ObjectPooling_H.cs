using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling_H : MonoBehaviour
{

    [SerializeField] private GameObject cube_Obj;
    [SerializeField] private GameObject[] poolPosition;
    
    private List<GameObject> cubePool = new List<GameObject>();
    public float answer;

    private void Start()
    {
        //문제 오브젝트의 갯수만큼 생성 및 Pool에 담기
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
    //Input.GetMouseButtonDown(0)
    //Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began
    private void Click_Obj()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;

        //    if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
        //    {
        //        hit.collider.gameObject.SetActive(false);
        //        Answer_Check(hit.collider.gameObject);
        //        return;
        //    }
        //}
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
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
            movingcube.result = AOP_Manager.Instance.Calculator_Random();
            if (i.Equals(randomResult))
            {
                TakeResult(movingcube.result);
            }
            int firstNum = AOP_Manager.Instance.first_num;
            int secondNum = AOP_Manager.Instance.second_num;
            string _operator = AOP_Manager.Instance.operator_ran;
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
            Debug.Log("틀렸습니다.");
            //시간 감소
        }        
    }

    private float TakeResult(float result)
    {
        answer = result;
        AOP_Manager.Instance.Show_Result(result);
        return result;
    }
    
}

