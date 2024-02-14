using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject CubePrefab; //Ǯ���� ���� & ���� ť�� ������
    [SerializeField] private Transform Pool_Position; //Ǯ�� ��ġ
    [SerializeField] private Transform ParentObject; // Ǯ���� ������Ʈ�� ������ ���̿�����Ʈ


    [SerializeField] private GameObject[] ItemPrefab; //������ Ǯ�� ���� ������



    //������ ť�� ����
    public int CubeCount;
    //������ ������ ����
    public int ItemCount;
    //������ ���õ� prefab�� ����
    public List<GameObject> cubePool = new List<GameObject>();
    //���׿� , ������ �� ������ ����.
    public List<GameObject> ItemPool = new List<GameObject>();
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
        for (int i = 0; i < CubeCount; i++)
        {
            GameObject cube = Instantiate(CubePrefab); //Ǯ���� ������Ʈ ����
            cube.SetActive(false);
            cube.transform.SetParent(ParentObject);
            cubePool.Add(cube);
        }

        CreateItem();
        //������ ������ 


        #endregion
    }

    private void Start()
    {
        StartCoroutine(Cube_Co());
        StartCoroutine(Meteor_Co());
    }

    private void Update()
    {
        print(cubePool.Count);
    }


    public IEnumerator Cube_Co()
    {
        while (cubePool.Count > 0)
        {
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            cubePool[0].transform.position = Pool_Position.transform.position + offset;
            cubePool[0].SetActive(true);
            cubePool.Remove(cubePool[0]);
            yield return new WaitForSeconds(10f); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
        }
    }

    public IEnumerator Meteor_Co()
    {
        while (ItemPool.Count > 0)
        {
            //������ ���� ����
            int Randnum = Random.Range(0, ItemPool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            ItemPool[Randnum].transform.position = Pool_Position.transform.position + offset;
            ItemPool[Randnum].SetActive(true);
            ItemPool.Remove(ItemPool[Randnum]);
            print(Randnum);
            yield return new WaitForSeconds(3f); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
        }
    }

    //������ ����
    private void CreateItem()
    {
        for (int i = 0; i < ItemPrefab.Length; i++)
        {
            for (int j = 0; j < ItemCount; j++)
            {
                GameObject item = Instantiate(ItemPrefab[i]); // �ش� �ε����� �������� �ν��Ͻ�ȭ
                item.SetActive(false); // Ȱ��ȭ���� ����
                item.transform.SetParent(ParentObject); // Ǯ ��ġ�� �θ� ����
                ItemPool.Add(item); // ������ Ǯ ����Ʈ�� �߰�
            }
        }
    }
}
