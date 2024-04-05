using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VeneziaItem
{
    Pause,
    Meteor,
    Boom
}

public enum BoomType
{
    NULL, PlayerOne, PlayerTwo
}

public class ItemFnc : MonoBehaviour
{
    //아이템 타입에 따라 게임에 영향을 줄 예정
    public  VeneziaItem VeneziaItem;
    public BoomType BoomType;
    public float MeteorSpeed;
    public float PauseSpeed;
    public float BoomSpeed;

    public int Decreasetime;

    private void Update()
    {
       if(!TimeSlider.Instance.IsStop) ItemMove();
        GameOver();
    }

    private void ItemMove()
    {
        if (VeneziaItem == VeneziaItem.Meteor)
        {
            float moveY = -1 * MeteorSpeed * Time.deltaTime;
            transform.Translate(0, moveY, 0);
        }
        else if(VeneziaItem == VeneziaItem.Pause)
        {
            float moveY = -1 * PauseSpeed * Time.deltaTime;
            transform.Translate(0, moveY, 0);
        }
        else if (VeneziaItem == VeneziaItem.Boom)
        {
            float moveY = -1 * BoomSpeed * Time.deltaTime;
            transform.Translate(0, moveY, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //바닥에 닿으면 제거
        if (other.gameObject.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
            if(VeneziaItem == VeneziaItem.Pause)
            {
                ObjectPooling.Instance.PausePool.Add(gameObject);
            }
            else if(VeneziaItem == VeneziaItem.Meteor)
            {
                ObjectPooling.Instance.MeteorPool.Add(gameObject);
            }else if(VeneziaItem == VeneziaItem.Boom)
            {
                ObjectPooling.Instance.BoomPool.Add(gameObject);
            }
        }
    }

    private void GameOver()
    {
        if (VeneziaManager.Instance.IsGameover) gameObject.SetActive(false);
    }
}
    