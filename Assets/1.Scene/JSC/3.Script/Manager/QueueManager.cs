using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//
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
    public Queue<PlayerInfo> WaitingQueue = new Queue<PlayerInfo>(); //��⿭

    List<PlayerInfo> roomPlayerList1 = new List<PlayerInfo>();
    List<PlayerInfo> roomPlayerList2 = new List<PlayerInfo>();
    //List<int> _roomPlayerList3 = new List<int>();
    
    List<int> exitPlayerList = new List<int>(); //���� �÷��̾� ���

    [SerializeField]
    private int peopleLimitCount = 1; //�� �濡 �� �� �ִ� �ο�
    
    public List<GameObject> PlayerPetList; //Ž���� ������ ���

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

        //Ŭ���̾�Ʈ ���������� �̺�Ʈ
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
                //SelectedPetIndex�� ���� �÷��̾� ������ ����
                CmdEnterPlayer(CrewSelectManager.Instance.SelectedCrewIndex);
        }
    }
    //�÷��̾ ������ �������� �� ó��
    [Command(requiresAuthority = false)]
    public void CmdEnterPlayer(int selectedPetIndex, NetworkConnectionToClient playerId = null)
    {
        var playerInfo = new PlayerInfo(playerId, PlayerPetList[selectedPetIndex], "NoNamed");
        
        //��⿭�� ����� ���ٸ� �ٷ� �濡 ��
        if (WaitingQueue.Count <= 0)
        {
            //���ο����� �����ο����� ���ٸ� ���������� �̵�
            if (roomPlayerList1.Count < peopleLimitCount)
            {
                //�÷��̾� �߰�
                roomPlayerList1.Add(playerInfo);
                //�÷��̾� ��
                SwitchPlayerPrefeb(playerId, PlayerPetList[selectedPetIndex], PlayerSpawnPos[0].position);
            }
/*            else if (_roomPlayerList2.Count < _peopleLimitCount)
            {
                _roomPlayerList2.Add(playerInfo);
                SwitchPlayerPrefeb(playerId, PlayerPetList[selectedPetIndex], PlayerSpawnPos[1].position);
            }*/
            /*            else if (_roomPlayerList3.Count < _peopleLimitCount)
                        {
                            _roomPlayerList3.Add(playerId.connectionId);
                        }*/
            //��� ���� �� á�ٸ� ��⿭�� �߰�
            else
            {
                WaitingQueue.Enqueue(playerInfo);
                //�÷��̾� ��⿭�� �ִ� �̺�Ʈ ȣ��
                playerId.identity.gameObject.GetComponent<MetaWorldLoadingUI>().OnQueueMetaWorld?.Invoke();
            }
        }
        //��⿭�� ����� �ִٸ�
        else
        {
            WaitingQueue.Enqueue(playerInfo);
            //�÷��̾� ��⿭�� �ִ� �̺�Ʈ ȣ��
            playerId.identity.gameObject.GetComponent<MetaWorldLoadingUI>().OnQueueMetaWorld?.Invoke();
            Debug.Log($"{playerInfo.PlayerConnection}/{playerInfo.PlayerName}/{playerInfo.PlayerPrefeb.name} add Queue");
        }
        ViewPlayerList();
    }

    //�濡 �ִ� �÷��̾� ���� �޼���
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
    //�÷��̾� ���� �� �߻� �̺�Ʈ
    public void OnExitPlayer(NetworkConnectionToClient playerId)
    {
        //�濡 �ִ� ���� ����
        if (RemovePlayerInRoom(roomPlayerList1, playerId))
        {
            Debug.Log($"RemovePlayer in Room1 {playerId}");
            EnterRoomInQueue(roomPlayerList1, 0);
        }
/*        else if (RemovePlayerInRoom(_roomPlayerList2, playerId))
        { 
            Debug.Log($"RemovePlayer in Room2 {playerId}");
        }*/
        //��⿭�� �ִ� �÷��̾ ������ exitPlayerList�� ���
        else
        {
            exitPlayerList.Add(playerId.connectionId);
        }
        ViewPlayerList();
    }
    //���� �÷��̾����� Ȯ��
    public PlayerInfo CheckExitPlayer(PlayerInfo player)
    {
        //��⿭�� �ִ� �÷��̾ ���� �÷��̾ ��Ͽ� ������ ���� ����ο� 
        while (exitPlayerList.Contains(player.PlayerConnection.connectionId))
        {
            Debug.Log("Aleady Exit Player");
            if (WaitingQueue.Count > 0)
            {
                player = WaitingQueue.Dequeue();
                exitPlayerList.Remove(player.PlayerConnection.connectionId);
            }
            //��⿭�� ����� ������
            else
            {
                Debug.Log("Queue is Empty");
                return null;
            }
        }
        return player;
    }
    //��⿭���� ������ ���� ����
    void EnterRoomInQueue(List<PlayerInfo> roomPlayerList, int roomNumber)
    {
        //��⿭�� �÷��̾ �ִٸ� �濡 �÷��̾� �ֱ�
        if (WaitingQueue.Count > 0)
        {
            var player = WaitingQueue.Dequeue();
            //��⿭���� �÷��̾ �������� ������ �÷��̾� ����
            player = CheckExitPlayer(player);
            if(player == null) return;
            //���ο����� �����ο����� ���ٸ� ���������� �̵�
            Transform playerSpawnPos = transform;
            if (roomPlayerList.Count < peopleLimitCount)
            {
                //�÷��̾� �߰�
                roomPlayerList.Add(player);
                playerSpawnPos = PlayerSpawnPos[roomNumber];
            }
            else
            {
                Debug.Log("Unbelievable error");
            }
            foreach (var playerList in NetworkServer.connections)
            {
                //�濡 �� �÷��̾� ������ ����
                if (player.PlayerConnection.connectionId == playerList.Value.connectionId)
                {
                    //�÷��̾� ������ ��ü
                    if (NetworkManager.singleton is CrewSwitchNetworkManager manager)
                    {
                        SwitchPlayerPrefeb(playerList.Value, player.PlayerPrefeb, playerSpawnPos.position);
                    }
                }
            }
            Debug.Log($"_waitPlayer Add {player}");
        }
    }
    //�÷��̾� ������ ��ü��û
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
        /*        for (int i = 0; i < _roomPlayerList3.Count; i++)
                {
                    Debug.Log($"RoomPlayerList3 : RoomNumber/{i} PlayerNumber/{_roomPlayerList3[i]}");
                }*/

    }
    //���� ���� �� ��������
    private void OnApplicationQuit()
    {
        NetworkClient.Disconnect();
    }
}
