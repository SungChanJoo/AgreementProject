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
    [SerializeField] private GameObject CubeParent;

    public bool isGameover = false;

    //�����۰��� ��ȣ�ۿ뵵 ���⿡ ����
    //���� ������ �Ŵ������� ���� , ���� ������ �̹����� �̸��� �������� ������ ������ �Ǵ� �� �� �־����.
    [SerializeField] public Image Quest_Img;
    [SerializeField] private TextMeshProUGUI Quest_text;
    //�ѱ� �� ���� ������ ��� �� �̹��� sprite �ѱ۰� ����� �������� ����ϰ� , ���ڴ� ���� 
    [SerializeField] private Sprite[] sprites_KE;    // 1������ ~ 5�������� step1 ,  step 2�� 
    private string[] KorWord = { "��" , "��", "��", "��", "�ϸ�", "ǥ��", "�Ҵ�",
            "Ÿ��", "��ī", "ġŸ", "����", "����", "����", "����", "����", "�Ǿ�", "����", "�罿", "����", "�⸰"};
    private string[] EnglishWord =
    {
        "Crane", "Horse", "Chicken", "Bear", "Hippo", "Leopard", "Panda",
        "Ostrich", "Quokka", "Cheetah", "Sparrow", "Swallow", "Cow", "Goat", "Fox",
        "Crocodile", "Lion", "Deer", "Pig", "Giraffe"
    };
    public int QuestCount;  // ��ųʸ��� �� ����Ʈ ���� //10���� <
    public int QuestRange;
    public int RemainAnswer; // ���� ������ ���� ���� ����
    public int CorrectAnswerCount; // ���� ���� ����
    public int ClickCount;
    public int LifeTime; // ���� ���� �ð�

    public int DestroyTime;

    private int randomIndex = 0;
    private int SaverandomIndex = 999;
    //�����ӵ� ���� �ð�
    float trueReactionTime;
    float totalReactionTime;

    //���� �����ӵ� ���� ����
    float ReactionTime;

    //�ѱ��� ���� ���� ������ ����
    public Dictionary<string, QuestData> Quest = new Dictionary<string, QuestData>();
    //���� ���� ���� ������ ����
    public Dictionary<string, QuestData> QuestEnglish = new Dictionary<string, QuestData>();
    //���� ���� ���� 
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
        //1 3 4 B D E �ѱ� ���� ����
        if (game_Type == Game_Type.B)
        {
            for (int i = 0; i < sprites_KE.Length; i++)
            {
                string key = KorWord[i];
                QuestData data = new QuestData(sprites_KE[i], KorWord[i]);
                Quest.Add(key, data);
            }
        }
        else if (game_Type == Game_Type.D)
        {
            for (int i = 0; i < sprites_KE.Length; i++)
            {
                string key = EnglishWord[i];
                QuestData data = new QuestData(sprites_KE[i], EnglishWord[i]);
                Quest.Add(key, data);
            }
        }
        else
        {
            //Todo : ���� ���� ���� ���ּ���..........
        }

        gameover.SetActive(false);

        Set_QuestCount();
        RemainAnswer = QuestCount;
        totalReactionTime = 0;
        trueReactionTime = 0;
        CorrectAnswerCount = 0;
        ClickCount = 0;
        ObjectPooling.Instance.CreateQuestPrefab(1); //Todo: �ӽ�
        DisplayRandomQuest();
        //�ð� ���� 
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
    //������Ʈ Ŭ���� �Է�ó��
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
                print(hit.collider.gameObject.name);
                //ť�� ������Ʈ �Ǵ�
                Cube Questprefab = hit.collider.gameObject.GetComponent<Cube>();
                if (Questprefab != null && Questprefab.objectType == ObjectType.CorrectAnswer) // ť�긦 �������� Quest ���� �˾ƾ���
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

                    if (RemainAnswer == 0) // ������ ��� �������� ���� ����
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
        TimeSlider.Instance.StartTime();
        TimeSlider.Instance.TimeStop = false;

    }
    #region Level����
    protected override void Level_1(int step)
    {
        QuestRange = 5;
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
        QuestRange = 10;
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
    protected override void Level_3(int step)
    {
        QuestRange = 20;
        switch (step)
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
    #endregion

    #region Quest����
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
        if (randomQuest == null) return;  //����Ʈ�� ���� ���� �Ǿ��� �� �������� ���� ����
        Quest_Img.sprite = randomQuest.sprite;
        Quest_text.text = randomQuest.description;
    }

    private QuestData GetRandomQuest()
    {
        if (QuestCount == 0) return null; //null �� ó�� (������ ���� �� ���)
        //QuestKorean.Count
        QuestData[] questArray = new QuestData[Quest.Count];
        Quest.Values.CopyTo(questArray, 0);
        QuestCount--;
        QuestData selectedQuest;
        //���� ������ ����
        while (randomIndex == SaverandomIndex)
        {
            randomIndex = Random.Range(0, ((QuestRange)));  // Todo : prototype�� �ƴ� cbt ���� ���������� 0 < �κ��� ���ܿ� �´� �ε����� ���� �� �� �ֵ��� ���� ���� �� ��.
        }
        // ����Ʈ�� 1���������¿��� ������ QuestCount������ -- �Ǿ� 0�����ȴ�. 01234<
        selectedQuest = questArray[randomIndex]; // 
        SaverandomIndex = randomIndex;
        // ������ 2���̻� �ִ� ��쿡�� , ���� �ڿ��ִ� ������ ������ ������ �ϳ��� ����
        // ��, ������ 1���� ������ ���� �׳༮�� ���� 
        foreach (var kvp in Quest)
        {
            if (kvp.Value == selectedQuest)
            {
                Quest.Remove(kvp.Key);
                Quest.Add(kvp.Key, kvp.Value);
                break;
                //���õ� ������ �����ϰ� �ٽ� ������ 1���� ����
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
                DestroyTime = 10;
                break;
            case 180:
                QuestCount = 14; // ���� ��
                DestroyTime = 10;
                break;
            case 300:
                QuestCount = 20; // ���� ��
                DestroyTime = 10;
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
        //�����ӵ� �Ҵ�
        reactionRate = ReactionTime;
        answersCount = CorrectAnswerCount;
        //����� ���
        AnswerRate();
        //���ǥ ���
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
