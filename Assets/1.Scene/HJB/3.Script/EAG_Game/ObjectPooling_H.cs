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
    //Start ��ư �̺�Ʈ�� �ݹ�Ǹ� ����
    public void ObjectPooling()
    {        
        aopManager.isStop = false;        
        //�ð� �帣��
        TimeSlider.Instance.StartTime();
        TimeSlider.Instance.TimeStop = false;
        //�⺻ �� �ʱ�ȭ
        DataDefaultSetting();
        //���� ������Ʈ�� ������ŭ ���� �� Pool�� ���
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
        //������ �����ٸ�
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
        //������ ��Ǯ�� 20�ʰ� ������ ���        
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
        //������Ʈ�� ��ġ �Ǵ� Ŭ�� �ߴٸ�
        if (Input.GetMouseButtonDown(0)||
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground") && !hit.collider.CompareTag("AnswerCheck"))
            {                
                //���� üũ
                MovingCube movingCube = hit.collider.GetComponent<MovingCube>();
                //�ߺ���ġ ������ ���� Tag ����
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
        //�غ�� ������ �����ٸ�
        if (problom_count == 0)
        {
            GameOver();
            return;
        }
        //���� ��� �ȳ������� ���� �� ����
        List<int> sameResultCheck = new List<int>();
        
        //���� ���� �̱�
        int randomResult = Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            //���� ���
            cubePool[i].SetActive(true);
            cubePool[i].tag = "Untagged";
            //Ǯ�� ����ִ� ������Ʈ�� �� ����
            MovingCube movingcube = cubePool[i].GetComponent<MovingCube>();
            //���� True�� ���� ����Ʈ
            bool_Pool.Add(cubePool[i]);
            movingcube.index = i;

            movingcube.reactionRate = 0; //�����ӵ� �ʱ�ȭ

            //���� �̱�
            aopManager.SplitLevelAndStep();
            //���� ����� ������ �ʵ��� ó��
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
            //���⿡ ���� �Ҵ�
            movingcube.Start_Obj(firstNum,_operator , secondNum);

            //���� ������ ��� üũ
            problom_count--;
        }
    }    
    
    private void Answer_Check(MovingCube movingCube)
    {        
        if (movingCube.result.Equals(answer))
        {
            answer_count++;
            waitTime = 0;
            //���������Ʈ�� �����ӵ��� ���
            reactionList.Add(movingCube.reactionRate);
            Next_Result();
        }
        else
        {
            //�ð� ����                        
            TimeSlider.Instance.DecreaseTime_Item(5);
        }        
    }
    private void Next_Result()
    {
        //ù��° ������ ���߾��� ��� ���� ������ �������� ����ϱ� ���� ����        
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

    //���� ��� ��� �޼���
    private void TakeResult(float result)
    {
        answer = result;
        aopManager.Show_Result(answer);
    }

    private void DataDefaultSetting()
    {
        int problom = aopManager.timeSet/60;
        //�ð� ������ ���� ���� �� ����
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
        //�����ӵ� ����Ʈ�� ������ ��չ����ӵ� ���
        float reaction = 0;
        for (int i = 0; i < reactionList.Count; i++)
        {
            reaction += reactionList[i];
        }
        aopManager.reactionRate = (reaction / reactionList.Count);        
    }

    private void AnswerRate()
    {
        //����� ���
        aopManager.answers = answer_count * 100 / totalQuestions;
    }

    private IEnumerator WaitExplosionBubble_Co(MovingCube obj)
    {
        //��� ������ �ִϸ��̼�
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
        //���� ���� �ֱ�        
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

