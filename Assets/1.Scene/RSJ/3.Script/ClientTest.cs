using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientTest : MonoBehaviour
{
    public void OnClickClientRequestLoadDataToDB()
    {
        Player_DB playerDB = Client.instance.AppStart_LoadAllDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadAllDataFromDB(), Current time : {System.DateTime.Now}");
        AnalyticsData gameAnalytics = Client.instance.AppStart_LoadGameAnalyticsDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadGameAnalyticsDataFromDB(), Current time : {System.DateTime.Now}");
        RankData rankData = Client.instance.AppStart_LoadRankDataFromDB();
        Debug.Log($"[ClientTest] use Client.instance.AppStart_LoadRankDataFromDB(), Current time : {System.DateTime.Now}");
    }
    
}
