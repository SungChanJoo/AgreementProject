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

    /*      ������ ����
                -���̼����� ���� DB������ ���� �ҷ�����
            ������ �߰�
                -DB ĳƽ�� ������ �ε��� + 1 �� �߰��ϱ�
    */
    //������ �ٲٴ� �޼ҵ�
    //�Ű����� �� = clientData.Charactor��ȣ
    //1. ClientLiscense ����
    //2. PlayerDataLoad() ����� Ŭ���̾�Ʈ�� ĳ���� �ѹ��� �÷��̾� ������ DB���� �ҷ�����
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
        //�� �κп��� �÷��̾� �߰� ���� �����
        if (PlayerCharacter.Count >= 30)
        {
            Debug.Log("�ִ� 30������ ��ϰ����մϴ�.");
            return;
        }
        //Player�� Charactor�߰�
        PlayerCharacter.Add(playerInfo);
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

            CharactorAdd();
            network_state = true;
        }
        catch (System.Exception)
        {
            playerInfo = null;
            network_state = false;
            Debug.Log("DB���� �÷��̾� �����͸� �ҷ����� ���߽��ϴ�.");
        }
    }

    

}
