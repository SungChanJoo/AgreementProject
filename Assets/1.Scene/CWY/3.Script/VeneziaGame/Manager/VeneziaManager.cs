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
    //�ѱ� �� ���� ������ ��� �� �̹��� sprite �ѱ۰� ����, ���� 
    [SerializeField] public Sprite[] sprites_K; 
    [SerializeField] public Sprite[] sprites_E; 
    [SerializeField] public Sprite[] sprites_H;

    public float StartSpeed;
    public int MaxTouchCount;
    public float AccelerationSpeed;

    private string[] KorWord =
    {"��","��","��","��","�ϸ�","ǥ��","�Ҵ�","Ÿ��","��ī","ġŸ","����",
     "����","����","����","����","�Ǿ�","����","�罿","����","�⸰","����","����",
     "����","����","��","����","���","����","����","�κ�","���","ƫ��","����",
     "«��","�쵿","ü��","�Ź�","����","ȣ��","�Ź�","���","�ܹ�","����","����",
     "����","�ɰ�","����","����","��ġ","����","ġ��","�ý�","ĩ��","Ʈ��","��Ʈ",
     "����","�ֽ�","�ٶ���","����","����","������","�ڵ���","������","������","�ڻԼ�","¥���",
     "��ħ��","�����","������","������","���ⱸ","ũ����","�ڳ���","�ξ���","�����","������","�����",
     "����","ȣ����","��踻","�ٳ���","�ҹ���","�ֹ���","�縶��","����ö","���ڸ�","�Ҷ��","Ļ�ŷ�",
     "���ܹ�","������","����ݸ�","�������","���ξ���","�罿����","���ٱ���","������","������","�︮����","�������",
     "�Ұ��縮","���۸���","�ǻ԰��","�عٶ��","���Ҹ��վ�","ū�����췷��"};

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
    "�� ��", "�� ��", "�� ��", "�� ��", "�ټ� ��", "���� ��", "�ϰ� ĥ", "���� ��", "��ȩ ��", "�� ��", "�Ϲ� ��",
    "��õ õ", "�ϸ� ��", "��� ��", "�ƺ� ��", "���� ��", "�Ƶ� ��", "�� ��", "�� ȭ", "�� ��", "���� ��",
    "�� ��", "�� ��", "�� ��", "���� ��", "���� ��", "���� ��", "�ϳ� ��", "�ϴ� õ", "�� ��", "���� ��",
    "��� ��", "ū ��", "�� ��", "ª�� ��", "���� ��", "�� ��", "�� ��", "�� ��", "�� ��", "�� ��",
    "�� õ", "�ٴ� ��", "�ٱ� ��", "�� ��", "�� ��", "�� ��", "�ӱ� ��", "�� ��", "�� ��", "�ٺ� ��",
    "�� ��", "�� ��", "�� ��", "���� ��", "�� ��", "�� ��", "���� ��", "�� ��", "���� ��", "�����̸� ��",
    "�鼺 ��", "�� ��", "���� ��", "�� ��", "��� ��", "���� ��", "���� ��", "�� ��", "�ۿ� ��"
    };

    public int QuestCount;  // ��ųʸ��� �� ����Ʈ ���� //10���� <
    public int QuestRange;
    public int RemainAnswer; // ���� ������ ���� ���� ����
    public int CorrectAnswerCount; // ���� ���� ����
    public int ClickCount;
    public int LifeTime; // ���� ���� �ð�

    public int PoolingCool; // ������Ʈ ���� �ð�

    private int index;
    private int QuestIndex;

    public int limitCount;

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
        if (game_Type == Game_Type.C) //�ѱ�
        {
            for (int i = 0; i < sprites_K.Length; i++)
            {
                string key = KorWord[i];
                QuestData data = new QuestData(sprites_K[i], KorWord[i]);
                Quest.Add(key, data);
            }
        }
        else if (game_Type == Game_Type.D) //���� 
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

        gameover.SetActive(false);
        QuestIndex = (game_Type == Game_Type.E) ? 20 : 10;
        Set_QuestCount();
        RemainAnswer = QuestCount;
        totalReactionTime = 0;
        trueReactionTime = 0;
        CorrectAnswerCount = 0;
        ClickCount = 0;
        ObjectPooling.Instance.CreateQuestPrefab(index, QuestIndex + index);
        limitCount = ObjectPooling.Instance.cubePool.Count - QuestRange;
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
        CheckCubeTypes();
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {
                //print(hit.collider.gameObject.name);
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
                    //print("����Ŭ��!");
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
                        //print("���׿� Ŭ��!"); 
                        ObjectPooling.Instance.MeteorPool.Add(hit.collider.gameObject);
                        TimeSlider.Instance.DecreaseTime_Item(5);
                        hit.collider.gameObject.SetActive(false);
                    }

                    if (item.veneziaItem == VeneziaItem.Pause) // Pause
                    {
                        //print("����!");
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
    #region Level����
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
        randomIndex = Random.Range(index, QuestIndex + index); //ù ������ ���� �δ콺 ���ϱ� 
        while (randomIndex == SaverandomIndex)
        {
            randomIndex = Random.Range(index, 10 + index);  // Todo : prototype�� �ƴ� cbt ���� ���������� 0 < �κ��� ���ܿ� �´� �ε����� ���� �� �� �ֵ��� ���� ���� �� ��.
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
        bool isFirst = true;
        if (ObjectPooling.Instance.cubePool.Count > limitCount && isFirst)
        {
            isFirst = false;
            StopCoroutine(ObjectPooling.Instance.CubePooling);
            ObjectPooling.Instance.ReStartCubePooling_co();
        }
        else if(ObjectPooling.Instance.cubePool.Count > limitCount && !isFirst)
        {
            StopCoroutine(ObjectPooling.Instance.CubeRestartPooling);
            ObjectPooling.Instance.ReStartCubePooling_co();
        }
    }


    //���� �δ콺 ����
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
        {   //���ڴ� lv�� ���� step���� ����
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
    //���� �ӵ� ����
    private void GetSpeed(int lv, int step)
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
        {   //���ڴ� lv�� ���� step���� ����
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
