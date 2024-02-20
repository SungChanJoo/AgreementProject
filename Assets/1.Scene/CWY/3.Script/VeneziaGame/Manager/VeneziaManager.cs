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

    //�����۰��� ��ȣ�ۿ뵵 ���⿡ ����

    //���� ������ �Ŵ������� ���� , ���� ������ �̹����� �̸��� �������� ������ ������ �Ǵ� �� �� �־����.
    [SerializeField] public Image Quest_Img;
    [SerializeField] private TextMeshProUGUI Quest_text;
    //�ѱ� �� ���� ������ ��� �� �̹��� sprite �ѱ۰� ����� �������� ����ϰ� , ���ڴ� ���� 
    [SerializeField] private Sprite[] sprites_KE;    // 1������ ~ 5�������� step1 ,  step 2�� 
    private string[] KorWord = { "�ð�" , "�Ƚð�"};

    int randomIndex;

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
    }

    private void Start()
    {
        DisplayRandomQuest();
    }

    private void Update()
    {
        //  GameStop();
        Click_Obj();
    }
    //������Ʈ Ŭ���� �Է�ó��
    //Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began > ��ġ�Է�
    private void Click_Obj()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {
                print(hit.collider.gameObject.name);
                if(Cube.objectType == ObjectType.CorrectAnswer)
                {
                    Score.Instance.Get_FirstScore();
                    ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                    hit.collider.gameObject.SetActive(false);
                }
                

                // ������ ������Ʈ �Ǵ�
                ItemFnc item = hit.collider.gameObject.GetComponent<ItemFnc>();
                if (item != null && item.veneziaItem == VeneziaItem.Meteor) // veneziaItem�� ���׿��� ���
                {
                    print("���׿� Ŭ��!"); // �Ǵ� �ٸ� ������ ����
                    TimeSlider.Instance.DecreaseTime_Item(5);
                    hit.collider.gameObject.SetActive(false);
                }

                if(item != null && item.veneziaItem == VeneziaItem.Pause) // Pause
                {
                    print("����!");
                    StartCoroutine(Pause_co());
                }

            }
        }
    }


    private void GameStop()
    {
        if(TimeSlider.Instance.slider.value <= 0)
        {
            ObjectPooling.Instance.gameObject.SetActive(false);
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
        // �������� ����Ʈ ������ ����
        QuestData randomQuest = GetRandomQuest_Kr();

        // ���õ� ����Ʈ �������� �̹����� �ؽ�Ʈ�� UI�� ǥ��
        Quest_Img.sprite = randomQuest.sprite;
        Quest_text.text = randomQuest.description;

       /* switch (StepManager.Instance._Level)
        {
            case Level.Low_Level:
                switch (StepManager.Instance._Setp)
                {
                    case Step._1:

                        break;
                    case Step._2:
                        break;
                    case Step._3:
                        break;
                    case Step._4:
                        break;
                    case Step._5:
                        break;
                    case Step._6:
                        break;
                    default:
                        break;
                }
                break;
            case Level.Middle_Level:
                switch (StepManager.Instance._Setp)
                {
                    case Step._1:
                        break;
                    case Step._2:
                        break;
                    case Step._3:
                        break;
                    case Step._4:
                        break;
                    case Step._5:
                        break;
                    case Step._6:
                        break;
                    default:
                        break;
                }
                break;
            case Level.Hight_Level:
                switch (StepManager.Instance._Setp)
                {
                    case Step._1:
                        break;
                    case Step._2:
                        break;
                    case Step._3:
                        break;
                    case Step._4:
                        break;
                    case Step._5:
                        break;
                    case Step._6:
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }*/
    }

    private QuestData GetRandomQuest_Kr()
    {
        // ��ųʸ��� ��� ���� �迭�� ��ȯ 10�� => 5�� <
        QuestData[] questArray = new QuestData[QuestKorean.Count];
        QuestKorean.Values.CopyTo(questArray, 0);

        // ������ �ε��� ����
        randomIndex = Random.Range(0, QuestKorean.Count);

        QuestData selectedQuest = questArray[randomIndex];

        foreach (var kvp in QuestKorean)
        {
            if (kvp.Value == selectedQuest)
            {
                QuestKorean.Remove(kvp.Key);
                break; // ���õ� ����Ʈ�� �� ���� ���ŵǵ��� �ϱ� ���� �ݺ����� �����մϴ�.
            }
        }

        // ������ ����Ʈ �����ʹ� ��ųʸ����� ���� => �ߺ� ���� ����
        return selectedQuest;
    }

    private void GetDictionaryRange()
    {

    }

    private void test()
    {

    }

}
