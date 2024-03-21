using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalysisChart : MonoBehaviour
{
    [Header("������ ������")]
    [SerializeField] private Image profile_img;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private ProfileText_M profileText;

    [Header("��Ʈ ����")]
    [SerializeField] private GameObject BackGroundTile_obj;    
    [SerializeField] private GameObject reactionRate_Dot;
    [SerializeField] private GameObject answersRate_Dot;
    [SerializeField] private GameObject reactionRate_Line;
    [SerializeField] private GameObject answersRate_Line;
    [SerializeField] private TextMeshProUGUI[] dayText;
    
    [SerializeField] private TextMeshProUGUI noDataText;


    private List<float> playerdata=new List<float>();
    private List<Vector2> answersRate_vector2 = new List<Vector2>();
    private List<Vector2> reactionRate_vector2 = new List<Vector2>();
    private List<GameObject> reDot_obj = new List<GameObject>();
    private List<GameObject> anDot_obj = new List<GameObject>();
    private List<GameObject> reLine_obj = new List<GameObject>();
    private List<GameObject> anLine_obj = new List<GameObject>();

    private Dictionary<(Game_Type, int), List<float>> answer_Type =
        new Dictionary<(Game_Type, int), List<float>>();
    private Dictionary<(Game_Type, int), List<float>> reaction_Type =
       new Dictionary<(Game_Type, int), List<float>>();

    private Game_Type game_type= Game_Type.A;
    private int level_num=1;

    private AnalyticsData analyticsData;



    private void Start()
    {
        analyticsData = Client.instance.AppStart_LoadAnalyticsDataFromDB();
        ObjectPooling();        
        ShowChartData();
    }
    private void OnEnable()
    {
        profile_img.sprite = profileText.PlayerSprite;
        playerName.text = $"({profileText.PlayerName})�� �м�ǥ";
    }
    private void ObjectPooling()
    {
        for (int i = 0; i < dayText.Length; i++)
        {
            anDot_obj.Add(Instantiate(answersRate_Dot,BackGroundTile_obj.transform));
            reDot_obj.Add(Instantiate(reactionRate_Dot, BackGroundTile_obj.transform));
           
            anDot_obj[i].SetActive(false);
            reDot_obj[i].SetActive(false);
            
        }
        for (int i = 0; i < dayText.Length-1; i++)
        {
            anLine_obj.Add(Instantiate(answersRate_Line, BackGroundTile_obj.transform));
            reLine_obj.Add(Instantiate(reactionRate_Line, BackGroundTile_obj.transform));
            anLine_obj[i].SetActive(false);
            reLine_obj[i].SetActive(false);
        }
    }
    
    private void DrawGraph_Dot(List<float> data,List<GameObject> dot_obj,bool first)
    {
        //�׷����� �׷��� ������Ʈ�� ������Ʈ ��������
        RectTransform backRect = BackGroundTile_obj.GetComponent<RectTransform>();
        float Back_Height = backRect.rect.height - 200;
        //��¥�� ���̸�ŭ �ݺ�
        for (int i = 0; i < dayText.Length; i++)
        {            
            //�׷����� �̹��� Ʋ �ȿ��� �׷����� ���� �����Ͽ� ������ ��Ÿ����
            float a = Mathf.Lerp(0, Back_Height, data[i]);
            //���� ������ �⺻��ġ�� 0���� �����ϱ� ���� ���
            float b = a - (Back_Height*0.5f+50f);            
            //���⿡�� Y���� ���� �޾Ƽ� �־����
            Vector2 dayVector2 = new Vector2(dayText[i].rectTransform.localPosition.x, b );
            
            RectTransform rectTransform = dot_obj[i].GetComponent<RectTransform>();
            rectTransform.anchoredPosition = dayVector2;
            //���� ������ Text�� ǥ��
            Dot_Data dot = dot_obj[i].GetComponent<Dot_Data>();
            if (first)
            {
                dot.Print_DotData(data[i]*100f,first);                
                answersRate_vector2.Add(dayVector2);
            }
            else
            {
                dot.Print_DotData(Mathf.Abs(data[i]-1f)*20f,first);                
                reactionRate_vector2.Add(dayVector2);
            }
            //�� ���� ��ġ ����
            dot_obj[i].SetActive(true);
        }
    }
    private void DrawGraph_Line(List<GameObject> line,List<Vector2> vector2)
    {        
        for (int i = 0; i < vector2.Count; i++)
        {
            if (i==6)
            {
                //������ ���̸� ����
                return;
            }            
            RectTransform RT_Line = line[i].GetComponent<RectTransform>();
            //���� �������� ���� ���ϱ�
            float lineWidth = Vector2.Distance(vector2[i], vector2[i + 1]);
            //���� ��ġ ����
            float linePivot_X = (vector2[i].x + vector2[i + 1].x) / 2;
            float linePivot_Y = (vector2[i].y + vector2[i + 1].y) / 2;
            //���� ��������
            Vector2 direction = (vector2[i] - vector2[i + 1]).normalized;
            //��ź��Ʈ ���� ����Ͽ� �κ��� ������ ���
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            RT_Line.anchoredPosition = new Vector2(linePivot_X, linePivot_Y);
            RT_Line.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lineWidth);
            RT_Line.localRotation = Quaternion.Euler(0,0,angle);
            line[i].SetActive(true);
        }
    }

    public void SelectGame(int num)
    {
        game_type = (Game_Type)num;
        ShowChartData();
    }
    public void SelectLevel(int num)
    {
        level_num = num+1;
        ShowChartData();
    }
    private void ShowChartData()
    {
        try
        {
            List<float> reactionRate_list = new List<float>();
            List<float> answerRate_list = new List<float>();
            for (int i = 1; i < 8; i++)
            {
                reactionRate_list.Add(analyticsData.Data[(i, game_type, level_num)].reactionRate);
                
                
                reactionRate_list[i - 1] = Mathf.Abs((reactionRate_list[i - 1] - (reactionRate_list[i - 1] % 0.5f)) - 20f) / 20f;
                
                dayText[i - 1].text = analyticsData.Data[(i, game_type, level_num)].day;
            }
            SelectType(answerRate_list, reactionRate_list);
        }
        catch (System.Exception)
        {
            noDataText.text = "�����Ͱ� �����ϴ�.";
        }
        

    }
    private void SelectType(List<float> answer,List<float> reaction)
    {        
        DrawGraph_Dot(answer,anDot_obj,true);
        DrawGraph_Dot(reaction,reDot_obj,false);
        DrawGraph_Line(anLine_obj, answersRate_vector2);
        DrawGraph_Line(reLine_obj, reactionRate_vector2);
        LineVectorList_Clear();        
    }
    private void LineVectorList_Clear()
    {
        //���� ���� ��ġ ����
        answersRate_vector2.Clear();
        reactionRate_vector2.Clear();        
    }

    public void  ReactionRate_Btn(bool check)
    {
        if (check)
        {
            
            RateSetActive(reDot_obj, reLine_obj);
        }
        else
        {
            RateSetActive(anDot_obj, anLine_obj);
        }
    }
    
    private void RateSetActive(List<GameObject> dot_obj, List<GameObject> line_obj)
    {
        foreach (var obj in dot_obj)
        {
            obj.SetActive(!obj.activeInHierarchy);   
        }
        foreach (var obj in line_obj)
        {
            obj.SetActive(!obj.activeInHierarchy);
        }        
    }

    
}      
