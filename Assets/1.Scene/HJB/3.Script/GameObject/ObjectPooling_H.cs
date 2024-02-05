using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling_H : MonoBehaviour
{

    [SerializeField] private GameObject cube_Obj;
    [SerializeField] private GameObject[] poolPosition;
    
    private List<GameObject> cubePool = new List<GameObject>();

    

    

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
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            CubeStart();
        }
        Click_Obj();
    }
    private void Click_Obj()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {                
                print(hit.collider.gameObject.name);                
                cubePool.Add(hit.collider.gameObject);                
                hit.collider.gameObject.SetActive(false);
            }
        }
    }

    //문제 오브젝트 이동
    private void CubeStart()
    {        
        for (int i = 0; i < cubePool.Count; i++)
        {
            cubePool[i].SetActive(true);
            MovingCube movingcube = cubePool[i].GetComponent<MovingCube>();
            AOP_Manager.Instance.Calculator_Random();
             
            int firstNum = AOP_Manager.Instance.first_num;
            int secondNum = AOP_Manager.Instance.second_num;
            string _operator = AOP_Manager.Instance.operator_ran;
            movingcube.Start_Obj(firstNum,_operator , secondNum);

        }
    }
}

