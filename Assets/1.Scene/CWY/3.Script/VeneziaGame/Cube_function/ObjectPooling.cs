using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject CubePrefab; //풀링할 정답 & 오답 큐브 프리팹
    [SerializeField] private Transform Pool_Position; //풀의 위치
    [SerializeField] private Transform ParentObject; // 풀링된 오브젝트를 저장할 더미오브젝트


    [SerializeField] private GameObject[] ItemPrefab; //아이템 풀링 해줄 프리팹



    //생성할 큐브 갯수
    public int CubeCount;
    //생성할 아이템 갯수
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
            cube.transform.SetParent(ParentObject);
            cubePool.Add(cube);
        }

        CreateItem();
        //아이템 프리팹 


        #endregion
    }

    private void Start()
    {
        StartCoroutine(Cube_Co());
        StartCoroutine(Meteor_Co());
    }

    private void Update()
    {
        print(cubePool.Count);
    }


    public IEnumerator Cube_Co()
    {
        while (cubePool.Count > 0)
        {
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            cubePool[0].transform.position = Pool_Position.transform.position + offset;
            cubePool[0].SetActive(true);
            cubePool.Remove(cubePool[0]);
            yield return new WaitForSeconds(10f); //난이도에 따라 재생되는 시간을 바꿀것
        }
    }

    public IEnumerator Meteor_Co()
    {
        while (ItemPool.Count > 0)
        {
            //아이템 랜덤 생성
            int Randnum = Random.Range(0, ItemPool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            ItemPool[Randnum].transform.position = Pool_Position.transform.position + offset;
            ItemPool[Randnum].SetActive(true);
            ItemPool.Remove(ItemPool[Randnum]);
            print(Randnum);
            yield return new WaitForSeconds(3f); //난이도에 따라 재생되는 시간을 바꿀것
        }
    }

    //아이템 생성
    private void CreateItem()
    {
        for (int i = 0; i < ItemPrefab.Length; i++)
        {
            for (int j = 0; j < ItemCount; j++)
            {
                GameObject item = Instantiate(ItemPrefab[i]); // 해당 인덱스의 프리팹을 인스턴스화
                item.SetActive(false); // 활성화하지 않음
                item.transform.SetParent(ParentObject); // 풀 위치에 부모 설정
                ItemPool.Add(item); // 아이템 풀 리스트에 추가
            }
        }
    }
}
