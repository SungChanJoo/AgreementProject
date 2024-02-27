using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject[] CubePrefab; //Ǯ���� ���� & ���� ť�� ������
    [SerializeField] private Transform Pool_Position; //Ǯ�� ��ġ
    [SerializeField] private Transform ParentObject; // Ǯ���� ������Ʈ�� ������ ���̿�����Ʈ


    [SerializeField] private GameObject MeteorPrefab; //������ Ǯ�� ���� ������
    [SerializeField] private GameObject PausePrefab; //������ Ǯ�� ���� ������

    public IEnumerator CubePooling;
    public IEnumerator CubeRestartPooling;


    //������ ť�� ����
    public int CubeCount;
    //������ ������ ����
    public int ItemCount;
    //������ ���õ� prefab�� ����
    public List<GameObject> cubePool = new List<GameObject>();
    //���׿� , ������ �� ������ ����.
    public List<GameObject> MeteorPool = new List<GameObject>();
    public List<GameObject> PausePool = new List<GameObject>();
    //ť���� �ִ� �ݰ�Ÿ� ����
    public float MaxDistance;
    private void Awake()
    {
        #region Singleton 
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        #endregion

        #region Object Pooling

    }

    private void Start()
    {
        CreateItem();
        //������ ������ 


        #endregion
        //StartCoroutine(Cube_Co());
        StartCoroutine(Meteor_Co());
        StartCoroutine(Pause_Co());
    }
    public int cool;

    public void StartCubePooling_co()
    {
        CubePooling = Cube_Co();
        StartCoroutine(CubePooling);
    }
    public void ReStartCubePooling_co()
    {
        CubePooling = Cube_Co();
        StartCoroutine(CubePooling);
    }
    public IEnumerator Cube_Co()
    {
        while (cubePool.Count > 0)
        {
            int Randnum = Random.Range(0, cubePool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            cubePool[Randnum].transform.position = Pool_Position.transform.position + offset;
            cubePool[Randnum].SetActive(true);
            cubePool.Remove(cubePool[Randnum]);
            yield return new WaitForSeconds(cool); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
        }

    }

    public IEnumerator ReStartCube_Co()
    {
        yield return new WaitForSeconds(cool);
        while (cubePool.Count > 0)
        {
            int Randnum = Random.Range(0, cubePool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            cubePool[Randnum].transform.position = Pool_Position.transform.position + offset;
            cubePool[Randnum].SetActive(true);
            cubePool.Remove(cubePool[Randnum]);
            yield return new WaitForSeconds(cool); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
        }
    }


    public IEnumerator Meteor_Co()
    {
        while (MeteorPool.Count > 0)
        {
            float cool = Random.Range(10, 16f);
            yield return new WaitForSeconds(cool); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
            //������ ���� ����
            int Randnum = Random.Range(0, MeteorPool.Count);
            float randomValueX = Random.Range(-100, 101);
            //  float randomValueZ = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValueX, 0, -40); // �¿� ���������� ������
            MeteorPool[Randnum].transform.position = Pool_Position.transform.position + offset;
            MeteorPool[Randnum].SetActive(true);
            MeteorPool.Remove(MeteorPool[Randnum]);
        }
    }

    public IEnumerator Pause_Co()
    {
        while (PausePool.Count > 0)
        {
            float cool = Random.Range(45, 61f);
            yield return new WaitForSeconds(cool); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
            //������ ���� ����
            int Randnum = Random.Range(0, MeteorPool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            PausePool[Randnum].transform.position = Pool_Position.transform.position + offset;
            PausePool[Randnum].SetActive(true);
            PausePool.Remove(PausePool[Randnum]);
        }
    }

    //������ ����
    private void CreateItem()
    {
        //���׿�
        for (int i = 0; i < ItemCount; i++)
        {
            GameObject item = Instantiate(MeteorPrefab); // �ش� �ε����� �������� �ν��Ͻ�ȭ
            item.SetActive(false); // Ȱ��ȭ���� ����
            item.transform.SetParent(ParentObject); // Ǯ ��ġ�� �θ� ����
            MeteorPool.Add(item); // ������ Ǯ ����Ʈ�� �߰�
        }
        //�Ͻ����� 
        for (int i = 0; i < ItemCount; i++)
        {
            GameObject item = Instantiate(PausePrefab); // �ش� �ε����� �������� �ν��Ͻ�ȭ
            item.SetActive(false); // Ȱ��ȭ���� ����
            item.transform.SetParent(ParentObject); // Ǯ ��ġ�� �θ� ����
            PausePool.Add(item); // ������ Ǯ ����Ʈ�� �߰�
        }
    }

    public void CreateQuestPrefab(int count) //���� ���������Ҷ� �μ����� ���� ��.
    {

        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < CubeCount; j++)
            {
                GameObject QuestPrefab = Instantiate(CubePrefab[i]); //Ǯ���� ������Ʈ ����
                QuestPrefab.SetActive(false);
                QuestPrefab.transform.SetParent(ParentObject);
                cubePool.Add(QuestPrefab);
            }
        }
    }
}