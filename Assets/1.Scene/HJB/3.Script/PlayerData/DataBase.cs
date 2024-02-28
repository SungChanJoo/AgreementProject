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
            //플레이어 정보 불러오기
            playerInfo = Client.instance.AppStart_LoadAllDataFromDB();        

        }
        catch (System.Exception)
        {
            Debug.Log("DB에서 플레이어 데이터를 불러오지 못했습니다.");
        }
    }

    

}
