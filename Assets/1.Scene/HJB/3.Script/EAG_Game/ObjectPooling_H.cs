using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling_H : MonoBehaviour
{

    [SerializeField] private GameObject cube_Obj;
    [SerializeField] private GameObject[] poolPosition;
    [SerializeField] private GameObject defaultPosition;
    [SerializeField] private EAG_Manager aopManager;
    [SerializeField] private EAG_Animation eag_ani;
    [SerializeField] private int speed;

    private List<GameObject> cubePool = new List<GameObject>();
    private List<GameObject> bool_Pool = new List<GameObject>();    
    private List<float> reactionList = new List<float>();
    private MovingCube resultObject;

    public float answer;
    private int problom_count=0;
    private int answer_count = 0;
    private int totalQuestions = 0;
    private float waitTime =0;
    private bool timeOut = false;
    private bool touchEnable = false;

    private IEnumerator WaitExplosionBubble_co;
    private IEnumerator NextQuestionAni_co;

    public static ObjectPooling_H Instance = null;
    private void Awake()
    {
        if (Instance ==null)
        {
            Instance = this;            
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        Click_Obj();
        TimeCheck();
    }
    //Start 버튼 이벤트가 콜백되면 실행
    public void ObjectPooling()
    {        
        aopManager.isStop = false;        
        //시간 흐르게
        TimeSlider.Instance.StartTime();
        TimeSlider.Instance.TimeStop = false;
        //기본 값 초기화
        DataDefaultSetting();
        //문제 오브젝트의 갯수만큼 생성 및 Pool에 담기
        for (int i = 0; i < poolPosition.Length; i++)
        {
            GameObject cube = Instantiate(cube_Obj);            
            cube.SetActive(false);
            cubePool.Add(cube);            
        }
        NextQuestionAni_co = NextQuestionAni_Co();
        StartCoroutine(NextQuestionAni_co);
    }
    private void TimeCheck()
    {
        //게임이 끝났다면
        if (aopManager.isStop || (timeOut && TimeSlider.Instance.TimeStop)||
            SettingManager.Instance.IsActive)
        {
            return;
        }
        if (TimeSlider.Instance.slider.value<=0)
        {
            timeOut = true;
            GameOver();
        }        
        //문제를 못풀고 20초가 지났을 경우        
        waitTime += Time.deltaTime;
        if (waitTime >20f)
        {
            waitTime = 0;
            TimeSlider.Instance.DecreaseTime_Item(5);
            WaitExplosionBubble_co = WaitExplosionBubble_Co(resultObject);
            StartCoroutine(WaitExplosionBubble_co);
            bool_Pool.Remove(resultObject.gameObject);
            Next_Result();
        }
    }
    private void Click_Obj()
    {
        if (!touchEnable)
        {
            return;
        }
        //오브젝트를 터치 또는 클릭 했다면
        if (Input.GetMouseButtonDown(0)||
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground") && !hit.collider.CompareTag("AnswerCheck"))
            {                
                //정답 체크
                MovingCube movingCube = hit.collider.GetComponent<MovingCube>();
                //중복터치 방지를 위한 Tag 변경
                hit.collider.tag = "AnswerCheck";
                
                WaitExplosionBubble_co = WaitExplosionBubble_Co(movingCube);                 
                StartCoroutine(WaitExplosionBubble_co);
                bool_Pool.Remove(movingCube.gameObject);
                Answer_Check(movingCube);
            }
        }
    }

    public void GameOver()
    {
        StopAllCoroutines();
        aopManager.isStop = true;
        aopManager.answersCount = answer_count;
        ReactionCalculation();
        AnswerRate();
        aopManager.EndGame();
    }
    private void CubeStart()
    {
        //준비된 문제가 끝났다면
        if (problom_count == 0)
        {
            GameOver();
            return;
        }
        //같은 결과 안나오도록 담을 것 선언
        List<int> sameResultCheck = new List<int>();
        
        //문제 랜덤 뽑기
        int randomResult = Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            //문제 출시
            cubePool[i].SetActive(true);
            cubePool[i].tag = "Untagged";
            //풀에 담겨있는 오브젝트에 값 지정
            MovingCube movingcube = cubePool[i].GetComponent<MovingCube>();
            //상태 True만 담기는 리스트
            bool_Pool.Add(cubePool[i]);
            movingcube.index = i;

            movingcube.reactionRate = 0; //반응속도 초기화

            //문제 뽑기
            aopManager.SplitLevelAndStep();
            //같은 결과가 나오지 않도록 처리
            if (i==2)
            {
                while (sameResultCheck[0] == aopManager.result ||
                    sameResultCheck[1] == aopManager.result)
                {
                    aopManager.SplitLevelAndStep();
                }
            }
            else if(i==1)
            {
                while (sameResultCheck[0] == aopManager.result)
                {
                    aopManager.SplitLevelAndStep();
                }
            }
            sameResultCheck.Add(aopManager.result);
            int firstNum = aopManager.firstNum;
            int secondNum = aopManager.secondNum;
            char _operator = aopManager._Operator;
            movingcube.result = aopManager.result;
            if (i.Equals(randomResult))
            {
                TakeResult(movingcube.result);
                resultObject = movingcube;
            }
            //여기에 숫자 할당
            movingcube.Start_Obj(firstNum,_operator , secondNum);

            //문제 개수를 계속 체크
            problom_count--;
        }
    }    
    
    private void Answer_Check(MovingCube movingCube)
    {        
        if (movingCube.result.Equals(answer))
        {
            answer_count++;
            waitTime = 0;
            //정답오브젝트의 반응속도를 담기
            reactionList.Add(movingCube.reactionRate);
            Next_Result();
        }
        else
        {
            //시간 감소                        
            TimeSlider.Instance.DecreaseTime_Item(5);
        }        
    }
    private void Next_Result()
    {
        //첫번째 문제를 맞추었을 경우 다음 문제를 랜덤으로 출시하기 위한 로직        
        int step = (Random.Range(0, 2) == 1) ? 1 : -1;
        int start = (step == 1) ? 0 : bool_Pool.Count - 1;

        if (bool_Pool.Count != 0)
        {
            for (int i = start; i >= 0 && i < bool_Pool.Count; i += step)
            {
                MovingCube movingCube = bool_Pool[i].GetComponent<MovingCube>();
                TakeResult(movingCube.result);
                resultObject = movingCube;
                return;
            }
        }
        else
        {
            NextQuestionAni_co = NextQuestionAni_Co();
            StartCoroutine(NextQuestionAni_co);
        }
    }

    //정답 결과 출력 메서드
    private void TakeResult(float result)
    {
        answer = result;
        aopManager.Show_Result(answer);
    }

    private void DataDefaultSetting()
    {
        int problom = aopManager.timeSet/60;
        //시간 설정에 따른 문제 수 변경
        switch (problom)
        {
            case 1:
                problom = 24;
                break;
            case 3:
                problom = 30;
                break;
            case 5:
                problom = 48;
                break;
        }
        problom_count = problom;
        totalQuestions = problom;
        aopManager.totalQuestions = totalQuestions;
        answer_count = 0;
    }
    

    private void ReactionCalculation()
    {
        //반응속도 리스트를 가지고 평균반응속도 계산
        float reaction = 0;
        for (int i = 0; i < reactionList.Count; i++)
        {
            reaction += reactionList[i];
        }
        aopManager.reactionRate = (reaction / reactionList.Count);        
    }

    private void AnswerRate()
    {
        //정답률 계산
        aopManager.answers = answer_count * 100 / totalQuestions;
    }

    private IEnumerator WaitExplosionBubble_Co(MovingCube obj)
    {
        //방울 터지는 애니메이션
        obj.ExplosionAni();
        yield return new WaitForSeconds(1f);
        obj.DefaultAni();
        obj.gameObject.SetActive(false);
        obj.transform.position = defaultPosition.transform.position;
        
    }

    private IEnumerator NextQuestionAni_Co()
    {
        touchEnable = false;        
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < cubePool.Count; i++)
        {
            cubePool[i].SetActive(true);
            cubePool[i].transform.localScale = new Vector3(5f, 5f, 1f);
        }
        eag_ani.CreateProblem();                
        for (int i = 0; i < cubePool.Count; i++)
        {
            MovingCube bubble = cubePool[i].GetComponent<MovingCube>();
            bubble.UpScale();
        }
        yield return new WaitForSeconds(0.3f);
        //문제 연출 넣기        
        float startTime = Time.time;
        float duration = 0.7f;

        while (Time.time - startTime < duration)
        {
            float fraction = (Time.time - startTime) / duration;

            for (int i = 0; i < cubePool.Count; i++)
            {
                cubePool[i].transform.position = Vector3.Lerp(
                    cubePool[i].transform.position,
                    poolPosition[i].transform.position,
                    fraction
                );
            }
            if (fraction>0.8f)
            {
                CubeStart();
                break;
            }
            
            yield return null;
        }
        touchEnable = true;
    }
}

