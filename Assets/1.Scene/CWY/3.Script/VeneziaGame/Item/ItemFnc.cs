using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VeneziaItem
{
    Pause,
    Meteor,  
}

public class ItemFnc : MonoBehaviour
{
    //아이템 타입에 따라 게임에 영향을 줄 예정
    public  VeneziaItem veneziaItem;
    public float MeteorSpeed;
    public float PauseSpeed;

    public int Decreasetime;

    private void Update()
    {
       if(!TimeSlider.Instance.isStop) ItemMove();
    }

    private void ItemMove()
    {
        if (veneziaItem == VeneziaItem.Meteor)
        {
            float moveY = -1 * MeteorSpeed * Time.deltaTime;
            transform.Translate(0, moveY, 0);
        }
        else
        {
            float moveY = -1 * PauseSpeed * Time.deltaTime;
            transform.Translate(0, moveY, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.tag);
        print(other.gameObject.name);
        if (other.gameObject.CompareTag("Ground"))
        {
            print("닿니?~~@");
            gameObject.SetActive(false);
            if(veneziaItem == VeneziaItem.Pause)
            {
                ObjectPooling.Instance.PausePool.Add(gameObject);
            }
            else
            {
                ObjectPooling.Instance.MeteorPool.Add(gameObject);
            }
        }
    }


}
    