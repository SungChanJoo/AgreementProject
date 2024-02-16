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
    //전반적인 배네치아 게임을 관리하기 위한 스크립트
    //게임을 진행하면서 정답을 판정해주고, 정답란에 표기해줄 ui를 갈아끼워줄 역할 을 수행 할 것.

    //아이템과의 상호작용도 여기에 구현

    //문제 출제를 매니저에서 관리 , 출제 문제의 이미지와 이름을 기준으로 정답의 유무를 판단 할 수 있어야함.
    [SerializeField] private Image Quset_Img;
    [SerializeField] private TextMeshProUGUI Quset_text;
    [SerializeField] private Sprite[] sprites;   //한글 및 영어 문제에 사용 할 이미지 sprite
    private string[] KorWord = { "ok" , "no"};


    public Dictionary<string, QuestData> QuestKorean = new Dictionary<string, QuestData>();

    private void Awake()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            string key = KorWord[i];
            QuestData data = new QuestData(sprites[i], KorWord[i]);
            QuestKorean.Add(key, data);
            // 딕셔너리에 입력되는 정보 확인용
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
    //오브젝트 클릭시 입력처리
    //Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began > 터치입력
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
                

                // 아이템 오브젝트 판단
                ItemFnc item = hit.collider.gameObject.GetComponent<ItemFnc>();
                if (item != null && item.veneziaItem == VeneziaItem.Meteor) // veneziaItem이 메테오인 경우
                {
                    print("메테오 클릭!"); // 또는 다른 동작을 수행
                    TimeSlider.Instance.DecreaseTime_Item(5);
                    hit.collider.gameObject.SetActive(false);
                }

                if(item != null && item.veneziaItem == VeneziaItem.Pause) // Pause
                {
                    print("멈춰!");
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
        // 랜덤으로 퀘스트 데이터 선택
        QuestData randomQuest = GetRandomQuest();

        // 선택된 퀘스트 데이터의 이미지와 텍스트를 UI에 표시
        Quset_Img.sprite = randomQuest.sprite;
        Quset_text.text = randomQuest.description;
    }

    private QuestData GetRandomQuest()
    {
        // 딕셔너리의 모든 값을 배열로 변환
        QuestData[] questArray = new QuestData[QuestKorean.Count];
        QuestKorean.Values.CopyTo(questArray, 0);

        // 랜덤한 인덱스 선택
        int randomIndex = Random.Range(0, questArray.Length);

        // 랜덤으로 선택된 퀘스트 데이터 반환
        return questArray[randomIndex];
    }

}
