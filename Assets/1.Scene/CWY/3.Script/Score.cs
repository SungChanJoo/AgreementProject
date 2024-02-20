using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum PlayMode
{
    Solo,
    Couple
}
public class Score : MonoBehaviour
{
    //점수는 모든게임에서 공통적으로 사용해야 할 요소기 때문에 접근용이성을 위해 싱글톤 패턴을 이용
    [SerializeField]private PlayMode playMode;

    public static Score Instance = null;
    //Todo : 추후에 게임모드별로 참조하도록 설정 변경 필요 => 현재는 개인모드여도 3개다 참조해야함 (기능구현에 문제는 없으나 불필요한 참조 발생)
    [SerializeField] private TextMeshProUGUI score_text;
    [SerializeField] private TextMeshProUGUI Firstscore_text;
    [SerializeField] private TextMeshProUGUI Secondscore_text;
    

    private int Count;
    private int FirstCount;
    private int SecondCount;

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
            if (playMode == PlayMode.Couple) return;
            Destroy(gameObject);
        }
    }

    //기본값 초기화
    private void Start()
    {
        if (playMode == PlayMode.Solo)
        {
            Firstscore_text.text = "0";
        }
        else
        {
            Firstscore_text.text = "0";
            Secondscore_text.text = "0";
        }
    }
    //---------------------------------------------------------
    public void Get_FirstScore()
    {
        if (true)//playMode == PlayMode.Solo)
        {
            FirstCount++;
            Firstscore_text.text = FirstCount.ToString();
        }
    }

    public void Get_SecondScore()
    {
        if (true)//playMode == PlayMode.커플)
        {
            SecondCount++;
            Secondscore_text.text = SecondCount.ToString();
        }
    }


}
