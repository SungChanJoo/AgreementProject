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
    //������ Ÿ�Կ� ���� ���ӿ� ������ �� ����
    public VeneziaItem veneziaItem;
    public float Speed;
    public int Decreasetime;

    private void Update()
    {
        ItemMove();
    }

    private void ItemMove()
    {
        if (gameObject.activeSelf)
        {
            float moveY = -1 * Speed * Time.deltaTime;
            transform.Translate(0, moveY, 0);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && veneziaItem == VeneziaItem.Meteor)
        {
            TimeSlider.Instance.DecreaseTime_Item(Decreasetime);
        }
    }


}
    