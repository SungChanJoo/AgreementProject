using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling_H : MonoBehaviour
{

    [SerializeField] private GameObject cube_Obj;
    [SerializeField] private GameObject[] poolPosition;
    
    [SerializeField]private EAG_Manager aopManager;    

    private List<GameObject> cubePool = new List<GameObject>();
    private List<float> reactionList = new List<float>();
    private MovingCube resultObject;

    public float answer;    
    private int problom_count=0;
    private int answer_count = 0;
    private int totalQuestions = 0;
    private float waitTime =0;
    private bool timeOut = false;

    private IEnumerator WaitExplosionBubble_co;

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
            cube.transform.position = poolPosition[i].transform.position;
            cube.SetActive(false);
            cubePool.Add(cube);
        }
        CubeStart();
    }
    private void Update()
    {        
        Click_Obj();        
        TimeCheck();        
    }   
    private void TimeCheck()
    {        
        //������ �����ٸ�
        if (aopManager.isStop || (timeOut && TimeSlider.Instance.TimeStop))
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
            WaitExplosionBubble_co = WaitExplosionBubble_Co(resultObject, true);
            StartCoroutine(WaitExplosionBubble_co);            
        }
    }
    private void Click_Obj()
    {        
        if (Input.GetMouseButtonDown(0)||
            Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && !hit.collider.gameObject.CompareTag("Ground"))
            {
                //���� üũ
                MovingCube movingCube = hit.collider.GetComponent<MovingCube>();
                WaitExplosionBubble_co = WaitExplosionBubble_Co(movingCube, true);
                StartCoroutine(WaitExplosionBubble_co);
            }
        }
    }

    public void GameOver()
    {
        StopCoroutine(WaitExplosionBubble_co);
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
            //Ǯ�� ����ִ� ������Ʈ�� �� ����
            MovingCube movingcube = cubePool[i].GetComponent<MovingCube>();            
            movingcube.reactionRate = 0;
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
        int start = (step == 1) ? 0 : cubePool.Count - 1;

        for (int i = start; i >= 0 && i < cubePool.Count; i += step)
        {
            if (cubePool[i].activeSelf)
            {
                MovingCube movingCube = cubePool[i].GetComponent<MovingCube>();
                TakeResult(movingCube.result);
                resultObject = movingCube;
                return;
            }
        }

        CubeStart();
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

    private IEnumerator WaitExplosionBubble_Co(MovingCube obj,bool answer)
    {
        obj.ExplosionAni();
        yield return new WaitForSeconds(1f);
        obj.DefaultAni();
        obj.gameObject.SetActive(false);
        if (answer)
        {
            Answer_Check(obj);
        }
        else
        {
            Next_Result();
        }
    }


}

