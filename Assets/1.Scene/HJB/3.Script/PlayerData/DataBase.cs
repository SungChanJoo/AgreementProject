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
            //CharacterIndex = ClientLiscense���� ���� �ҷ��;���
            return 0;
        }
        set
        {
            CharacterIndex = value;
            //�߰� ��ư ���� ��
            if(PlayerCharacter.Count < CharacterIndex+1)
            {

            }

            //1. ClientLiscense ����
            //2. ClientLoginSet() clientLisence ���� �ݿ�
            //3. PlayerDataLoad() ����� Ŭ���̾�Ʈ�� ĳ���� �ѹ��� �÷��̾� ������ DB���� �ҷ�����

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
        }
        catch (System.Exception)
        {
            playerInfo = null;
            Debug.Log("DB���� �÷��̾� �����͸� �ҷ����� ���߽��ϴ�.");
        }
    }

    

}
