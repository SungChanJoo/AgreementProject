using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Mirror; 

public class MetaWorldLoadingUI : NetworkBehaviour
{
    public GameObject LoadingUI;
    public GameObject QueueUI;
    public Text QueueCountText;
    public Text QueueTimeText;

    public Action OnEnterMetaWorld;
    public Action OnQueueMetaWorld;

    private void Awake()
    {
        if (QueueUI.activeSelf) QueueUI.SetActive(false);

        OnEnterMetaWorld = () =>
        {
            //StartCoroutine(LoadMetaWorld_co());
        };
        //대기열 상태이면 대기열 카운트다운, 카운트가 0되면 메타월드 불러오기 
        OnQueueMetaWorld = () =>
        {
            StartCoroutine(UpdateQueueCount_co());
        };
    }
/*    public override void OnStartLocalPlayer()
    {
        LoadingUI.SetActive(true);
    }*/

/*    //로딩 딜레이
    IEnumerator LoadMetaWorld_co()
    {
        yield return new WaitForSeconds(2f);
        RpcSyncLoadingUI();
    }

    //입장시 로딩화면 비활성화
    [ClientRpc]
    public void RpcSyncLoadingUI()
    {
        LoadingUI.SetActive(false);
    }*/


    IEnumerator currentSeTimeQueue = null;
    //대기 화면 시간 갱신
    IEnumerator SetTimeQueue_co(int count)
    {
        int time = 0;
        while (count>0)
        {
            yield return new WaitForSeconds(1f);
            time++;
            QueueTimeText.text = $"대기 시간 : {time}";
        }
        currentSeTimeQueue = null;
    }
    //대기 화면 인원수, 시간 동기화
    [ClientRpc]
    public void RpcSyncQueueUI(int count)
    {
        if (!isLocalPlayer) return;
        //시간을 누적한다.
        if(currentSeTimeQueue == null)
        {
            currentSeTimeQueue = SetTimeQueue_co(count);
            StartCoroutine(currentSeTimeQueue);
        }
        //대기열에 사람이 있으면 UI 활성화
        if (count > 0)
        {
            Debug.Log($"count {count}");
            if (!QueueUI.activeSelf) QueueUI.SetActive(true);
            QueueCountText.text = $"남은 인원 : {count}";
        }
        else QueueUI.SetActive(false);

    }
    
    IEnumerator UpdateQueueCount_co(NetworkConnectionToClient playerId = null)
    {
        while (QueueManager.Instance.WaitingQueue.Count > 0)
        {
            var currentQueue = QueueManager.Instance.WaitingQueue;
            int i = 1;
            //현재 대기열에 플레이어를 찾아서 앞에 남은 인원 체크
            foreach(var playerInfo in currentQueue)
            {
                //대기열에 있는 플레이어가 나가지 않았다면
                if (playerInfo.PlayerConnection.identity != null)
                {
                    playerInfo.PlayerConnection.identity.gameObject.GetComponent<MetaWorldLoadingUI>().RpcSyncQueueUI(i);
                    i++;
                }

            }
            yield return new WaitForSeconds(1f);
        }
    }
}
