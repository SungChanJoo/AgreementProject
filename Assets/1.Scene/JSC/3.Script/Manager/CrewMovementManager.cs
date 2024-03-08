using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CrewMovementManager : MonoBehaviour
{
    public static CrewMovementManager Instance = null;

    Dictionary<(Game_Type, int), int> FinalPlayStepTable; //(게임 타입, 레벨)에 따른 스텝정보 

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
        //FinalPlayStepTable.Add((Game_Type.A, 1), 1); // Game_Type.A, 1레벨에서 마지막으로 플레이한 스텝이 1스텝
        //FinalPlayStepTable[(Game_Type.A, 1)] = 2;// 스텝 2로 변경
        //todo 0308 DB연동해서 최종 플레이한 스텝 초기화해줘
        FinalPlayStepTable = new Dictionary<(Game_Type, int), int>();

        for (int i = 0; i<= (int)Game_Type.E; i++)//게임 타입 5개
        {
            for(int j = 1; j <=3; j++)// 레벨 3개
            {
                FinalPlayStepTable.Add(((Game_Type)i, j), 1); //(Game_Type)i, j레벨에서 마지막으로 플레이한 스텝
            }
        }
        for(int i = 0; i < SeleteableCrew.Count; i++)
        {
            if (SeleteableCrew[i].activeSelf)
                SeleteableCrew[i].SetActive(false);
        }
        FadeImg = FadeObj.GetComponent<Image>();
    }
    //레벨 선택시 현재 대원보이기
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
            //초기 탐험대원 위치
            finalPos = FinalPlayStepTable[(SelectedGame, SelectedLevel)]; // 선택된 게임의 선택된 레벨의 스텝
            SeleteableCrew[SeletedCrewIndex].transform.position = CrewPos[finalPos - 1].position;
        }
            SeleteableCrew[SeletedCrewIndex].transform.LookAt(Camera.main.transform.position);

    }
    //스텝 선택에서 나가기
    public void ExitStep()
    {
        if (SeleteableCrew[SeletedCrewIndex].activeSelf)
            SeleteableCrew[SeletedCrewIndex].SetActive(false);
    }

    //스텝 선택시
    //1. 대원 이동
    //2. 마지막으로 플레이한 스텝 변경
    public void SelectStep()
    {
        //todo 0308 DB에서 별 개수 받아서 이동 가능한지 조건 설정해줘
        SelectedStep = StepManager.Instance.CurrentStep;
        Debug.Log($"CrewMovementManager : {SelectedGame},{SelectedLevel},{SelectedStep}");
        StartCoroutine(MoveCrew_co());
        FadeObj.SetActive(true);
    }
    //대원 움직이기
    IEnumerator MoveCrew_co()
    {
        //1 -> 6 5번의 별을 거쳐감
        //finalPos가 SelectedStep보다 작으면 ->방향으로 이동
        // 크면 <- 방향으로 이동
        for (int i =1; i <= Mathf.Abs(finalPos - SelectedStep); i++)
        {
            Vector3 nextPos;
            //오른쪽이동
            if(finalPos <= SelectedStep)
            {
                nextPos = CrewPos[finalPos - 1 + i].position;
            }
            //왼쪽이동
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
        //마지막으로 플레이한 스텝
        FinalPlayStepTable[(SelectedGame, SelectedLevel)] = SelectedStep;
        StartCoroutine(FadeOutImg());
    }
    //화면 전환 효과
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
