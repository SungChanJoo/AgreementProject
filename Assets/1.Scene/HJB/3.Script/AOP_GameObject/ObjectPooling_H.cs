using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling_H : MonoBehaviour
{

    [SerializeField] private GameObject cube_Obj;
    [SerializeField] private GameObject[] poolPosition;
    
    [SerializeField]private AOP_Manager aopManager;
    [SerializeField] private Result_Printer result_Printer;

    private List<GameObject> cubePool = new List<GameObject>();
    
    public float answer;

    //
    private int problom_count=0;
    private int answer_count = 0;

    
    
    //Start 버튼 이벤트가 콜백되면 실행
    public void ObjectPooling()
    {
        //기본 값 초기화
        DataDefaultSetting();
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
            int firstNum = aopManager.firstNum;
            int secondNum = aopManager.secondNum;
            char _operator = aopManager._Operator;
            movingcube.result = aopManager.result;
            if (i.Equals(randomResult))
            {
                TakeResult(movingcube.result);
            }
            //여기에 숫자 할당
            movingcube.Start_Obj(firstNum,_operator , secondNum);

            //문제 개수를 계속 체크
            problom_count++;
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
            answer_count++;
        }
        else
        {
            //시간 감소            
            TimeSlider.Instance.DecreaseTime_Item(5);
        }        
    }

    //정답 결과 출력 메서드
    private void TakeResult(float result)
    {
        answer = result;
        aopManager.Show_Result(answer);
    }

    private void DataDefaultSetting()
    {
        problom_count = 0;
        problom_count = 0;
    }

    private void ResultPrinter_UI()
    {
        result_Printer.ShowText(1,answer_count,20,aopManager.timeSet,100);
    }

    
    
}

