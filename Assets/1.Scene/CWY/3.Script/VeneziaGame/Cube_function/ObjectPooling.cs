using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject CubePrefab; //Ǯ���� ���� & ���� ť�� ������
    [SerializeField] private Transform Pool_Position; //Ǯ�� ��ġ


    [SerializeField] private GameObject MeteorPrefab; //������ Ǯ�� ���� ������



    //������ ť�� ����
    public int CubeCount;
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
            cube.transform.SetParent(Pool_Position);
            cubePool.Add(cube);
        }

        //������ ������ 
        for (int i = 0; i < CubeCount; i++)
        {
            GameObject Meteor = Instantiate(MeteorPrefab); //Ǯ���� ������Ʈ ����
            Meteor.SetActive(false);
            //Vector3 offset = new Vector3(0, 0, 20);
            Meteor.transform.SetParent(Pool_Position);
            cubePool.Add(Meteor);
        }

        #endregion
    }

    private void Start()
    {
        StartCoroutine(Cube_Co());
    }

    private void Update()
    {
        print(cubePool.Count);
    }


    IEnumerator Cube_Co()
    {
        while (cubePool.Count > 0)
        {
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            cubePool[0].transform.position = Pool_Position.transform.position + offset;
            cubePool[0].SetActive(true);
            cubePool.Remove(cubePool[0]);
            yield return new WaitForSeconds(3f); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
        }
    }

    IEnumerator Meteor_Co()
    {
        while (cubePool.Count > 0)
        {
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            cubePool[0].transform.position = Pool_Position.transform.position + offset;
            cubePool[0].SetActive(true);
            cubePool.Remove(cubePool[0]);
            yield return new WaitForSeconds(3f); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
        }
    }

}
