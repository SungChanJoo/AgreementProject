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
    //������ Ÿ�Կ� ���� ���ӿ� ������ �� ����
    public  VeneziaItem veneziaItem;
    public BoomType boomType;
    public float MeteorSpeed;
    public float PauseSpeed;
    public float BoomSpeed;

    public int Decreasetime;

    private void Update()
    {
       if(!TimeSlider.Instance.isStop) ItemMove();
        GameOver();
    }

    private void ItemMove()
    {
        if (veneziaItem == VeneziaItem.Meteor)
        {
            float moveY = -1 * MeteorSpeed * Time.deltaTime;
            transform.Translate(0, moveY, 0);
        }
        else if(veneziaItem == VeneziaItem.Pause)
        {
            float moveY = -1 * PauseSpeed * Time.deltaTime;
            transform.Translate(0, moveY, 0);
        }
        else if (veneziaItem == VeneziaItem.Boom)
        {
            float moveY = -1 * BoomSpeed * Time.deltaTime;
            transform.Translate(0, moveY, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //�ٴڿ� ������ ����
        if (other.gameObject.CompareTag("Ground"))
        {
            gameObject.SetActive(false);
            if(veneziaItem == VeneziaItem.Pause)
            {
                ObjectPooling.Instance.PausePool.Add(gameObject);
            }
            else if(veneziaItem == VeneziaItem.Meteor)
            {
                ObjectPooling.Instance.MeteorPool.Add(gameObject);
            }else if(veneziaItem == VeneziaItem.Boom)
            {
                ObjectPooling.Instance.BoomPool.Add(gameObject);
            }
        }
    }

    private void GameOver()
    {
        if (VeneziaManager.Instance.isGameover) gameObject.SetActive(false);
    }
}
    