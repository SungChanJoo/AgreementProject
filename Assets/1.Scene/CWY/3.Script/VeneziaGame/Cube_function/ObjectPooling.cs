using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject CubePrefab; //풀링할 정답 & 오답 큐브 프리팹
    [SerializeField] private Transform Pool_Position; //풀의 위치


    [SerializeField] private GameObject MeteorPrefab; //아이템 풀링 해줄 프리팹



    //생성할 큐브 갯수
    public int CubeCount;
    public int ItemCount;
    //문제에 관련된 prefab들 생성
    public List<GameObject> cubePool = new List<GameObject>();
    //메테오 , 프리즈 등 아이템 생성.
    public List<GameObject> ItemPool = new List<GameObject>();
    //큐브의 최대 반경거리 제한
    public float MaxDistance;
    private void Awake()
    {
        #region Singleton 
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        #endregion

        #region Object Pooling
        //정답 오브젝트 풀링
        for (int i = 0; i < CubeCount; i++)
        {
            GameObject cube = Instantiate(CubePrefab); //풀링할 오브잭트 생성
            cube.SetActive(false);
            cube.transform.SetParent(Pool_Position);
            cubePool.Add(cube);
        }

        //아이템 프리팹 
        for (int i = 0; i < CubeCount; i++)
        {
            GameObject Meteor = Instantiate(MeteorPrefab); //풀링할 오브잭트 생성
            Meteor.SetActive(false);
            //Vector3 offset = new Vector3(0, 0, 20);
            Meteor.transform.SetParent(Pool_Position);
            cubePool.Add(Meteor);
        }

        #endregion
    }

    private void Start()
    {
        StartCoroutine(Cube_Co());
    }

    private void Update()
    {
        print(cubePool.Count);
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

    IEnumerator Meteor_Co()
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

}
