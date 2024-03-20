using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestData
{
    public Sprite sprite;
    public string description;

    public QuestData(Sprite sprite, string description)
    {
        this.sprite = sprite;
        this.description = description;
    }
}

public enum VeneGameMode
{
    Sole, Couple
}

public class VeneziaManager : GameSetting
{
    public static VeneziaManager Instance = null;
    //전반적인 배네치아 게임을 관리하기 위한 스크립트
    //게임을 진행하면서 정답을 판정해주고, 정답란에 표기해줄 ui를 갈아끼워줄 역할 을 수행 할 것.
    [Header("veneGameOver")]
    [SerializeField] private GameObject veneGameOver;


    [SerializeField] private GameObject CubeParent;

    public VeneGameMode veneGameMode;


    public bool isGameover = false;

    //아이템과의 상호작용도 여기에 구현
    //문제 출제를 매니저에서 관리 , 출제 문제의 이미지와 이름을 기준으로 정답의 유무를 판단 할 수 있어야함.
    [Header("PlayerOne")]
    [SerializeField] public Image Quest_Img;
    [SerializeField] private TextMeshProUGUI Quest_text;
    [Header("PlayerTwo")]
    [SerializeField] public Image Quest2_Img;
    [SerializeField] private TextMeshProUGUI Quest2_text;
    //한글 및 영어 문제에 사용 할 이미지 sprite 한글과 영어, 한자 
    [SerializeField] public Sprite[] sprites_K; 
    [SerializeField] public Sprite[] sprites_E; 
    [SerializeField] public Sprite[] sprites_H;

    public float StartSpeed;
    public int MaxTouchCount;
    public float AccelerationSpeed;

    //커플모드일때 Win/Lose 판단

    private string[] KorWord =
    {"학","말","닭","곰","하마","표범","팬더","타조","쿼카","치타","참새",
     "제비","젖소","염소","여우","악어","사자","사슴","돼지","기린","개미","오리",
     "문어","공작","배","나비","당근","오이","감자","로봇","장미","튤립","수국",
     "짬뽕","우동","체리","매미","버섯","호박","거미","사과","꿀벌","조개","가지",
     "수박","꽃게","포도","레몬","까치","우유","치즈","택시","칫솔","트럭","요트",
     "버스","주스","다람쥐","휴지","기차","자전거","자동차","독수리","원숭이","코뿔소","짜장면",
     "부침개","비빔밥","떡볶이","탕수육","열기구","크레인","코끼리","부엉이","비행기","경찰차","잠수함",
     "고구마","호랑이","얼룩말","바나나","소방차","애벌레","사마귀","지하철","잠자리","소라게","캥거루",
     "돛단배","달팽이","브로콜리","무당벌레","파인애플","사슴벌레","딱다구리","구급차","지렁이","헬리콥터","오토바이",
     "불가사리","슈퍼마켓","피뿔고둥","해바라기","말뚝망둥어","큰구슬우렁이"};

    private string[] EnglishWord =
    {"pig","ant","fox","bee","car","bus","owl","deer","ship","duck","milk","crap",
     "clam","bear","cake","taxi","goat","lion","pear","rose","udon","apple","grape","panda",
     "lemon","juice","train","pizza","tiger","eagle","truck","zebra","tulip","robot","crane",
     "snail","larva","plane","sloth","hippo","yacht","koala","donut","horse","spider","carrot",
     "cosmos","cherry","potato","monkey","yogurt","tissue","banana","mantis","subway","cheese",
     "brocoli","quokka","giraffe","pumpkin","bicycle","peacock","octopus","chicken","cheetah",
     "swallow","millkcow","meerkat","eggplant","anteater","platypus","elephant","ladybug","leopard",
     "starfish","icecream","butterfly","sandwich","sparrow","cucumber","airballon","hospital","squirrel",
     "crocodile","mushroom","policecar","dragonfly","pharmacy","kangaroo","forsythia","sunflower","pineapple",
     "watermelon","earthworm","sweetpotato","submarine","woodpecker","ambulance","motorcycle","supermarket",
     "viviparidae","dandelion","hermitcrap","hydrangea","rapanavenosa"};
    private string[] HanJa =
    {
    "한 일", "두 이", "석 삼", "넋 사", "다섯 오", "여섯 육", "일곱 칠", "여덟 팔", "아홉 구", "열 십", "일백 백",
    "일천 천", "일만 만", "어미 모", "아비 부", "여자 녀", "아들 자", "달 월", "불 화", "물 수", "나무 목",
    "쇠 금", "흙 토", "날 일", "동녘 동", "서녘 서", "남녘 남", "북녘 북", "하늘 천", "땅 지", "작을 소",
    "가운데 중", "큰 대", "긴 장", "짧을 단", "수레 차", "말 마", "소 우", "눈 목", "입 구", "뫼 산",
    "내 천", "바다 해", "바깥 외", "안 내", "집 가", "맏 형", "임금 왕", "갈 왕", "올 래", "근본 본",
    "밭 전", "앞 전", "뒤 후", "오른 우", "왼 좌", "비 우", "번개 전", "눈 설", "나라 국", "나라이름 한",
    "백성 민", "돌 석", "높을 고", "벗 우", "사람 인", "장인 공", "늙을 로", "길 도", "글월 문"
    };

    public int QuestCount;  // 딕셔너리에 들어갈 퀘스트 갯수 //10문제 <
    public int QuestRange;
    public int RemainAnswer; // 게임 진행중 남은 정답 갯수
    public int CorrectAnswerCount; // 맞춘 정답 갯수
    //1인모드일때는 Click / 2인일때는 두개사용
    public int ClickCount;
    public int Click_SecondPlayerCount;
    public int LifeTime; // 게임 진행 시간

    public int PoolingCool; // 오브젝트 생성 시간

    private int index;
    private int QuestIndex;

    public int limitCount;

    public int DestroyTime;

    private int randomIndex = 0;
    private int randomIndexSecond = 0;
    private int[] SaverandomIndex = new int[] { 999, 999 };
    //반응속도 측정 시간
    float trueReactionTime;
    float totalReactionTime;

    //최종 반응속도 저장 변수
    float ReactionTime;

    //문제 데이터 저장
    public Dictionary<string, QuestData> Quest = new Dictionary<string, QuestData>();

    public bool isFirstPlayerTouch = false;
    private bool isStart = false;

    //커플모드일때 누가 클릭했는지 판단
    bool isFirst = false;
    bool isSecond =false;
    bool isNull = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void StartSet()
    {
        //1 3 4 B D E 한글 영어 한좌veneGameMode == VeneGameMode.Sole
        if (true)
        {
            if (game_Type == Game_Type.C) //한글
            {
                for (int i = 0; i < sprites_K.Length; i++)
                {
                    string key = KorWord[i];
                    QuestData data = new QuestData(sprites_K[i], KorWord[i]);
                    Quest.Add(key, data);
                }
            }
            else if (game_Type == Game_Type.D) //영어 
            {
                for (int i = 0; i < sprites_E.Length; i++)
                {
                    string key = EnglishWord[i];
                    QuestData data = new QuestData(sprites_E[i], EnglishWord[i]);
                    Quest.Add(key, data);
                }
            }
            else
            {
                for (int i = 0; i < sprites_H.Length; i++)
                {
                    string key = HanJa[i];
                    QuestData data = new QuestData(sprites_H[i], HanJa[i]);
                    Quest.Add(key, data);
                }
            }
        }

        QuestIndex = (game_Type == Game_Type.E) ? 20 : 10;
        Set_QuestCount();
        RemainAnswer = QuestCount;
        totalReactionTime = 0;
        trueReactionTime = 0;
        CorrectAnswerCount = 0;
        ClickCount = 0;
        
        //퀘스트 오브젝트 생성
        if(veneGameMode == VeneGameMode.Sole)
        {
            ObjectPooling.Instance.CreateQuestPrefab(index, QuestIndex + index);
            limitCount = ObjectPooling.Instance.cubePool.Count - QuestRange;
        }
        else
        {
            limitCount = (game_Type == Game_Type.E) ? 65 : 100;
            int indexSet = (game_Type == Game_Type.E) ? 70 : 105;
            ObjectPooling.Instance.CreateQuestPrefab(0, indexSet);
        }
        
        DisplayRandomQuest();
        //시간 시작 
        StartTime();
        if(veneGameMode == VeneGameMode.Sole)
        {
            ObjectPooling.Instance.StartCubeOne_Pooling_co();
        }
        else
        {
            ObjectPooling.Instance.StartCubeOne_Pooling_co();
            ObjectPooling.Instance.StartCubeTwo_Pooling_co();
        }
        
    }
    private void Update()
    {
        //  GameStop();
        if (!isGameover)
        {
            Click_Obj();
        }
        CanClickOn();

        GameOver();
    }

    //오브젝트 클릭시 입력처리
    //|| (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)

    private float timer = 0f;
    private bool canClick = true;
    private void CanClickOn()
    {
        // 시간을 업데이트
        if (!canClick)
        {
            timer += Time.deltaTime;

            // 0.2초가 지나면 bool 값을 true로 변경하고 타이머를 리셋
            if (timer >= 0.2f)
            {
                timer = 0f;
                canClick = true;
            }
        }

        CheckCubeTypes();
    }

    private void Click_Obj()
    {
        if (Input.GetMouseButtonDown(0) && canClick)
        {
            canClick = false;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null && !hit.collider.gameObject.CompareTag("Ground"))
                {
                    if (hit.collider.gameObject == null)
                    {
                        isNull = true;
                        return;
                    }
                    //print(hit.collider.gameObject.name);
                    //큐브 오브젝트 판단
                    Cube Questprefab = hit.collider.gameObject.GetComponent<Cube>();

                    if (Questprefab != null && Questprefab.objectType == ObjectType.CorrectAnswer) // 큐브를 눌렀을때 Quest 인지 알아야함
                    {
                        isFirstPlayerTouch = (Questprefab.playerNum == PlayerNum.One) ? true : false;
                        if (QuestCount > -1)
                        {
                            RemainAnswer--;
                            CorrectAnswerCount++;
                            trueReactionTime += totalReactionTime;
                            totalReactionTime = 0;
                            NextQuest();
                        }
                        if (isFirstPlayerTouch) // 솔로 모드에서는 One으로 지정됨
                        {
                            ClickCount++;
                            isFirst = true;
                            ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                            hit.collider.gameObject.SetActive(false);
                            if(veneGameMode == VeneGameMode.Couple)
                            {
                                ObjectPooling.Instance.CreateBoom(ObjectPooling.Instance.PlayerOne_Boom);
                            }
                        }
                        else
                        {
                            Click_SecondPlayerCount++;
                            isSecond = true;
                            ObjectPooling.Instance.cubePoolTwo.Add(hit.collider.gameObject);
                            hit.collider.gameObject.SetActive(false);
                            ObjectPooling.Instance.CreateBoom(ObjectPooling.Instance.PlayerTwo_Boom);
                        }


                        if (RemainAnswer == 0) // 정답을 모두 맞췄을때 게임 종료
                        {
                            GameClear();
                        }
                    }
                    else if (Questprefab != null && Questprefab.objectType != ObjectType.CorrectAnswer)
                    {
                        //print("오답클릭!");
                        isFirstPlayerTouch = (Questprefab.playerNum == PlayerNum.One) ? true : false;
                        if (veneGameMode == VeneGameMode.Sole) ClickCount++; // 오답은 1인모드일때만 체크
                        if (isFirstPlayerTouch)
                        {
                            isFirst = true;
                            ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                            hit.collider.gameObject.SetActive(false);
                            TimeSlider.Instance.DecreaseTime_Item(5);
                        }
                        else
                        {
                            isSecond = true;
                            ObjectPooling.Instance.cubePoolTwo.Add(hit.collider.gameObject);
                            hit.collider.gameObject.SetActive(false);
                            TimeSlider.Instance.DecreaseTime_CoupleMode(5, TimeSlider.Instance.slider_PlayerTwo);
                        }
                        totalReactionTime = 0;
                    }
                    else
                    {
                        // 아이템 오브젝트 판단
                        ItemFnc item = hit.collider.gameObject.GetComponent<ItemFnc>();
                        if (veneGameMode == VeneGameMode.Sole && item != null) // veneziaItem이 메테오인 경우
                        {
                            if (item.veneziaItem == VeneziaItem.Meteor)
                            {
                                //print("메테오 클릭!"); 
                                ObjectPooling.Instance.MeteorPool.Add(hit.collider.gameObject);
                                TimeSlider.Instance.DecreaseTime_Item(5);
                                hit.collider.gameObject.SetActive(false);
                            }
                            else if (item.veneziaItem == VeneziaItem.Pause)
                            {
                                //print("멈춰!");
                                ObjectPooling.Instance.PausePool.Add(hit.collider.gameObject);
                                hit.collider.gameObject.SetActive(false);
                                StartCoroutine(Pause_co());
                            }
                        }
                        else
                        {
                            if(item != null)
                            {
                                if (item.veneziaItem == VeneziaItem.Boom && item.boomType == BoomType.PlayerOne)
                                {
                                    ObjectPooling.Instance.BoomPool.Add(hit.collider.gameObject);
                                    hit.collider.gameObject.SetActive(false);
                                    TimeSlider.Instance.DecreaseTime_CoupleMode(5, TimeSlider.Instance.slider_PlayerTwo);
                                }
                                else if (item.veneziaItem == VeneziaItem.Boom && item.boomType == BoomType.PlayerTwo)
                                {
                                    ObjectPooling.Instance.BoomPool.Add(hit.collider.gameObject);
                                    hit.collider.gameObject.SetActive(false);
                                    TimeSlider.Instance.DecreaseTime_CoupleMode(5, TimeSlider.Instance.slider);
                                }
                            }
                        }
                    }
                }
            }
            ResetCube();
        }
    }

    IEnumerator Pause_co()
    {
        TimeSlider.Instance.isStop = true;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(5f);
        TimeSlider.Instance.isStop = false;
        Time.timeScale = 1;
    }
    private void StartTime()
    {
        isStop = false;
        Start_Btn();
        TimeSlider.Instance.StartTime();
        TimeSlider.Instance.TimeStop = false;

    }
    #region Level설정
    protected override void Level_1(int step)
    {
        GetIndex(1, step);
        StartSet();
        GetSpeed(1, step);
        switch (step)
        {
            case 1:
                NextQuest();
                break;
            case 2:
                NextQuest();
                break;
            case 3:
                NextQuest();
                break;
            case 4:
                NextQuest();
                break;
            case 5:
                NextQuest();
                break;
            case 6:
                NextQuest();
                break;
            default:
                break;
        }
    }
    protected override void Level_2(int step)
    {
        GetIndex(2, step);
        GetSpeed(2, step);
        StartSet();
        switch (step)
        {
            case 1:
                NextQuest();
                break;
            case 2:
                NextQuest();
                break;
            case 3:
                NextQuest();
                break;
            case 4:
                NextQuest();
                break;
            case 5:
                NextQuest();
                break;
            case 6:
                NextQuest();
                break;
            default:
                break;
        }
    }

    protected override void Level_3(int step)
    {
        GetIndex(3, step);
        GetSpeed(3, step);
        StartSet();
        switch (step)
        {
            case 1:
                NextQuest();
                break;
            case 2:
                NextQuest();
                break;
            case 3:
                NextQuest();
                break;
            case 4:
                NextQuest();
                break;
            case 5:
                NextQuest();
                break;
            case 6:
                NextQuest();
                break;
            default:
                break;
        }
    }
    #endregion

    #region Quest설정
    public void DisplayRandomQuest()
    {
        if (QuestCount == 0)
        {
            Time.timeScale = 0;
            ObjectPooling.Instance.StopAllCoroutines();
            return;
        }

    }

    public void NextQuest()
    {
        QuestData[] randomQuest = GetRandomQuest();
        if (randomQuest == null) return;  //퀘스트가 전부 출제 되었을 때 다음문제 실행 방지
        if(veneGameMode == VeneGameMode.Sole)
        {
            Quest_Img.sprite = randomQuest[0].sprite;
            Quest_text.text = randomQuest[0].description;
            Quest2_Img = null;
            Quest2_text = null;
        }
        else
        {
            if (!isStart)
            {
                isStart = true;
                Quest_Img.sprite = randomQuest[0].sprite;
                Quest_text.text = randomQuest[0].description;
                Quest2_Img.sprite = randomQuest[1].sprite;
                Quest2_text.text = randomQuest[1].description;
            }
            else
            {
                //첫번 쨰 플레이어가 터치 했을 경우, 첫 번쨰 플레이어의 문제만 바꾸고
                //두번째 플레이어가 터치했을 경우 두번째 플레이어의 문제만 바꿀것.
                if (isFirstPlayerTouch)
                {
                    //첫번째 플레이어
                    Quest_Img.sprite = randomQuest[0].sprite;
                    Quest_text.text = randomQuest[0].description;
                }
                else
                {
                    //두번째플레이어
                    Quest2_Img.sprite = randomQuest[1].sprite;
                    Quest2_text.text = randomQuest[1].description;
                }
            }

        }
        


    }

    private QuestData[] GetRandomQuest()
    {
        if (QuestCount == 0) return null; //null 값 처리 (문제가 없을 때 사용)
        QuestData[] questArray = new QuestData[Quest.Count];
        Quest.Values.CopyTo(questArray, 0);
        QuestCount--;
        QuestData[] selectedQuest = new QuestData[2];
        //연속 출제를 방지
        if(veneGameMode == VeneGameMode.Sole)
        {
            randomIndex = Random.Range(index, QuestIndex + index); //첫 출제시 랜덤 인댁스 정하기 
            while (randomIndex == SaverandomIndex[0])
            {
                randomIndex = Random.Range(index, 10 + index);
            }
            SaverandomIndex[0] = randomIndex;
            selectedQuest[0] = questArray[randomIndex];
        }
        else
        {
            //2인 모드일 때는 2가지의 정보가 필요
            randomIndex = Random.Range(0, ObjectPooling.Instance.cubePool.Count); 
            randomIndexSecond = Random.Range(0, ObjectPooling.Instance.cubePoolTwo.Count);
            while (randomIndex == SaverandomIndex[0])
            {
                randomIndex = Random.Range(0, ObjectPooling.Instance.cubePool.Count);
            }
            while (randomIndexSecond == SaverandomIndex[1])
            {
                randomIndexSecond = Random.Range(0, ObjectPooling.Instance.cubePoolTwo.Count);
            }
            SaverandomIndex[0] = randomIndex;
            SaverandomIndex[1] = randomIndexSecond;

            selectedQuest[0] = questArray[randomIndex];
            selectedQuest[1] = questArray[randomIndexSecond];
        }        
        return selectedQuest;
    }
    private void Set_QuestCount()
    {
        switch (timeSet)
        {
            case 60:
                QuestCount = 10;
                QuestRange = 5;
                DestroyTime = 10;
                PoolingCool = 1;
                break;
            case 180:
                QuestCount = 14; 
                QuestRange = 7;
                DestroyTime = 20;
                PoolingCool = 5;
                break;
            case 300:
                QuestCount = 20; 
                QuestRange = 10;
                DestroyTime = 30;
                PoolingCool = 5;
                break;
            default:
                break;
        }

        if(veneGameMode == VeneGameMode.Couple)
        {
            QuestCount = 9999; //2인모드에서는 퀘스트 갯수 제한x
        }
    }
    #endregion
    //타임아웃 게임오버
    private void GameOver()
    {
        if (isGameover) return;
        if ((veneGameMode == VeneGameMode.Sole && TimeSlider.Instance.slider.value <= 0))
        {
            isGameover = true;
            totalQuestions = ClickCount;
            ReactionTime = trueReactionTime / CorrectAnswerCount;
            //반응속도 할당
            reactionRate = ReactionTime;
            answersCount = CorrectAnswerCount;
            //정답률 계산
            AnswerRate();
            //결과표 출력
            EndGame();
        }
        else if(veneGameMode == VeneGameMode.Couple)
        {
            if(TimeSlider.Instance.slider.value == 0 || TimeSlider.Instance.slider_PlayerTwo.value == 0)
            {
                isGameover = true;
                veneGameOver.SetActive(true);
                StopAllCoroutines();
                TimeSlider.Instance.StopAll();
            }
        }

    }
    //출제된 문제를 전부 맞췄을때 
    private void GameClear()
    {
        isGameover = true;
        totalQuestions = ClickCount;
        ReactionTime = trueReactionTime / CorrectAnswerCount;
        //반응속도 할당
        reactionRate = ReactionTime;
        answersCount = CorrectAnswerCount;
        //정답률 계산
        AnswerRate();
        //결과표 출력
        EndGame();
    }
    private void AnswerRate()
    {
        if (CorrectAnswerCount == 0)
        {
            answers = 0;
            return;
        }
        answers = CorrectAnswerCount * 100 / ClickCount;
    }

    public void ResetCube()
    {
        if(veneGameMode == VeneGameMode.Sole)
        {
            bool isFirst = true;
            if (ObjectPooling.Instance.cubePool.Count > limitCount && isFirst)
            {
                isFirst = false;
                StopCoroutine(ObjectPooling.Instance.CubePooling_PlayerOne);
                ObjectPooling.Instance.ReStartCubePooling_One_co();
            }
            else if (ObjectPooling.Instance.cubePool.Count > limitCount && !isFirst)
            {
                StopCoroutine(ObjectPooling.Instance.CubeRestartPooling_One);
                ObjectPooling.Instance.ReStartCubePooling_One_co();
            }
        }
        else
        {
            //첫번째 플레이어
            bool isFirst = true;
            bool isSecond = true;
            if (ObjectPooling.Instance.cubePool.Count > limitCount && isFirst) 
            {
                isFirst = false;
                StopCoroutine(ObjectPooling.Instance.CubePooling_PlayerOne);
                ObjectPooling.Instance.ReStartCubePooling_One_co();
            }
            else if (ObjectPooling.Instance.cubePool.Count > limitCount && !isFirst)
            {
                StopCoroutine(ObjectPooling.Instance.CubeRestartPooling_One);
                ObjectPooling.Instance.ReStartCubePooling_One_co();
            }

            //두번째 플레이어
            if(ObjectPooling.Instance.cubePoolTwo.Count > limitCount && isSecond)
            {
                isSecond = false;
                StopCoroutine(ObjectPooling.Instance.CubePooling_PlayerTwo);
                ObjectPooling.Instance.ReStartCubePooling_Two_co();
            }
            else if (ObjectPooling.Instance.cubePoolTwo.Count > limitCount && !isSecond)
            {
                StopCoroutine(ObjectPooling.Instance.CubeRestartPooling_Two);
                ObjectPooling.Instance.ReStartCubePooling_Two_co();
            }

        }
 
    }
      

    //문제 인댁스 설정   
    private void GetIndex(int lv, int step)
    {
        if (game_Type == Game_Type.C || game_Type == Game_Type.D)
        {
            switch (lv)
            {
                case 1:
                    switch (step)
                    {
                        case 1:
                            index = 0;
                            break;
                        case 2:
                            index = 5;
                            break;
                        case 3:
                            index = 10;
                            break;
                        case 4:
                            index = 15;
                            break;
                        case 5:
                            index = 20;
                            break;
                        case 6:
                            index = 25;
                            break;
                        default:
                            break;
                    }
                    break;
                case 2:
                    switch (step)
                    {
                        case 1:
                            index = 30;
                            break;
                        case 2:
                            index = 35;
                            break;
                        case 3:
                            index = 40;
                            break;
                        case 4:
                            index = 45;
                            break;
                        case 5:
                            index = 50;
                            break;
                        case 6:
                            index = 55;
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    switch (step)
                    {
                        case 1:
                            index = 60;
                            break;
                        case 2:
                            index = 65;
                            break;
                        case 3:
                            index = 70;
                            break;
                        case 4:
                            index = 75;
                            break;
                        case 5:
                            index = 80;
                            break;
                        case 6:
                            index = 85;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {   //한자는 lv가 없음 step으로 지정
            switch (step)
            {
                case 1:
                    index = 0;
                    break;
                case 2:
                    index = 10;
                    break;
                case 3:
                    index = 20;
                    break;
                case 4:
                    index = 30;
                    break;
                case 5:
                    index = 40;
                    break;
                case 6:
                    index = 50;
                    break;
                default:
                    break;
            }
        }

    }
    //문제 속도 설정
    private void GetSpeed(int lv, int step)
    {
        if(veneGameMode == VeneGameMode.Sole)
        {
            if (game_Type == Game_Type.C || game_Type == Game_Type.D)
            {
                switch (lv)
                {
                    case 1:
                        switch (step)
                        {
                            case 1:
                                StartSpeed = 20;
                                MaxTouchCount = 10;
                                break;
                            case 2:
                                StartSpeed = 20;
                                MaxTouchCount = 10;
                                break;
                            case 3:
                                StartSpeed = 24;
                                MaxTouchCount = 11;
                                break;
                            case 4:
                                StartSpeed = 24;
                                MaxTouchCount = 11;
                                break;
                            case 5:
                                StartSpeed = 28;
                                MaxTouchCount = 12;
                                break;
                            case 6:
                                StartSpeed = 28;
                                MaxTouchCount = 12;
                                break;
                            default:
                                break;
                        }
                        break;
                    case 2:
                        switch (step)
                        {
                            case 1:
                                StartSpeed = 22;
                                MaxTouchCount = 11;
                                break;
                            case 2:
                                StartSpeed = 22;
                                MaxTouchCount = 11;
                                break;
                            case 3:
                                StartSpeed = 26;
                                MaxTouchCount = 12;
                                break;
                            case 4:
                                StartSpeed = 26;
                                MaxTouchCount = 12;
                                break;
                            case 5:
                                StartSpeed = 30;
                                MaxTouchCount = 13;
                                break;
                            case 6:
                                StartSpeed = 30;
                                MaxTouchCount = 13;
                                break;
                            default:
                                break;
                        }
                        break;
                    case 3:
                        switch (step)
                        {
                            case 1:
                                StartSpeed = 24;
                                MaxTouchCount = 12;
                                break;
                            case 2:
                                StartSpeed = 24;
                                MaxTouchCount = 12;
                                break;
                            case 3:
                                StartSpeed = 28;
                                MaxTouchCount = 13;
                                break;
                            case 4:
                                StartSpeed = 28;
                                MaxTouchCount = 13;
                                break;
                            case 5:
                                StartSpeed = 32;
                                MaxTouchCount = 14;
                                break;
                            case 6:
                                StartSpeed = 32;
                                MaxTouchCount = 14;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {   //한자는 lv가 없음 step으로 지정
                switch (step)
                {
                    case 1:
                        StartSpeed = 20;
                        MaxTouchCount = 10;
                        break;
                    case 2:
                        StartSpeed = 20;
                        MaxTouchCount = 10;
                        break;
                    case 3:
                        StartSpeed = 24;
                        MaxTouchCount = 11;
                        break;
                    case 4:
                        StartSpeed = 24;
                        MaxTouchCount = 11;
                        break;
                    case 5:
                        StartSpeed = 28;
                        MaxTouchCount = 12;
                        break;
                    case 6:
                        StartSpeed = 28;
                        MaxTouchCount = 12;
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            StartSpeed = 26;
            MaxTouchCount = 13;
        }
        

    }

    private void CheckCubeTypes()
    {

        for (int i = 0; i < QuestRange; i++)
        {
            if(CubeParent.transform.GetChild(i).GetComponent<Cube>().objectType == ObjectType.CorrectAnswer && CubeParent.transform.GetChild(i).gameObject.activeSelf)
            {
                totalReactionTime += Time.deltaTime;
            }
        }
    }


}