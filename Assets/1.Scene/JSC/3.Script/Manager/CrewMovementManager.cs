using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class CrewMovementManager : MonoBehaviour
{
    public static CrewMovementManager Instance = null;

    LastPlayData LastPlayStepTable; //(���� Ÿ��, ����)�� ���� �������� 

    public Game_Type SelectedGame;
    public int SelectedLevel;
    public int SelectedStep;
    public List<GameObject> Level;
    public List<Transform> Level1;
    public List<Transform> Level2;
    public List<Transform> Level3;
    public List<GameObject> SeleteableCrew;
    public int SeletedCrewIndex;
    List<Transform> CrewPos;
    int finalPos;
    public GameObject FadeObj;
    private Image FadeImg;
    [SerializeField] private Sprite star;
    [SerializeField] private Sprite starGray;
    Player_DB stepInfo;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        InitLastPlayStep();
        InitPlayerDBData();

        for (int i = 0; i < SeleteableCrew.Count; i++)
        {
            if (SeleteableCrew[i].activeSelf)
                SeleteableCrew[i].SetActive(false);
        }
        FadeImg = FadeObj.GetComponent<Image>();

        for(int i = 0; i< Level.Count; i++)
        {
            if(Level[i].activeSelf)
                Level[i].SetActive(false);
        }

    }
    #region DB Data Init
    private void InitLastPlayStep()
    {
        Debug.Log("InitLastPlayStep");
        //FinalPlayStepTable.Add((Game_Type.A, 1), 1); // Game_Type.A, 1�������� ���������� �÷����� ������ 1����
        //FinalPlayStepTable[(Game_Type.A, 1)] = 2;// ���� 2�� ����
        //DB�����ؼ� ���� �÷����� ���� �ʱ�ȭ
        if (Client.instance != null)
        {
            LastPlayStepTable = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].LastPlayStepData;
        }
        else
        {
            LastPlayStepTable = new LastPlayData();

            for (int i = 0; i <= (int)Game_Type.E; i++)//���� Ÿ�� 5��
            {
                for (int j = 1; j <= 3; j++)// ���� 3��
                {
                    LastPlayStepTable.Step.Add(((Game_Type)i, j), 1); //(Game_Type)i, j�������� ���������� �÷����� ����
                }
            }
        }

    }
    private void InitPlayerDBData()
    {
        //���� ������ �ʱ�ȭ
        if (Client.instance != null)
        {
            stepInfo = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex];
        }
        //DB�����ȵǾ�������
        else
        {
            stepInfo = new Player_DB();
            for (int i = 0; i <= (int)Game_Type.E; i++)//���� Ÿ�� 5��
            {
                for (int j = 1; j <= 3; j++)// ���� 3��
                {
                    for (int k = 1; k <= 6; k++)
                    {
                        //(Game_Type)i, j����, k������ ������ �ʱ�ȭ
                        var data = new Data_value(0,0,0,0,0,1);
                        stepInfo.Data.Add(((Game_Type)i, j, k), data);
                    }
                }
            }
        }
    } 
    #endregion
    //�� ������ ���� ���� ���͸��� ����
    //�� 0���� �Ͼ��, �� 1�� �̻��� �����
    private void LoadStarColor(List<Transform> level)
    {
        //�� ������Ʈ �ҷ�����
        for (int i =0; i < level.Count; i++)
        {
            var step = level[i];

            if (stepInfo.Data[(SelectedGame, SelectedLevel, i+1)].StarCount >0)
            {
                //�� ������ �����
                step.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(255f, 189f, 0f);
            }
            else
            {
                step.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(255f, 255f, 255f);
            }
        }
    }

    public void StepByStarPoint(List<GameObject> starPointPanel)
    {
        int starCount;
        //starPointPanel == ���� �� ����
        for (int i = 0; i < starPointPanel.Count; i++)
        {
            var step = starPointPanel[i];
            starCount = stepInfo.Data[(SelectedGame, SelectedLevel, i + 1)].StarCount;
            //���ܺ� ��Ÿ �̹��� ����
            for (int j = 0; j < 3; j++)
            {
                var starImg = step.transform.GetChild(j).gameObject.GetComponent<Image>();
                //  Debug.Log("starImg : " + step.transform.GetChild(j).gameObject.GetComponent<Image>().sprite.name);   
                //DB���� �� ������ ���� ����
                if(j < starCount)
                    starImg.sprite = star;
                else
                    starImg.sprite = starGray;
            }
        }

    }

    //���� ���ý� ���� ������̱�
    public void ViewCrew()
    {
        if(CollectionsManager.Instance != null)
            SeletedCrewIndex = CollectionsManager.Instance.Collections.SelectedCrew;
        if (!SeleteableCrew[SeletedCrewIndex].activeSelf)
        {
            SeleteableCrew[SeletedCrewIndex].SetActive(true);
        }

        SelectedGame = StepManager.Instance.game_Type;
        SelectedLevel = StepManager.Instance.CurrentLevel;

        for(int i = 0; i < Level.Count; i++)
        {
            if(SelectedLevel -1 == i)
            {
                Level[i].SetActive(true);
            }
            else
            {
                Level[i].SetActive(false);
            }
        }

        CrewPos = SelectedLevel switch
        { 
            1 => Level1,
            2 => Level2,
            3 => Level3,
            _ => null,
        };

        //�� ������ ���� ���� ���͸��� �� ����
        LoadStarColor(CrewPos);

        if (CrewPos != null)
        {
            //�ʱ� Ž���� ��ġ
            finalPos = LastPlayStepTable.Step[(SelectedGame, SelectedLevel)]; // ���õ� ������ ���õ� ������ ����
            SeleteableCrew[SeletedCrewIndex].transform.position = CrewPos[finalPos - 1].position;
        }
            SeleteableCrew[SeletedCrewIndex].transform.LookAt(Camera.main.transform.position);

    }
    //���� ���ÿ��� ������
    public void ExitStep()
    {
        if (SeleteableCrew[SeletedCrewIndex].activeSelf)
            SeleteableCrew[SeletedCrewIndex].SetActive(false);
        for (int i = 0; i < Level.Count; i++)
        {
            Level[i].SetActive(false);
        }
    }

    //���� ���ý�
    //1. ��� �̵�
    //2. ���������� �÷����� ���� ����
    public void SelectStep()
    {
        //todo 0308 DB���� �� ���� �޾Ƽ� �̵� �������� ���� ��������
        SelectedStep = StepManager.Instance.CurrentStep;
        //���� �������� �Ѿ�� ���ؼ��� ���� ������ ���� 1�� �̻� �־���
        if(SelectedStep >1) // ���� 1�� �÷��� �� �� ����
        {
            if(stepInfo.Data[(SelectedGame, SelectedLevel, SelectedStep - 1)].StarCount < 1)
            {
                Debug.Log("���� ���ܿ��� ���� ȹ�����ּ���");
                return;
            }
        }

        Debug.Log($"CrewMovementManager : {SelectedGame},{SelectedLevel},{SelectedStep}");
        StartCoroutine(MoveCrew_co());
        FadeObj.SetActive(true);
    }
    //��� �����̱�
    IEnumerator MoveCrew_co()
    {
        //1 -> 6 5���� ���� ���İ�
        //finalPos�� SelectedStep���� ������ ->�������� �̵�
        // ũ�� <- �������� �̵�
        var crewAni = SeleteableCrew[SeletedCrewIndex].gameObject.GetComponent<Animator>();
        crewAni.SetBool("JumpEnd", false);
        crewAni.SetBool("IsWalk", true);
        for (int i =1; i <= Mathf.Abs(finalPos - SelectedStep); i++)
        {
            Vector3 nextPos;
            //�������̵�
            if(finalPos <= SelectedStep)
            {
                nextPos = CrewPos[finalPos - 1 + i].position;
            }
            //�����̵�
            else
            {
                nextPos = CrewPos[finalPos - 1 - i].position;
            }
            float SetTime = 0f;
            //ȸ���ӵ�
            while (SetTime < 0.3f)
            {
                SetTime += Time.deltaTime;
                var targetPos = new Vector3(nextPos.x, SeleteableCrew[SeletedCrewIndex].transform.rotation.y, nextPos.z) - SeleteableCrew[SeletedCrewIndex].transform.position;
                SeleteableCrew[SeletedCrewIndex].transform.rotation = Quaternion.Lerp(SeleteableCrew[SeletedCrewIndex].transform.rotation, Quaternion.LookRotation(targetPos), 0.05f);
                yield return null;
            }
            //�̵��ӵ�
            while ((SeleteableCrew[SeletedCrewIndex].transform.position - nextPos).sqrMagnitude >0.01f)
            {
                SeleteableCrew[SeletedCrewIndex].transform.position = Vector3.MoveTowards(SeleteableCrew[SeletedCrewIndex].transform.position, nextPos, Time.deltaTime * 80f);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }
        crewAni.SetBool("IsWalk", false);
        SeleteableCrew[SeletedCrewIndex].transform.LookAt(Camera.main.transform.position);
        crewAni.SetTrigger("Jump");
        yield return new WaitForSeconds(1f);
        crewAni.SetBool("JumpEnd", true);
        //���������� �÷����� ����
        LastPlayStepTable.Step[(SelectedGame, SelectedLevel)] = SelectedStep;
        StartCoroutine(FadeOutImg());
    }
    //ȭ�� ��ȯ ȿ��
    IEnumerator FadeOutImg()
    {
        var fadeCount = 0f;
        while (FadeImg.color.a < 1)
        {
            fadeCount += 0.02f;
            FadeImg.color = new Color(0,0,0, fadeCount);
            yield return null;
        }
        int mode_num = (StepManager.Instance.playMode == PlayMode.Couple) ? 6 : 3;        
        if ((int)SelectedGame >= 2 && 5> (int)SelectedGame)
        {
            SceneManager.LoadScene(2+mode_num);
        }        
        else
        {
            SceneManager.LoadScene((int)SelectedGame + mode_num);
        }        
        StartCoroutine(FadeInImg());
        ExitStep();
    }
    IEnumerator FadeInImg()
    {
        var fadeCount = 1f;
        while (FadeImg.color.a > 0)
        {
            fadeCount -= 0.02f;
            FadeImg.color = new Color(0, 0, 0, fadeCount);
            yield return null;
        }
        FadeObj.SetActive(false);

    }

    private void OnApplicationQuit()
    {
        if (Client.instance != null)
        {
            //��������� DB�� ���������� �÷����� ���� ��ġ ���� 
              Client.instance.AppExit_SaveLastPlayDataToDB(LastPlayStepTable);
        }
    }
}
