using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using System.IO;

public class ClientData
{
    public string LicenseNumber;
    public int Charactor;

    public ClientData(int ci, string ln = "10000")
    {
        LicenseNumber = ln;
        Charactor = ci;
    }
}
public class DataBase : MonoBehaviour
{
    public static DataBase Instance = null;
    public Player_DB playerInfo;
    public List<Player_DB> PlayerCharacter = new List<Player_DB>();
    [SerializeField] TextAsset clientInfo;
    ClientData clientData;
    private int characterIndex = 0;
    private string dataPath;
    public bool network_state;
    public int CharacterIndex {
        get
        {

            return characterIndex;
        }
        set
        {

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
        if(Application.platform == RuntimePlatform.Android)
        {
            dataPath = Application.persistentDataPath + "/License/clientlicense.json";
        }
        else
        {
            dataPath = Application.dataPath + "/License/clientlicense.json";
        }
    }

    /*      프로필 변경
                -라이센스에 대한 DB데이터 전부 불러오기
            프로필 추가
                -DB 캐틱터 마지막 인덱스 + 1 에 추가하기
    */
    //프로필 바꾸는 메소드
    //매개변수 값 = clientData.Charactor번호
    //1. ClientLiscense 변경
    //2. PlayerDataLoad() 변경된 클라이언트의 캐릭터 넘버로 플레이어 데이터 DB에서 불러오기
    public void ChangeCharactor(int charNum)
    {
        clientData = new ClientData(charNum);
        characterIndex = charNum;
        clientData.Charactor = characterIndex;
        JsonData cleintData = JsonMapper.ToJson(clientData);
        File.WriteAllText(dataPath, cleintData.ToString());
        PlayerDataLoad();
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
            network_state = true;
        }
        catch (System.Exception)
        {
            playerInfo = null;
            network_state = false;
            Debug.Log("DB에서 플레이어 데이터를 불러오지 못했습니다.");
        }
    }

    

}
