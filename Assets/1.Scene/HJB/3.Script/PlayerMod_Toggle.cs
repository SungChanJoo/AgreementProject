using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMod_Toggle : MonoBehaviour
{
    [SerializeField] private float setTime = 0f;
    [SerializeField] private GameObject select_obj;
    [SerializeField] private int playerMod = 1;
    RectTransform rectTransform;
    private void Start()
    {
        rectTransform = select_obj.GetComponent<RectTransform>();
    }
    public void PlayerModSelect_Btn()
    {
        if (playerMod.Equals(1))
        {
            playerMod = 2;
        }
        else
        {
            playerMod = 1;
        }
        StartCoroutine(MoveSelect_Obj());
    }
    
    private IEnumerator MoveSelect_Obj()
    {

        RectTransform modSet = null;
        if (playerMod.Equals(1))
        {
            
            modSet.position = new Vector3(-70, rectTransform.position.y);
        }
        else
        {
            modSet.position = new Vector3(70, rectTransform.position.y);
        }
        
        float currntTime = 0f;
        
        while (currntTime<setTime)
        {
            currntTime += Time.deltaTime;
            rectTransform.position = Vector3.Lerp(rectTransform.position, modSet.position, currntTime / setTime);
            
            yield return null;
        }
    }
}
