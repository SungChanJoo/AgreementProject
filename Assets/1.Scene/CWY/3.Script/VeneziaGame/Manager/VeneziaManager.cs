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
    //�������� ���ġ�� ������ �����ϱ� ���� ��ũ��Ʈ
    //������ �����ϸ鼭 ������ �������ְ�, ������� ǥ������ ui�� ���Ƴ����� ���� �� ���� �� ��.

    [SerializeField] private GameObject gameover; // �׽�Ʈ�� ���ӿ��� �̹���

    public bool isGameover = false;

    //�����۰��� ��ȣ�ۿ뵵 ���⿡ ����
    public int Level;
    public int Step;
    //���� ������ �Ŵ������� ���� , ���� ������ �̹����� �̸��� �������� ������ ������ �Ǵ� �� �� �־����.
    [SerializeField] public Image Quest_Img;
    [SerializeField] private TextMeshProUGUI Quest_text;
    //�ѱ� �� ���� ������ ��� �� �̹��� sprite �ѱ۰� ����� �������� ����ϰ� , ���ڴ� ���� 
    [SerializeField] private Sprite[] sprites_KE;    // 1������ ~ 5�������� step1 ,  step 2�� 
    private string[] KorWord = { "��" , "��", "��", "��", "�ϸ�", "ǥ��", "�Ҵ�",
            "Ÿ��", "��ī", "ġŸ", "����", "����", "����", "����", "����", "�Ǿ�", "����", "�罿", "����", "�⸰"};

    public int QuestCount;  // ��ųʸ��� �� ����Ʈ ����
    public int SaveQuestStartCountData;
    public int RemainAnswer; // ���� ������ ���� ���� ����
    public int CorrectAnswerCount; // ���� ���� ����
    public int ClickCount;
    public int LifeTime; // ���� ���� �ð�

    //�����ӵ� ���� �ð�
    float trueReactionTime;
    float totalReactionTime;

    //���� �����ӵ� ���� ����
    float ReactionTime;

    //�ѱ��� ���� ���� ������ ����
    public Dictionary<string, QuestData> QuestKorean = new Dictionary<string, QuestData>();
    //���� ���� ���� ������ ����
    public Dictionary<string, QuestData> QuestEnglish = new Dictionary<string, QuestData>();
    //���� ���� ���� 
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
        }
        gameover.SetActive(false);

        Set_QuestCount();
        RemainAnswer = QuestCount;
        SaveQuestStartCountData = QuestCount;
        totalReactionTime = 0;
        trueReactionTime = 0;
        CorrectAnswerCount = 0;
        ClickCount = 0;
        ObjectPooling.Instance.CreateQuestPrefab(SaveQuestStartCountData/2);
        DisplayRandomQuest();
        //�ð� ���� 
        StartTime();
        StartCoroutine(ObjectPooling.Instance.Cube_Co(QuestCount/2));
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
    //������Ʈ Ŭ���� �Է�ó��
    //Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began > ��ġ�Է�
    private void Click_Obj()
    {
        totalReactionTime += Time.deltaTime; // �����ӵ� ����
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {
                print(hit.collider.gameObject.name);
                //ť�� ������Ʈ �Ǵ�
                Cube Questprefab = hit.collider.gameObject.GetComponent<Cube>();
                if(Questprefab != null && Questprefab.objectType == ObjectType.CorrectAnswer) // ť�긦 �������� Quest ���� �˾ƾ���
                {
                    if(QuestCount > -1)
                    {
                        Score.Instance.Get_FirstScore();
                        RemainAnswer--;
                        CorrectAnswerCount++;
                        ClickCount++;
                        trueReactionTime += totalReactionTime;
                        totalReactionTime = 0;
                        NextQuest();
                    }
                    if(RemainAnswer == 0) // ������ ��� �������� ���� ����
                    {
                        GameOver();
                    }
                    
                    ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                }
                else if (Questprefab != null && Questprefab.objectType != ObjectType.CorrectAnswer)
                {
                    print("����Ŭ��!");
                    ClickCount++;
                    ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                    totalReactionTime = 0;
                    TimeSlider.Instance.DecreaseTime_Item(5);
                }
                else
                {
                    // ������ ������Ʈ �Ǵ�
                    ItemFnc item = hit.collider.gameObject.GetComponent<ItemFnc>();
                    if (item.veneziaItem == VeneziaItem.Meteor) // veneziaItem�� ���׿��� ���
                    {
                        print("���׿� Ŭ��!"); // �Ǵ� �ٸ� ������ ����
                        ObjectPooling.Instance.MeteorPool.Add(hit.collider.gameObject);
                        TimeSlider.Instance.DecreaseTime_Item(5);
                        hit.collider.gameObject.SetActive(false);
                    }

                    if (item.veneziaItem == VeneziaItem.Pause) // Pause
                    {
                        print("����!");
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
        if (randomQuest == null) return;  //����Ʈ�� ���� ���� �Ǿ��� �� �������� ���� ����
        Quest_Img.sprite = randomQuest.sprite;
        Quest_text.text = randomQuest.description;
    }

    private QuestData GetRandomQuest_Kr()
    {
        if (QuestCount == 0) return null; //null �� ó�� (������ ���� �� ���)
        //QuestKorean.Count
        QuestData[] questArray = new QuestData[QuestKorean.Count];
        QuestKorean.Values.CopyTo(questArray, 0);
        QuestCount--;
        QuestData selectedQuest;// ���̻��� �־ȳ�����
        //���� ������ �����ϱ� ���� �������� �ִ� ���� ���� ����
        int randomIndex = Random.Range(0, ((SaveQuestStartCountData/2)-1));  
        // ����Ʈ�� 1���������¿��� ������ QuestCount������ -- �Ǿ� 0�����ȴ�. 
        selectedQuest = questArray[randomIndex]; // 
        // ������ 2���̻� �ִ� ��쿡�� , ���� �ڿ��ִ� ������ ������ ������ �ϳ��� ����
        // ��, ������ 1���� ������ ���� �׳༮�� ���� 
        foreach (var kvp in QuestKorean)
        {
            if (kvp.Value == selectedQuest)
            {
                QuestKorean.Remove(kvp.Key);
                QuestKorean.Add(kvp.Key, kvp.Value);
                break;
                //���õ� ������ �����ϰ� �ٽ� ������ 1���� ����
            }
        }
        return selectedQuest;
    }
    private void GameOver()
    {
        isGameover = true;
        totalQuestions = ClickCount;
        ReactionTime = trueReactionTime / CorrectAnswerCount;
        //�����ӵ� �Ҵ�
        reactionRate = ReactionTime;
        answersCount = CorrectAnswerCount;
        //����� ���
        AnswerRate();
        //���ǥ ���
        EndGame();
    }

    private void Set_QuestCount()
    {
        switch (timeSet)
        {
            case 60:
                QuestCount = 10;
                break;
            case 180:
                QuestCount = 14; // ���� ��
                break;
            case 300:
                QuestCount = 20; // ���� ��
                break;
            default:
                break;
        }
        
        Debug.Log(totalQuestions);
    }
    private void AnswerRate()
    {
        answers = CorrectAnswerCount * 100 / ClickCount;
    }
}
