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
    public Action<int> OnQueueMetaWorld;

    private void Awake()
    {
        if (QueueUI.activeSelf) QueueUI.SetActive(false);

        OnEnterMetaWorld = () =>
        {
            StartCoroutine(LoadMetaWorld_co());
        };
        //대기열 상태이면 대기열 카운트다운, 카운트가 0되면 메타월드 불러오기 
        OnQueueMetaWorld = (Count) =>
        {
            RpcSyncQueueUI(Count);
        };
    }
    public override void OnStartLocalPlayer()
    {
        LoadingUI.SetActive(true);
    }

    IEnumerator LoadMetaWorld_co()
    {
        //로딩 시간
        yield return new WaitForSeconds(2f);
        RpcSyncLoadingUI();
    }
    [ClientRpc]
    public void RpcSyncLoadingUI()
    {
        Debug.Log("LoadingUI False : " + gameObject.name);
        LoadingUI.SetActive(false);
    }

    //대기열 로직
    // 1. 대기열에 걸리면 시간이 지나간다.
    // 2. Queue카운트가 0이되면 UI를 끈다.
    // 3. Queue카운트가 바뀔때마다 호출
    IEnumerator currentSeTimeQueue = null;
    IEnumerator SetTimeQueue_co(int count)
    {
        int time = 0;
        RpcSyncQueueUI(count);
        while (count>0)
        {
            yield return new WaitForSeconds(1f);
            time++;
            QueueTimeText.text = $"{time}";
        }
        currentSeTimeQueue = null;
    }
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
            if (!QueueUI.activeSelf) QueueUI.SetActive(true);

            QueueCountText.text = $"{count}";
        }
        else QueueUI.SetActive(false);

    }
}
