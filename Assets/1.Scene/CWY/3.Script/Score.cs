using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public static Score Instance = null;
    [SerializeField] private Text score_text;

    private int Count;

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
