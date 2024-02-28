using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalysisChart : MonoBehaviour
{
    [SerializeField] private GameObject BackGroundTile_obj;    
    [SerializeField] private GameObject reactionRate_Dot;
    [SerializeField] private GameObject answersRate_Dot;
    [SerializeField] private GameObject Line_obj;
    [SerializeField] private TextMeshProUGUI[] dayText;
    
    private List<float> playerdata=new List<float>();
    private List<Vector2> Dot_vector2 = new List<Vector2>();

    private Dictionary<(Game_Type, int), List<float>> select_Type =
        new Dictionary<(Game_Type, int), List<float>>();

    private Game_Type game_type=0;
    private int level_num=0;



    Player_DB result_Data = new Player_DB();

    private void Start()
    {
        DataSet();
        SelectType();
        
    }
    private void DataSet()
    {
        
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                List<float> playerdata = new List<float>();
                for (int i = 0; i < 7; i++)
                {
                    //Data_value data = new Data_value(i, i, i, i, i);
                    //result_Data.Data.Add((Game_Type.A, 1, 1), data);

                    playerdata.Add(Random.Range(0, 1f));
                }
                select_Type.Add(((Game_Type)x, y), playerdata);                
            }
        }

    }    
    private void DrawGraph_Dot(List<float> data)
    {
        //�׷����� �׷��� ������Ʈ�� ������Ʈ ��������
        RectTransform backRect = BackGroundTile_obj.GetComponent<RectTransform>();
        float Back_Height = backRect.rect.height - 100;
        //��¥�� ���̸�ŭ �ݺ�
        for (int i = 0; i < dayText.Length; i++)
        {
            //�׷����� �̹��� Ʋ �ȿ��� �׷����� ���� �����Ͽ� ������ ��Ÿ����
            float a = Mathf.Lerp(0, Back_Height, data[i]);
            //���� ������ �⺻��ġ�� 0���� �����ϱ� ���� ���
            float b = a - (Back_Height*0.5f);            
            //���⿡�� Y���� ���� �޾Ƽ� �־����
            Vector2 dayVector2 = new Vector2(dayText[i].rectTransform.localPosition.x, b );
            GameObject R_Dot = Instantiate(reactionRate_Dot,BackGroundTile_obj.transform);
            RectTransform rectTransform = R_Dot.GetComponent<RectTransform>();
            //���� ������ Text�� ǥ��
            Dot_Data dot = R_Dot.GetComponent<Dot_Data>();
            dot.Print_DotData(data[i]);
            //�� ���� ��ġ ����
            rectTransform.anchoredPosition = dayVector2;
            Dot_vector2.Add(dayVector2);
        }
    }
    private void DrawGraph_Line()
    {        
        for (int i = 0; i < Dot_vector2.Count; i++)
        {
            if (i==6)
            {
                //������ ���̸� ����
                return;
            }
            GameObject Line = Instantiate(Line_obj, BackGroundTile_obj.transform);
            RectTransform RT_Line = Line.GetComponent<RectTransform>();
            //���� �������� ���� ���ϱ�
            float lineWidth = Vector2.Distance(Dot_vector2[i], Dot_vector2[i + 1]);
            //���� ��ġ ����
            float linePivot_X = (Dot_vector2[i].x + Dot_vector2[i + 1].x) / 2;
            float linePivot_Y = (Dot_vector2[i].y + Dot_vector2[i + 1].y) / 2;
            //���� ��������
            Vector2 direction = (Dot_vector2[i] - Dot_vector2[i + 1]).normalized;
            //��ź��Ʈ ���� ����Ͽ� �κ��� ������ ���
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            RT_Line.anchoredPosition = new Vector2(linePivot_X, linePivot_Y);
            RT_Line.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, lineWidth);
            RT_Line.localRotation = Quaternion.Euler(0,0,angle);
        }
    }

    public void SelectGame(int num)
    {
        game_type = (Game_Type)num;
        SelectType();
    }
    public void SelectLevel(int num)
    {
        level_num = num;
        SelectType();
    }

    private void SelectType()
    {
        //���� �׷��� ����
        ObjectyDestory();

        List<float> data= select_Type[(game_type, level_num)];        
        DrawGraph_Dot(data);
        DrawGraph_Line();
    }
    private void ObjectyDestory()
    {
        //���� ���� ��ġ�� ����
        Dot_vector2.Clear();

        Transform[] childObjects = BackGroundTile_obj.GetComponentsInChildren<Transform>();

        //�迭�� ù ��° ��Ҵ� �θ� ��ü�̹Ƿ� �����ϰ� �ε��� 1���� �����մϴ�.
        for (int i = 1; i < childObjects.Length; i++)
        {
            //�� �ڽ� GameObject�� �����մϴ�.
            Destroy(childObjects[i].gameObject);
        }
    }
    
}      
