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

    public bool isGameover = false;

    //아이템과의 상호작용도 여기에 구현
    public int Level;
    public int Step;
    //문제 출제를 매니저에서 관리 , 출제 문제의 이미지와 이름을 기준으로 정답의 유무를 판단 할 수 있어야함.
    [SerializeField] public Image Quest_Img;
    [SerializeField] private TextMeshProUGUI Quest_text;
    //한글 및 영어 문제에 사용 할 이미지 sprite 한글과 영어는 공용으로 사용하고 , 한자는 따로 
    [SerializeField] private Sprite[] sprites_KE;    // 1번부터 ~ 5번까지는 step1 ,  step 2는 
    private string[] KorWord = { "곰" , "닭", "학", "하마", "말"};

    int randomIndex;

    public int QuestCount;  // 딕셔너리에 들어갈 퀘스트 갯수
    public int RemainAnswer; // 게임 진행중 남은 정답 갯수
    public int CorrectAnswerCount; // 맞춘 정답 갯수
    public int LifeTime; // 게임 진행 시간

    //반응속도 측정 시간
    float trueReactionTime;
    float totalReactionTime;

    //최종 반응속도 저장 변수
    float ReactionTime;

    //한국어 관련 문제 데이터 저장
    public Dictionary<string, QuestData> QuestKorean = new Dictionary<string, QuestData>();
    //영어 관련 문제 데이터 저장
    public Dictionary<string, QuestData> QuestEnglish = new Dictionary<string, QuestData>();
    //한자 관련 문제 
    public Dictionary<string, QuestData> QuestHanja = new Dictionary<string, QuestData>();
    private void Awake()
    {
        if(Instance == null)
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
        for (int i = 0; i < sprites_KE.Length; i++)
        {
            string key = KorWord[i];
            QuestData data = new QuestData(sprites_KE[i], KorWord[i]);
            QuestKorean.Add(key, data);
            // 딕셔너리에 입력되는 정보 확인용
            print("Added Quest Data: Key = " + key + ", Description = " + data.description);
        }
        gameover.SetActive(false);

        Set_QuestCount();

        RemainAnswer = QuestCount;
        totalReactionTime = 0;
        trueReactionTime = 0;
        CorrectAnswerCount = 0;
        DisplayRandomQuest();
        //시간 시작 
        StartTime();
    }
    private void Update()
    {
        //  GameStop();
        Click_Obj();        
        if(TimeSlider.Instance.startTime == 0)
        {                
            GameOver();            
        }
    }
    //오브젝트 클릭시 입력처리
    //Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began > 터치입력
    private void Click_Obj()
    {
        totalReactionTime += Time.deltaTime; // 반응속도 측정
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {
                print(hit.collider.gameObject.name);
                //큐브 오브젝트 판단
                Cube Questprefab = hit.collider.gameObject.GetComponent<Cube>();
                if(Questprefab != null && Questprefab.objectType == ObjectType.CorrectAnswer) // 큐브를 눌렀을때 Quest 인지 알아야함
                {
                    if(QuestCount > -1)
                    {
                        Score.Instance.Get_FirstScore();
                        RemainAnswer--;
                        CorrectAnswerCount++;
                        trueReactionTime += totalReactionTime;
                        totalReactionTime = 0;
                        NextQuest();
                    }
                    if(RemainAnswer == 0) // 정답을 모두 맞췄을때 게임 종료
                    {
                        GameOver();
                    }
                    
                    ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                }
                else if (Questprefab != null && Questprefab.objectType != ObjectType.CorrectAnswer)
                {
                    print("오답클릭!");
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
                        print("메테오 클릭!"); // 또는 다른 동작을 수행
                        ObjectPooling.Instance.MeteorPool.Add(hit.collider.gameObject);
                        TimeSlider.Instance.DecreaseTime_Item(5);
                        hit.collider.gameObject.SetActive(false);
                    }

                    if (item.veneziaItem == VeneziaItem.Pause) // Pause
                    {
                        print("멈춰!");
                        ObjectPooling.Instance.PausePool.Add(hit.collider.gameObject);
                        hit.collider.gameObject.SetActive(false);
                        StartCoroutine(Pause_co());
                    }
                }
            }
            if (ObjectPooling.Instance.cubePool.Count == 1)
            {
                StartCoroutine(ObjectPooling.Instance.ReStartCube_Co());
            }
        }


    }
    private void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red);
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
        TimeSlider.Instance.StartTime();
        TimeSlider.Instance.TimeStop = false;

    }
    protected override void Level_1(int step)
    {
        StartSet();        
        switch (step)
        {
            case 1:
                NextQuest();
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            default:
                break;
        }
    }
    protected override void Level_2(int step)
    {
        switch (Step)
        {
            case 1:

                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            default:
                break;
        }
    }
    protected override void Level_3(int step)
    {
        switch (Step)
        {
            case 1:

                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            default:
                break;
        }
    }

    
    public void DisplayRandomQuest()
    {
        if(QuestCount == 0)
        {

            Time.timeScale = 0;
            ObjectPooling.Instance.StopAllCoroutines();            


            
            return;
        }        

    }

    public void NextQuest()
    {
        QuestData randomQuest = GetRandomQuest_Kr();
        Quest_Img.sprite = randomQuest.sprite;
        Quest_text.text = randomQuest.description;
    }

    private QuestData GetRandomQuest_Kr()
    {
        QuestData[] questArray = new QuestData[QuestKorean.Count];
        QuestKorean.Values.CopyTo(questArray, 0);
        // 랜덤한 인덱스 선택
        randomIndex = Random.Range(0, QuestCount);
        QuestCount--;
        if(questArray.Length == 0)
        {
            return null;
        }
        QuestData selectedQuest = questArray[randomIndex];
        // 출제된 퀘스트 데이터는 딕셔너리에서 제거 => 중복 출제 방지
        foreach (var kvp in QuestKorean)
        {
            if (kvp.Value == selectedQuest)
            {
                QuestKorean.Remove(kvp.Key);
                break; // 선택된 퀘스트가 한 번만 제거되도록 break설정
            }
        }
        print("확인");
        return selectedQuest;       
    }

    private void GameOver()
    {
        isGameover = true;
        ReactionTime = trueReactionTime / CorrectAnswerCount;
        //반응속도 할당
        reactionRate = ReactionTime;
        //정답률 계산
        AnswerRate();
        //결과표 출력
        EndGame();
    }

    private void Set_QuestCount()
    {
        switch (timeSet)
        {
            case 60:
                QuestCount = 2;
                break;
            case 180:
                QuestCount = 3; // 예시 값
                break;
            case 300:
                QuestCount = 5; // 예시 값
                break;
            default:
                break;
        }
        totalQuestions = QuestCount;
        Debug.Log(totalQuestions);
    }
    private void AnswerRate()
    {
        answers = totalQuestions * 100 / CorrectAnswerCount;
    }
}
