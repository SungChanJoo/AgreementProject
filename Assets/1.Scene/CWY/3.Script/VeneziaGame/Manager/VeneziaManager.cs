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


public class VeneziaManager : MonoBehaviour
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
    private string[] KorWord = { "��" , "��", "��", "�ϸ�", "��"};

    int randomIndex;

    float trueReactionTime = 0f; //������ ���������� �����ӵ�
    float totalReactionTime = 0f;

    public int QuestCount; // 1�� 5�� 3�� 7�� 5�� 10�� <
    public int RemainAnswer; // ������ ���� ���� ������ �����ɶ� �Ǵ��ϴ� �뵵
    public int CorrectAnswerCount = 0; // ������ ���� ����
    public int LifeTime;
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < sprites_KE.Length; i++)
        {
            string key = KorWord[i];
            QuestData data = new QuestData(sprites_KE[i], KorWord[i]);
            QuestKorean.Add(key, data);
            // ��ųʸ��� �ԷµǴ� ���� Ȯ�ο�
            print("Added Quest Data: Key = " + key + ", Description = " + data.description);
        }
        gameover.SetActive(false);

        Set_QuestCount();
        RemainAnswer = QuestCount;
    }

    private void Start()
    {
        DisplayRandomQuest();
    }

    private void Update()
    {
        //  GameStop();
        Click_Obj();
        print("����ġ�ƸŴ���" + isGameover);
    }
    //������Ʈ Ŭ���� �Է�ó��
    // > ��ġ�Է�
    private void Click_Obj()
    {
        totalReactionTime += Time.deltaTime; // �����ӵ� üũ
        if (Input.GetMouseButtonDown(0) || ( Input.touchCount > 0 && Input.touchCount < 11 && Input.GetTouch(0).phase == TouchPhase.Began))
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
                    if(QuestCount > -1) // ������ ���� �ִ� ��� ���� ���� �� �����ӵ� Ȯ��
                    {
                        Score.Instance.Get_FirstScore();
                        trueReactionTime += totalReactionTime;
                        totalReactionTime = 0;
                        RemainAnswer--;
                        CorrectAnswerCount++;
                        DisplayRandomQuest();
                    }
                    if(RemainAnswer == 0) // ������ ��� �������� ���� ����
                    {
                       isGameover = true; //�̶� �����ð� �޾ƿ��� ��.
                        print(trueReactionTime / CorrectAnswerCount);
                    }
                    
                    ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                }
                else if (Questprefab != null && Questprefab.objectType != ObjectType.CorrectAnswer) //������ Ŭ�� ���� ���
                {
                    print("����Ŭ��!");
                    ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                    TimeSlider.Instance.DecreaseTime_Item(5);
                    totalReactionTime = 0;
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

    public void DisplayRandomQuest()
    {
        if(QuestCount == 0)
        {
            Time.timeScale = 0;
            ObjectPooling.Instance.StopAllCoroutines();
            gameover.SetActive(true);
            return;
        }
        else
        {
            switch (Level)
            {
                case 1:
  
                    switch (Step)
                    {
                        case 1:
                            QuestData randomQuest = GetRandomQuest_Kr();
                            Quest_Img.sprite = randomQuest.sprite;
                            Quest_text.text = randomQuest.description;
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
                    break;
                case 2:
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
                    break;
                case 3:
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
                    break;
                default:
                    break;
            }
        }

    }

    private QuestData GetRandomQuest_Kr()
    {
        QuestData[] questArray = new QuestData[QuestKorean.Count];
        QuestKorean.Values.CopyTo(questArray, 0);
        // ������ �ε��� ����
        randomIndex = Random.Range(0, QuestCount);
        QuestCount--;
        if(questArray.Length == 0)
        {
            return null;
        }
        QuestData selectedQuest = questArray[randomIndex];
        // ������ ����Ʈ �����ʹ� ��ųʸ����� ���� => �ߺ� ���� ����
        foreach (var kvp in QuestKorean)
        {
            if (kvp.Value == selectedQuest)
            {
                QuestKorean.Remove(kvp.Key);
                break; // ���õ� ����Ʈ�� �� ���� ���ŵǵ��� break����
            }
        }
        print("Ȯ��");
        return selectedQuest;       
    }
    private void Set_QuestCount()
    {
        switch (LifeTime)
        {
            case 60:
                QuestCount = 2;
                break;
            case 180:
                QuestCount = 3; // ���� ��
                break;
            case 300:
                QuestCount = 5; // ���� ��
                break;
            default:
                break;
        }
    }

    

}
