using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Before,
    After,
    CorrectAnswer,
    Wronganswer,
}


public class Cube : MonoBehaviour
{ 
    public static ObjectType objectType;

    public float StartSpeed; // ���� ������ �ӵ�  1 
    public float CurrentSpeed; // ���� ������ �ӵ�  1 
    public float SaveSpeed; // �ӵ� ����    
    public float AccelerationSpeed; // �ִ� �ӵ� ����    

    [SerializeField] private bool isStart;
    private bool isFloor = false;
    private bool isLeftWall = false;
    private bool isRightWall = false;
    private bool isCeiling = false;

    public int count = 0;

    private void OnEnable()
    {
        isStart = true;
    }


    private void Update()
    {
        if (!gameObject.activeSelf) isStart = false;
       // if (gameObject.activeSelf) isStart = true;
        if (isStart && gameObject.activeSelf)
        {
            Cube_StartMove();
        }
        if (!isStart) StartSpeed = 0;
        //Translate�� �����̱� ������ ���� ���������� ���õǾ� ���� �մ°�츦 ���
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
        //�� ������Ʈ���� �浹�ϴ� colider �̸��� �������� gameobj�� �������
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

    //ó�� ���� ���� ���� �Ʒ��� �����̰� 
    //Todo : ��ȹ�� ��ȹ�� �Ѿ���� ó�� ���������� �����̴� ������ ������ ����
    private void Cube_StartMove()
    {
        float moveY = -1 * StartSpeed * Time.deltaTime;
        transform.Translate(0, moveY, 0);
    }

    //�ٴڿ� ����� �� z���� �׻����
    //x��(�¿�) �������� ����
    //y���� �׻� �������� 
    private IEnumerator Cube_Touch_Floor()
    {
        //y�ప�� �ö󰡰� ����
        //�ٸ� �༮�� bool�� �ʱ�ȭ 
        isLeftWall = false;
        isRightWall = false;
        isCeiling = false;
        if(count == 0)
        {
            CurrentSpeed = StartSpeed * AccelerationSpeed;
        }
        else
        {
            if(count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        }
        
        float moveY = CurrentSpeed * Time.deltaTime ;
        float moveX = CurrentSpeed * Time.deltaTime ;

        // �������� �¿� ���� ����
        if (!isFloor)
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
        }
        isFloor = true;
        count++;
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
        if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        float moveY = -1 * CurrentSpeed * Time.deltaTime;
        float moveX = CurrentSpeed * Time.deltaTime;

        // �������� �¿� ���� ����
        if (!isCeiling) // ó�� õ�忡 ������� ������ ������ �� , �ߺ�����(����x��������ٲٴ�) ����
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
        }
        isCeiling = true;
        count++;
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
        if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        //���� ���� ��ġ =>  x���� right����(���) ������ ����
        float moveY = CurrentSpeed * Time.deltaTime ;
        float moveX = CurrentSpeed * Time.deltaTime ;
        if (!isLeftWall)
        {
            switch (Random.Range(0,2))
            {
                case 0:
                    moveY *= -1; // �������� �̵�
                    break;
                case 1:
                    moveY *= 1; // ���������� �̵�
                    break;
            }
        }
        isLeftWall = true;
        count++;
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
        if (count <= 10) CurrentSpeed = CurrentSpeed * AccelerationSpeed;
        //������ ���� ��ġ =>  x���� Left����(����) ������ ����
        float moveY = CurrentSpeed * Time.deltaTime * AccelerationSpeed;
        float moveX = -1 * CurrentSpeed * Time.deltaTime * AccelerationSpeed;
        if (!isRightWall)
        {
            switch (Random.Range(0, 2))
            {
                case 0:
                    moveY *= -1; // �������� �̵�
                    break;
                case 1:
                    moveY *= 1; // ���������� �̵�
                    break;
            }
        }
        isRightWall = true;
        count++;
        while (isRightWall)
        {
            transform.Translate(moveX, moveY, 0);
            yield return null;
        }
        
    }

    public float CubeStop()
    {
        SaveSpeed = CurrentSpeed;
        
        if (VeneziaManager.isStop)
        {
            CurrentSpeed = 0f;
            return CurrentSpeed;
        }
        else
        {
            CurrentSpeed = SaveSpeed;
            return CurrentSpeed;
        }
    }


}
