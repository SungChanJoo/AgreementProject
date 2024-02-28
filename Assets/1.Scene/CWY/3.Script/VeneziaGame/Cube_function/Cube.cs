using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Wronganswer,
    CorrectAnswer,
}

/*public enum Quset_exam
{
    Before,
    Current,
    After,
}*/


public class Cube : MonoBehaviour
{
    public ObjectType objectType;
    //  public  Quset_exam quset_Exam;

    public float StartSpeed; // 공을 움직일 속도  1 
    public float CurrentSpeed; // 공을 움직일 속도  1 
    public float SaveSpeed; // 속도 저장    
    public float AccelerationSpeed; // 최대 속도 제한    

    public float removeTime = 0;

    [SerializeField] private bool isStart;
    [SerializeField] private Sprite sprite;
    private Rigidbody rb;
    private bool isFloor = false;
    private bool isLeftWall = false;
    private bool isRightWall = false;
    private bool isCeiling = false;

    public int count = 0;
    private void Awake()
    {
        TryGetComponent(out rb);
    }
    private void OnEnable()
    {
        isStart = true;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            isStart = false;
            isFloor = false;
            isLeftWall = false;
            isRightWall = false;
            isCeiling = false;
        }

        // if (gameObject.activeSelf) isStart = true;
        if (isStart && gameObject.activeSelf)
        {
            Cube_StartMove_();
        }
        JudgeCubeObjType();
        GameOver();
        DestoryCube();
    }
    private void OnTriggerEnter(Collider other)
    {
        //각 오브젝트들이 충돌하는 colider 이름을 기준으로 gameobj의 방향결정
        if (other.gameObject.name == "Floor")
        {
            isStart = false;
            StartCoroutine(Cube_Touch_Floor());
        }
        if (other.gameObject.name == "LeftWall")
        {
            isStart = false;
            StartCoroutine(Cube_Touch_LeftWall());
        }
        if (other.gameObject.name == "RightWall")
        {
            isStart = false;
            StartCoroutine(Cube_Touch_RightWall());
        }
        if (other.gameObject.name == "Ceiling")
        {
            isStart = false;
            StartCoroutine(Cube_Touch_Ceiling());
        }
    }

    //처음 시작 했을 때는 아래로 움직이게 
    //Todo : 기획팀 기획안 넘어오면 처음 시작했을때 움직이는 로직은 변경할 예정
    private void Cube_StartMove_()
    {
        // 오브젝트를 아래 방향으로 이동할 속도 설정
        float moveSpeedY = -StartSpeed;
        rb.velocity = new Vector3(0f, moveSpeedY, 0f);
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
        if (count == 0)
        {
            CurrentSpeed = StartSpeed * AccelerationSpeed;
        }
        else
        {
            if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        }

        float moveX = CurrentSpeed;
        float moveY = CurrentSpeed; //방향을 위로 전환

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
        count++;
        while (isFloor)
        {
            // Time.timeScale이 0일 때는 즉시 종료
            if (Time.timeScale == 0)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                rb.velocity = new Vector3(moveX, moveY, 0);
            }
            yield return null;
        }

    }

    private IEnumerator Cube_Touch_Ceiling()
    {
        isFloor = false;
        isLeftWall = false;
        isRightWall = false;
        if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        float moveY = -1 * CurrentSpeed;
        float moveX = CurrentSpeed;

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
        count++;
        /*        while (isCeiling)
                {
                    transform.Translate(moveX, moveY, 0);
                    yield return null;
                }*/
        while (isCeiling)
        {
            // Time.timeScale이 0일 때는 즉시 종료
            if (Time.timeScale == 0)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                rb.velocity = new Vector3(moveX, moveY, 0);
            }
            yield return null;
        }
    }


    private IEnumerator Cube_Touch_LeftWall()
    {
        isFloor = false;
        isRightWall = false;
        isCeiling = false;
        if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        //왼쪽 벽을 터치 =>  x값은 right방향(양수) 값으로 고정
        float moveY = CurrentSpeed;
        float moveX = CurrentSpeed;
        if (!isLeftWall)
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
        isLeftWall = true;
        count++;
        /*        while (isLeftWall)
                {
                    transform.Translate(moveX, moveY, 0);
                    yield return null;
                }*/
        while (isLeftWall)
        {
            // Time.timeScale이 0일 때는 즉시 종료
            if (Time.timeScale == 0)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                rb.velocity = new Vector3(moveX, moveY, 0);
            }
            yield return null;
        }

    }

    private IEnumerator Cube_Touch_RightWall()
    {

        isFloor = false;
        isLeftWall = false;
        isCeiling = false;
        if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        //오른쪽 벽을 터치 =>  x값은 Left방향(음수) 값으로 고정
        float moveY = CurrentSpeed;
        float moveX = -1 * CurrentSpeed;
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
        count++;
        while (isRightWall)
        {
            // Time.timeScale이 0일 때는 즉시 종료
            if (Time.timeScale == 0)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                rb.velocity = new Vector3(moveX, moveY, 0);
            }
            yield return null;
        }
    }
    private void JudgeCubeObjType()
    {
        if (VeneziaManager.Instance.Quest_Img.sprite == sprite)
        {
            objectType = ObjectType.CorrectAnswer;
        }
        else
        {
            objectType = ObjectType.Wronganswer;
        }
    }

    private void DestoryCube()
    {
        if (objectType == ObjectType.CorrectAnswer && gameObject.activeSelf)
        {
            if (removeTime >= VeneziaManager.Instance.DestroyTime)
            {
                objectType = ObjectType.Wronganswer;
                TimeSlider.Instance.DecreaseTime_Item(3);
                VeneziaManager.Instance.NextQuest();
                ObjectPooling.Instance.cubePool.Add(gameObject);
                gameObject.SetActive(false);
                removeTime = 0;
                if (ObjectPooling.Instance.cubePool.Count != 0)
                {
                    StopCoroutine(ObjectPooling.Instance.CubePooling);
                    ObjectPooling.Instance.ReStartCubePooling_co();
                }
            }
            else
            {
                removeTime += Time.deltaTime;
            }
        }
    }

    private void GameOver()
    {
        if (VeneziaManager.Instance.isGameover)
        {
            gameObject.SetActive(false);
        }
    }



}