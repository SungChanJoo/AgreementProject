using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject CubePrefab; //풀링할 큐브 프리팹
    [SerializeField] private Transform Pool_Position; //풀의 위치
    


    //생성할 큐브 갯수
    public int CubeCount;
    public List<GameObject> cubePool = new List<GameObject>();

    //큐브의 최대 반경거리 제한
    public float MaxDistance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        for (int i = 0; i < CubeCount; i++)
        {
            GameObject cube = Instantiate(CubePrefab); //풀링할 오브잭트 생성
            cube.SetActive(false);
            cube.transform.SetParent(Pool_Position);
            cubePool.Add(cube);
        }
    }

    private void Start()
    {
        StartCoroutine(Cube_Co());
    }

    private void Update()
    {
        Click_Obj();
        print(cubePool.Count);
    }

    private void Click_Obj()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {
                print(hit.collider.gameObject.name);
                Score.Instance.Get_Score();
                cubePool.Add(hit.collider.gameObject);
                hit.collider.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator Cube_Co()
    {
        while (cubePool.Count > 0)
        {
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            cubePool[0].transform.position = Pool_Position.transform.position + offset;
            cubePool[0].SetActive(true);
            cubePool.Remove(cubePool[0]);
            yield return new WaitForSeconds(3f); //난이도에 따라 재생되는 시간을 바꿀것
        }
    }

    private void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
    }


}
