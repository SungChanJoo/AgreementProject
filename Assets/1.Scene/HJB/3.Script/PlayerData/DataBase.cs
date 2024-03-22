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
    public UserData UserList;
    public List<Player_DB> PlayerCharacter = new List<Player_DB>();
    [SerializeField] TextAsset clientInfo;
    public ClientData ClientData;
    private string dataPath;
    public bool network_state;
    public int CharacterIndex {
        get
        {
            return 0;
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
    //전체 캐릭터 프로필 불러오기
    public void LoadUserList()
    {
        string jsonStringFromFile = File.ReadAllText(dataPath);
        JsonData client_JsonFile = JsonMapper.ToObject(jsonStringFromFile);
        ClientData = new ClientData(Int32.Parse(client_JsonFile["Charactor"].ToString()), client_JsonFile["LicenseNumber"].ToString());
        try
        {
            UserList = Client.instance.AppStart_LoadUserDataFromDB();

        }
        catch(Exception e)
        {
            Debug.Log(e + "UserList Don't Load");
        }
        //createdCharactorCount 이걸로 프로필 프리펩 비활성화
        //UserList에 따라 프로필 데이터 갱신(이름, 사진)
    }

    //프로필 바꾸는 메소드
    //매개변수 값 = clientData.Charactor번호
    //1. ClientLiscense 변경
    //2. PlayerDataLoad() 변경된 클라이언트의 캐릭터 넘버로 플레이어 데이터 DB에서 불러오기
    public void ChangeCharactor(int charNum)
    {
        ClientData.Charactor = charNum+1;
        Client.instance.ChangeCharactorData(ClientData.Charactor);

        //clientData.Charactor에 따라 playerInfo를 불러옴, todo 0321 playerInfo에 따른 데이터(init)들을 불러와줘
        PlayerDataLoad();
        Debug.Log("playerName : " + PlayerCharacter[CharacterIndex].playerName);
        Debug.Log("playerInfo : " + playerInfo.playerName);
        
    }
    public void CharactorAdd(string name)
    {
        
        //이 부분에서 플레이어 추가 로직 만들기
        if (UserList.createdCharactorCount >= 5)
        {
            Debug.Log("최대 5개까지 등록가능합니다.");
            return;
        }
        Client.instance.CreateCharactorData(name);
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
            network_state = true;
            PlayerCharacter[CharacterIndex] = playerInfo;
        }
        catch (System.Exception)
        {
            playerInfo = null;
            network_state = false;
            Debug.Log("DB에서 플레이어 데이터를 불러오지 못했습니다.");
        }
    }

    

}
