using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    //점수는 모든게임에서 공통적으로 사용해야 할 요소기 때문에 접근용이성을 위해 싱글톤 패턴을 이용

    public static Score Instance = null;
    [SerializeField] private Text score_text;

    private int Count;

    private bool isOk; // 이걸통해서 관리할것 (isOk true면 겜오브젝트삭제 아니면 말고.) <- 나중에 변경 

    //요구사항 피버타임 
    //피버타임 때 는 점수는 올라가지만 정답률에는 관여하지않음 (오로지 정답만 떨어지게 할꺼라서)

    // 100 * 95 * (lv 1 > 1.1 lv2 1.2 ...) * step(1~6) * 


    //싱글톤패턴 생성
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
        }
    }

    //기본값 초기화
    private void Start()
    {
        score_text.text = "0";
    }

    //Todo : 정답을 맞췄을 경우 정답 실행 > 추후 기획팀에서 점수관련 조직도 보내주면 그에 맞게 로직 구성 할 것.
    public void Get_Score()
    {
        Count++;
        score_text.text = Count.ToString();
    }



}
