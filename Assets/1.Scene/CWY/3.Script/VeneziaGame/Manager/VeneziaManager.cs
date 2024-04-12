using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestData
{
    public Sprite Sprite;
    public string description;

    public QuestData(Sprite sprite, string description)
    {
        this.Sprite = sprite;
        this.description = description;
    }
}

public enum VeneGameMode
{
    Solo, Couple
}

public class VeneziaManager : GameSetting
{
    public static VeneziaManager Instance = null;
    //�������� ���ġ�� ������ �����ϱ� ���� ��ũ��Ʈ
    //������ �����ϸ鼭 ������ �������ְ�, ������� ǥ������ ui�� ���Ƴ����� ���� �� ���� �� ��.
    [Header("veneGameOver")]
    [SerializeField] private GameObject veneGameOver;


    [SerializeField] private GameObject cubeParent;

    public VeneGameMode VeneGameMode;


    public bool IsGameover = false;

    //�����۰��� ��ȣ�ۿ뵵 ���⿡ ����
    //���� ������ �Ŵ������� ���� , ���� ������ �̹����� �̸��� �������� ������ ������ �Ǵ� �� �� �־����.
    [Header("PlayerOne")]
    [SerializeField] public Image Quest_Img;
    [SerializeField] private TextMeshProUGUI quest_text;
    [Header("PlayerTwo")]
    [SerializeField] public Image Quest2_Img;
    [SerializeField] private TextMeshProUGUI quest2_text;
    //�ѱ� �� ���� ������ ��� �� �̹��� sprite �ѱ۰� ����, ���� 
    [SerializeField] public Sprite[] Sprites_K; 
    [SerializeField] public Sprite[] Sprites_E; 
    [SerializeField] public Sprite[] Sprites_H;

    public float StartSpeed;
    public int MaxTouchCount;
    public float AccelerationSpeed;

    //Ŀ�ø���϶� Win/Lose �Ǵ�

    #region QuestData
    private string[] korWord =
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

    private string[] englishWord =
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
    private string[] hanJa =
    {
    "�� ��", "�� ��", "�� ��", "�� ��", "�ټ� ��", "���� ��", "�ϰ� ĥ", "���� ��", "��ȩ ��", "�� ��", "�Ϲ� ��",
    "��õ õ", "�ϸ� ��", "��� ��", "�ƺ� ��", "���� ��", "�Ƶ� ��", "�� ��", "�� ȭ", "�� ��", "���� ��",
    "�� ��", "�� ��", "�� ��", "���� ��", "���� ��", "���� ��", "�ϳ� ��", "�ϴ� õ", "�� ��", "���� ��",
    "��� ��", "ū ��", "�� ��", "ª�� ��", "���� ��", "�� ��", "�� ��", "�� ��", "�� ��", "�� ��",
    "�� õ", "�ٴ� ��", "�ٱ� ��", "�� ��", "�� ��", "�� ��", "�ӱ� ��", "�� ��", "�� ��", "�ٺ� ��",
    "�� ��", "�� ��", "�� ��", "���� ��", "�� ��", "�� ��", "���� ��", "�� ��", "���� ��", "�����̸� ��",
    "�鼺 ��", "�� ��", "���� ��", "�� ��", "��� ��", "���� ��", "���� ��", "�� ��", "�ۿ� ��"
    };
    #endregion

    #region ������ �� ���� ����
    public int QuestCount;  // ��ųʸ��� �� ����Ʈ ���� //10���� <
    public int QuestRange; //���� ����
    public int RemainAnswer; // ���� ������ ���� ���� ����
    public int CorrectAnswerCount; // ���� ���� ����
    //1�θ���϶��� Click / 2���϶��� �ΰ����
    public int ClickCount;
    public int Click_SecondPlayerCount;
    public int LifeTime; // ���� ���� �ð�

    public int PoolingCool; // ������Ʈ ���� �ð�

    private int index;
    private int QuestIndex;

    public int limitCount;

    public int DestroyTime;

    private int randomIndex = 0;
    private int randomIndexSecond = 0;
    private int[] SaverandomIndex = new int[] { 999, 999 };
    //�����ӵ� ���� �ð�
    float trueReactionTime;
    float totalReactionTime;

    //���� �����ӵ� ���� ����
    float ReactionTime;

    //���� ������ ����
    public Dictionary<string, QuestData> Quest = new Dictionary<string, QuestData>();

    public bool isFirstPlayerTouch = false;
    private bool isStart = false;

    //Ŀ�ø���϶� ���� Ŭ���ߴ��� �Ǵ�
    bool isFirst = false;
    bool isSecond =false;
    bool isNull = false;
    #endregion
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
        //1 3 4 B D E �ѱ� ���� ����play_mode == PlayMode.Solo
        if (true)
        {
            if (game_Type == Game_Type.C) //�ѱ�
            {
                for (int i = 0; i < Sprites_K.Length; i++)
                {
                    string key = korWord[i];
                    QuestData data = new QuestData(Sprites_K[i], korWord[i]);
                    Quest.Add(key, data);
                }
            }
            else if (game_Type == Game_Type.D) //���� 
            {
                for (int i = 0; i < Sprites_E.Length; i++)
                {
                    string key = englishWord[i];
                    QuestData data = new QuestData(Sprites_E[i], englishWord[i]);
                    Quest.Add(key, data);
                }
            }
            else
            {
                for (int i = 0; i < Sprites_H.Length; i++)
                {
                    string key = hanJa[i];
                    QuestData data = new QuestData(Sprites_H[i], hanJa[i]);
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
        
        //����Ʈ ������Ʈ ����
        if(play_mode == PlayMode.Solo)
        {
            ObjectPooling.Instance.CreateQuestPrefab(index, QuestIndex + index);
            limitCount = ObjectPooling.Instance.CubePool.Count - QuestRange;
        }
        else
        {
            limitCount = (game_Type == Game_Type.E) ? 65 : 100;
            int indexSet = (game_Type == Game_Type.E) ? 70 : 105;
            ObjectPooling.Instance.CreateQuestPrefab(0, indexSet);
        }
        
        DisplayRandomQuest();
        //�ð� ����
        StartTime();
        if(play_mode == PlayMode.Solo)
        {
            ObjectPooling.Instance.StartCubeOne_Pooling_co();
        }
        else
        {
            ObjectPooling.Instance.StartCubeOne_Pooling_co();
            ObjectPooling.Instance.StartCubeTwo_Pooling_co();
        }

        ObjectPooling.Instance.CreateItem();
        
    }
    private void Update()
    {
        //  GameStop();
        if (!IsGameover || !settingStop)
        {
            Click_Obj();
        }
        CanClickOn();

        GameOver();
    }

    //������Ʈ Ŭ���� �Է�ó��
    //|| (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)

    private float timer = 0f;
    private bool canClick = true;
    private void CanClickOn()
    {
        // �ð��� ������Ʈ
        if (!canClick)
        {
            timer += Time.deltaTime;

            // 0.2�ʰ� ������ bool ���� true�� �����ϰ� Ÿ�̸Ӹ� ����
            if (timer >= 0.2f)
            {
                timer = 0f;
                canClick = true;
            }
        }

        CheckCubeTypes();
    }
    //Ŭ��������(��ġ������) �� �������� ��ġ�� ���� �������� �������
    // ������ �� ť�긦 ��ġ�� ��� �ش� ������ ���� ����
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
                    //ť�� ������Ʈ �Ǵ�
                    Cube Questprefab = hit.collider.gameObject.GetComponent<Cube>();

                    if (Questprefab != null && Questprefab.ObjectType == ObjectType.CorrectAnswer) // ť�긦 �������� Quest ���� �˾ƾ���
                    {
                        isFirstPlayerTouch = (Questprefab.PlayerNum == PlayerNum.One) ? true : false;
                        if (QuestCount > -1)
                        {
                            RemainAnswer--;
                            CorrectAnswerCount++;
                            trueReactionTime += totalReactionTime;
                            totalReactionTime = 0;
                            NextQuest();
                        }
                        if (isFirstPlayerTouch) // �ַ� ��忡���� One���� ������
                        {
                            ClickCount++;
                            isFirst = true;
                            ObjectPooling.Instance.CubePool.Add(hit.collider.gameObject);
                            hit.collider.gameObject.SetActive(false);
                            if(VeneGameMode == VeneGameMode.Couple)
                            {
                                ObjectPooling.Instance.CreateBoom(ObjectPooling.Instance.PlayerOne_Boom);
                            }
                        }
                        else
                        {
                            Click_SecondPlayerCount++;
                            isSecond = true;
                            ObjectPooling.Instance.CubePoolTwo.Add(hit.collider.gameObject);
                            hit.collider.gameObject.SetActive(false);
                            ObjectPooling.Instance.CreateBoom(ObjectPooling.Instance.PlayerTwo_Boom);
                        }


                        if (RemainAnswer == 0) // ������ ��� �������� ���� ����
                        {
                            GameClear();
                        }
                    }
                    else if (Questprefab != null && Questprefab.ObjectType != ObjectType.CorrectAnswer)
                    {
                        //print("����Ŭ��!");
                        isFirstPlayerTouch = (Questprefab.PlayerNum == PlayerNum.One) ? true : false;
                        if (play_mode == PlayMode.Solo) ClickCount++; // ������ 1�θ���϶��� üũ
                        if (isFirstPlayerTouch)
                        {
                            isFirst = true;
                            ObjectPooling.Instance.CubePool.Add(hit.collider.gameObject);
                            hit.collider.gameObject.SetActive(false);
                            TimeSlider.Instance.DecreaseTime_Item(5);
                        }
                        else
                        {
                            isSecond = true;
                            ObjectPooling.Instance.CubePoolTwo.Add(hit.collider.gameObject);
                            hit.collider.gameObject.SetActive(false);
                            TimeSlider.Instance.DecreaseTime_CoupleMode(5, TimeSlider.Instance.Slider_PlayerTwo);
                        }
                        totalReactionTime = 0;
                    }
                    else
                    {
                        // ������ ������Ʈ �Ǵ�
                        ItemFnc item = hit.collider.gameObject.GetComponent<ItemFnc>();
                        if (play_mode == PlayMode.Solo && item != null) // veneziaItem�� ���׿��� ���
                        {
                            if (item.VeneziaItem == VeneziaItem.Meteor)
                            {
                                //print("���׿� Ŭ��!"); 
                                ObjectPooling.Instance.MeteorPool.Add(hit.collider.gameObject);
                                TimeSlider.Instance.DecreaseTime_Item(5);
                                hit.collider.gameObject.SetActive(false);
                            }
                            else if (item.VeneziaItem == VeneziaItem.Pause)
                            {
                                //print("����!");
                                ObjectPooling.Instance.PausePool.Add(hit.collider.gameObject);
                                hit.collider.gameObject.SetActive(false);
                                StartCoroutine(Pause_co());
                            }
                        }
                        else
                        {
                            if(item != null)
                            {
                                if (item.VeneziaItem == VeneziaItem.Boom && item.BoomType == BoomType.PlayerOne)
                                {
                                    ObjectPooling.Instance.BoomPool.Add(hit.collider.gameObject);
                                    hit.collider.gameObject.SetActive(false);
                                    TimeSlider.Instance.DecreaseTime_CoupleMode(5, TimeSlider.Instance.Slider_PlayerTwo);
                                }
                                else if (item.VeneziaItem == VeneziaItem.Boom && item.BoomType == BoomType.PlayerTwo)
                                {
                                    ObjectPooling.Instance.BoomPool.Add(hit.collider.gameObject);
                                    hit.collider.gameObject.SetActive(false);
                                    TimeSlider.Instance.DecreaseTime_CoupleMode(5, TimeSlider.Instance.Slider);
                                }
                            }
                        }
                    }
                }
            }
            ResetCube();
        }
    }

    //Pause�� ������� �ð��������
    IEnumerator Pause_co()
    {
        TimeSlider.Instance.IsStop = true;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(5f);
        TimeSlider.Instance.IsStop = false;
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
        QuestData[] randomQuest = GetRandomQuest();
        if (randomQuest == null) return;  //����Ʈ�� ���� ���� �Ǿ��� �� �������� ���� ����
        if(play_mode == PlayMode.Solo)
        {
            Quest_Img.sprite = randomQuest[0].Sprite;
            quest_text.text = randomQuest[0].description;
            Quest2_Img = null;
            quest2_text = null;
        }
        else
        {
            if (!isStart)
            {
                isStart = true;
                Quest_Img.sprite = randomQuest[0].Sprite;
                quest_text.text = randomQuest[0].description;
                Quest2_Img.sprite = randomQuest[1].Sprite;
                quest2_text.text = randomQuest[1].description;
            }
            else
            {
                //ù�� �� �÷��̾ ��ġ ���� ���, ù ���� �÷��̾��� ������ �ٲٰ�
                //�ι�° �÷��̾ ��ġ���� ��� �ι�° �÷��̾��� ������ �ٲܰ�.
                if (isFirstPlayerTouch)
                {
                    //ù��° �÷��̾�
                    Quest_Img.sprite = randomQuest[0].Sprite;
                    quest_text.text = randomQuest[0].description;
                }
                else
                {
                    //�ι�°�÷��̾�
                    Quest2_Img.sprite = randomQuest[1].Sprite;
                    quest2_text.text = randomQuest[1].description;
                }
            }

        }
        


    }

    private QuestData[] GetRandomQuest()
    {
        if (QuestCount == 0) return null; //null �� ó�� (������ ���� �� ���)
        QuestData[] questArray = new QuestData[Quest.Count];
        Quest.Values.CopyTo(questArray, 0);
        QuestCount--;
        QuestData[] selectedQuest = new QuestData[2];
        //���� ������ ����
        if(play_mode == PlayMode.Solo)
        {
            randomIndex = Random.Range(index, QuestIndex + index); //ù ������ ���� �δ콺 ���ϱ� 
            while (randomIndex == SaverandomIndex[0])
            {
                randomIndex = Random.Range(index, 10 + index);
            }
            SaverandomIndex[0] = randomIndex;
            selectedQuest[0] = questArray[randomIndex];
        }
        else
        {
            //2�� ����� ���� 2������ ������ �ʿ�
            randomIndex = Random.Range(0, ObjectPooling.Instance.CubePool.Count); 
            randomIndexSecond = Random.Range(0, ObjectPooling.Instance.CubePoolTwo.Count);
            while (randomIndex == SaverandomIndex[0])
            {
                randomIndex = Random.Range(0, ObjectPooling.Instance.CubePool.Count);
            }
            while (randomIndexSecond == SaverandomIndex[1])
            {
                randomIndexSecond = Random.Range(0, ObjectPooling.Instance.CubePoolTwo.Count);
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

        if(VeneGameMode == VeneGameMode.Couple)
        {
            QuestCount = 9999; //2�θ�忡���� ����Ʈ ���� ����x
        }
    }
    #endregion
    //Ÿ�Ӿƿ� ���ӿ���
    private void GameOver()
    {
        if (IsGameover) return;
        if ((play_mode == PlayMode.Solo && TimeSlider.Instance.Slider.value <= 0))
        {
            IsGameover = true;
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
        else if(VeneGameMode == VeneGameMode.Couple)
        {
            if(TimeSlider.Instance.Slider.value == 0 || TimeSlider.Instance.Slider_PlayerTwo.value == 0)
            {
                IsGameover = true;
                veneGameOver.SetActive(true);
                StopAllCoroutines();
                TimeSlider.Instance.StopAll();
            }
        }

    }
    //������ ������ ���� �������� 
    private void GameClear()
    {
        IsGameover = true;
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

    //ť�긦 ������ ����Ʈ�� �ְ� �ٽ� Ǯ���� �ϱ� ���� �۾�
    public void ResetCube()
    {
        if(play_mode == PlayMode.Solo)
        {
            bool isFirst = true;
            if (ObjectPooling.Instance.CubePool.Count > limitCount && isFirst)
            {
                isFirst = false;
                StopCoroutine(ObjectPooling.Instance.CubePooling_PlayerOne);
                ObjectPooling.Instance.ReStartCubePooling_One_co();
            }
            else if (ObjectPooling.Instance.CubePool.Count > limitCount && !isFirst)
            {
                StopCoroutine(ObjectPooling.Instance.CubeRestartPooling_One);
                ObjectPooling.Instance.ReStartCubePooling_One_co();
            }
        }
        else
        {
            //ù��° �÷��̾�
            bool isFirst = true;
            bool isSecond = true;
            if (ObjectPooling.Instance.CubePool.Count > limitCount && isFirst) 
            {
                isFirst = false;
                StopCoroutine(ObjectPooling.Instance.CubePooling_PlayerOne);
                ObjectPooling.Instance.ReStartCubePooling_One_co();
            }
            else if (ObjectPooling.Instance.CubePool.Count > limitCount && !isFirst)
            {
                StopCoroutine(ObjectPooling.Instance.CubeRestartPooling_One);
                ObjectPooling.Instance.ReStartCubePooling_One_co();
            }

            //�ι�° �÷��̾�
            if(ObjectPooling.Instance.CubePoolTwo.Count > limitCount && isSecond)
            {
                isSecond = false;
                StopCoroutine(ObjectPooling.Instance.CubePooling_PlayerTwo);
                ObjectPooling.Instance.ReStartCubePooling_Two_co();
            }
            else if (ObjectPooling.Instance.CubePoolTwo.Count > limitCount && !isSecond)
            {
                StopCoroutine(ObjectPooling.Instance.CubeRestartPooling_Two);
                ObjectPooling.Instance.ReStartCubePooling_Two_co();
            }

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
        if(play_mode == PlayMode.Solo)
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
        else
        {
            StartSpeed = 26;
            MaxTouchCount = 13;
        }
        

    }

    //ť�� Ÿ�� ����
    private void CheckCubeTypes()
    {

        for (int i = 0; i < QuestRange; i++)
        {
            if(cubeParent.transform.GetChild(i).GetComponent<Cube>().ObjectType == ObjectType.CorrectAnswer && cubeParent.transform.GetChild(i).gameObject.activeSelf)
            {
                totalReactionTime += Time.deltaTime;
            }
        }
    }


}