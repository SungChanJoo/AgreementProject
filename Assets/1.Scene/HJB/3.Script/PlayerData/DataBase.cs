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
    //��ü ĳ���� ������ �ҷ�����
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
        //createdCharactorCount �̰ɷ� ������ ������ ��Ȱ��ȭ
        //UserList�� ���� ������ ������ ����(�̸�, ����)
    }

    //������ �ٲٴ� �޼ҵ�
    //�Ű����� �� = clientData.Charactor��ȣ
    //1. ClientLiscense ����
    //2. PlayerDataLoad() ����� Ŭ���̾�Ʈ�� ĳ���� �ѹ��� �÷��̾� ������ DB���� �ҷ�����
    public void ChangeCharactor(int charNum)
    {
        ClientData.Charactor = charNum+1;
        Client.instance.ChangeCharactorData(ClientData.Charactor);

        //clientData.Charactor�� ���� playerInfo�� �ҷ���, todo 0321 playerInfo�� ���� ������(init)���� �ҷ�����
        PlayerDataLoad();
        Debug.Log("playerName : " + PlayerCharacter[CharacterIndex].playerName);
        Debug.Log("playerInfo : " + playerInfo.playerName);
        
    }
    public void CharactorAdd(string name)
    {
        
        //�� �κп��� �÷��̾� �߰� ���� �����
        if (UserList.createdCharactorCount >= 5)
        {
            Debug.Log("�ִ� 5������ ��ϰ����մϴ�.");
            return;
        }
        Client.instance.CreateCharactorData(name);
    }

    public void PlayerDataLoad()
    {
        try
        {
            //�÷��̾� ���� �ҷ�����
            playerInfo = Client.instance.AppStart_LoadCharactorDataFromDB();
            //��ŷ������ �ҷ�����
            playerInfo.RankingInfo = Client.instance.AppStart_LoadRankDataFromDB();
            //Ž���� ���������� �ҷ�����
            playerInfo.Collections = Client.instance.AppStart_LoadExpenditionCrewFromDB();
            //������ �÷����� ���� ������ �ҷ�����
            playerInfo.LastPlayStepData = Client.instance.AppStart_LoadLastPlayFromDB();
            //�÷��̾� ������ ������ ��������
            playerInfo.analyticsProfileData = Client.instance.AppStart_LoadAnalyticsProfileDataFromDB();
            network_state = true;
            PlayerCharacter[CharacterIndex] = playerInfo;
        }
        catch (System.Exception)
        {
            playerInfo = null;
            network_state = false;
            Debug.Log("DB���� �÷��̾� �����͸� �ҷ����� ���߽��ϴ�.");
        }
    }

    

}
