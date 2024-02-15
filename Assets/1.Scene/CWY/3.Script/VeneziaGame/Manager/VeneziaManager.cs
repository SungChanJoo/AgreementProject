using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
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
    //�������� ���ġ�� ������ �����ϱ� ���� ��ũ��Ʈ
    //������ �����ϸ鼭 ������ �������ְ�, ������� ǥ������ ui�� ���Ƴ����� ���� �� ���� �� ��.

    //�����۰��� ��ȣ�ۿ뵵 ���⿡ ����

    //���� ������ �Ŵ������� ���� , ���� ������ �̹����� �̸��� �������� ������ ������ �Ǵ� �� �� �־����.
    [SerializeField] private Image Quset_Img;
    [SerializeField] private TextMeshProUGUI Quset_text;
    [SerializeField] private Sprite[] sprites;   //�ѱ� �� ���� ������ ��� �� �̹��� sprite
    private string[] KorWord = { "ok" , "no"};


    public Dictionary<string, QuestData> QuestKorean = new Dictionary<string, QuestData>();

    private void Awake()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            string key = KorWord[i];
            QuestData data = new QuestData(sprites[i], KorWord[i]);
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
        QuestData randomQuest = GetRandomQuest();

        // ���õ� ����Ʈ �������� �̹����� �ؽ�Ʈ�� UI�� ǥ��
        Quset_Img.sprite = randomQuest.sprite;
        Quset_text.text = randomQuest.description;
    }

    private QuestData GetRandomQuest()
    {
        // ��ųʸ��� ��� ���� �迭�� ��ȯ
        QuestData[] questArray = new QuestData[QuestKorean.Count];
        QuestKorean.Values.CopyTo(questArray, 0);

        // ������ �ε��� ����
        int randomIndex = Random.Range(0, questArray.Length);

        // �������� ���õ� ����Ʈ ������ ��ȯ
        return questArray[randomIndex];
    }

}
