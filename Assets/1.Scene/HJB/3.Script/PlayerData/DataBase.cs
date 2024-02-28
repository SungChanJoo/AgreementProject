using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBase : MonoBehaviour
{
    public static DataBase Instance = null;
    public Player_DB playerInfo;
    public List<Player_DB> PlayerCharacter = new List<Player_DB>();
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
            playerInfo = Client.instance.AppStart_LoadAllDataFromDB();
            CharactorAdd();

        }
        catch (System.Exception)
        {
            Debug.Log("DB���� �÷��̾� �����͸� �ҷ����� ���߽��ϴ�.");
        }
    }

    

}
