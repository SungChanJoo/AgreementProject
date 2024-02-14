using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;
using Mirror.Examples.Basic;

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
    Queue<PlayerInfo> _waitingQueue = new Queue<PlayerInfo>(); //��⿭

    List<PlayerInfo> _roomPlayerList1 = new List<PlayerInfo>();
    List<PlayerInfo> _roomPlayerList2 = new List<PlayerInfo>();
    //List<int> _roomPlayerList3 = new List<int>();
    
    List<int> _exitPlayerList = new List<int>(); //���� �÷��̾� ���

    private int _peopleLimitCount = 1; //�� �濡 �� �� �ִ� �ο�
    public GameObject NonePlayer;
    
    public List<GameObject> PlayerPetList;
    public int SelectedPetIndex = 0;

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
            if (SelectPetManager.Instance != null)
                CmdEnterPlayer(SelectPetManager.Instance.SelectedPetIndex);
        }
    }
    //�÷��̾ ������ �������� �� ó��
    [Command(requiresAuthority = false)]
    public void CmdEnterPlayer(int selectedPetIndex, NetworkConnectionToClient playerId = null)
    {
        var playerInfo = new PlayerInfo(playerId, PlayerPetList[selectedPetIndex], "NoNamed");
        
        //��⿭�� ����� ���ٸ� �ٷ� �濡 ��
        if (_waitingQueue.Count <= 0)
        {
            //���ο����� �����ο����� ���ٸ� ���������� �̵�
            if (_roomPlayerList1.Count < _peopleLimitCount)
            {
                //�÷��̾� �߰�
                _roomPlayerList1.Add(playerInfo);
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
                _waitingQueue.Enqueue(playerInfo);
                playerId.identity.gameObject.GetComponent<MetaWorldLoadingUI>().OnQueueMetaWorld?.Invoke(_waitingQueue.Count);
            }
        }
        //��⿭�� ����� �ִٸ�
        else
        {
            _waitingQueue.Enqueue(playerInfo);
            playerId.identity.gameObject.GetComponent<MetaWorldLoadingUI>().OnQueueMetaWorld?.Invoke(_waitingQueue.Count);

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
    //�濡 �ִ� �÷��̾����� �޼���
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
        //�濡 �ִ� ���� ����
        if (RemovePlayerInRoom(_roomPlayerList1, playerId))
        {
            Debug.Log($"RemovePlayer in Room1 {playerId}");
        }
/*        else if (RemovePlayerInRoom(_roomPlayerList2, playerId))
        { 
            Debug.Log($"RemovePlayer in Room2 {playerId}");
        }*/
        //��⿭�� �ִ� �÷��̾ ������ exitPlayerList�� ���
        else
        {
            _exitPlayerList.Add(playerId.connectionId);
        }

        //��⿭�� �÷��̾ �ִٸ� �濡 �÷��̾� �ֱ�
        if (_waitingQueue.Count >0)
        {
            var player = _waitingQueue.Dequeue();
            //��⿭���� �÷��̾ �������� ������ �÷��̾� ����
            while(_exitPlayerList.Contains(player.PlayerConnection.connectionId))
            {
                Debug.Log("Aleady Exit Player");
                if(_waitingQueue.Count>0)
                {
                    player = _waitingQueue.Dequeue();
                    playerId.identity.gameObject.GetComponent<MetaWorldLoadingUI>().OnQueueMetaWorld?.Invoke(_waitingQueue.Count);
                    
                }
                //��⿭�� ����� ������
                else
                {
                    Debug.Log("Queue is Empty");
                    return;
                }
            }
            //���ο����� �����ο����� ���ٸ� ���������� �̵�
            Transform playerSpawnPos = transform;
            if (_roomPlayerList1.Count < _peopleLimitCount)
            {
                //�÷��̾� �߰�
                _roomPlayerList1.Add(player);
                playerSpawnPos = PlayerSpawnPos[0];
            }
/*            else if (_roomPlayerList2.Count < _peopleLimitCount)
            {
                _roomPlayerList2.Add(player);
                playerSpawnPos = PlayerSpawnPos[1];
            }*/
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
                    if (NetworkManager.singleton is PetSwitchNetworkManager manager)
                    {
                        SwitchPlayerPrefeb(playerList.Value, player.PlayerPrefeb, playerSpawnPos.position);
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
    
    private void SwitchPlayerPrefeb(NetworkConnectionToClient playerId, GameObject playerPrefeb, Vector3 SpawnPos)
    {
        if (NetworkManager.singleton is PetSwitchNetworkManager manager)
        {
            manager.ReplacePlayer(playerId, playerPrefeb, SpawnPos);
            //�÷��̾� �ε�ȭ�� ������ �̺�Ʈ ȣ��
        }
    }
}
