using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Wronganswer,
    CorrectAnswer,
}

public enum PlayerNum
{
    One,
    Two
}


public class Cube : MonoBehaviour
{
    public ObjectType objectType;
    public PlayerNum playerNum;
    //  public  Quset_exam quset_Exam;

    public float StartSpeed; // ���� ������ �ӵ�  1 
    public float CurrentSpeed; // ���� ������ �ӵ�  1 
    public float SaveSpeed; // �ӵ� ����    
    public float AccelerationSpeed; // ����
    public float MaxAccelerationSpeed; // �ִ�ӵ�


    public float removeTime = 0;
    public float removeTimeSecond = 0;

    [SerializeField] private bool isStart;
    [SerializeField] public Sprite sprite;
    private Rigidbody rb;
    private bool isFloor = false;
    private bool isLeftWall = false;
    private bool isRightWall = false;
    private bool isCeiling = false;
    private bool isTouch = false;

    private bool isDirFloorSelect = false;
    private bool isDirCelingSelect = false;
    private bool isDirLeftSelect = false;
    private bool isDirRightSelect = false;

    private bool isCubeMoving = false;

    public float moveX;
    public float moveY;
    public int count = 0;
    public int TouchCount = 0;

    private IEnumerator Start_Floor;
    private IEnumerator Start_Ceiling;
    private IEnumerator Start_Left;
    private IEnumerator Start_Right;

    private float StartDirX;
    private float StartDirY;

    private void Awake()
    {
        TryGetComponent(out rb);
    }
    private void OnEnable()
    {
     
        isStart = true;
    }

    private void OnDisable()
    {        
        CurrentSpeed = 0;
        count = 0;
        isStart = false;
        isFloor = false;
        isLeftWall = false;
        isRightWall = false;
        isCeiling = false;
        isDirFloorSelect = false;
        isDirCelingSelect = false;
        isDirLeftSelect = false;
        isDirRightSelect = false;
        isTouch = false;
    }

    private void Start()
    {
        StartDirX = Random.Range(0, 3);
        StartDirY = Random.Range(0, 2) == 0 ? -1f : 1f;
    }

    private void Update()
    {
        if (isStart && gameObject.activeSelf)
        {
            Cube_StartMove_();
        }
        JudgeCubeObjType();
        GameOver();
        DestoryCube();
        ShootRaycasts();
    }

    public Sprite GetCubeSprite()
    {
        return sprite;
    }

    //ó�� ���� ���� ���� �Ʒ��� �����̰� 
    //Todo : ��ȹ�� ��ȹ�� �Ѿ���� ó�� ���������� �����̴� ������ ������ ����
    private void Cube_StartMove_()
    {
        // ������Ʈ�� �Ʒ� �������� �̵��� �ӵ� ����
        CurrentSpeed = VeneziaManager.Instance.StartSpeed;
        rb.velocity = new Vector3(VeneziaManager.Instance.StartSpeed * StartDirX, VeneziaManager.Instance.StartSpeed * StartDirY, 0f);
    }


    #region �ڷ�ƾ �������������� �ӽ�����
    //�ٴڿ� ����� �� z���� �׻����
    //x��(�¿�) �������� ����
    //y���� �׻� �������� 


    private void Cube_Touch_Floor()
    {
        isLeftWall = false;
        isRightWall = false;
        isCeiling = false;
        if (count <= 10 && isFloor) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        moveX = CurrentSpeed;
        moveY = Mathf.Abs(CurrentSpeed); //������ ���� ��ȯ

        // �������� �¿� ���� ����
        if (isFloor)
        {

            switch (Random.Range(0, 2))
            {
                case 0:
                    moveX *= -1; // �������� �̵�
                    break;
                case 1:
                    moveX *= 1; // ���������� �̵�
                    break;
            }
            count++;
        }

        if (isFloor)
        {
            // Time.timeScale�� 0�� ���� ��� ����
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

        // �������� �¿� ���� ����
        switch (Random.Range(0, 2))
        {
            case 0:
                moveX *= -1; // �������� �̵�
                break;
            case 1:
                moveX *= 1; // ���������� �̵�
                break;
        }
        isCeiling = true;
        count++;
        while (isCeiling)
        {
            // Time.timeScale�� 0�� ���� ��� ����
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
        //���� ���� ��ġ =>  x���� right����(���) ������ ����
         moveY = CurrentSpeed;
         moveX = Mathf.Abs(CurrentSpeed);
        switch (Random.Range(0, 2))
        {
            case 0:
                moveX *= -1; // �������� �̵�
                break;
            case 1:
                moveX *= 1; // ���������� �̵�
                break;
        }
        isLeftWall = true;
        count++;
        while (isLeftWall)
        {
            // Time.timeScale�� 0�� ���� ��� ����
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
        //������ ���� ��ġ =>  x���� Left����(����) ������ ����
         moveY = CurrentSpeed;
         moveX = -Mathf.Abs(CurrentSpeed);

        switch (Random.Range(0, 2))
        {
            case 0:
                moveX *= -1; // �������� �̵�
                break;
            case 1:
                moveX *= 1; // ���������� �̵�
                break;
        }

        isRightWall = true;
        count++;
        while (isRightWall)
        {
            // Time.timeScale�� 0�� ���� ��� ����
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
        if(VeneziaManager.Instance.veneGameMode == VeneGameMode.Couple)
        {
            if (VeneziaManager.Instance.Quest_Img.sprite == sprite || VeneziaManager.Instance.Quest2_Img.sprite == sprite)
            {
                objectType = ObjectType.CorrectAnswer;
            }
            else
            {
                objectType = ObjectType.Wronganswer;
            }
        }
        else
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

    }

    private void DestoryCube()
    {
        if(VeneziaManager.Instance.veneGameMode == VeneGameMode.Sole)
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
                    VeneziaManager.Instance.ResetCube();
                }
                else
                {
                    removeTime += Time.deltaTime;
                }
            }
        }
/*        else
        {
            if(objectType == ObjectType.CorrectAnswer && gameObject.activeSelf)
            {
                if (removeTime >= VeneziaManager.Instance.DestroyTime && playerNum == PlayerNum.One)
                {
                    objectType = ObjectType.Wronganswer;
                    TimeSlider.Instance.DecreaseTime_Item(3);
                    ObjectPooling.Instance.cubePool.Add(gameObject);
                    gameObject.SetActive(false);
                    removeTime = 0;
                    VeneziaManager.Instance.NextQuest();
                    VeneziaManager.Instance.ResetCube();
                }
                else
                {
                    removeTime += Time.deltaTime;
                }

                if (removeTimeSecond >= VeneziaManager.Instance.DestroyTime && playerNum == PlayerNum.Two)
                {
                    objectType = ObjectType.Wronganswer;
                    TimeSlider.Instance.DecreaseTime_Item(3);
                    VeneziaManager.Instance.NextQuest();
                    ObjectPooling.Instance.cubePoolTwo.Add(gameObject);
                    gameObject.SetActive(false);
                    removeTimeSecond = 0;
                    VeneziaManager.Instance.ResetCube();
                }
                else
                {
                    removeTimeSecond += Time.deltaTime;
                }
            }
        }*/

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
        // �Ʒ��� ���� ����ĳ��Ʈ
        int layerMask = ~LayerMask.GetMask("Quest");
        float collideDistance = 13.5f;        
        RaycastHit hitDown;
        if (Physics.Raycast(transform.position, -Vector3.up, out hitDown, Mathf.Infinity, layerMask))
        {
            // ����ĳ��Ʈ�� �浹�� ����� ó��
            Debug.DrawLine(transform.position, hitDown.point, Color.green);

            // �浹 ���������� �Ÿ�
            float hitDistance = hitDown.distance;  // 13.5f ���� �Ÿ����۰� , �ٴڿ� �ε������� ���ٸ�...
            if (hitDistance <= collideDistance) 
            {                
                isFloor = true;
                isCeiling = false;
                isLeftWall = false;
                isRightWall = false;
                isStart = false;

                isDirCelingSelect = false;
                isDirLeftSelect = false;
                isDirRightSelect = false;

                //���ӵ� ���� ����
            }
            if (isFloor)
            {
                Debug.DrawLine(transform.position, hitDown.point, Color.red);
                // �¿� ������ �������� ����
                if (!isDirFloorSelect)
                {
                    isDirFloorSelect = true;
                    isTouch = true;
                    if (isTouch && count <= VeneziaManager.Instance.MaxTouchCount)
                    {
                        CurrentSpeed = CurrentSpeed * AccelerationSpeed;
                        isTouch = false;
                        count++;
                    }
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    moveX = randomDir * CurrentSpeed;
                }
                moveY = Mathf.Abs(CurrentSpeed); // y�� ������ ���� ����
                
                rb.velocity = new Vector3(moveX, moveY, 0);
                Debug.DrawLine(transform.position, hitDown.point, Color.green);
            }
        }
        // ���� ���� ����ĳ��Ʈ
        RaycastHit hitUp;
        if (Physics.Raycast(transform.position, Vector3.up, out hitUp, Mathf.Infinity, layerMask))
        {
            Debug.DrawLine(transform.position, hitUp.point, Color.green);
            float hitDistance = hitUp.distance;
            if (hitDistance <= collideDistance)
            {
                isFloor = false;
                isCeiling = true;
                isLeftWall = false;
                isRightWall = false;
                isStart = false;

                isDirLeftSelect = false;
                isDirFloorSelect = false;
                isDirRightSelect = false;
            }
            if (isCeiling)
            {
                Debug.DrawLine(transform.position, hitUp.point, Color.red);
                // �¿� ������ �������� ����
                if (!isDirCelingSelect)
                {
                    isTouch = true;
                    if (isTouch && count <= VeneziaManager.Instance.MaxTouchCount)
                    {
                        CurrentSpeed = CurrentSpeed * AccelerationSpeed;
                        isTouch = false;
                        count++;
                    }
                    isDirCelingSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    moveX = randomDir * CurrentSpeed;
                }
                moveY = -Mathf.Abs(CurrentSpeed); // y�� ������ ���� ����
                rb.velocity = new Vector3(moveX, moveY, 0);
                Debug.DrawLine(transform.position, hitUp.point, Color.green);
            }
        }

        // ���� ���� ����ĳ��Ʈ
        RaycastHit hitLeft;
        if (Physics.Raycast(transform.position, -Vector3.right, out hitLeft, Mathf.Infinity, layerMask))
        {
            Debug.DrawLine(transform.position, hitLeft.point, Color.green);
            float hitDistance = hitLeft.distance;
            if (hitDistance <= collideDistance)
            {
                isFloor = false;
                isCeiling = false;
                isLeftWall = true;
                isRightWall = false;
                isStart = false;

                isDirCelingSelect = false;
                isDirFloorSelect = false;
                isDirRightSelect = false;
            }
            if (isLeftWall)
            {
                Debug.DrawLine(transform.position, hitLeft.point, Color.red);
                // �¿� ������ �������� ����
                if (!isDirLeftSelect && count <= VeneziaManager.Instance.MaxTouchCount)
                {
                    isTouch = true;
                    if (isTouch)
                    {
                        CurrentSpeed = CurrentSpeed * AccelerationSpeed;
                        isTouch = false;
                        count++;
                    }
                    isDirLeftSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    moveY = randomDir * CurrentSpeed;
                }
                moveX = Mathf.Abs(CurrentSpeed); // y�� ������ ���� ����
                rb.velocity = new Vector3(moveX, moveY, 0);
                Debug.DrawLine(transform.position, hitLeft.point, Color.green);
            }
        }

        // ������ ���� ����ĳ��Ʈ
        RaycastHit hitRight;
        if (Physics.Raycast(transform.position, Vector3.right, out hitRight, Mathf.Infinity, layerMask))
        {
            Debug.DrawLine(transform.position, hitRight.point, Color.green);
            float hitDistance = hitRight.distance;
            if (hitDistance <= collideDistance)
            {
                isFloor = false;
                isCeiling = false;
                isLeftWall = false;
                isRightWall = true;
                isStart = false;

                isDirLeftSelect = false;
                isDirCelingSelect = false;
                isDirFloorSelect = false;
            }
            if (isRightWall)
            {
                Debug.DrawLine(transform.position, hitRight.point, Color.red);
                // �¿� ������ �������� ����
                if (!isDirRightSelect)
                {
                    isTouch = true;
                    if (isTouch && count <= VeneziaManager.Instance.MaxTouchCount)
                    {
                        CurrentSpeed = CurrentSpeed * AccelerationSpeed;
                        isTouch = false;
                        count++;
                    }
                    isDirRightSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    moveY = randomDir * CurrentSpeed;
                }
                moveX = -Mathf.Abs(CurrentSpeed); // y�� ������ ���� ����
                rb.velocity = new Vector3(moveX, moveY, 0);
                Debug.DrawLine(transform.position, hitRight.point, Color.green);
            }
        }
    }



    public Color gizmoColor = Color.blue;
    public float radius = 15f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}