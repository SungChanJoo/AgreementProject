using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;

public class PlayerInfo
{
    public NetworkConnectionToClient PlayerConnection;
    public GameObject PlayerPrefeb;
    public string PlayerName;

    public PlayerInfo(NetworkConnectionToClient Connection, GameObject Prefeb , string Name)
    {
        PlayerConnection = Connection;
        PlayerPrefeb = Prefeb;
        PlayerName = Name;
    }
}
public class QueueManager : NetworkBehaviour
{
    public static QueueManager Instance = null;
    Queue<PlayerInfo> _waitingQueue = new Queue<PlayerInfo>();
    
    List<PlayerInfo> _roomPlayerList1 = new List<PlayerInfo>();
    List<PlayerInfo> _roomPlayerList2 = new List<PlayerInfo>();
    //List<int> _roomPlayerList3 = new List<int>();
    List<int> _exitPlayerList = new List<int>();
    private int _peopleLimitCount = 1;
    public GameObject NonePlayer;
    public List<GameObject> TestPetPrefebs;
    public List<Transform> PlayerSpawnPos = new List<Transform>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = new QueueManager();
        }
        else
        {
            Destroy(gameObject);
        }
        NetworkServer.OnConnectedEvent += (NetworkConnectionToClient) =>
        {
            OnEnterPlayer(NetworkConnectionToClient);
        };
        NetworkServer.OnDisconnectedEvent += (NetworkConnectionToClient) =>
        {
            OnExitPlayer(NetworkConnectionToClient);
        };
    }


    //플레이어가 서버에 접속했을 때 처리
    public void OnEnterPlayer(NetworkConnectionToClient playerId)
    {
        var playerInfo = new PlayerInfo(playerId, NonePlayer, "NoNamed");

        //대기열에 사람이 없다면 바로 방에 들어감
        if (_waitingQueue.Count <= 0)
        {
            //방인원수가 제한인원보다 적다면 다음방으로 이동
            if (_roomPlayerList1.Count < _peopleLimitCount)
            {
                //플레이어 추가
                _roomPlayerList1.Add(playerInfo);
                StartCoroutine(SwitchPlayerPrefeb_co(playerId, TestPetPrefebs[0], PlayerSpawnPos[0].position));
            }
            else if (_roomPlayerList2.Count < _peopleLimitCount)
            {
                _roomPlayerList2.Add(playerInfo);
                StartCoroutine(SwitchPlayerPrefeb_co(playerId, TestPetPrefebs[1], PlayerSpawnPos[1].position));
            }
            /*            else if (_roomPlayerList3.Count < _peopleLimitCount)
                        {
                            _roomPlayerList3.Add(playerId.connectionId);
                        }*/
            //모든 방이 다 찼다면 대기열에 추가
            else
            {
                _waitingQueue.Enqueue(playerInfo);
            }
        }
        //대기열에 사람이 있다면
        else
        {
            _waitingQueue.Enqueue(playerInfo);
            Debug.Log($"{playerInfo.PlayerConnection}/{playerInfo.PlayerName}/{playerInfo.PlayerPrefeb.name} add Queue");
        }
        ViewPlayerList();

    }
    public void ViewPlayerList()
    {
        for(int i = 0; i < _roomPlayerList1.Count;i ++)
        {
            Debug.Log($"RoomPlayerList1 : RoomNumber/{i} PlayerNumber/{_roomPlayerList1[i].PlayerConnection.address}");
        }
        for (int i = 0; i < _roomPlayerList2.Count; i++)
        {
            Debug.Log($"RoomPlayerList2 : RoomNumber/{i} PlayerNumber/{_roomPlayerList2[i].PlayerConnection.address}");
        }
/*        for (int i = 0; i < _roomPlayerList3.Count; i++)
        {
            Debug.Log($"RoomPlayerList3 : RoomNumber/{i} PlayerNumber/{_roomPlayerList3[i]}");
        }*/

    }
    public bool RemovePlayerInRoom(List<PlayerInfo> PlayerList, NetworkConnectionToClient playerId)
    {
        for (int i = 0; i < PlayerList.Count; i++)
        {
            if (PlayerList[i].PlayerConnection == playerId)
            {
                PlayerList.Remove(PlayerList[i]);
                return true;
            }
        }
        return false;
    }
    public void OnExitPlayer(NetworkConnectionToClient playerId)
    {
        //방에 있는 유저 제거
        if (RemovePlayerInRoom(_roomPlayerList1, playerId))
        {
            Debug.Log($"RemovePlayer in Room1 {playerId}");
        }
        else if (RemovePlayerInRoom(_roomPlayerList2, playerId))
        { 
            Debug.Log($"RemovePlayer in Room2 {playerId}");
        }
        //대기열에 있는 플레이어가 나가면 exitPlayerList에 등록
        else
        {
            _exitPlayerList.Add(playerId.connectionId);
        }

        //대기열에 플레이어가 있다면 방에 플레이어 넣기
        if (_waitingQueue.Count >0)
        {
            var player = _waitingQueue.Dequeue();
            //대기열에서 플레이어가 나갔으면 다음번 플레이어 접속
            while(_exitPlayerList.Contains(player.PlayerConnection.connectionId))
            {
                Debug.Log("Aleady Exit Player");
                if(_waitingQueue.Count>0)
                {
                    player = _waitingQueue.Dequeue();
                }
                //대기열에 사람이 없을때
                else
                {
                    Debug.Log("Queue is Empty");
                    return;
                }
            }
            //방인원수가 제한인원보다 적다면 다음방으로 이동
            Transform playerSpawnPos = transform;
            if (_roomPlayerList1.Count < _peopleLimitCount)
            {
                //플레이어 추가
                _roomPlayerList1.Add(player);
                playerSpawnPos = PlayerSpawnPos[0];
            }
            else if (_roomPlayerList2.Count < _peopleLimitCount)
            {
                _roomPlayerList2.Add(player);
                playerSpawnPos = PlayerSpawnPos[1];
            }
            else
            {
                Debug.Log("Unbelievable error");
            }
            foreach (var playerList in NetworkServer.connections)
            {
                //방에 들어간 플레이어 프리펩 변경
                if (player.PlayerConnection.connectionId == playerList.Value.connectionId)
                {
                    //플레이어 프리펩 교체
                    if (NetworkManager.singleton is PetSwitchNetworkManager manager)
                    {
                        StartCoroutine(SwitchPlayerPrefeb_co(playerList.Value, player.PlayerPrefeb, playerSpawnPos.position));
                    }
                }
            }
            /*            else if (_roomPlayerList3.Count < _peopleLimitCount)
                        {
                            _roomPlayerList3.Add(player);
                        }*/

            Debug.Log($"_waitPlayer Add {player}");
        }
        ViewPlayerList();
    }
    
    IEnumerator SwitchPlayerPrefeb_co(NetworkConnectionToClient playerId, GameObject playerPrefeb, Vector3 SpawnPos)
    {
        yield return new WaitForSeconds(1.4f);
        if (NetworkManager.singleton is PetSwitchNetworkManager manager)
        {
            manager.ReplacePlayer(playerId, playerPrefeb, SpawnPos);
        }
    }
}
