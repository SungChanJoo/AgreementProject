using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    //공이 처음 시작할 때 사방에서 날라올 수 있게 끔..
    
    public float StartSpeed; // 공을 움직일 속도
    public float MaxSpeed; // 최대 속도 제한
    [SerializeField]private bool isStart = true;
    private bool isFloor = false;
    private bool isLeftWall = false;
    private bool isRightWall = false;
    private bool isCeiling = false;
    private void Update()
    {
        if (!gameObject.activeSelf) isStart = false;
        if (gameObject.activeSelf) isStart = true;
        if (isStart && gameObject.activeSelf)
        {
            Cube_StartMove();
        }
        //Translate로 움직이기 때문에 가끔 물리판정이 무시되어 벽을 뚫는경우를 고려
        if(gameObject.transform.position.x >= ObjectPooling.Instance.MaxDistance || gameObject.transform.position.x <= -ObjectPooling.Instance.MaxDistance)
        {
            ObjectPooling.Instance.cubePool.Remove(gameObject);
            gameObject.SetActive(false);
            ObjectPooling.Instance.cubePool.Add(gameObject);
            gameObject.SetActive(true);
            gameObject.transform.position = ObjectPooling.Instance.transform.position;
            isStart = true;
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        //각 오브젝트들이 충돌하는 colider 이름을 기준으로 gameobj의 방향결정
        if(other.gameObject.name == "Floor")
        {
            isStart = false;
            StartCoroutine(Cube_Touch_Floor());
        }
        if(other.gameObject.name == "LeftWall")
        {
            isStart = false;
            StartCoroutine(Cube_Touch_LeftWall());
        }
        if(other.gameObject.name == "RightWall")
        {
            isStart = false;
            StartCoroutine(Cube_Touch_RightWall());
        }
        if(other.gameObject.name == "Ceiling")
        {
            isStart = false;
            StartCoroutine(Cube_Touch_Ceiling());
        }
    }

    //처음 시작 했을 때는 아래로 움직이게 
    //Todo : ㅇ
    private void Cube_StartMove()
    {
        float moveY = -1 * StartSpeed * Time.deltaTime;
        transform.Translate(0, moveY, 0);
    }

    //바닥에 닿았을 때 z축은 항상고정
    //x값(좌우) 랜덤으로 설정
    //y값은 항상 위쪽으로 
    private IEnumerator Cube_Touch_Floor()
    {
        //y축값은 올라가게 설정
        //다른 녀석들 bool값 초기화 
        isLeftWall = false;
        isRightWall = false;
        isCeiling = false;

        float moveY = StartSpeed * Time.deltaTime;
        float moveX = StartSpeed * Time.deltaTime;

        // 랜덤으로 좌우 방향 선택
        if (!isFloor)
        {
            switch (Random.Range(0, 2))
            {
                case 0:
                    moveX *= -1; // 왼쪽으로 이동
                    break;
                case 1:
                    moveX *= 1; // 오른쪽으로 이동
                    break;
            }
        }
        isFloor = true;
        while (isFloor)
        {
            transform.Translate(moveX, moveY, 0);
            yield return null;
        }
        
    }

    private IEnumerator Cube_Touch_Ceiling()
    {
        isFloor = false;
        isLeftWall = false;
        isRightWall = false;

        //천장을 터치했으니 바닥으로 y값은 -값으로 고정
        float moveY = -1 * StartSpeed * Time.deltaTime;
        float moveX = StartSpeed * Time.deltaTime;

        // 랜덤으로 좌우 방향 선택
        if (!isCeiling) // 처음 천장에 닿았을때 방향을 정해준 후 , 중복실행(방향x축방향을바꾸는) 방지
        {
            switch (Random.Range(0, 2))
            {
                case 0:
                    moveX *= -1; // 왼쪽으로 이동
                    break;
                case 1:
                    moveX *= 1; // 오른쪽으로 이동
                    break;
            }
        }
        isCeiling = true;
        while (isCeiling)
        {
            transform.Translate(moveX, moveY, 0);
            yield return null;
        }
    }


    private IEnumerator Cube_Touch_LeftWall()
    {
        isFloor = false;
        isRightWall = false;
        isCeiling = false;

        //왼쪽 벽을 터치 =>  x값은 right방향(양수) 값으로 고정
        float moveY = StartSpeed * Time.deltaTime;
        float moveX = StartSpeed * Time.deltaTime;
        if (!isLeftWall)
        {
            switch (Random.Range(0,2))
            {
                case 0:
                    moveY *= -1; // 왼쪽으로 이동
                    break;
                case 1:
                    moveY *= 1; // 오른쪽으로 이동
                    break;
            }
        }
        isLeftWall = true;
        while (isLeftWall)
        {
            transform.Translate(moveX, moveY, 0);
            yield return null;
        }
        
    }

    private IEnumerator Cube_Touch_RightWall()
    {
        isFloor = false;
        isLeftWall = false;
        isCeiling = false;

        //오른쪽 벽을 터치 =>  x값은 Left방향(음수) 값으로 고정
        float moveY = StartSpeed * Time.deltaTime;
        float moveX = -1 * StartSpeed * Time.deltaTime;
        if (!isRightWall)
        {
            switch (Random.Range(0, 2))
            {
                case 0:
                    moveY *= -1; // 왼쪽으로 이동
                    break;
                case 1:
                    moveY *= 1; // 오른쪽으로 이동
                    break;
            }
        }
        isRightWall = true;
        while (isRightWall)
        {
            transform.Translate(moveX, moveY, 0);
            yield return null;
        }
        
    }

}
