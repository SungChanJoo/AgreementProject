using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject cube;
    [SerializeField] private Transform pool_Position; //풀의 위치
    [SerializeField] private Transform pool_PositionTwo; //풀의 위치
    [SerializeField] private Transform cubeParentObject; // 풀링된 오브젝트를 저장할 더미오브젝트
    [SerializeField] private Transform cubeParentObjectTwo; // 풀링된 오브젝트를 저장할 더미오브젝트
    [SerializeField] private Transform itemParentObject; // 풀링된 오브젝트를 저장할 더미오브젝트

    [SerializeField] public Transform PlayerOne_Boom;
    [SerializeField] public Transform PlayerTwo_Boom;


    [SerializeField] private GameObject meteorPrefab; //아이템 풀링 해줄 프리팹
    [SerializeField] private GameObject pausePrefab; //아이템 풀링 해줄 프리팹
    [SerializeField] private GameObject boomPrefab; //아이템 풀링 해줄 프리팹

    public IEnumerator CubePooling_PlayerOne;
    public IEnumerator CubePooling_PlayerTwo;
    public IEnumerator CubeRestartPooling_One;
    public IEnumerator CubeRestartPooling_Two;

    [SerializeField] Sprite sprite;
    [SerializeField] Image questImg;
    [SerializeField] Image questTwoImg;

    //생성할 큐브 갯수
    public int CubeCount;
    //생성할 아이템 갯수
    public int ItemCount;
    //문제에 관련된 prefab들 생성
    public List<GameObject> CubePool = new List<GameObject>();
    public List<GameObject> CubePoolTwo = new List<GameObject>();
    //메테오 , 프리즈 등 아이템 생성.
    public List<GameObject> MeteorPool = new List<GameObject>();
    public List<GameObject> PausePool = new List<GameObject>();
    public List<GameObject> BoomPool = new List<GameObject>();

    private Vector3 offset;
    private Vector3 offsetTwo;


    public int FirstPool = 0;
    public int SecondPool = 0;
    private void Awake()
    {
        #region Singleton 
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
           // Destroy(gameObject);
        }
        #endregion
    }

    private void Start()
    {
        //CreateItem();
        ItemCreatForveneGameMode();
    }

    #region 코루틴 시작 및 재시작
    public void StartCubeOne_Pooling_co()
    {
        CubePooling_PlayerOne = PlayerONe_Cube_Co(VeneziaManager.Instance.PoolingCool);
        StartCoroutine(CubePooling_PlayerOne);
    }
    public void StartCubeTwo_Pooling_co()
    {
        CubePooling_PlayerTwo = PlayerTwo_Cube_Co(VeneziaManager.Instance.PoolingCool);
        StartCoroutine(CubePooling_PlayerTwo);
    }
    public void ReStartCubePooling_One_co()
    {
        CubeRestartPooling_One = PlayerONe_ReStartCube_Co(VeneziaManager.Instance.PoolingCool);
        StartCoroutine(CubeRestartPooling_One);
    }
    public void ReStartCubePooling_Two_co()
    {
        CubeRestartPooling_Two = PlayerTwo_ReStartCube_Co(VeneziaManager.Instance.PoolingCool);
        StartCoroutine(CubeRestartPooling_Two);
    }
    #endregion

    #region 첫번째 플레이어 및 두번째 플레이어 큐브 코루틴
    public IEnumerator PlayerONe_Cube_Co(int cool)
    {
        if (CubePool.Count <= VeneziaManager.Instance.limitCount) yield break;
        while (CubePool.Count > VeneziaManager.Instance.limitCount || CubePoolTwo.Count > VeneziaManager.Instance.limitCount)
        {
            int Randnum = Random.Range(0, CubePool.Count);
            int RandnumTwo = Random.Range(0, CubePoolTwo.Count);
            //솔로모드
            if (VeneziaManager.Instance.play_mode== PlayMode.Solo)
            {
                float randomValue = Random.Range(-95, 95);
                offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            }
            else
            {
                //2인모드
                float random = Random.Range(5, 10);
                offset = new Vector3(random, 0, 0); // 좌우 변경을위한 랜덤값
            }

            //솔로모드
            if (VeneziaManager.Instance.play_mode== PlayMode.Solo)
            {
                if (CubePool.Count - VeneziaManager.Instance.limitCount == 1)
                {
                    bool foundSameSprite = false;
                    foreach (var cube in CubePool)
                    {
                        Cube cubeScript = cube.GetComponent<Cube>();

                        if (cubeScript.Sprite == questImg.sprite)
                        {
                            cube.SetActive(true);
                            cube.transform.position = pool_Position.transform.position + offset;
                            CubePool.Remove(cube);
                            foundSameSprite = true;
                            break;
                        }
                    }
                    if (!foundSameSprite)
                    {
                        CubePool[Randnum].transform.position = pool_Position.transform.position + offset;
                        CubePool[Randnum].SetActive(true);
                        CubePool.Remove(CubePool[Randnum]);
                    }
                }
                else
                {
                    CubePool[Randnum].transform.position = pool_Position.transform.position + offset;
                    CubePool[Randnum].SetActive(true);
                    CubePool.Remove(CubePool[Randnum]);
                }
            }//커플모드
            else if(VeneziaManager.Instance.play_mode == PlayMode.Couple)
            {
                //First Player pool
                if (CubePool.Count - VeneziaManager.Instance.limitCount == 1)
                {
                    bool foundSameSprite = false;
                    foreach (var cube in CubePool)
                    {
                        Cube cubeScript = cube.GetComponent<Cube>();

                        if (cubeScript.Sprite == questImg.sprite)
                        {
                            cube.SetActive(true);
                            cube.transform.position = pool_Position.transform.position + offset;
                            CubePool.Remove(cube);
                            foundSameSprite = true;
                            break;
                        }
                    }
                    if (!foundSameSprite)
                    {
                        CubePool[Randnum].transform.position = pool_Position.transform.position + offset;
                        CubePool[Randnum].SetActive(true);
                        CubePool.Remove(CubePool[Randnum]);
                    }
                }
                else
                {
                    CubePool[Randnum].transform.position = pool_Position.transform.position + offset;
                    CubePool[Randnum].SetActive(true);
                    CubePool.Remove(CubePool[Randnum]);
                }
            }
            yield return new WaitForSeconds(cool); //난이도에 따라 재생되는 시간을 바꿀것
        }
    }

    public IEnumerator PlayerTwo_Cube_Co(int cool)
    {
        if (CubePoolTwo.Count <= VeneziaManager.Instance.limitCount) yield break;
        while (CubePoolTwo.Count > VeneziaManager.Instance.limitCount)
        {
            int RandnumTwo = Random.Range(0, CubePoolTwo.Count);
            float random = Random.Range(5, 10);
            offset = new Vector3(random, 0, 0); // 좌우 변경을 위한 랜덤값

            if (CubePoolTwo.Count - VeneziaManager.Instance.limitCount == 1)
            {
                bool foundSameSprite = false;
                foreach (var cube in CubePoolTwo)
                {
                    Cube cubeScript = cube.GetComponent<Cube>();

                    if (cubeScript.Sprite == questTwoImg.sprite)
                    {
                        cube.SetActive(true);
                        cube.transform.position = pool_PositionTwo.transform.position + offset;
                        CubePoolTwo.Remove(cube);
                        foundSameSprite = true;
                        break;
                    }
                }
                if (!foundSameSprite)
                {
                    CubePoolTwo[RandnumTwo].transform.position = pool_PositionTwo.transform.position + offset;
                    CubePoolTwo[RandnumTwo].SetActive(true);
                    CubePoolTwo.Remove(CubePoolTwo[RandnumTwo]);
                }
            }
            else
            {
                CubePoolTwo[RandnumTwo].transform.position = pool_PositionTwo.transform.position + offset;
                CubePoolTwo[RandnumTwo].SetActive(true);
                CubePoolTwo.Remove(CubePoolTwo[RandnumTwo]);
            }
            yield return new WaitForSeconds(cool); //난이도에 따라 재생되는 시간을 바꿀것
        }
    }

    public IEnumerator PlayerONe_ReStartCube_Co(int cool)
    {
        yield return new WaitForSeconds(cool);
        while (CubePool.Count > VeneziaManager.Instance.limitCount || CubePoolTwo.Count > VeneziaManager.Instance.limitCount)
        {
            int Randnum = Random.Range(0, CubePool.Count);
            int RandnumTwo = Random.Range(0, CubePoolTwo.Count);
            //솔로모드
            if (VeneziaManager.Instance.play_mode== PlayMode.Solo)
            {
                float randomValue = Random.Range(-95, 95);
                offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            }
            else
            {
                //2인모드
                float random = Random.Range(5, 10);
                offset = new Vector3(random, 0, 0); // 좌우 변경을위한 랜덤값
            }
            //솔로모드
            if (VeneziaManager.Instance.play_mode== PlayMode.Solo)
            {
                if (CubePool.Count - VeneziaManager.Instance.limitCount == 1)
                {
                    bool foundSameSprite = false;
                    foreach (var cube in CubePool)
                    {
                        Cube cubeScript = cube.GetComponent<Cube>();

                        if (cubeScript.Sprite == questImg.sprite)
                        {
                            cube.SetActive(true);
                            cube.transform.position = pool_Position.transform.position + offset;
                            CubePool.Remove(cube);
                            foundSameSprite = true;
                            break;
                        }
                    }
                    if (!foundSameSprite)
                    {
                        CubePool[Randnum].transform.position = pool_Position.transform.position + offset;
                        CubePool[Randnum].SetActive(true);
                        CubePool.Remove(CubePool[Randnum]);
                    }
                }
                else
                {
                    CubePool[Randnum].transform.position = pool_Position.transform.position + offset;
                    CubePool[Randnum].SetActive(true);
                    CubePool.Remove(CubePool[Randnum]);
                }
            }
            else if (VeneziaManager.Instance.play_mode == PlayMode.Couple)
            {
                //First Player pool
                if (CubePool.Count - VeneziaManager.Instance.limitCount == 1)
                {
                    bool foundSameSprite = false;
                    foreach (var cube in CubePool)
                    {
                        Cube cubeScript = cube.GetComponent<Cube>();

                        if (cubeScript.Sprite == questImg.sprite)
                        {
                            cube.SetActive(true);
                            cube.transform.position = pool_Position.transform.position + offset;
                            CubePool.Remove(cube);
                            foundSameSprite = true;
                            break;
                        }
                    }
                    if (!foundSameSprite)
                    {
                        CubePool[Randnum].transform.position = pool_Position.transform.position + offset;
                        CubePool[Randnum].SetActive(true);
                        CubePool.Remove(CubePool[Randnum]);
                    }
                }
                else
                {
                    if (VeneziaManager.Instance.isFirstPlayerTouch)
                    {
                        CubePool[Randnum].transform.position = pool_Position.transform.position + offset;
                        CubePool[Randnum].SetActive(true);
                        CubePool.Remove(CubePool[Randnum]);
                    }
                }
            }
            yield return new WaitForSeconds(cool); //난이도에 따라 재생되는 시간을 바꿀것
        }
    }

    public IEnumerator PlayerTwo_ReStartCube_Co(int cool)
    {
        yield return new WaitForSeconds(cool);
        while (CubePoolTwo.Count > VeneziaManager.Instance.limitCount)
        {
            int RandnumTwo = Random.Range(0, CubePoolTwo.Count);
            float random = Random.Range(5, 10);
            offset = new Vector3(random, 0, 0); // 좌우 변경을 위한 랜덤값

            if (CubePoolTwo.Count - VeneziaManager.Instance.limitCount == 1)
            {
                bool foundSameSprite = false;
                foreach (var cube in CubePoolTwo)
                {
                    Cube cubeScript = cube.GetComponent<Cube>();

                    if (cubeScript.Sprite == questTwoImg.sprite)
                    {
                        cube.SetActive(true);
                        cube.transform.position = pool_PositionTwo.transform.position + offset;
                        CubePoolTwo.Remove(cube);
                        foundSameSprite = true;
                        break;
                    }
                }
                if (!foundSameSprite)
                {
                    CubePoolTwo[RandnumTwo].transform.position = pool_PositionTwo.transform.position + offset;
                    CubePoolTwo[RandnumTwo].SetActive(true);
                    CubePoolTwo.Remove(CubePoolTwo[RandnumTwo]);
                }
            }
            else
            {
                CubePoolTwo[RandnumTwo].transform.position = pool_PositionTwo.transform.position + offset;
                CubePoolTwo[RandnumTwo].SetActive(true);
                CubePoolTwo.Remove(CubePoolTwo[RandnumTwo]);
            }

            yield return new WaitForSeconds(cool); //난이도에 따라 재생되는 시간을 바꿀것
        }
    }
    #endregion

    #region Item Coroutine
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
            MeteorPool[0].transform.position = pool_Position.transform.position + offset;
            MeteorPool[0].SetActive(true);
            MeteorPool.Remove(MeteorPool[0]);
        }
    }

    public IEnumerator Pause_Co()
    {
        if (VeneziaManager.Instance.play_mode == PlayMode.Couple) yield break; // 커플모드에서는 생성x
        while (PausePool.Count > 0)
        {
            float cool = Random.Range(45, 51f);
            yield return new WaitForSeconds(cool); //난이도에 따라 재생되는 시간을 바꿀것
            //아이템 랜덤 생성
            int Randnum = Random.Range(0, MeteorPool.Count);
            float randomValue = Random.Range(-90, 90);
            Vector3 offset = new Vector3(randomValue, 0, 0); // 좌우 변경을위한 랜덤값
            PausePool[0].transform.position = pool_Position.transform.position + offset;
            PausePool[0].SetActive(true);
            PausePool.Remove(PausePool[0]);
        }
    }
    #endregion

    public void CreateBoom(Transform transform)
    {
        //2인모드에서만 사용하는 boom 생성 첫 번째 플레리어와 두번째 플레이어를 타입으로 구분
        if (VeneziaManager.Instance.ClickCount == 3 && BoomPool[0] != null)
        {
            float randomValue = Random.Range(0, 40);
            Vector3 offset = new Vector3(randomValue, 0, 0);
            BoomPool[0].transform.position = transform.position + offset;
            BoomPool[0].GetComponent<ItemFnc>().BoomType = BoomType.PlayerOne;
            BoomPool[0].SetActive(true);
            BoomPool.Remove(BoomPool[0]);
            VeneziaManager.Instance.ClickCount = 0;
        }
        else if (VeneziaManager.Instance.Click_SecondPlayerCount == 3 && BoomPool[0] != null)
        {
            float randomValue = Random.Range(0, 40);
            Vector3 offset = new Vector3(randomValue, 0, 0);
            BoomPool[0].transform.position = transform.position + offset;
            BoomPool[0].GetComponent<ItemFnc>().BoomType = BoomType.PlayerTwo;
            BoomPool[0].SetActive(true);
            BoomPool.Remove(BoomPool[0]);
            VeneziaManager.Instance.Click_SecondPlayerCount = 0;
        }
    }

    //아이템 생성
    public void CreateItem()
    {
        print(VeneziaManager.Instance.play_mode);
        if(VeneziaManager.Instance.play_mode== PlayMode.Solo)
        {
            for (int i = 0; i < ItemCount; i++)
            {
                GameObject item = Instantiate(meteorPrefab); // 해당 인덱스의 프리팹을 인스턴스화
                item.SetActive(false); // 활성화하지 않음
                item.transform.SetParent(itemParentObject); // 풀 위치에 부모 설정
                MeteorPool.Add(item); // 아이템 풀 리스트에 추가
            }
            //일시정지 
            for (int i = 0; i < ItemCount; i++)
            {
                GameObject item = Instantiate(pausePrefab); // 해당 인덱스의 프리팹을 인스턴스화
                item.SetActive(false); // 활성화하지 않음
                item.transform.SetParent(itemParentObject); // 풀 위치에 부모 설정
                PausePool.Add(item); // 아이템 풀 리스트에 추가
            }
        }        
        else if(VeneziaManager.Instance.play_mode == PlayMode.Couple)
        {
            for (int i = 0; i < 20; i++)
            {
                GameObject item = Instantiate(boomPrefab);
                item.SetActive(false); // 활성화하지 않음
                item.transform.SetParent(itemParentObject); // 풀 위치에 부모 설정
                BoomPool.Add(item); // 아이템 풀 리스트에 추가
            }
        }
    }

    public void CreateQuestPrefab(int index, int count) //추후 범위제어할때 인수좀더 넣을 것.
    {
        for (int i = index; i < count; i++) 
        {
            for (int j = 0; j < CubeCount; j++)
            {
                if(VeneziaManager.Instance.play_mode== PlayMode.Solo)
                {
                    if (VeneziaManager.Instance.game_Type == Game_Type.C)
                    {
                        GameObject QuestPrefab = Instantiate(cube); // 한글 오브젝트 생성
                        Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                        Sprite sprite = VeneziaManager.Instance.Sprites_K[i];
                        Cube_sprite.Sprite = sprite;
                        QuestPrefab.SetActive(false);
                        QuestPrefab.transform.SetParent(cubeParentObject);
                        CubePool.Add(QuestPrefab);
                    }
                    else if (VeneziaManager.Instance.game_Type == Game_Type.D)
                    {
                        GameObject QuestPrefab = Instantiate(cube); // 영어 오브젝트 생성
                        Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                        Sprite sprite = VeneziaManager.Instance.Sprites_E[i];
                        Cube_sprite.Sprite = sprite;
                        QuestPrefab.SetActive(false);
                        QuestPrefab.transform.SetParent(cubeParentObject);
                        CubePool.Add(QuestPrefab);
                    }
                    else
                    {
                        GameObject QuestPrefab = Instantiate(cube); // 한자 오브젝트 생성
                        Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                        Sprite sprite = VeneziaManager.Instance.Sprites_H[i];
                        Cube_sprite.Sprite = sprite;
                        QuestPrefab.SetActive(false);
                        QuestPrefab.transform.SetParent(cubeParentObject);
                        CubePool.Add(QuestPrefab);
                    }
                }
                else
                {
                    if (VeneziaManager.Instance.game_Type == Game_Type.C)
                    {
                        //첫번째 풀에 문제 삽입
                        GameObject QuestPrefab = Instantiate(cube); // 한글 오브젝트 생성
                        Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                        Sprite sprite = VeneziaManager.Instance.Sprites_K[i];
                        Cube_sprite.Sprite = sprite;
                        QuestPrefab.SetActive(false);
                        QuestPrefab.transform.SetParent(cubeParentObject);
                        CubePool.Add(QuestPrefab);
                        //두번째 풀에 문제 삽입
                        GameObject QuestPrefab2 = Instantiate(cube); // 한글 오브젝트 생성
                        Cube Cube_sprite2 = QuestPrefab2.GetComponent<Cube>();
                        Sprite sprite2 = VeneziaManager.Instance.Sprites_K[i];
                        //정답의 판정 여부를 주기 위해 타입을 지정해 줄 것. defalt가 one이기 때문에 player1은 지정x
                        Cube_sprite2.PlayerNum = PlayerNum.Two;
                        Cube_sprite2.Sprite = sprite2;
                        QuestPrefab2.SetActive(false);
                        QuestPrefab2.transform.SetParent(cubeParentObjectTwo);
                        CubePoolTwo.Add(QuestPrefab2);
                    }
                    else if (VeneziaManager.Instance.game_Type == Game_Type.D)
                    {
                        GameObject QuestPrefab = Instantiate(cube); // 영어 오브젝트 생성
                        Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                        Sprite sprite = VeneziaManager.Instance.Sprites_E[i];
                        Cube_sprite.Sprite = sprite;
                        QuestPrefab.SetActive(false);
                        QuestPrefab.transform.SetParent(cubeParentObject);
                        CubePool.Add(QuestPrefab);
                        //두번째
                        GameObject QuestPrefab2 = Instantiate(cube); // 한글 오브젝트 생성
                        Cube Cube_sprite2 = QuestPrefab2.GetComponent<Cube>();
                        Sprite sprite2 = VeneziaManager.Instance.Sprites_E[i];
                        Cube_sprite2.PlayerNum = PlayerNum.Two;
                        Cube_sprite2.Sprite = sprite2;
                        QuestPrefab2.SetActive(false);
                        QuestPrefab2.transform.SetParent(cubeParentObjectTwo);
                        CubePoolTwo.Add(QuestPrefab2);
                    }
                    else
                    {
                        GameObject QuestPrefab = Instantiate(cube); // 한자 오브젝트 생성
                        Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                        Sprite sprite = VeneziaManager.Instance.Sprites_H[i];
                        Cube_sprite.Sprite = sprite;
                        QuestPrefab.SetActive(false);
                        QuestPrefab.transform.SetParent(cubeParentObject);
                        CubePool.Add(QuestPrefab);
                        //두번
                        GameObject QuestPrefab2 = Instantiate(cube); // 한글 오브젝트 생성
                        Cube Cube_sprite2 = QuestPrefab2.GetComponent<Cube>();
                        Sprite sprite2 = VeneziaManager.Instance.Sprites_H[i];
                        Cube_sprite2.PlayerNum = PlayerNum.Two;
                        Cube_sprite2.Sprite = sprite2;
                        QuestPrefab2.SetActive(false);
                        QuestPrefab2.transform.SetParent(cubeParentObjectTwo);
                        CubePoolTwo.Add(QuestPrefab2);
                    }
                }
            }
        }
    }


    public void ItemCreatForveneGameMode()
    {
        if (VeneziaManager.Instance.play_mode== PlayMode.Solo)
        {
            StartCoroutine(Meteor_Co());
            StartCoroutine(Pause_Co());
        }
    }
}