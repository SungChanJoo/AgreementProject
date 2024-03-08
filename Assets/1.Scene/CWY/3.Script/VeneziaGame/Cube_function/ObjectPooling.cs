using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject[] Prefab_K; //풀링할 정답 & 오답 큐브 프리팹
    [SerializeField] private GameObject[] Prefab_E; //풀링할 정답 & 오답 큐브 프리팹
    [SerializeField] private GameObject[] Prefab_H; //풀링할 정답 & 오답 큐브 프리팹
    [SerializeField] private Transform Pool_Position; //풀의 위치
    [SerializeField] private Transform CubeParentObject; // 풀링된 오브젝트를 저장할 더미오브젝트
    [SerializeField] private Transform ItemParentObject; // 풀링된 오브젝트를 저장할 더미오브젝트


    [SerializeField] private GameObject MeteorPrefab; //아이템 풀링 해줄 프리팹
    [SerializeField] private GameObject PausePrefab; //아이템 풀링 해줄 프리팹

    public IEnumerator CubePooling;
    public IEnumerator CubeRestartPooling;

    [SerializeField] Sprite[] sprites;

    //생성할 큐브 갯수
    public int CubeCount;
    //생성할 아이템 갯수
    public int ItemCount;
    //문제에 관련된 prefab들 생성
    public List<GameObject> cubePool = new List<GameObject>();
    //메테오 , 프리즈 등 아이템 생성.
    public List<GameObject> MeteorPool = new List<GameObject>();
    public List<GameObject> PausePool = new List<GameObject>();

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
    }

    private void Start()
    {
        CreateItem();
        StartCoroutine(Meteor_Co());
        StartCoroutine(Pause_Co());
    }
    

    public void StartCubePooling_co()
    {
        CubePooling = Cube_Co(VeneziaManager.Instance.PoolingCool);
        StartCoroutine(CubePooling);
    }
    public void ReStartCubePooling_co()
    {
        CubeRestartPooling = ReStartCube_Co(VeneziaManager.Instance.PoolingCool);
        StartCoroutine(CubeRestartPooling);
    }
    public IEnumerator Cube_Co(int cool)
    {
        int Count = cubePool.Count - VeneziaManager.Instance.QuestRange; //풀링 갯수에 맞게 생성하는 조건  5
        while (cubePool.Count >= Count)
        {
            //
            int Randnum = Random.Range(0, cubePool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            if (cubePool.Count - Count == 1)
            {
                foreach (var cube in cubePool)
                {
                    Cube cubeType = cube.GetComponent<Cube>();

                    if (cubeType.objectType == ObjectType.CorrectAnswer)
                    {
                        cube.SetActive(true);
                        cube.transform.position = Pool_Position.transform.position + offset;
                        cubePool.Remove(cube);
                        yield break;
                    }
                }
            }
            else
            {
                cubePool[Randnum].transform.position = Pool_Position.transform.position + offset;
                cubePool[Randnum].SetActive(true);
                cubePool.Remove(cubePool[Randnum]);
            }

            yield return new WaitForSeconds(cool); //난이도에 따라 재생되는 시간을 바꿀것
        }
    }

    public IEnumerator ReStartCube_Co(int cool)
    {
        int Count = cubePool.Count - VeneziaManager.Instance.QuestRange;
        yield return new WaitForSeconds(cool);
        while (cubePool.Count >= Count)
        {
            int Randnum = Random.Range(0, cubePool.Count);
            float randomValue = Random.Range(-90, 90);
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
            float randomValueX = Random.Range(-90, 90);
            //  float randomValueZ = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValueX, 0, -40); // 좌우 변경을위한 랜덤값
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
            float randomValue = Random.Range(-80, 90);
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
            item.transform.SetParent(ItemParentObject); // 풀 위치에 부모 설정
            MeteorPool.Add(item); // 아이템 풀 리스트에 추가
        }
        //일시정지 
        for (int i = 0; i < ItemCount; i++)
        {
            GameObject item = Instantiate(PausePrefab); // 해당 인덱스의 프리팹을 인스턴스화
            item.SetActive(false); // 활성화하지 않음
            item.transform.SetParent(ItemParentObject); // 풀 위치에 부모 설정
            PausePool.Add(item); // 아이템 풀 리스트에 추가
        }
    }

    public void CreateQuestPrefab(int index, int count) //추후 범위제어할때 인수좀더 넣을 것.
    {
        int PrefabCount = count + index;  
        for (int i = index; i < PrefabCount; i++) 
        {
            for (int j = 0; j < CubeCount; j++)
            {
                if(VeneziaManager.Instance.game_Type == Game_Type.B)
                {
                    GameObject QuestPrefab = Instantiate(Prefab_K[i]); // 한글 오브젝트 생성
                    SpriteRenderer spriteRenderer = QuestPrefab.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = VeneziaManager.Instance.sprites_K[i];
                    QuestPrefab.SetActive(false);
                    QuestPrefab.transform.SetParent(CubeParentObject);
                    cubePool.Add(QuestPrefab);
                }
                else if(VeneziaManager.Instance.game_Type == Game_Type.D)
                {
                    GameObject QuestPrefab = Instantiate(Prefab_E[i]); // 영어 오브젝트 생성
                    SpriteRenderer spriteRenderer = QuestPrefab.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = VeneziaManager.Instance.sprites_E[i];
                    QuestPrefab.SetActive(false);
                    QuestPrefab.transform.SetParent(CubeParentObject);
                    cubePool.Add(QuestPrefab);
                }
                else
                {
                    GameObject QuestPrefab = Instantiate(Prefab_H[i]); // 한자 오브젝트 생성
                    SpriteRenderer spriteRenderer = QuestPrefab.GetComponent<SpriteRenderer>();
                    spriteRenderer.sprite = VeneziaManager.Instance.sprites_H[i];
                    QuestPrefab.SetActive(false);
                    QuestPrefab.transform.SetParent(CubeParentObject);
                    cubePool.Add(QuestPrefab);
                }
            }
        }
    }
}