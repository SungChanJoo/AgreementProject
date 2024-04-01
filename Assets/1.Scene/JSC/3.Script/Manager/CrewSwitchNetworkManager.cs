using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//메타별 탐험대원 프리펩 변경 클래스
public class CrewSwitchNetworkManager : NetworkManager
{
    public void ReplacePlayer(NetworkConnectionToClient conn, GameObject newPrefab, Vector3 spawnPos)
    {

        GameObject oldPlayer = conn.identity.gameObject;
        NetworkServer.ReplacePlayerForConnection(conn, Instantiate(newPrefab, spawnPos, Quaternion.Euler(0f,180f,0f)), true);
        Destroy(oldPlayer, 0.1f);
        Debug.Log("newPlayer" + conn.identity.gameObject.name);
        conn.identity.gameObject.GetComponent<MetaWorldLoadingUI>().OnEnterMetaWorld?.Invoke();
    }

}
