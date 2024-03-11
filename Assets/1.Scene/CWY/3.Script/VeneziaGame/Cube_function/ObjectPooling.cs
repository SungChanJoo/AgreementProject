using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject Cube;
    [SerializeField] private Transform Pool_Position; //Ǯ�� ��ġ
    [SerializeField] private Transform CubeParentObject; // Ǯ���� ������Ʈ�� ������ ���̿�����Ʈ
    [SerializeField] private Transform ItemParentObject; // Ǯ���� ������Ʈ�� ������ ���̿�����Ʈ


    [SerializeField] private GameObject MeteorPrefab; //������ Ǯ�� ���� ������
    [SerializeField] private GameObject PausePrefab; //������ Ǯ�� ���� ������

    public IEnumerator CubePooling;
    public IEnumerator CubeRestartPooling;

    [SerializeField] Sprite sprite;
    [SerializeField] Image QuestImg;

    //������ ť�� ����
    public int CubeCount;
    //������ ������ ����
    public int ItemCount;
    //������ ���õ� prefab�� ����
    public List<GameObject> cubePool = new List<GameObject>();
    //���׿� , ������ �� ������ ����.
    public List<GameObject> MeteorPool = new List<GameObject>();
    public List<GameObject> PausePool = new List<GameObject>();
   
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
    }

    private void Start()
    {
        CreateItem();
        StartCoroutine(Meteor_Co());
        StartCoroutine(Pause_Co());
    }
    

    public void StartCubePooling_co()
    {
        CubePooling = Cube_Co(VeneziaManager.Instance.PoolingCool);
        StartCoroutine(CubePooling);
    }
    public void ReStartCubePooling_co()
    {
        CubeRestartPooling = ReStartCube_Co(VeneziaManager.Instance.PoolingCool);
        StartCoroutine(CubeRestartPooling);
    }
    public IEnumerator Cube_Co(int cool)
    {
        if (cubePool.Count <= VeneziaManager.Instance.limitCount) yield break;
        while (cubePool.Count > VeneziaManager.Instance.limitCount)
        {
            int Randnum = Random.Range(0, cubePool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            if (cubePool.Count - VeneziaManager.Instance.limitCount == 1)
            {
                bool foundSameSprite = false;
                foreach (var cube in cubePool)
                {
                    Cube cubeScript = cube.GetComponent<Cube>();

                    if (cubeScript.sprite == QuestImg.sprite)
                    {
                        cube.SetActive(true);
                        cube.transform.position = Pool_Position.transform.position + offset;
                        cubePool.Remove(cube);
                        foundSameSprite = true;
                        break;
                    }
                }
                if (!foundSameSprite)
                {
                    cubePool[Randnum].transform.position = Pool_Position.transform.position + offset;
                    cubePool[Randnum].SetActive(true);
                    cubePool.Remove(cubePool[Randnum]);
                }
            }
            else
            {
                cubePool[Randnum].transform.position = Pool_Position.transform.position + offset;
                cubePool[Randnum].SetActive(true);
                cubePool.Remove(cubePool[Randnum]);
            }

            yield return new WaitForSeconds(cool); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
        }
    }

    public IEnumerator ReStartCube_Co(int cool)
    {
        yield return new WaitForSeconds(cool);
        while (cubePool.Count > VeneziaManager.Instance.limitCount)
        {
            int Randnum = Random.Range(0, cubePool.Count);
            float randomValue = Random.Range(-100, 101);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            if (cubePool.Count - VeneziaManager.Instance.limitCount == 1)
            {
                bool foundSameSprite = false;
                foreach (var cube in cubePool)
                {
                    Cube cubeScript = cube.GetComponent<Cube>();

                    if (cubeScript.sprite == QuestImg.sprite)
                    {
                        cube.SetActive(true);
                        cube.transform.position = Pool_Position.transform.position + offset;
                        cubePool.Remove(cube);
                        foundSameSprite = true;
                        break;
                    }
                }
                if (!foundSameSprite)
                {
                    cubePool[Randnum].transform.position = Pool_Position.transform.position + offset;
                    cubePool[Randnum].SetActive(true);
                    cubePool.Remove(cubePool[Randnum]);
                }
            }
            else
            {
                cubePool[Randnum].transform.position = Pool_Position.transform.position + offset;
                cubePool[Randnum].SetActive(true);
                cubePool.Remove(cubePool[Randnum]);
            }

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
            float randomValueX = Random.Range(-90, 90);
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
            float randomValue = Random.Range(-80, 90);
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
            item.transform.SetParent(ItemParentObject); // Ǯ ��ġ�� �θ� ����
            MeteorPool.Add(item); // ������ Ǯ ����Ʈ�� �߰�
        }
        //�Ͻ����� 
        for (int i = 0; i < ItemCount; i++)
        {
            GameObject item = Instantiate(PausePrefab); // �ش� �ε����� �������� �ν��Ͻ�ȭ
            item.SetActive(false); // Ȱ��ȭ���� ����
            item.transform.SetParent(ItemParentObject); // Ǯ ��ġ�� �θ� ����
            PausePool.Add(item); // ������ Ǯ ����Ʈ�� �߰�
        }
    }

    public void CreateQuestPrefab(int index, int count) //���� ���������Ҷ� �μ����� ���� ��.
    {
        for (int i = index; i < count; i++) 
        {
            for (int j = 0; j < CubeCount; j++)
            {
                if(VeneziaManager.Instance.game_Type == Game_Type.C)
                {
                    GameObject QuestPrefab = Instantiate(Cube); // �ѱ� ������Ʈ ����
                    Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                    Sprite sprite = VeneziaManager.Instance.sprites_K[i];
                    Cube_sprite.sprite = sprite;
                    QuestPrefab.SetActive(false);
                    QuestPrefab.transform.SetParent(CubeParentObject);
                    cubePool.Add(QuestPrefab);
                }
                else if(VeneziaManager.Instance.game_Type == Game_Type.D)
                {
                    GameObject QuestPrefab = Instantiate(Cube); // ���� ������Ʈ ����
                    Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                    Sprite sprite = VeneziaManager.Instance.sprites_E[i];
                    Cube_sprite.sprite = sprite;
                    QuestPrefab.SetActive(false);
                    QuestPrefab.transform.SetParent(CubeParentObject);
                    cubePool.Add(QuestPrefab);
                }
                else
                {
                    GameObject QuestPrefab = Instantiate(Cube); // ���� ������Ʈ ����
                    Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                    Sprite sprite = VeneziaManager.Instance.sprites_H[i];
                    Cube_sprite.sprite = sprite;
                    QuestPrefab.SetActive(false);
                    QuestPrefab.transform.SetParent(CubeParentObject);
                    cubePool.Add(QuestPrefab);
                }
            }
        }
    }
}