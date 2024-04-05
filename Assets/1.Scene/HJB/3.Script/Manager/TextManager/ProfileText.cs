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
    [Header("�˾�â �ؽ�Ʈ �����")]
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
            //���κм�ǥ ���
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
            Debug.Log("�޾ƿ� �����Ͱ� �����ϴ�.");
        }
    }    


    public void LoadCharacterProfile()
    {
        //�ڽ��� ����ŭ �迭����
        //- 1�� �÷��̾� �߰� ��ư�� �����ϱ� ����
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
    //�÷��̾� ���� Panel�� Ŭ�� �� ȣ��
    public void CharacterChangeNameLoad()
    {
        var characters = Characters.transform.GetChild(DataBase.Instance.ClientData.Charactor).GetComponent<PlayerCharacter>();
        characters.SetProfile(DataBase.Instance.ClientData.Charactor-1);
    }

    private void CalculationPlayTime(int player_num)
    {
        //�ð� ���                     
        float totalTime = DataBase.Instance.PlayerCharacter[player_num].TotalTime;
        int minute = (int)(totalTime / 60f);
        float second = totalTime - (minute * 60);
        if (minute == 0)
        {
            //60�ʸ� �ѱ��� �ʾҴٸ� �� ǥ�� ����
            playTime.text = $"{(int)second}��";
        }
        else
        {
            //60�ʸ� �Ѱ�ٸ� ǥ��
            playTime.text = $"{minute}�� {(int)second}��";
        }
    }
    public void StartLoadCharactorData()
    {
        int player_num = DataBase.Instance.CharacterIndex;
        //�߰������� �÷��̾� ���� â���� �̸��� �ٲ�� ���� ���� ��
        CharacterName = DataBase.Instance.PlayerCharacter[player_num].playerName;
        Birthday.text = DataBase.Instance.PlayerCharacter[player_num].BirthDay;        
        totalAnswer.text = DataBase.Instance.PlayerCharacter[player_num].TotalAnswers.ToString();
        //�ð� ��� �� ���
        CalculationPlayTime(player_num);
        profileButtonEvent.ChangeImage_Btn(1);
    }
    public void NameChange()
    {
        try
        {
            //������ �ٲٱ�
            DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].playerName = ChangeName_Input.text;            
            //DB�� Load�ϱ�
            CharacterChangeNameLoad();
            Client.Instance.RegisterCharactorName_SaveDataToDB(ChangeName_Input.text);            
        }
        catch (System.Exception)
        {            
            Debug.Log("��Ʈ��ũ ���� �Ǵ� DB�� ������ �Ұ��մϴ�.");            
        }
        finally
        {
            //�ٲ� �̸� ���
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
            //������ �ٲٱ�
            DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].BirthDay 
                = ChangeBirthday_Input.text;
            //DB�� Load�ϱ�
            Client.Instance.RegisterCharactorBirthday_SaveDataToDB(ChangeBirthday_Input.text);

        }
        catch (System.Exception)
        {
            Debug.Log("��Ʈ��ũ ���� �Ǵ� DB�� ������ �Ұ��մϴ�.");
        }
        Birthday.text = text;
        profileManager.PlayerBirthDay_UI();
        TextClear(ChangeBirthday_Input);
    }
    
    public void BirthDayChangeErrorText_EndEdit()
    {   //���� �Է� ������ ���� ���� ��
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
    {   //�÷��̾� ��� �ʱ�ȭ
        try
        {
            Client.instance.ResetCharactorProfile();
            DataBase.Instance.PlayerRecordLoad();
            CalculationPlayTime(DataBase.Instance.CharacterIndex);
            profileButtonEvent.ChangeImage_Btn(1);
        }
        catch (System.Exception)
        {

            Debug.Log("�ʱ�ȭ ���� ������ �߻��߽��ϴ�.");
        }
              
    }

}
