using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMod_Toggle : MonoBehaviour
{
    [SerializeField] private float setTime = 0f;
    [SerializeField] private GameObject select_obj;
    [SerializeField] private GameObject[] next_obj;
    [SerializeField] private GameObject filter_Canvas;
    [SerializeField] private GameObject nonfilter_Canvas;
    private int playerMod = 1;    
    RectTransform rectTransform;
    RectTransform[] rects;
    Vector3 nextVector3;
    float nextVector = 0;
    bool onFilter = false;
    bool playing = false;
    
    private void Start()
    {
        rects = new RectTransform[next_obj.Length];
        rectTransform = select_obj.GetComponent<RectTransform>();
        for (int i = 0; i < next_obj.Length; i++)
        {
            rects[i] = next_obj[i].GetComponent<RectTransform>();
        }        
    }

    public void NextVector()
    {
        if (playing)
        {
            return;
        }
        playing = true;
        if (onFilter)
        {
            onFilter = false;
            nextVector3 = rects[0].position;            
        }
        else
        {
            onFilter = true;
            nextVector3 = rects[1].position;
        }
        filter_Canvas.SetActive(!filter_Canvas.activeSelf);
        nonfilter_Canvas.SetActive(!nonfilter_Canvas.activeSelf);
        StartCoroutine(MoveSelect_Obj(nextVector3));
        
    }
    public void PlayerModSelect_Btn()
    {
        //�ڷ�ƾ�� �������̶�� ��ȯ
        if (playing)
        {
            return;
        }
        playing = true;
        //���� �÷��̾��� ��带 Ȯ�� �� ����
        if (playerMod.Equals(1))
        {
            playerMod = 2;
            nextVector3 = rects[1].position;
        }
        else
        {
            playerMod = 1;
            nextVector3 = rects[0].position;
        }
        Debug.Log(playerMod);

        //���� �̹����� �̵����� �� ��ġ ���� �Ѱ��ֱ�
        StartCoroutine(MoveSelect_Obj(nextVector3));
    }
    
    private IEnumerator MoveSelect_Obj(Vector3 nextVector)
    {        
        Vector3 nextTransform = nextVector3;

        float currntTime = 0f;
        
        //������ �ð� ������ PlayMod �̹��� �̵�
        while (currntTime<setTime)
        {
            currntTime += Time.deltaTime;            
            rectTransform.position = Vector3.Lerp(rectTransform.position, nextTransform, currntTime / setTime);
            yield return null;
        }
        playing = false;
    }
}
