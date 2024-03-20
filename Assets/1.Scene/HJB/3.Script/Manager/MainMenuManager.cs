using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class MainMenuManager : MonoBehaviour
{    
    [SerializeField] private GameObject MainMenuCanvas;
    [SerializeField] private GameObject CrewSelectCavas;
    [SerializeField] private GameObject CollectionCavas;
    [SerializeField] private GameObject LevelCavas;
    [SerializeField] private GameObject CameraCavas;
    [SerializeField] private GameObject AlbumCavas;
    [SerializeField] private GameObject ProfileCanvas;
    [SerializeField] private GameObject StepCanvas;
    [SerializeField] private GameObject ChartCanvas;
    [SerializeField] private GameObject RankingCanvas;

    [SerializeField] private List<GameObject> StepOfLevel;
    [SerializeField] private List<GameObject> Level1StarPoint_Panel;
    [SerializeField] private List<GameObject> Level2StarPoint_Panel;
    [SerializeField] private List<GameObject> Level3StarPoint_Panel;

    [SerializeField] private TextMeshProUGUI Cost_UI;
    [SerializeField] private TextMeshProUGUI NetworkState_UI;


    public Game_Type game_Type;
    
    private void Start()
    {
        SettingManager.Instance.EnableSettingBtn();        
        AudioManager.Instance.BGM_Play(0);
        Cost_UI.text = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].StarCoin.ToString();
        string net_state = DataBase.Instance.network_state ?  "����": "����ȵ�";
        NetworkState_UI.text = net_state;
    }
    public void GameScene()
    {        
        if(CrewMovementManager.Instance != null)
            CrewMovementManager.Instance.SelectStep();
    }
    public void GameType_Btn(int Type)
    {
        game_Type = (Game_Type)Type;
        StepManager.Instance.game_Type = game_Type;

    }
    //Application ���� ��ư
    public void Exit_Btn()
    {
        SettingManager.Instance.ApplicationExit_Btn();
    }
    public void CollectionShop_UI()
    {
        CollectionCavas.SetActive(!CollectionCavas.activeSelf);        
    }

    
    //������ Level �Ҵ� �̺�Ʈ
    public void SelectLevel_Btn(int level)
    {        
        //StepUI�� ��Ȱ��ȭ�̸�..
        if (!StepCanvas.activeInHierarchy)
        {
            //Level ���� �� StepUI Ȱ��ȭ, LevelUI ��Ȱ��ȭ
            Step_UI();
            Level_UI();
        }
        StepManager.Instance.SelectLevel(level);
        if (CrewMovementManager.Instance != null)
        {
            //Ž���� ����
            CrewMovementManager.Instance.ViewCrew();
            var stepList = new List<GameObject>();
            for (int i =0; i< StepOfLevel[level-1].transform.childCount; i++)
            {
                //������ ������ UI ������Ʈ
                var step = StepOfLevel[level-1].transform.GetChild(i).Find("StarPoint_Panel").gameObject;
                stepList.Add(step);
            }
            CrewMovementManager.Instance.StepByStarPoint(stepList);
        }
        for (int i = 0; i < StepOfLevel.Count; i++)
        {
            if (level - 1 == i)
            {
                StepOfLevel[i].SetActive(true);
            }
            else
            {
                StepOfLevel[i].SetActive(false);
            }
        }
    }
    
    
    //������ Step �Ҵ� �̺�Ʈ
    public void SelectStep(int step)
    {
        StepManager.Instance.SelectStep(step);
        if(CrewMovementManager.Instance != null)
            CrewMovementManager.Instance.ViewCrew();
        GameScene();
    } 
    public void SelectTime(int time)
    {
        StepManager.Instance.SelectTimeSet(time);
        Debug.Log(time);
    }

    
    public void Level_UI()
    {
        LevelCavas.SetActive(!LevelCavas.activeSelf);
    }
    public void Camera_UI()
    {
        CameraCavas.SetActive(!CameraCavas.activeSelf);
    }
    public void Album_UI()
    {
        AlbumCavas.SetActive(!AlbumCavas.activeSelf);
    }
    public void Chart_UI()
    {
        ChartCanvas.SetActive(!ChartCanvas.activeSelf);
    }
    public void MetaWorldPetSelect_UI()
    {
        CrewSelectCavas.SetActive(!CrewSelectCavas.activeSelf);        
    }

    
    //ȯ�漳�� â On
    public void Setting_UI()
    {
        SettingManager.Instance.Setting_Btn();
    }
    public void Profile_UI()
    {
        ProfileCanvas.SetActive(!ProfileCanvas.activeSelf);
    }
    public void Step_UI()
    {
        StepCanvas.SetActive(!StepCanvas.activeSelf);
        if (CrewMovementManager.Instance != null)
            CrewMovementManager.Instance.ExitStep();
    }

    public void Ranking_UI()
    {
        RankingCanvas.SetActive(!RankingCanvas.activeSelf);
    }
}
