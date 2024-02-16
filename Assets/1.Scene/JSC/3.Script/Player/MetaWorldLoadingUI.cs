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
            StartCoroutine(LoadMetaWorld_co());
        };
        //��⿭ �����̸� ��⿭ ī��Ʈ�ٿ�, ī��Ʈ�� 0�Ǹ� ��Ÿ���� �ҷ����� 
        OnQueueMetaWorld = () =>
        {
            StartCoroutine(UpdateQueueCount_co());
        };
    }
    public override void OnStartLocalPlayer()
    {
        LoadingUI.SetActive(true);
    }

    IEnumerator LoadMetaWorld_co()
    {
        //�ε� �ð�
        yield return new WaitForSeconds(2f);
        RpcSyncLoadingUI();
    }
    [ClientRpc]
    public void RpcSyncLoadingUI()
    {
        Debug.Log("LoadingUI False : " + gameObject.name);
        LoadingUI.SetActive(false);
    }

    //��⿭ ����
    // 1. ��⿭�� �ɸ��� �ð��� ��������.
    // 2. Queueī��Ʈ�� 0�̵Ǹ� UI�� ����.
    // 3. Queueī��Ʈ�� �ٲ𶧸��� ȣ��
    IEnumerator currentSeTimeQueue = null;
    IEnumerator SetTimeQueue_co(int count)
    {
        int time = 0;
        //RpcSyncQueueUI(count);
        while (count>0)
        {
            yield return new WaitForSeconds(1f);
            time++;
            QueueTimeText.text = $"��� �ð� : {time}";
        }
        currentSeTimeQueue = null;
    }
    [ClientRpc]
    public void RpcSyncQueueUI(int count)
    {
        if (!isLocalPlayer) return;
        //�ð��� �����Ѵ�.
        if(currentSeTimeQueue == null)
        {
            currentSeTimeQueue = SetTimeQueue_co(count);
            StartCoroutine(currentSeTimeQueue);
        }
        //��⿭�� ����� ������ UI Ȱ��ȭ
        if (count > 0)
        {
            Debug.Log($"count {count}");
            if (!QueueUI.activeSelf) QueueUI.SetActive(true);
            QueueCountText.text = $"���� �ο� : {count}";
        }
        else QueueUI.SetActive(false);

    }
    
    IEnumerator UpdateQueueCount_co(NetworkConnectionToClient playerId = null)
    {
        Debug.Log("UpdateQueueCount_co");

        Debug.Log($"{QueueManager.Instance.WaitingQueue.Count}");
        //Debug.Log($"{playerId.connectionId}");
        while (QueueManager.Instance.WaitingQueue.Count > 0)
        {
            var currentQueue = QueueManager.Instance.WaitingQueue;
            int i = 1;
            //���� ��⿭�� �÷��̾ ã�Ƽ� �տ� ���� �ο� üũ
            foreach(var playerInfo in currentQueue)
            {
                if (playerInfo.PlayerConnection.identity != null)
                {
                    Debug.Log("?");
                    playerInfo.PlayerConnection.identity.gameObject.GetComponent<MetaWorldLoadingUI>().RpcSyncQueueUI(i);
                    i++;
                }

            }
            yield return new WaitForSeconds(1f);
        }
    }
}
