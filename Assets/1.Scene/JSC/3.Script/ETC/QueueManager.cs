using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;

public class QueueManager : NetworkBehaviour
{
    public static QueueManager Instance = null;
    Queue<int> _waitingQueue = new Queue<int>();
    
    List<int> _roomPlayerList1 = new List<int>();
    List<int> _roomPlayerList2 = new List<int>();
    //List<int> _roomPlayerList3 = new List<int>();
    List<int> _exitPlayerList = new List<int>();
    private int _peopleLimitCount = 1;

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
    
    //�÷��̾ ������ �������� �� ó��
    public void OnEnterPlayer(NetworkConnectionToClient playerId)
    {
        //��⿭�� ����� ���ٸ� �ٷ� �濡 ��
        if (_waitingQueue.Count <= 0)
        {
            //���ο����� �����ο����� ���ٸ� ���������� �̵�
            if (_roomPlayerList1.Count < _peopleLimitCount)
            {
                //�÷��̾� �߰�
                _roomPlayerList1.Add(playerId.connectionId);
            }
            else if (_roomPlayerList2.Count < _peopleLimitCount)
            {
                _roomPlayerList2.Add(playerId.connectionId);
            }
/*            else if (_roomPlayerList3.Count < _peopleLimitCount)
            {
                _roomPlayerList3.Add(playerId.connectionId);
            }*/
            //��� ���� �� á�ٸ� ��⿭�� �߰�
            else
            {
                _waitingQueue.Enqueue(playerId.connectionId);
            }
        }
        //��⿭�� ����� �ִٸ�
        else
        {
            _waitingQueue.Enqueue(playerId.connectionId);
            Debug.Log($"{playerId.connectionId} add Queue");
        }
        ViewPlayerList();

    }
    public void ViewPlayerList()
    {
        for(int i = 0; i < _roomPlayerList1.Count;i ++)
        {
            Debug.Log($"RoomPlayerList1 : RoomNumber/{i} PlayerNumber/{_roomPlayerList1[i]}");
        }
        for (int i = 0; i < _roomPlayerList2.Count; i++)
        {
            Debug.Log($"RoomPlayerList2 : RoomNumber/{i} PlayerNumber/{_roomPlayerList2[i]}");
        }
/*        for (int i = 0; i < _roomPlayerList3.Count; i++)
        {
            Debug.Log($"RoomPlayerList3 : RoomNumber/{i} PlayerNumber/{_roomPlayerList3[i]}");
        }*/

    }
    public void OnExitPlayer(NetworkConnectionToClient playerId)
    {
        //�濡 �ִ� ���� ����
        if(_roomPlayerList1.Contains(playerId.connectionId))
        {
            _roomPlayerList1.Remove(playerId.connectionId);
        }
        else if (_roomPlayerList2.Contains(playerId.connectionId))
        {
            _roomPlayerList2.Remove(playerId.connectionId);
        }
/*        else if (_roomPlayerList3.Contains(playerId.connectionId))
        {
            _roomPlayerList3.Remove(playerId.connectionId);
        }*/
        //��⿭ ������ �� �÷��̾ ������ �����÷��̾��Ͽ� ���
        else
        {
            _exitPlayerList.Add(playerId.connectionId);
        }

        //��⿭�� ����� �ִٸ� �濡 ��� �ֱ�
        if(_waitingQueue.Count >0)
        {
            int player = _waitingQueue.Dequeue();
            //��⿭���� �÷��̾ �������� ������ �÷��̾� ����
            while(_exitPlayerList.Contains(player))
            {
                Debug.Log("Aleady Exit Player");
                if(_waitingQueue.Count>0)
                {
                    player = _waitingQueue.Dequeue();
                }
                //��⿭�� ����� ������
                else
                {
                    Debug.Log("Queue is Empty");
                    return;
                }
            }
            //���ο����� �����ο����� ���ٸ� ���������� �̵�
            if (_roomPlayerList1.Count < _peopleLimitCount)
            {
                //�÷��̾� �߰�
                _roomPlayerList1.Add(player);
            }
            else if (_roomPlayerList2.Count < _peopleLimitCount)
            {
                _roomPlayerList2.Add(player);
            }
/*            else if (_roomPlayerList3.Count < _peopleLimitCount)
            {
                _roomPlayerList3.Add(player);
            }*/
            else
            {
                Debug.Log("Unbelievable error");
            }
            Debug.Log($"_waitPlayer Add {player}");
        }
        ViewPlayerList();
    }
    
}
