using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CrewMovementManager : MonoBehaviour
{
    public static CrewMovementManager Instance = null;

    Dictionary<(Game_Type, int), int> FinalPlayStepTable; //(���� Ÿ��, ����)�� ���� �������� 

    public Game_Type SelectedGame;
    public int SelectedLevel;
    public int SelectedStep;
    public List<Transform> Level1;
    public List<Transform> Level2;
    public List<Transform> Level3;
    public List<GameObject> SeleteableCrew;
    public int SeletedCrewIndex;
    List<Transform> CrewPos;
    int finalPos;
    public GameObject FadeObj;
    private Image FadeImg;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        //FinalPlayStepTable.Add((Game_Type.A, 1), 1); // Game_Type.A, 1�������� ���������� �÷����� ������ 1����
        //FinalPlayStepTable[(Game_Type.A, 1)] = 2;// ���� 2�� ����
        //todo 0308 DB�����ؼ� ���� �÷����� ���� �ʱ�ȭ����
        FinalPlayStepTable = new Dictionary<(Game_Type, int), int>();

        for (int i = 0; i<= (int)Game_Type.E; i++)//���� Ÿ�� 5��
        {
            for(int j = 1; j <=3; j++)// ���� 3��
            {
                FinalPlayStepTable.Add(((Game_Type)i, j), 1); //(Game_Type)i, j�������� ���������� �÷����� ����
            }
        }
        for(int i = 0; i < SeleteableCrew.Count; i++)
        {
            if (SeleteableCrew[i].activeSelf)
                SeleteableCrew[i].SetActive(false);
        }
        FadeImg = FadeObj.GetComponent<Image>();
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

        CrewPos = SelectedLevel switch
        { 
            1 => Level1,
            2 => Level2,
            3 => Level3,
            _ => null,
        };

        if(CrewPos != null)
        {
            //�ʱ� Ž���� ��ġ
            finalPos = FinalPlayStepTable[(SelectedGame, SelectedLevel)]; // ���õ� ������ ���õ� ������ ����
            SeleteableCrew[SeletedCrewIndex].transform.position = CrewPos[finalPos - 1].position;
        }
            SeleteableCrew[SeletedCrewIndex].transform.LookAt(Camera.main.transform.position);

    }
    //���� ���ÿ��� ������
    public void ExitStep()
    {
        if (SeleteableCrew[SeletedCrewIndex].activeSelf)
            SeleteableCrew[SeletedCrewIndex].SetActive(false);
    }

    //���� ���ý�
    //1. ��� �̵�
    //2. ���������� �÷����� ���� ����
    public void SelectStep()
    {
        //todo 0308 DB���� �� ���� �޾Ƽ� �̵� �������� ���� ��������
        SelectedStep = StepManager.Instance.CurrentStep;
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
            while (SetTime < 1f)
            {
                SetTime += Time.deltaTime;
                var targetPos = new Vector3(nextPos.x, SeleteableCrew[SeletedCrewIndex].transform.rotation.y, nextPos.z) - SeleteableCrew[SeletedCrewIndex].transform.position;
                SeleteableCrew[SeletedCrewIndex].transform.rotation = Quaternion.Lerp(SeleteableCrew[SeletedCrewIndex].transform.rotation, Quaternion.LookRotation(targetPos), 0.05f);
                yield return null;
            }
            //SeletedCrew.transform.LookAt(nextPos);

            while ((SeleteableCrew[SeletedCrewIndex].transform.position - nextPos).sqrMagnitude >0.01f)
            {
                SeleteableCrew[SeletedCrewIndex].transform.position = Vector3.MoveTowards(SeleteableCrew[SeletedCrewIndex].transform.position, nextPos, Time.deltaTime * 50f);
                yield return null;
            }

            yield return new WaitForSeconds(1f);
        }

/*        float SetTime = 0f;
        while (SetTime < 1f)
        {
            SetTime += Time.deltaTime;
            var targetPos = new Vector3(Camera.main.transform.position.x, SeletedCrew.transform.rotation.y, Camera.main.transform.position.z) - SeletedCrew.transform.position;
            SeletedCrew.transform.rotation = Quaternion.Lerp(SeletedCrew.transform.rotation, Quaternion.LookRotation(targetPos), 0.1f);
            yield return null;
        }*/
        //SeletedCrew.transform.position = CrewPos[SelectedStep-1].position;
        Debug.Log("CrewPos[SelectedStep].position" + CrewPos[SelectedStep-1].position);
        SeleteableCrew[SeletedCrewIndex].transform.LookAt(Camera.main.transform.position);
        //���������� �÷����� ����
        FinalPlayStepTable[(SelectedGame, SelectedLevel)] = SelectedStep;
        StartCoroutine(FadeOutImg());
    }
    //ȭ�� ��ȯ ȿ��
    IEnumerator FadeOutImg()
    {
        var fadeCount = 0f;
        while (FadeImg.color.a < 1)
        {
            fadeCount += 0.01f;
            FadeImg.color = new Color(0,0,0, fadeCount);
            yield return null;
        }
        SceneManager.LoadScene((int)SelectedGame + 2);
        StartCoroutine(FadeInImg());
        ExitStep();
    }
    IEnumerator FadeInImg()
    {
        var fadeCount = 1f;
        while (FadeImg.color.a > 0)
        {
            fadeCount -= 0.01f;
            FadeImg.color = new Color(0, 0, 0, fadeCount);
            yield return null;
        }
        FadeObj.SetActive(false);

    }

}
