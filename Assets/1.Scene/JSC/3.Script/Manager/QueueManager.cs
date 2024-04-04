using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//탐험대원 = 플레이어 정보를 담고 있는 클래스
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
//대기열 관리 클래스
public class QueueManager : NetworkBehaviour
{
    public static QueueManager Instance = null;
    public Queue<PlayerInfo> WaitingQueue = new Queue<PlayerInfo>(); //대기열

    List<PlayerInfo> roomPlayerList1 = new List<PlayerInfo>(); //1번방
    List<PlayerInfo> roomPlayerList2 = new List<PlayerInfo>(); //2번방
    List<PlayerInfo> roomPlayerList3 = new List<PlayerInfo>(); //3번방
    
    List<int> exitPlayerList = new List<int>(); //나간 플레이어 목록

    [SerializeField]
    private int peopleLimitCount = 4; //한 방에 들어갈 수 있는 최대인원
    
    public List<GameObject> PlayerCrewList; //탐험대원 프리펩 목록

    public List<Transform> PlayerSpawnPos = new List<Transform>();
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        //클라이언트 연결해제시 이벤트
        NetworkServer.OnDisconnectedEvent += (NetworkConnectionToClient) =>
        {
            OnExitPlayer(NetworkConnectionToClient);
        };
    }
    private void Start()
    {
        if (isClient)
        {
            if (CrewSelectManager.Instance != null)
                //SelectedPetIndex에 따라 플레이어 프리펩 변경
                CmdEnterPlayer(CrewSelectManager.Instance.SelectedCrewIndex);
        }
    }
    //플레이어가 서버에 접속했을 때 처리
    [Command(requiresAuthority = false)]
    public void CmdEnterPlayer(int selectedPetIndex, NetworkConnectionToClient playerId = null)
    {
        var playerInfo = new PlayerInfo(playerId, PlayerCrewList[selectedPetIndex], "NoNamed");
        
        //대기열에 사람이 없다면 바로 방에 들어감
        if (WaitingQueue.Count <= 0)
        {
            //방인원수가 제한인원보다 적다면 다음방으로 이동
            if (roomPlayerList1.Count < peopleLimitCount)
            {
                //플레이어 추가
                roomPlayerList1.Add(playerInfo);
                //플레이어 변경
                SwitchPlayerPrefeb(playerId, PlayerCrewList[selectedPetIndex], PlayerSpawnPos[0].position);
            }
            else if (roomPlayerList2.Count < peopleLimitCount)
            {
                roomPlayerList2.Add(playerInfo);
                SwitchPlayerPrefeb(playerId, PlayerCrewList[selectedPetIndex], PlayerSpawnPos[1].position);
            }
            else if (roomPlayerList3.Count < peopleLimitCount)
            {
                roomPlayerList3.Add(playerInfo);
                SwitchPlayerPrefeb(playerId, PlayerCrewList[selectedPetIndex], PlayerSpawnPos[2].position);
            }
            //모든 방이 다 찼다면 대기열에 추가
            else
            {
                WaitingQueue.Enqueue(playerInfo);
                //플레이어 대기열에 넣는 이벤트 호출
                playerId.identity.gameObject.GetComponent<MetaWorldLoadingUI>().OnQueueMetaWorld?.Invoke();
            }
        }
        //대기열에 사람이 있다면
        else
        {
            WaitingQueue.Enqueue(playerInfo);
            //플레이어 대기열에 넣는 이벤트 호출
            playerId.identity.gameObject.GetComponent<MetaWorldLoadingUI>().OnQueueMetaWorld?.Invoke();
            Debug.Log($"{playerInfo.PlayerConnection}/{playerInfo.PlayerName}/{playerInfo.PlayerPrefeb.name} add Queue");
        }
        ViewPlayerList();
    }

    //방에 있는 플레이어 제거 메서드
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
    //플레이어 나갈 때 발생 이벤트
    public void OnExitPlayer(NetworkConnectionToClient playerId)
    {
        //방에 있는 유저 제거
        if (RemovePlayerInRoom(roomPlayerList1, playerId))
        {
            Debug.Log($"RemovePlayer in Room1 {playerId}");
            EnterRoomInQueue(roomPlayerList1, 0);
        }
        else if (RemovePlayerInRoom(roomPlayerList2, playerId))
        {
            Debug.Log($"RemovePlayer in Room2 {playerId}");
            EnterRoomInQueue(roomPlayerList2, 1);
        }
        else if (RemovePlayerInRoom(roomPlayerList3, playerId))
        {
            EnterRoomInQueue(roomPlayerList3, 3);
            Debug.Log($"RemovePlayer in Room3 {playerId}");
        }
        //대기열에 있는 플레이어가 나가면 exitPlayerList에 등록
        else
        {
            exitPlayerList.Add(playerId.connectionId);
        }
        ViewPlayerList();
    }
    //나간 플레이어인지 확인
    public PlayerInfo CheckExitPlayer(PlayerInfo player)
    {
        //대기열에 있는 플레이어가 나간 플레이어가 목록에 있으면 다음 대기인원 
        while (exitPlayerList.Contains(player.PlayerConnection.connectionId))
        {
            Debug.Log("Aleady Exit Player");
            if (WaitingQueue.Count > 0)
            {
                player = WaitingQueue.Dequeue();
                exitPlayerList.Remove(player.PlayerConnection.connectionId);
            }
            //대기열에 사람이 없을때
            else
            {
                Debug.Log("Queue is Empty");
                return null;
            }
        }
        return player;
    }
    //대기열에서 방으로 들어가는 로직
    void EnterRoomInQueue(List<PlayerInfo> roomPlayerList, int roomNumber)
    {
        //대기열에 플레이어가 있다면 방에 플레이어 넣기
        if (WaitingQueue.Count > 0)
        {
            var player = WaitingQueue.Dequeue();
            //대기열에서 플레이어가 나갔으면 다음번 플레이어 접속
            player = CheckExitPlayer(player);
            if(player == null) return;
            //방인원수가 제한인원보다 적다면 다음방으로 이동
            Transform playerSpawnPos = transform;
            if (roomPlayerList.Count < peopleLimitCount)
            {
                //플레이어 추가
                roomPlayerList.Add(player);
                playerSpawnPos = PlayerSpawnPos[roomNumber];
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
                    if (NetworkManager.singleton is CrewSwitchNetworkManager manager)
                    {
                        SwitchPlayerPrefeb(playerList.Value, player.PlayerPrefeb, playerSpawnPos.position);
                    }
                }
            }
            Debug.Log($"_waitPlayer Add {player}");
        }
    }
    //플레이어 프리펩 교체요청
    private void SwitchPlayerPrefeb(NetworkConnectionToClient playerId, GameObject playerPrefeb, Vector3 SpawnPos)
    {
        if (NetworkManager.singleton is CrewSwitchNetworkManager manager)
        {
            manager.ReplacePlayer(playerId, playerPrefeb, SpawnPos);
        }
    }
    
    public void ViewPlayerList()
    {
        for (int i = 0; i < roomPlayerList1.Count; i++)
        {
            Debug.Log($"RoomPlayerList1 : RoomNumber/{i} PlayerNumber/{roomPlayerList1[i].PlayerConnection.address}");
        }
        for (int i = 0; i < roomPlayerList2.Count; i++)
        {
            Debug.Log($"RoomPlayerList2 : RoomNumber/{i} PlayerNumber/{roomPlayerList2[i].PlayerConnection.address}");
        }
        for (int i = 0; i < roomPlayerList3.Count; i++)
        {
            Debug.Log($"RoomPlayerList3 : RoomNumber/{i}PlayerNumber/{roomPlayerList3[i].PlayerConnection.address}");
        }

    }
    //어플 종료 시 연결해제
    private void OnApplicationQuit()
    {
        NetworkClient.Disconnect();
    }
}
