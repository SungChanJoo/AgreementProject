using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance = null;

    [SerializeField] private GameObject Cube;
    [SerializeField] private Transform Pool_Position; //Ǯ�� ��ġ
    [SerializeField] private Transform Pool_PositionTwo; //Ǯ�� ��ġ
    [SerializeField] private Transform CubeParentObject; // Ǯ���� ������Ʈ�� ������ ���̿�����Ʈ
    [SerializeField] private Transform CubeParentObjectTwo; // Ǯ���� ������Ʈ�� ������ ���̿�����Ʈ
    [SerializeField] private Transform ItemParentObject; // Ǯ���� ������Ʈ�� ������ ���̿�����Ʈ


    [SerializeField] private GameObject MeteorPrefab; //������ Ǯ�� ���� ������
    [SerializeField] private GameObject PausePrefab; //������ Ǯ�� ���� ������

    public IEnumerator CubePooling;
    public IEnumerator CubeRestartPooling;

    [SerializeField] Sprite sprite;
    [SerializeField] Image QuestImg;
    [SerializeField] Image QuestTwoImg;

    //������ ť�� ����
    public int CubeCount;
    //������ ������ ����
    public int ItemCount;
    //������ ���õ� prefab�� ����
    public List<GameObject> cubePool = new List<GameObject>();
    public List<GameObject> cubePoolTwo = new List<GameObject>();
    //���׿� , ������ �� ������ ����.
    public List<GameObject> MeteorPool = new List<GameObject>();
    public List<GameObject> PausePool = new List<GameObject>();

    private Vector3 offset;
    private Vector3 offsetTwo;


    public int FirstPool = 0;
    public int SecondPool = 0;

    private void Awake()
    {
        #region Singleton 
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
           // Destroy(gameObject);
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
        while (cubePool.Count > VeneziaManager.Instance.limitCount || cubePoolTwo.Count > VeneziaManager.Instance.limitCount)
        {
            int Randnum = Random.Range(0, cubePool.Count);
            int RandnumTwo = Random.Range(0, cubePoolTwo.Count);
            //�ַθ��
            if(VeneziaManager.Instance.veneGameMode == VeneGameMode.Sole)
            {
                float randomValue = Random.Range(-95, 95);
                offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            }
            else
            {
                //2�θ��
                float random = Random.Range(5, 10);
                offset = new Vector3(random, 0, 0); // �¿� ���������� ������
            }
            
            //�ַθ��
            if (VeneziaManager.Instance.veneGameMode == VeneGameMode.Sole)
            {
                if(cubePool.Count - VeneziaManager.Instance.limitCount == 1)
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
            }
            //Ŀ�ø��
            if(VeneziaManager.Instance.veneGameMode == VeneGameMode.Couple)
            {
                //First Player pool
                if(cubePool.Count - VeneziaManager.Instance.limitCount == 1)
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

                //Second Player pool
                if (cubePoolTwo.Count - VeneziaManager.Instance.limitCount == 1)
                {
                    bool foundSameSprite = false;
                    foreach (var cube in cubePoolTwo)
                    {
                        Cube cubeScript = cube.GetComponent<Cube>();
                        if (cubeScript.sprite == QuestTwoImg.sprite)
                        {
                            cube.SetActive(true);
                            cube.transform.position = Pool_PositionTwo.transform.position + offset;
                            cubePoolTwo.Remove(cube);
                            foundSameSprite = true;
                            break;
                        }
                    }
                    if (!foundSameSprite)
                    {
                        cubePoolTwo[RandnumTwo].transform.position = Pool_PositionTwo.transform.position + offset;
                        cubePoolTwo[RandnumTwo].SetActive(true);
                        cubePoolTwo.Remove(cubePoolTwo[RandnumTwo]);
                    }
                }
                else
                {
                    cubePoolTwo[RandnumTwo].transform.position = Pool_PositionTwo.transform.position + offset;
                    cubePoolTwo[RandnumTwo].SetActive(true);
                    cubePoolTwo.Remove(cubePoolTwo[RandnumTwo]);
                }

            }


            yield return new WaitForSeconds(cool); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
        }
    }

    public IEnumerator ReStartCube_Co(int cool)
    {
        yield return new WaitForSeconds(cool);
        while (cubePool.Count > VeneziaManager.Instance.limitCount || cubePoolTwo.Count > VeneziaManager.Instance.limitCount)
        {
            int Randnum = Random.Range(0, cubePool.Count);
            int RandnumTwo = Random.Range(0, cubePoolTwo.Count);
            //�ַθ��
            if (VeneziaManager.Instance.veneGameMode == VeneGameMode.Sole)
            {
                float randomValue = Random.Range(-95, 95);
                offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            }
            else
            {
                //2�θ��
                float random = Random.Range(5, 10);
                offset = new Vector3(random, 0, 0); // �¿� ���������� ������
            }

            //�ַθ��
            if (VeneziaManager.Instance.veneGameMode == VeneGameMode.Sole)
            {
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
            }
            //Ŀ�ø��
            if (VeneziaManager.Instance.veneGameMode == VeneGameMode.Couple)
            {
                //First Player pool
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
                    if (VeneziaManager.Instance.isFirstPlayerTouch)
                    {
                        cubePool[Randnum].transform.position = Pool_Position.transform.position + offset;
                        cubePool[Randnum].SetActive(true);
                        cubePool.Remove(cubePool[Randnum]);
                    }
                }

                //Second Player pool
                if (cubePoolTwo.Count - VeneziaManager.Instance.limitCount == 1)
                {
                    bool foundSameSprite = false;
                    foreach (var cube in cubePoolTwo)
                    {
                        Cube cubeScript = cube.GetComponent<Cube>();
                        if (cubeScript.sprite == QuestTwoImg.sprite)
                        {
                            cube.SetActive(true);
                            cube.transform.position = Pool_PositionTwo.transform.position + offset;
                            cubePoolTwo.Remove(cube);
                            foundSameSprite = true;
                            break;
                        }
                    }
                    if (!foundSameSprite)
                    {
                        cubePoolTwo[RandnumTwo].transform.position = Pool_PositionTwo.transform.position + offset;
                        cubePoolTwo[RandnumTwo].SetActive(true);
                        cubePoolTwo.Remove(cubePoolTwo[RandnumTwo]);
                    }
                }
                else
                {
                    if (!VeneziaManager.Instance.isFirstPlayerTouch)
                    {
                        cubePoolTwo[RandnumTwo].transform.position = Pool_PositionTwo.transform.position + offset;
                        cubePoolTwo[RandnumTwo].SetActive(true);
                        cubePoolTwo.Remove(cubePoolTwo[RandnumTwo]);
                    }
                }

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
            MeteorPool[0].transform.position = Pool_Position.transform.position + offset;
            MeteorPool[0].SetActive(true);
            MeteorPool.Remove(MeteorPool[0]);
        }
    }

    public IEnumerator Pause_Co()
    {
        while (PausePool.Count > 0)
        {
            float cool = Random.Range(45, 51f);
            yield return new WaitForSeconds(cool); //���̵��� ���� ����Ǵ� �ð��� �ٲܰ�
            //������ ���� ����
            int Randnum = Random.Range(0, MeteorPool.Count);
            float randomValue = Random.Range(-90, 90);
            Vector3 offset = new Vector3(randomValue, 0, 0); // �¿� ���������� ������
            PausePool[0].transform.position = Pool_Position.transform.position + offset;
            PausePool[0].SetActive(true);
            PausePool.Remove(PausePool[0]);
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
                if(VeneziaManager.Instance.veneGameMode == VeneGameMode.Sole)
                {
                    if (VeneziaManager.Instance.game_Type == Game_Type.C)
                    {
                        GameObject QuestPrefab = Instantiate(Cube); // �ѱ� ������Ʈ ����
                        Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                        Sprite sprite = VeneziaManager.Instance.sprites_K[i];
                        Cube_sprite.sprite = sprite;
                        QuestPrefab.SetActive(false);
                        QuestPrefab.transform.SetParent(CubeParentObject);
                        cubePool.Add(QuestPrefab);
                    }
                    else if (VeneziaManager.Instance.game_Type == Game_Type.D)
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
                else
                {
                    if (VeneziaManager.Instance.game_Type == Game_Type.C)
                    {
                        //ù��° Ǯ�� ���� ����
                        GameObject QuestPrefab = Instantiate(Cube); // �ѱ� ������Ʈ ����
                        Cube Cube_sprite = QuestPrefab.GetComponent<Cube>();
                        Sprite sprite = VeneziaManager.Instance.sprites_K[i];
                        Cube_sprite.sprite = sprite;
                        QuestPrefab.SetActive(false);
                        QuestPrefab.transform.SetParent(CubeParentObject);
                        cubePool.Add(QuestPrefab);
                        //�ι�° Ǯ�� ���� ����
                        GameObject QuestPrefab2 = Instantiate(Cube); // �ѱ� ������Ʈ ����
                        Cube Cube_sprite2 = QuestPrefab2.GetComponent<Cube>();
                        Sprite sprite2 = VeneziaManager.Instance.sprites_K[i];
                        //������ ���� ���θ� �ֱ� ���� Ÿ���� ������ �� ��. defalt�� one�̱� ������ player1�� ����x
                        Cube_sprite2.playerNum = PlayerNum.Two;
                        Cube_sprite2.sprite = sprite2;
                        QuestPrefab2.SetActive(false);
                        QuestPrefab2.transform.SetParent(CubeParentObjectTwo);
                        cubePoolTwo.Add(QuestPrefab2);
                    }
                    else if (VeneziaManager.Instance.game_Type == Game_Type.D)
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
}