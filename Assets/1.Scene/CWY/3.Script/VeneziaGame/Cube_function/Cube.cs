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

    public float StartSpeed; // ���� ������ �ӵ�  1 
    public float CurrentSpeed; // ���� ������ �ӵ�  1 
    public float SaveSpeed; // �ӵ� ����    
    public float AccelerationSpeed; // �ִ� �ӵ� ����    

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

    //ó�� ���� ���� ���� �Ʒ��� �����̰� 
    //Todo : ��ȹ�� ��ȹ�� �Ѿ���� ó�� ���������� �����̴� ������ ������ ����
    private void Cube_StartMove_()
    {
        // ������Ʈ�� �Ʒ� �������� �̵��� �ӵ� ����
        float moveSpeedY = -StartSpeed;
        CurrentSpeed = StartSpeed;
        rb.velocity = new Vector3(0f, moveSpeedY, 0f);
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
            if (isFloor) //�ٴڰ� ��ұ� ������ Cube_Touch ����.
            {
                Debug.DrawLine(transform.position, hitDown.point, Color.red);
                // �¿� ������ �������� ����
                if (!isDirFloorSelect)
                {
                    isDirFloorSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    print(randomDir);
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
            if (isCeiling) //�ٴڰ� ��ұ� ������ Cube_Touch ����.
            {
                Debug.DrawLine(transform.position, hitUp.point, Color.red);
                // �¿� ������ �������� ����
                if (!isDirCelingSelect)
                {
                    isDirCelingSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    print(randomDir);
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
            if (isLeftWall) //�ٴڰ� ��ұ� ������ Cube_Touch ����.
            {
                Debug.DrawLine(transform.position, hitLeft.point, Color.red);
                // �¿� ������ �������� ����
                if (!isDirLeftSelect)
                {
                    isDirLeftSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    print(randomDir);
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
                // �¿� ������ �������� ����
                if (!isDirRightSelect)
                {
                    isDirRightSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    print(randomDir);
                    moveY = randomDir * CurrentSpeed;
                }
                moveX = -Mathf.Abs(CurrentSpeed); // y�� ������ ���� ����
                rb.velocity = new Vector3(moveX, moveY, 0);
                Debug.DrawLine(transform.position, hitRight.point, Color.green);
            }
        }
    }


}