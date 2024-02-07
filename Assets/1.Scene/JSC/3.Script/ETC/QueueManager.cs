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
    
    //플레이어가 서버에 접속했을 때 처리
    public void OnEnterPlayer(NetworkConnectionToClient playerId)
    {
        //대기열에 사람이 없다면 바로 방에 들어감
        if (_waitingQueue.Count <= 0)
        {
            //방인원수가 제한인원보다 적다면 다음방으로 이동
            if (_roomPlayerList1.Count < _peopleLimitCount)
            {
                //플레이어 추가
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
            //모든 방이 다 찼다면 대기열에 추가
            else
            {
                _waitingQueue.Enqueue(playerId.connectionId);
            }
        }
        //대기열에 사람이 있다면
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
        //방에 있는 유저 제거
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
        //대기열 상태일 때 플레이어가 나가면 나간플레이어목록에 등록
        else
        {
            _exitPlayerList.Add(playerId.connectionId);
        }

        //대기열에 사람이 있다면 방에 사람 넣기
        if(_waitingQueue.Count >0)
        {
            int player = _waitingQueue.Dequeue();
            //대기열에서 플레이어가 나갔으면 다음번 플레이어 접속
            while(_exitPlayerList.Contains(player))
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
            if (_roomPlayerList1.Count < _peopleLimitCount)
            {
                //플레이어 추가
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
