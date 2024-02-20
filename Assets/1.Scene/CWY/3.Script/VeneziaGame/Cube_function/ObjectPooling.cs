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
        //���� ������Ʈ Ǯ��
        CreateQuestPrefab();

        CreateItem();
        //������ ������ 


        #endregion
    }

    private void Start()
    {
        StartCoroutine(Cube_Co());
        StartCoroutine(Meteor_Co());        
        StartCoroutine(Pause_Co());        
    }
    public int cool;
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

    public IEnumerator Meteor_Co()
    {
        while (MeteorPool.Count > 0)
        {
            float cool = Random.Range(10, 16f);
            yield return new WaitForSeconds(cool); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
            //������ ���� ����
            int Randnum = Random.Range(0, MeteorPool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
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

    private void CreateQuestPrefab()
    {
        for (int i = 0; i < CubePrefab.Length; i++)
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
