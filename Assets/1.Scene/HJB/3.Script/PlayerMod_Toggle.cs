using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMod_Toggle : MonoBehaviour
{
    [SerializeField] private float setTime = 0f;
    [SerializeField] private GameObject select_obj;
    [SerializeField] private int playerMod = 1;
    RectTransform rectTransform;    
    float nextVector = 0;
    bool playing = false;
    
    private void Start()
    {
        rectTransform = select_obj.GetComponent<RectTransform>();
        
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
            nextVector = 140f;
        }
        else
        {
            playerMod = 1;
            nextVector = -140f;
        }
        Debug.Log(playerMod);

        //���� �̹����� �̵����� �� ��ġ ���� �Ѱ��ֱ�
        StartCoroutine(MoveSelect_Obj(nextVector));
    }
    
    private IEnumerator MoveSelect_Obj(float nextVector)
    {
        
        Vector3 nextTransform = new Vector3(rectTransform.position.x + nextVector, rectTransform.position.y);
        
        float currntTime = 0f;
        
        // ������ �ð� ������ PlayMod �̹��� �̵�
        while (currntTime<setTime)
        {
            currntTime += Time.deltaTime;            
            rectTransform.position = Vector3.Lerp(rectTransform.position, nextTransform, currntTime / setTime);
            yield return null;
        }
        playing = false;
    }
}
