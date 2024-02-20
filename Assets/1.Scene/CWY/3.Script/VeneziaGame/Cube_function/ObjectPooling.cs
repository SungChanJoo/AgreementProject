using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject[] CubePrefab; //풀링할 정답 & 오답 큐브 프리팹
    [SerializeField] private Transform Pool_Position; //풀의 위치
    [SerializeField] private Transform ParentObject; // 풀링된 오브젝트를 저장할 더미오브젝트


    [SerializeField] private GameObject MeteorPrefab; //아이템 풀링 해줄 프리팹
    [SerializeField] private GameObject PausePrefab; //아이템 풀링 해줄 프리팹



    //생성할 큐브 갯수
    public int CubeCount;
    //생성할 아이템 갯수
    public int ItemCount;
    //문제에 관련된 prefab들 생성
    public List<GameObject> cubePool = new List<GameObject>();
    //메테오 , 프리즈 등 아이템 생성.
    public List<GameObject> MeteorPool = new List<GameObject>();
    public List<GameObject> PausePool = new List<GameObject>();
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
        CreateQuestPrefab();

        CreateItem();
        //아이템 프리팹 


        #endregion
    }

    private void Start()
    {
        StartCoroutine(Cube_Co());
        StartCoroutine(Meteor_Co());        
        StartCoroutine(Pause_Co());        
    }
    public int cool;
    public IEnumerator Cube_Co()
    {
        while (cubePool.Count > 0)
        {
            int Randnum = Random.Range(0, cubePool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            cubePool[Randnum].transform.position = Pool_Position.transform.position + offset;
            cubePool[Randnum].SetActive(true);
            cubePool.Remove(cubePool[Randnum]);
            yield return new WaitForSeconds(cool); //난이도에 따라 재생되는 시간을 바꿀것
        }
    }

    public IEnumerator Meteor_Co()
    {
        while (MeteorPool.Count > 0)
        {
            float cool = Random.Range(10, 16f);
            yield return new WaitForSeconds(cool); //난이도에 따라 재생되는 시간을 바꿀것
            //아이템 랜덤 생성
            int Randnum = Random.Range(0, MeteorPool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            MeteorPool[Randnum].transform.position = Pool_Position.transform.position + offset;
            MeteorPool[Randnum].SetActive(true);
            MeteorPool.Remove(MeteorPool[Randnum]);
        }
    }

    public IEnumerator Pause_Co()
    {
        while (PausePool.Count > 0)
        {
            float cool = Random.Range(45, 61f);
            yield return new WaitForSeconds(cool); //난이도에 따라 재생되는 시간을 바꿀것
            //아이템 랜덤 생성
            int Randnum = Random.Range(0, MeteorPool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            PausePool[Randnum].transform.position = Pool_Position.transform.position + offset;
            PausePool[Randnum].SetActive(true);
            PausePool.Remove(PausePool[Randnum]);
        }
    }

    //아이템 생성
    private void CreateItem()
    {
        //메테오
        for (int i = 0; i < ItemCount; i++)
        {
            GameObject item = Instantiate(MeteorPrefab); // 해당 인덱스의 프리팹을 인스턴스화
            item.SetActive(false); // 활성화하지 않음
            item.transform.SetParent(ParentObject); // 풀 위치에 부모 설정
            MeteorPool.Add(item); // 아이템 풀 리스트에 추가
        }
        //일시정지 
        for (int i = 0; i < ItemCount; i++)
        {
            GameObject item = Instantiate(PausePrefab); // 해당 인덱스의 프리팹을 인스턴스화
            item.SetActive(false); // 활성화하지 않음
            item.transform.SetParent(ParentObject); // 풀 위치에 부모 설정
            PausePool.Add(item); // 아이템 풀 리스트에 추가
        }
    }

    private void CreateQuestPrefab()
    {
        for (int i = 0; i < CubePrefab.Length; i++)
        {
            for (int j = 0; j < CubeCount; j++)
            {
                GameObject QuestPrefab = Instantiate(CubePrefab[i]); //풀링할 오브잭트 생성
                QuestPrefab.SetActive(false);
                QuestPrefab.transform.SetParent(ParentObject);
                cubePool.Add(QuestPrefab);
            }
        }
    }
}
