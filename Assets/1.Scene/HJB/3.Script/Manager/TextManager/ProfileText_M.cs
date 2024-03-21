using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ProfileText_M : MonoBehaviour
{
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

    [SerializeField] private List<GameObject> PlayerInfo = new List<GameObject>();

    private ProfileManager profileManager;
    
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
        profileManager = GetComponent<ProfileManager>();
    }
    private void OnEnable()
    {
        playTime.text = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].TotalTime.ToString();
        totalAnswer.text = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].TotalAnswers.ToString();
        
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

    private void PlayerDataLoad_Time_Answer()
    {
        
    }


    public void LoadCharacterProfile()
    {
        //�ڽ��� ����ŭ �迭����
        //- 1�� �÷��̾� �߰� ��ư�� �����ϱ� ����
        for (int i = 0; i < Characters.transform.childCount - 1; i++)
        {
            var characters = Characters.transform.GetChild(i).GetComponent<PlayerCharacter>();
            characters.InputName(i);

            if (i < DataBase.Instance.UserList.createdCharactorCount)
            {

                if (!characters.gameObject.activeSelf)
                    characters.gameObject.SetActive(true);
            }
            else
            {
                if (characters.gameObject.activeSelf)
                    characters.gameObject.SetActive(false);
            }
        }
    }
    //�÷��̾� ���� Panel�� Ŭ�� �� ȣ��
    public void CharacterChangeNameLoad()
    {
        var characters = Characters.transform.GetChild(DataBase.Instance.ClientData.Charactor).GetComponent<PlayerCharacter>();
        characters.InputName(DataBase.Instance.ClientData.Charactor);
    }

    private void CalculationPlayTime()
    {
        //�ð� ���
        Debug.Log(DataBase.Instance.PlayerCharacter[0].TotalTime);
        string time = DataBase.Instance.PlayerCharacter[0].TotalTime.ToString();
        Debug.Log(time);
        int count = time.Length-2;
        
        //float currentTime = float.Parse(time.Insert(count, ".")); 
        float totalTime = DataBase.Instance.PlayerCharacter[0].TotalTime;
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
    private void StartLoadCharactorData()
    {
        //�߰������� �÷��̾� ���� â���� �̸��� �ٲ�� ���� ���� ��
        CharacterName = DataBase.Instance.PlayerCharacter[0].playerName;
        Birthday.text = DataBase.Instance.PlayerCharacter[0].BirthDay;           
        totalAnswer.text = DataBase.Instance.PlayerCharacter[0].TotalAnswers.ToString();
        //�ð� ��� �� ���
        CalculationPlayTime();        
    }
    public void NameChange()
    {
        try
        {
            //������ �ٲٱ�
            DataBase.Instance.PlayerCharacter[0].playerName = ChangeName_Input.text;            
            //DB�� Load�ϱ�
            CharacterChangeNameLoad();
            Client.instance.RegisterCharactorName_SaveDataToDB(ChangeName_Input.text);            
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
            DataBase.Instance.PlayerCharacter[0].BirthDay = ChangeBirthday_Input.text;
            //DB�� Load�ϱ�
            Client.instance.RegisterCharactorBirthday_SaveDataToDB(ChangeBirthday_Input.text);

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
    {
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
        
    }

}
