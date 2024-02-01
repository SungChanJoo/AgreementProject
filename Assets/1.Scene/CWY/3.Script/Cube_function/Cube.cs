using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public float speed;
    private bool isDown = true;
    private void Update()
    {
        if (isDown)
        {
            Cube_DownMove();
        }
        else
        {
            Cube_Up_Left_Right();
        }
        print(isDown);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            isDown = false;
            //ObjectPooling.Instance.cubePool.Add(gameObject);
            //gameObject.SetActive(false);
        }
    }

    private void Cube_DownMove()
    {
        float moveY = -1 * speed * Time.deltaTime;
        transform.Translate(0, moveY, 0);
    }

    private void Cube_Up_Left_Right()
    {
        //y축값은 올라가게 설정 
        float moveY = 1 * speed * Time.deltaTime;
        float moveX = speed * Time.deltaTime;

        // 랜덤으로 좌우 방향 선택
        switch (Random.Range(0, 2)) // 0 또는 1을 반환
        {
            case 0:
                moveX *= -1; // 왼쪽으로 이동
                break;
            case 1:
                moveX *= 1; // 오른쪽으로 이동
                break;
        }
        transform.Translate(moveX, moveY, 0);
    }

    private void Cube_Down_Left_RightMove()
    {
        float moveX = -1 * speed * Time.deltaTime;
        transform.Translate(moveX, 0, 0);
    }

}
