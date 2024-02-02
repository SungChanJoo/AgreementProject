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
        //코루틴이 실행중이라면 반환
        if (playing)
        {
            return;
        }
        playing = true;
        //현재 플레이어의 모드를 확인 후 변경
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

        //현재 이미지가 이동히야 할 위치 값을 넘겨주기
        StartCoroutine(MoveSelect_Obj(nextVector));
    }
    
    private IEnumerator MoveSelect_Obj(float nextVector)
    {
        
        Vector3 nextTransform = new Vector3(rectTransform.position.x + nextVector, rectTransform.position.y);
        
        float currntTime = 0f;
        
        // 설정한 시간 값으로 PlayMod 이미지 이동
        while (currntTime<setTime)
        {
            currntTime += Time.deltaTime;            
            rectTransform.position = Vector3.Lerp(rectTransform.position, nextTransform, currntTime / setTime);
            yield return null;
        }
        playing = false;
    }
}
