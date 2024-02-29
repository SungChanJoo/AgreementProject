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


    private bool isDirFloorSelect = false;
    private bool isDirCelingSelect = false;
    private bool isDirLeftSelect = false;
    private bool isDirRightSelect = false;

    private bool isCubeMoving = false;

    public float moveX;
    public float moveY;
    public int count = 0;

    private IEnumerator Start_Floor;
    private IEnumerator Start_Ceiling;
    private IEnumerator Start_Left;
    private IEnumerator Start_Right;

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

    private void FixedUpdate()
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
            isDirFloorSelect = false;
            isDirCelingSelect = false;
            isDirLeftSelect = false;
            isDirRightSelect = false;
        }

        // if (gameObject.activeSelf) isStart = true;
        if (isStart && gameObject.activeSelf)
        {
            Cube_StartMove_();
        }
        JudgeCubeObjType();
        GameOver();
        //DestoryCube();
        ShootRaycasts();
    }

    //처음 시작 했을 때는 아래로 움직이게 
    //Todo : 기획팀 기획안 넘어오면 처음 시작했을때 움직이는 로직은 변경할 예정
    private void Cube_StartMove_()
    {
        // 오브젝트를 아래 방향으로 이동할 속도 설정
        float moveSpeedY = -StartSpeed;
        CurrentSpeed = StartSpeed;
        rb.velocity = new Vector3(0f, moveSpeedY, 0f);
    }


    #region 코루틴 현재사용하지않음 임시저장
    //바닥에 닿았을 때 z축은 항상고정
    //x값(좌우) 랜덤으로 설정
    //y값은 항상 위쪽으로 
    private void Cube_Touch_Floor()
    {
        isLeftWall = false;
        isRightWall = false;
        isCeiling = false;
        if (count <= 10 && isFloor) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        moveX = CurrentSpeed;
        moveY = Mathf.Abs(CurrentSpeed); //방향을 위로 전환

        // 랜덤으로 좌우 방향 선택
        if (isFloor)
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
            count++;
        }

        if (isFloor)
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
        }
    }
    private IEnumerator Cube_Touch_Ceiling()
    {
        isFloor = false;
        isLeftWall = false;
        isRightWall = false;
        if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
         moveY = -Mathf.Abs(CurrentSpeed);
         moveX = CurrentSpeed;

        // 랜덤으로 좌우 방향 선택
        switch (Random.Range(0, 2))
        {
            case 0:
                moveX *= -1; // 왼쪽으로 이동
                break;
            case 1:
                moveX *= 1; // 오른쪽으로 이동
                break;
        }
        isCeiling = true;
        count++;
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
            yield return new WaitForFixedUpdate();
        }
    }

    private void C_Ceiling()
    {
        Start_Ceiling = Cube_Touch_Ceiling();
        StartCoroutine(Start_Ceiling);
    }

    private IEnumerator Cube_Touch_LeftWall()
    {
        isFloor = false;
        isRightWall = false;
        isCeiling = false;
        if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        //왼쪽 벽을 터치 =>  x값은 right방향(양수) 값으로 고정
         moveY = CurrentSpeed;
         moveX = Mathf.Abs(CurrentSpeed);
        switch (Random.Range(0, 2))
        {
            case 0:
                moveX *= -1; // 왼쪽으로 이동
                break;
            case 1:
                moveX *= 1; // 오른쪽으로 이동
                break;
        }
        isLeftWall = true;
        count++;
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
            yield return new WaitForFixedUpdate();
        }
    }
    private void C_Left()
    {
        Start_Left = Cube_Touch_Ceiling();
        StartCoroutine(Start_Left);
    }
    private IEnumerator Cube_Touch_RightWall()
    {
        isFloor = false;
        isLeftWall = false;
        isCeiling = false;
        if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        //오른쪽 벽을 터치 =>  x값은 Left방향(음수) 값으로 고정
         moveY = CurrentSpeed;
         moveX = -Mathf.Abs(CurrentSpeed);

        switch (Random.Range(0, 2))
        {
            case 0:
                moveX *= -1; // 왼쪽으로 이동
                break;
            case 1:
                moveX *= 1; // 오른쪽으로 이동
                break;
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
            yield return new WaitForFixedUpdate();
        }
    }
    private void C_Right()
    {
        Start_Right = Cube_Touch_Ceiling();
        StartCoroutine(Start_Right);
    }

    #endregion
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

    private void ShootRaycasts()
    {
        // 아래쪽 방향 레이캐스트
        int layerMask = ~LayerMask.GetMask("Quest");
        float collideDistance = 13.5f;
        RaycastHit hitDown;
        if (Physics.Raycast(transform.position, -Vector3.up, out hitDown, Mathf.Infinity, layerMask))
        {
            // 레이캐스트가 충돌한 경우의 처리
            Debug.DrawLine(transform.position, hitDown.point, Color.green);

            // 충돌 지점까지의 거리
            float hitDistance = hitDown.distance;  // 13.5f 보다 거리가작고 , 바닥에 부딪힌적이 없다면...
            if (hitDistance < collideDistance) 
            {
                isFloor = true;
                isCeiling = false;
                isLeftWall = false;
                isRightWall = false;

                isDirCelingSelect = false;
                isDirLeftSelect = false;
                isDirRightSelect = false;
            } 
            if (isFloor) //바닥과 닿았기 때문에 Cube_Touch 실행.
            {
                Debug.DrawLine(transform.position, hitDown.point, Color.red);
                // 좌우 방향을 랜덤으로 선택
                if (!isDirFloorSelect)
                {
                    isDirFloorSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    print(randomDir);
                    moveX = randomDir * CurrentSpeed;
                }
                moveY = Mathf.Abs(CurrentSpeed); // y축 방향은 위로 설정
                
                rb.velocity = new Vector3(moveX, moveY, 0);
                Debug.DrawLine(transform.position, hitDown.point, Color.green);
            }
        }
        // 위쪽 방향 레이캐스트
        RaycastHit hitUp;
        if (Physics.Raycast(transform.position, Vector3.up, out hitUp, Mathf.Infinity, layerMask))
        {
            Debug.DrawLine(transform.position, hitUp.point, Color.green);
            float hitDistance = hitUp.distance;
            if (hitDistance < collideDistance)
            {
                isFloor = false;
                isCeiling = true;
                isLeftWall = false;
                isRightWall = false;

                isDirLeftSelect = false;
                isDirFloorSelect = false;
                isDirRightSelect = false;
            }
            if (isCeiling) //바닥과 닿았기 때문에 Cube_Touch 실행.
            {
                Debug.DrawLine(transform.position, hitUp.point, Color.red);
                // 좌우 방향을 랜덤으로 선택
                if (!isDirCelingSelect)
                {
                    isDirCelingSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    print(randomDir);
                    moveX = randomDir * CurrentSpeed;
                }
                moveY = -Mathf.Abs(CurrentSpeed); // y축 방향은 위로 설정
                rb.velocity = new Vector3(moveX, moveY, 0);
                Debug.DrawLine(transform.position, hitUp.point, Color.green);
            }
        }

        // 왼쪽 방향 레이캐스트
        RaycastHit hitLeft;
        if (Physics.Raycast(transform.position, -Vector3.right, out hitLeft, Mathf.Infinity, layerMask))
        {
            Debug.DrawLine(transform.position, hitLeft.point, Color.green);
            float hitDistance = hitLeft.distance;
            if (hitDistance < collideDistance)
            {
                isFloor = false;
                isCeiling = false;
                isLeftWall = true;
                isRightWall = false;

                isDirCelingSelect = false;
                isDirFloorSelect = false;
                isDirRightSelect = false;
            }
            if (isLeftWall) //바닥과 닿았기 때문에 Cube_Touch 실행.
            {
                Debug.DrawLine(transform.position, hitLeft.point, Color.red);
                // 좌우 방향을 랜덤으로 선택
                if (!isDirLeftSelect)
                {
                    isDirLeftSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    print(randomDir);
                    moveY = randomDir * CurrentSpeed;
                }
                moveX = Mathf.Abs(CurrentSpeed); // y축 방향은 위로 설정
                rb.velocity = new Vector3(moveX, moveY, 0);
                Debug.DrawLine(transform.position, hitLeft.point, Color.green);
            }
        }

        // 오른쪽 방향 레이캐스트
        RaycastHit hitRight;
        if (Physics.Raycast(transform.position, Vector3.right, out hitRight, Mathf.Infinity, layerMask))
        {
            Debug.DrawLine(transform.position, hitRight.point, Color.green);
            float hitDistance = hitRight.distance;
            if (hitDistance < collideDistance)
            {
                isFloor = false;
                isCeiling = false;
                isLeftWall = false;
                isRightWall = true;

                isDirLeftSelect = false;
                isDirCelingSelect = false;
                isDirFloorSelect = false;
            }
            if (isRightWall)
            {
                Debug.DrawLine(transform.position, hitRight.point, Color.red);
                // 좌우 방향을 랜덤으로 선택
                if (!isDirRightSelect)
                {
                    isDirRightSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    print(randomDir);
                    moveY = randomDir * CurrentSpeed;
                }
                moveX = -Mathf.Abs(CurrentSpeed); // y축 방향은 위로 설정
                rb.velocity = new Vector3(moveX, moveY, 0);
                Debug.DrawLine(transform.position, hitRight.point, Color.green);
            }
        }
    }


}