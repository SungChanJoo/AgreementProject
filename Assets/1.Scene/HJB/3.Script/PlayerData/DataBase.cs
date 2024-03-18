using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBase : MonoBehaviour
{
    public static DataBase Instance = null;
    public Player_DB playerInfo;
    public List<Player_DB> PlayerCharacter = new List<Player_DB>();


    public int CharacterIndex {
        get
        {
            //CharacterIndex = ClientLiscense에서 숫자 불러와야함
            return 0;
        }
        set
        {
            CharacterIndex = value;
            //추가 버튼 누를 시
            if(PlayerCharacter.Count < CharacterIndex+1)
            {

            }

            //1. ClientLiscense 변경
            //2. ClientLoginSet() clientLisence 변경 반영
            //3. PlayerDataLoad() 변경된 클라이언트의 캐릭터 넘버로 플레이어 데이터 DB에서 불러오기

            PlayerDataLoad();
        }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void CharactorAdd()
    {
        //이 부분에서 플레이어 추가 로직 만들기
        if (PlayerCharacter.Count >= 30)
        {
            Debug.Log("최대 30개까지 등록가능합니다.");
            return;
        }
        //Player의 Charactor추가
        PlayerCharacter.Add(playerInfo);
    }
    public void PlayerDataLoad()
    {
        try
        {
            //플레이어 정보 불러오기
            playerInfo = Client.instance.AppStart_LoadCharactorDataFromDB();
            //랭킹데이터 불러오기
            playerInfo.RankingInfo = Client.instance.AppStart_LoadRankDataFromDB();
            //탐험대원 도감데이터 불러오기
            playerInfo.Collections = Client.instance.AppStart_LoadExpenditionCrewFromDB();
            //마지막 플레이한 스텝 데이터 불러오기
            playerInfo.LastPlayStepData = Client.instance.AppStart_LoadLastPlayFromDB();
            //플레이어 프로필 데이터 가져오기
            playerInfo.analyticsProfileData = Client.instance.AppStart_LoadAnalyticsProfileDataFromDB();


            CharactorAdd();
        }
        catch (System.Exception)
        {
            playerInfo = null;
            Debug.Log("DB에서 플레이어 데이터를 불러오지 못했습니다.");
        }
    }

    

}
