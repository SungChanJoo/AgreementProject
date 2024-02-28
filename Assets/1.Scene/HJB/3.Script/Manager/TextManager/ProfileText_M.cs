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
    [SerializeField] private TextMeshProUGUI ChangeName_Input;
    [SerializeField] private TextMeshProUGUI ChangeBirthday_Input;

    [SerializeField] private GameObject Characters;


    private void Start()
    {
        StartLoadCharactorData();
    }
    
    //�÷��̾� ���� Panel�� Ŭ�� �� ȣ��
    public void CharacterChangeNameLoad()
    {
        //�ڽ��� ����ŭ �迭����
        PlayerCharacter[] characters = new PlayerCharacter[Characters.transform.childCount];
        //- 1�� �÷��̾� �߰� ��ư�� �����ϱ� ����
        for (int i = 0; i < Characters.transform.childCount - 1; i++)
        {
            //�� �̸��� �����ϱ� ����
            characters[i] = Characters.transform.GetChild(i).GetComponent<PlayerCharacter>();            
            characters[i].InputName(i);
        }
    }
    private void CalculationPlayTime()
    {
        //�ð� ���
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
        characterName.text = DataBase.Instance.PlayerCharacter[0].playerName;
        Birthday.text = DataBase.Instance.PlayerCharacter[0].BirthDay;
        totalAnswer.text = DataBase.Instance.PlayerCharacter[0].TotalAnswers.ToString();
        //�ð� ��� �� ���
        CalculationPlayTime();
        
    }
    public void NameChange()
    {
        //������ �ٲٱ�
        DataBase.Instance.PlayerCharacter[0].playerName = ChangeName_Input.text;
        //�ٲ� �̸� ���
        characterName.text = ChangeName_Input.text;
        CharacterChangeNameLoad();
        Client.instance.RegisterCharactorName_SaveDataToDB(ChangeName_Input.text);
    }

    public void BirthDayChange()
    {
        //������ �ٲٱ�
        DataBase.Instance.PlayerCharacter[0].BirthDay = ChangeBirthday_Input.text;
        //�ٲ� ���� ���
        Birthday.text = ChangeBirthday_Input.text;
    }

}
