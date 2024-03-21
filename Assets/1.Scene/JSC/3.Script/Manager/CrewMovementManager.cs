using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class CrewMovementManager : MonoBehaviour
{
    public static CrewMovementManager Instance = null;

    LastPlayData LastPlayStepTable; //(게임 타입, 레벨)에 따른 스텝정보 

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
        //FinalPlayStepTable.Add((Game_Type.A, 1), 1); // Game_Type.A, 1레벨에서 마지막으로 플레이한 스텝이 1스텝
        //FinalPlayStepTable[(Game_Type.A, 1)] = 2;// 스텝 2로 변경
        //DB연동해서 최종 플레이한 스텝 초기화
        if (Client.instance != null)
        {
            LastPlayStepTable = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].LastPlayStepData;
        }
        else
        {
            LastPlayStepTable = new LastPlayData();

            for (int i = 0; i <= (int)Game_Type.E; i++)//게임 타입 5개
            {
                for (int j = 1; j <= 3; j++)// 레벨 3개
                {
                    LastPlayStepTable.Step.Add(((Game_Type)i, j), 1); //(Game_Type)i, j레벨에서 마지막으로 플레이한 스텝
                }
            }
        }

    }
    private void InitPlayerDBData()
    {
        //스텝 별개수 초기화
        if (Client.instance != null)
        {
            stepInfo = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex];
        }
        //DB연동안되어있으면
        else
        {
            stepInfo = new Player_DB();
            for (int i = 0; i <= (int)Game_Type.E; i++)//게임 타입 5개
            {
                for (int j = 1; j <= 3; j++)// 레벨 3개
                {
                    for (int k = 1; k <= 6; k++)
                    {
                        //(Game_Type)i, j레벨, k스텝의 별개수 초기화
                        var data = new Data_value(0,0,0,0,0,1);
                        stepInfo.Data.Add(((Game_Type)i, j, k), data);
                    }
                }
            }
        }
    } 
    #endregion
    //별 개수에 따라 스텝 머터리얼 변경
    //별 0개는 하얀색, 별 1개 이상은 노란색
    private void LoadStarColor(List<Transform> level)
    {
        //별 오브젝트 불러오기
        for (int i =0; i < level.Count; i++)
        {
            var step = level[i];

            if (stepInfo.Data[(SelectedGame, SelectedLevel, i+1)].StarCount >0)
            {
                //별 있으면 노란색
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
        //starPointPanel == 레벨 별 스텝
        for (int i = 0; i < starPointPanel.Count; i++)
        {
            var step = starPointPanel[i];
            starCount = stepInfo.Data[(SelectedGame, SelectedLevel, i + 1)].StarCount;
            //스텝별 스타 이미지 변경
            for (int j = 0; j < 3; j++)
            {
                var starImg = step.transform.GetChild(j).gameObject.GetComponent<Image>();
                //  Debug.Log("starImg : " + step.transform.GetChild(j).gameObject.GetComponent<Image>().sprite.name);   
                //DB에서 별 개수에 따라 배정
                if(j < starCount)
                    starImg.sprite = star;
                else
                    starImg.sprite = starGray;
            }
        }

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

        //별 개수에 따른 스텝 머터리얼 색 변경
        LoadStarColor(CrewPos);

        if (CrewPos != null)
        {
            //초기 탐험대원 위치
            finalPos = LastPlayStepTable.Step[(SelectedGame, SelectedLevel)]; // 선택된 게임의 선택된 레벨의 스텝
            SeleteableCrew[SeletedCrewIndex].transform.position = CrewPos[finalPos - 1].position;
        }
            SeleteableCrew[SeletedCrewIndex].transform.LookAt(Camera.main.transform.position);

    }
    //스텝 선택에서 나가기
    public void ExitStep()
    {
        if (SeleteableCrew[SeletedCrewIndex].activeSelf)
            SeleteableCrew[SeletedCrewIndex].SetActive(false);
        for (int i = 0; i < Level.Count; i++)
        {
            Level[i].SetActive(false);
        }
    }

    //스텝 선택시
    //1. 대원 이동
    //2. 마지막으로 플레이한 스텝 변경
    public void SelectStep()
    {
        //todo 0308 DB에서 별 개수 받아서 이동 가능한지 조건 설정해줘
        SelectedStep = StepManager.Instance.CurrentStep;
        //다음 스텝으로 넘어가기 위해서는 이전 스텝의 별이 1개 이상 있어함
        if(SelectedStep >1) // 스텝 1은 플레이 할 수 있음
        {
            if(stepInfo.Data[(SelectedGame, SelectedLevel, SelectedStep - 1)].StarCount < 1)
            {
                Debug.Log("이전 스텝에서 별을 획득해주세요");
                return;
            }
        }

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
        var crewAni = SeleteableCrew[SeletedCrewIndex].gameObject.GetComponent<Animator>();
        crewAni.SetBool("JumpEnd", false);
        crewAni.SetBool("IsWalk", true);
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
            //회전속도
            while (SetTime < 0.3f)
            {
                SetTime += Time.deltaTime;
                var targetPos = new Vector3(nextPos.x, SeleteableCrew[SeletedCrewIndex].transform.rotation.y, nextPos.z) - SeleteableCrew[SeletedCrewIndex].transform.position;
                SeleteableCrew[SeletedCrewIndex].transform.rotation = Quaternion.Lerp(SeleteableCrew[SeletedCrewIndex].transform.rotation, Quaternion.LookRotation(targetPos), 0.05f);
                yield return null;
            }
            //이동속도
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
        //마지막으로 플레이한 스텝
        LastPlayStepTable.Step[(SelectedGame, SelectedLevel)] = SelectedStep;
        StartCoroutine(FadeOutImg());
    }
    //화면 전환 효과
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
            //어플종료시 DB에 마지막으로 플레이한 스텝 위치 저장 
              Client.instance.AppExit_SaveLastPlayDataToDB(LastPlayStepTable);
        }
    }
}
