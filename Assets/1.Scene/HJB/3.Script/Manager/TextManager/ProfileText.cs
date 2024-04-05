using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ProfileText : MonoBehaviour
{
    public static ProfileText Instance = null;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI Birthday;
    [SerializeField] private TextMeshProUGUI playTime;
    [SerializeField] private TextMeshProUGUI totalAnswer;
    [Header("팝업창 텍스트 지우기")]
    [SerializeField] private TMP_InputField ChangeName_Input;
    [SerializeField] private TMP_InputField ChangeBirthday_Input;
    [SerializeField] private TMP_InputField Registration_Input;

    public GameObject Characters;
    [SerializeField] private GameObject ErrorText;

    private ProfileManager profileManager;
    [SerializeField] ProfileButtonEvent profileButtonEvent;

    public string PlayerName;       

    public string CharacterName
    {
        get { return characterName.text; }
        set
        {
            characterName.text = value;
            //개인분석표 출력
            PlayerName = characterName.text;
        }
    }
    public string PlayTime_text
    {
        get { return playTime.text; }
        set
        {
            playTime.text = value;            
        }
    }

    public Sprite PlayerSprite;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }        
        profileManager = GetComponent<ProfileManager>();        
    }
    private void Start()
    {
        try
        {
            StartLoadCharactorData();
        }
        catch (System.Exception)
        {
            Debug.Log("받아온 데이터가 없습니다.");
        }
    }    


    public void LoadCharacterProfile()
    {
        //자식의 수만큼 배열선언
        //- 1은 플레이어 추가 버튼을 제외하기 위함
        for (int i = 0; i < Characters.transform.childCount - 1; i++)
        {
            var characters = Characters.transform.GetChild(i).GetComponent<PlayerCharacter>();

            if (i < DataBase.Instance.UserList.createdCharactorCount)
            {
                characters.SetProfile(i);
                if (!characters.gameObject.activeSelf)
                    characters.gameObject.SetActive(true);
            }
            else
            {
                break;
            }
        }
    }
    //플레이어 변경 Panel을 클릭 시 호출
    public void CharacterChangeNameLoad()
    {
        var characters = Characters.transform.GetChild(DataBase.Instance.ClientData.Charactor).GetComponent<PlayerCharacter>();
        characters.SetProfile(DataBase.Instance.ClientData.Charactor-1);
    }

    private void CalculationPlayTime(int player_num)
    {
        //시간 계산                     
        float totalTime = DataBase.Instance.PlayerCharacter[player_num].TotalTime;
        int minute = (int)(totalTime / 60f);
        float second = totalTime - (minute * 60);
        if (minute == 0)
        {
            //60초를 넘기지 않았다면 분 표시 생략
            playTime.text = $"{(int)second}초";
        }
        else
        {
            //60초를 넘겼다면 표시
            playTime.text = $"{minute}분 {(int)second}초";
        }
    }
    public void StartLoadCharactorData()
    {
        int player_num = DataBase.Instance.CharacterIndex;
        //추가적으로 플레이어 변경 창에서 이름도 바뀌도록 로직 넣을 것
        CharacterName = DataBase.Instance.PlayerCharacter[player_num].playerName;
        Birthday.text = DataBase.Instance.PlayerCharacter[player_num].BirthDay;        
        totalAnswer.text = DataBase.Instance.PlayerCharacter[player_num].TotalAnswers.ToString();
        //시간 계산 후 출력
        CalculationPlayTime(player_num);
        profileButtonEvent.ChangeImage_Btn(1);
    }
    public void NameChange()
    {
        try
        {
            //데이터 바꾸기
            DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].playerName = ChangeName_Input.text;            
            //DB에 Load하기
            CharacterChangeNameLoad();
            Client.Instance.RegisterCharactorName_SaveDataToDB(ChangeName_Input.text);            
        }
        catch (System.Exception)
        {            
            Debug.Log("네트워크 연결 또는 DB에 접속이 불가합니다.");            
        }
        finally
        {
            //바꾼 이름 출력
            CharacterName = ChangeName_Input.text;
        }
        TextClear(ChangeName_Input);
    }

    public void BirthDayChange()
    {
        if (ChangeBirthday_Input.text == string.Empty)
        {
            ErrorText.SetActive(true);
            return;
        }
        string text = ChangeBirthday_Input.text.PadLeft(8,'0').Insert(6,".").Insert(4,".");
        try
        {
            //데이터 바꾸기
            DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].BirthDay 
                = ChangeBirthday_Input.text;
            //DB에 Load하기
            Client.Instance.RegisterCharactorBirthday_SaveDataToDB(ChangeBirthday_Input.text);

        }
        catch (System.Exception)
        {
            Debug.Log("네트워크 연결 또는 DB에 접속이 불가합니다.");
        }
        Birthday.text = text;
        profileManager.PlayerBirthDay_UI();
        TextClear(ChangeBirthday_Input);
    }
    
    public void BirthDayChangeErrorText_EndEdit()
    {   //생일 입력 조건이 맞지 않을 시
        string text = ChangeBirthday_Input.text;
        if (text.Length!=8)
        {
            ErrorText.SetActive(true);
            TextClear(ChangeBirthday_Input);
        }
    }
    public void TextClear(TMP_InputField tmp)
    {
        tmp.text = string.Empty;
    }

    public void Registration_InputField()
    {
        Registration_Input.text = "";
    }

    public void RecordDelete_Btn()
    {   //플레이어 기록 초기화
        try
        {
            Client.instance.ResetCharactorProfile();
            DataBase.Instance.PlayerRecordLoad();
            CalculationPlayTime(DataBase.Instance.CharacterIndex);
            profileButtonEvent.ChangeImage_Btn(1);
        }
        catch (System.Exception)
        {

            Debug.Log("초기화 도중 오류가 발생했습니다.");
        }
              
    }

}
