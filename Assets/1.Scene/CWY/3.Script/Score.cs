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

    private void Start()
    {
        score_text.text = "0";
    }

    public void Get_Score()
    {
        Count++;
        score_text.text = Count.ToString();
    }



}
