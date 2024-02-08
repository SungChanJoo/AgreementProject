using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PetSwitchNetworkManager : NetworkManager
{
    private int _PlayerCount = 0;
    public void ReplacePlayer(NetworkConnectionToClient conn, GameObject newPrefab, Vector3 spawnPos)
    {

        GameObject oldPlayer = conn.identity.gameObject;
        NetworkServer.ReplacePlayerForConnection(conn, Instantiate(newPrefab, spawnPos, Quaternion.identity), true);
        Destroy(oldPlayer, 0.1f);
    }
}
