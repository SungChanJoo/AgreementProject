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
        //��⿭ �����̸� ��⿭ ī��Ʈ�ٿ�, ī��Ʈ�� 0�Ǹ� ��Ÿ���� �ҷ����� 
        OnQueueMetaWorld = () =>
        {
            StartCoroutine(UpdateQueueCount_co());
        };
    }
/*    public override void OnStartLocalPlayer()
    {
        LoadingUI.SetActive(true);
    }*/

/*    //�ε� ������
    IEnumerator LoadMetaWorld_co()
    {
        yield return new WaitForSeconds(2f);
        RpcSyncLoadingUI();
    }

    //����� �ε�ȭ�� ��Ȱ��ȭ
    [ClientRpc]
    public void RpcSyncLoadingUI()
    {
        LoadingUI.SetActive(false);
    }*/


    IEnumerator currentSeTimeQueue = null;
    //��� ȭ�� �ð� ����
    IEnumerator SetTimeQueue_co(int count)
    {
        int time = 0;
        while (count>0)
        {
            yield return new WaitForSeconds(1f);
            time++;
            QueueTimeText.text = $"��� �ð� : {time}";
        }
        currentSeTimeQueue = null;
    }
    //��� ȭ�� �ο���, �ð� ����ȭ
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
        while (QueueManager.Instance.WaitingQueue.Count > 0)
        {
            var currentQueue = QueueManager.Instance.WaitingQueue;
            int i = 1;
            //���� ��⿭�� �÷��̾ ã�Ƽ� �տ� ���� �ο� üũ
            foreach(var playerInfo in currentQueue)
            {
                //��⿭�� �ִ� �÷��̾ ������ �ʾҴٸ�
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
