using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBase : MonoBehaviour
{
    public static DataBase Instance = null;
    Player_DB playerInfo;
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

    private void  PlayerDataLoad()
    {
        playerInfo = Client.instance.AppStart_LoadAllDataFromDB();
    }

    

}
