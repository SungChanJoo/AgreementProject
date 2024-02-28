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
    [Header("팝업창 텍스트 지우기")]
    [SerializeField] private TextMeshProUGUI ChangeName_Input;
    [SerializeField] private TextMeshProUGUI ChangeBirthday_Input;

    [SerializeField] private GameObject Characters;


    private void Start()
    {
        StartLoadCharactorData();
    }
    
    //플레이어 변경 Panel을 클릭 시 호출
    public void CharacterChangeNameLoad()
    {
        //자식의 수만큼 배열선언
        PlayerCharacter[] characters = new PlayerCharacter[Characters.transform.childCount];
        //- 1은 플레이어 추가 버튼을 제외하기 위함
        for (int i = 0; i < Characters.transform.childCount - 1; i++)
        {
            //각 이름을 변경하기 위함
            characters[i] = Characters.transform.GetChild(i).GetComponent<PlayerCharacter>();            
            characters[i].InputName(i);
        }
    }
    private void CalculationPlayTime()
    {
        //시간 계산
        float totalTime = DataBase.Instance.PlayerCharacter[0].TotalTime;
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
    private void StartLoadCharactorData()
    {
        //추가적으로 플레이어 변경 창에서 이름도 바뀌도록 로직 넣을 것
        characterName.text = DataBase.Instance.PlayerCharacter[0].playerName;
        Birthday.text = DataBase.Instance.PlayerCharacter[0].BirthDay;
        totalAnswer.text = DataBase.Instance.PlayerCharacter[0].TotalAnswers.ToString();
        //시간 계산 후 출력
        CalculationPlayTime();
        
    }
    public void NameChange()
    {
        //데이터 바꾸기
        DataBase.Instance.PlayerCharacter[0].playerName = ChangeName_Input.text;
        //바꾼 이름 출력
        characterName.text = ChangeName_Input.text;
        CharacterChangeNameLoad();
        Client.instance.RegisterCharactorName_SaveDataToDB(ChangeName_Input.text);
    }

    public void BirthDayChange()
    {
        //데이터 바꾸기
        DataBase.Instance.PlayerCharacter[0].BirthDay = ChangeBirthday_Input.text;
        //바꾼 생일 출력
        Birthday.text = ChangeBirthday_Input.text;
    }

}
