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

public class VeneziaManager : GameSetting
{
    public static VeneziaManager Instance = null;
    //전반적인 배네치아 게임을 관리하기 위한 스크립트
    //게임을 진행하면서 정답을 판정해주고, 정답란에 표기해줄 ui를 갈아끼워줄 역할 을 수행 할 것.

    [SerializeField] private GameObject gameover; // 테스트용 게임오버 이미지
    [SerializeField] private GameObject CubeParent;

    public bool isGameover = false;

    //아이템과의 상호작용도 여기에 구현
    //문제 출제를 매니저에서 관리 , 출제 문제의 이미지와 이름을 기준으로 정답의 유무를 판단 할 수 있어야함.
    [SerializeField] public Image Quest_Img;
    [SerializeField] private TextMeshProUGUI Quest_text;
    //한글 및 영어 문제에 사용 할 이미지 sprite 한글과 영어, 한자 
    [SerializeField] public Sprite[] sprites_K; 
    [SerializeField] public Sprite[] sprites_E; 
    [SerializeField] public Sprite[] sprites_H;
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
    public int QuestCount;  // 딕셔너리에 들어갈 퀘스트 갯수 //10문제 <
    public int QuestRange;
    public int RemainAnswer; // 게임 진행중 남은 정답 갯수
    public int CorrectAnswerCount; // 맞춘 정답 갯수
    public int ClickCount;
    public int LifeTime; // 게임 진행 시간

    public int PoolingCool; // 오브젝트 생성 시간

    private int index;

    public int DestroyTime;

    private int randomIndex = 0;
    private int SaverandomIndex = 999;
    //반응속도 측정 시간
    float trueReactionTime;
    float totalReactionTime;

    //최종 반응속도 저장 변수
    float ReactionTime;

    //한국어 관련 문제 데이터 저장
    public Dictionary<string, QuestData> Quest = new Dictionary<string, QuestData>();
    //영어 관련 문제 데이터 저장
    public Dictionary<string, QuestData> QuestEnglish = new Dictionary<string, QuestData>();
    //한자 관련 문제 
    public Dictionary<string, QuestData> QuestHanja = new Dictionary<string, QuestData>();
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
        //1 3 4 B D E 한글 영어 한좌
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
            for (int i = 0; i < sprites_K.Length; i++)
            {
                string key = EnglishWord[i];
                QuestData data = new QuestData(sprites_K[i], EnglishWord[i]);
                Quest.Add(key, data);
            }
        }
        else
        {
            //Todo : 한자 문제 셋팅 해주세요..........
        }

        gameover.SetActive(false);

        Set_QuestCount();
        RemainAnswer = QuestCount;
        totalReactionTime = 0;
        trueReactionTime = 0;
        CorrectAnswerCount = 0;
        ClickCount = 0;
        ObjectPooling.Instance.CreateQuestPrefab(index, 10); //Todo: 임시 0~9 10개 <
        DisplayRandomQuest();
        //시간 시작 
        StartTime();
        ObjectPooling.Instance.StartCubePooling_co();

        


    }
    private void Update()
    {
        //  GameStop();
        Click_Obj();
        if (TimeSlider.Instance.startTime <= 0)
        {
            GameOver();
        }
    }
    //오브젝트 클릭시 입력처리
    //|| (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    private void Click_Obj()
    {
       // CheckCubeTypes();
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {
                //print(hit.collider.gameObject.name);
                //큐브 오브젝트 판단
                Cube Questprefab = hit.collider.gameObject.GetComponent<Cube>();
                if (Questprefab != null && Questprefab.objectType == ObjectType.CorrectAnswer) // 큐브를 눌렀을때 Quest 인지 알아야함
                {
                    if (QuestCount > -1)
                    {
                        Score.Instance.Get_FirstScore();
                        RemainAnswer--;
                        CorrectAnswerCount++;
                        ClickCount++;
                        trueReactionTime += totalReactionTime;
                        totalReactionTime = 0;
                        NextQuest();
                    }

                    if (RemainAnswer == 0) // 정답을 모두 맞췄을때 게임 종료
                    {
                        GameOver();
                    }

                    ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                }
                else if (Questprefab != null && Questprefab.objectType != ObjectType.CorrectAnswer)
                {
                    //print("오답클릭!");
                    ClickCount++;
                    ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                    totalReactionTime = 0;
                    TimeSlider.Instance.DecreaseTime_Item(5);
                }
                else
                {
                    // 아이템 오브젝트 판단
                    ItemFnc item = hit.collider.gameObject.GetComponent<ItemFnc>();
                    if (item.veneziaItem == VeneziaItem.Meteor) // veneziaItem이 메테오인 경우
                    {
                        //print("메테오 클릭!"); 
                        ObjectPooling.Instance.MeteorPool.Add(hit.collider.gameObject);
                        TimeSlider.Instance.DecreaseTime_Item(5);
                        hit.collider.gameObject.SetActive(false);
                    }

                    if (item.veneziaItem == VeneziaItem.Pause) // Pause
                    {
                        //print("멈춰!");
                        ObjectPooling.Instance.PausePool.Add(hit.collider.gameObject);
                        hit.collider.gameObject.SetActive(false);
                        StartCoroutine(Pause_co());
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
        StartSet();
        switch (step)
        {
            case 1:
                index = 0;
                NextQuest();
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
    }
    protected override void Level_2(int step)
    {
        StartSet();
        switch (step)
        {
            case 1:
                index = 30;
                NextQuest();
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
    }
    protected override void Level_3(int step)
    {
        StartSet();
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
        QuestData randomQuest = GetRandomQuest();
        if (randomQuest == null) return;  //퀘스트가 전부 출제 되었을 때 다음문제 실행 방지
        Quest_Img.sprite = randomQuest.sprite;
        Quest_text.text = randomQuest.description;
    }

    private QuestData GetRandomQuest()
    {
        if (QuestCount == 0) return null; //null 값 처리 (문제가 없을 때 사용)
        //QuestKorean.Count
        QuestData[] questArray = new QuestData[Quest.Count];
        Quest.Values.CopyTo(questArray, 0);
        QuestCount--;
        QuestData selectedQuest;
        //연속 출제를 방지
        while (randomIndex == SaverandomIndex)
        {
            randomIndex = Random.Range(0, ((QuestRange)));  // Todo : prototype이 아닌 cbt 제작 과정에서는 0 < 부분을 스텝에 맞는 인덱스를 가져 올 수 있도록 설정 변경 할 것.
        }
        // 퀘스트가 1개남은상태에서 들어오면 QuestCount에의해 -- 되어 0개가된다. 01234<
        selectedQuest = questArray[randomIndex]; // 
        SaverandomIndex = randomIndex;
        // 문제가 2개이상 있는 경우에는 , 가장 뒤에있는 문제를 제외한 문제중 하나를 출제
        // 단, 문제가 1개가 남았을 때는 그녀석을 출제 
        foreach (var kvp in Quest)
        {
            if (kvp.Value == selectedQuest)
            {
                Quest.Remove(kvp.Key);
                Quest.Add(kvp.Key, kvp.Value);
                break;
                //선택된 문제를 출제하고 다시 저장을 1번만 실행
            }
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
                PoolingCool = 5;
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
    }
    #endregion
    private void GameOver()
    {
        if (isGameover) return;
        TimeSlider.Instance.startTime = 0;
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
        if (ObjectPooling.Instance.cubePool.Count != 0)
        {
            StopCoroutine(ObjectPooling.Instance.CubePooling);
            ObjectPooling.Instance.ReStartCubePooling_co();
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
