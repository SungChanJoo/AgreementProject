using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public static Score Instance = null;
    [SerializeField] private Text score_text;

    private int Count;

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
