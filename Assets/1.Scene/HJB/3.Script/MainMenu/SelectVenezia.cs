using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectVenezia : MonoBehaviour
{
    [SerializeField] private GameObject[] venezia_Btn;

    [SerializeField] private RectTransform selectPoint;
    [SerializeField] private RectTransform closePoint;

    [SerializeField] private float setTime;

    [SerializeField]private Sprite[] select_venezia;
    [SerializeField]private Image venezia;

    RectTransform[] select_rect;
    Image[] select_Image;
    Color closeColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    Color SelectColor = new Color(1f, 1f, 1f, 1f);
    private bool isPlay = false;


    private void Awake()
    {
        select_rect = new RectTransform[venezia_Btn.Length];
        select_Image = new Image[venezia_Btn.Length];        
        for (int i = 0; i < venezia_Btn.Length; i++)
        {
            select_rect[i] = venezia_Btn[i].GetComponent<RectTransform>();
            select_Image[i] = venezia_Btn[i].GetComponent<Image>();
        }
    }
    private void Start()
    {
        
    }
    public void SelectVeneziaGame(int num)
    {
        if (isPlay)
        {
            return;
        }
        isPlay = true;
        StepManager.Instance.game_Type = (Game_Type)num;
        venezia.sprite = select_venezia[num-2]; 
        //num 파라미터를 (빌드 순서)2,3,4로 받아오기 때문에 인덱스랑 일치시기기 위해서 -2
        StartCoroutine(SelectVeneziaGame_Co(num-2));
    }

    private IEnumerator SelectVeneziaGame_Co(int num)
    {
        float currentTime = 0;
        float speedFactor = 1.0f;
        while (currentTime <setTime)
        {
            currentTime += Time.deltaTime * speedFactor;
            for (int i = 0; i < venezia_Btn.Length; i++)
            {
                if (num == i)
                {
                    select_rect[i].position = Vector3.Lerp(select_rect[i].position,
                         new Vector3(selectPoint.position.x,select_rect[i].position.y, select_rect[i].position.z), currentTime / setTime);
                    select_Image[i].color = SelectColor;
                        
                }
                else
                {
                    select_rect[i].position = Vector3.Lerp(select_rect[i].position,
                        new Vector3(closePoint.position.x, select_rect[i].position.y, select_rect[i].position.z), currentTime / setTime);
                    select_Image[i].color = closeColor;
                }
                yield return null;
            }
        }
        isPlay = false;
    }

}
