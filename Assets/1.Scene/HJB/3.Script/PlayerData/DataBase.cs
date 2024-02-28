using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBase : MonoBehaviour
{
    public static DataBase Instance = null;
    public Player_DB playerInfo;
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

    public void PlayerDataLoad()
    {
        try
        {
            //�÷��̾� ���� �ҷ�����
            playerInfo = Client.instance.AppStart_LoadAllDataFromDB();        

        }
        catch (System.Exception)
        {
            Debug.Log("DB���� �÷��̾� �����͸� �ҷ����� ���߽��ϴ�.");
        }
    }

    

}
