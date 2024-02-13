using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum Quest_Korean
{
    시계, 사과
}

public class VeneziaManager : MonoBehaviour
{
    //전반적인 배네치아 게임을 관리하기 위한 스크립트
    //ex) 시간이 0 일때 게임을 멈추거나, UI관리를 해주거나 등의 활동

    //아이템과의 상호작용도 여기에 구현

    //문제 출제를 매니저에서 관리 , 출제 문제의 이미지와 이름을 기준으로 정답의 유무를 판단 할 수 있어야함.
    [SerializeField] private Sprite[] sprites;

    private Dictionary<string, Sprite> QuestKorean = new Dictionary<string, Sprite>();


    private void Start()
    {
        QuestKorean.Add("시계", sprites[0]);
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
                }
                ObjectPooling.Instance.cubePool.Add(hit.collider.gameObject);
                hit.collider.gameObject.SetActive(false);
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

}
