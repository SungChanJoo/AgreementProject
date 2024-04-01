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
    public ObjectType ObjectType;
    public PlayerNum PlayerNum;
    //  public  Quset_exam quset_Exam;

    public float StartSpeed; // ���� ������ �ӵ�  1 
    public float CurrentSpeed; // ���� ������ �ӵ�  1 
    public float SaveSpeed; // �ӵ� ����    
    public float AccelerationSpeed; // ����
    public float MaxAccelerationSpeed; // �ִ�ӵ�


    public float RemoveTime = 0;
    public float RemoveTimeSecond = 0;

    [SerializeField] private bool isStart;
    [SerializeField] public Sprite Sprite;
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

    public float MoveX;
    public float MoveY;
    public int Count = 0;
    public int TouchCount = 0;

    private IEnumerator startFloor;
    private IEnumerator startCeiling;
    private IEnumerator startLeft;
    private IEnumerator startRight;

    private float startDirX;
    private float startDirY;

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
        Count = 0;
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
        startDirX = Random.Range(0, 3);
        startDirY = Random.Range(0, 2) == 0 ? -1f : 1f;
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
        return Sprite;
    }
    //ù ���۽� ������ �������� �����̰� ����
    private void Cube_StartMove_()
    {
        CurrentSpeed = VeneziaManager.Instance.StartSpeed;
        rb.velocity = new Vector3(VeneziaManager.Instance.StartSpeed * startDirX, VeneziaManager.Instance.StartSpeed * startDirY, 0f);
    }
    //���� ����
    private void JudgeCubeObjType()
    {
        if(VeneziaManager.Instance.play_mode == PlayMode.Couple)
        {
            if (VeneziaManager.Instance.Quest_Img.sprite == Sprite || VeneziaManager.Instance.Quest2_Img.sprite == Sprite)
            {
                ObjectType = ObjectType.CorrectAnswer;
            }
            else
            {
                ObjectType = ObjectType.Wronganswer;
            }
        }
        else
        {
            if (VeneziaManager.Instance.Quest_Img.sprite == Sprite)
            {
                ObjectType = ObjectType.CorrectAnswer;
            }
            else
            {
                ObjectType = ObjectType.Wronganswer;
            }
        }

    }

    private void DestoryCube()
    {
        if(VeneziaManager.Instance.play_mode == PlayMode.Solo)
        {
            if (ObjectType == ObjectType.CorrectAnswer && gameObject.activeSelf)
            {
                if (RemoveTime >= VeneziaManager.Instance.DestroyTime)
                {
                    ObjectType = ObjectType.Wronganswer;
                    TimeSlider.Instance.DecreaseTime_Item(3);
                    VeneziaManager.Instance.NextQuest();
                    ObjectPooling.Instance.CubePool.Add(gameObject);
                    gameObject.SetActive(false);
                    RemoveTime = 0;
                    VeneziaManager.Instance.ResetCube();
                }
                else
                {
                    RemoveTime += Time.deltaTime;
                }
            }
        }
    }

    private void GameOver()
    {
        if (VeneziaManager.Instance.IsGameover)
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
                    if (isTouch && Count <= VeneziaManager.Instance.MaxTouchCount)
                    {
                        CurrentSpeed = CurrentSpeed * AccelerationSpeed;
                        isTouch = false;
                        Count++;
                    }
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    MoveX = randomDir * CurrentSpeed;
                }
                MoveY = Mathf.Abs(CurrentSpeed); // y�� ������ ���� ����
                
                rb.velocity = new Vector3(MoveX, MoveY, 0);
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
                    if (isTouch && Count <= VeneziaManager.Instance.MaxTouchCount)
                    {
                        CurrentSpeed = CurrentSpeed * AccelerationSpeed;
                        isTouch = false;
                        Count++;
                    }
                    isDirCelingSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    MoveX = randomDir * CurrentSpeed;
                }
                MoveY = -Mathf.Abs(CurrentSpeed); // y�� ������ ���� ����
                rb.velocity = new Vector3(MoveX, MoveY, 0);
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
                if (!isDirLeftSelect && Count <= VeneziaManager.Instance.MaxTouchCount)
                {
                    isTouch = true;
                    if (isTouch)
                    {
                        CurrentSpeed = CurrentSpeed * AccelerationSpeed;
                        isTouch = false;
                        Count++;
                    }
                    isDirLeftSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    MoveY = randomDir * CurrentSpeed;
                }
                MoveX = Mathf.Abs(CurrentSpeed); // y�� ������ ���� ����
                rb.velocity = new Vector3(MoveX, MoveY, 0);
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
                    if (isTouch && Count <= VeneziaManager.Instance.MaxTouchCount)
                    {
                        CurrentSpeed = CurrentSpeed * AccelerationSpeed;
                        isTouch = false;
                        Count++;
                    }
                    isDirRightSelect = true;
                    float randomDir = Random.Range(0, 2) == 0 ? -1f : 1f;
                    MoveY = randomDir * CurrentSpeed;
                }
                MoveX = -Mathf.Abs(CurrentSpeed); // y�� ������ ���� ����
                rb.velocity = new Vector3(MoveX, MoveY, 0);
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