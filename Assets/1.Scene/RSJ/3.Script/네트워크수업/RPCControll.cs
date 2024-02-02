using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class RPCControll : NetworkBehaviour
{
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private TMP_InputField inputfield;
    [SerializeField] private GameObject canvas;

    private static event Action<string> onMessage;

    //client가 server에 connect되었을때 콜백함수
    public override void OnStartAuthority()
    {
        if(isLocalPlayer)
        {
            canvas.SetActive(true);
        }
        onMessage += newMessage;

    }

    private void newMessage(string message)
    {
        chatText.text += message;
    }

    //클라이언트가 Server를 나갔을 때
    [ClientCallback]
    private void OnDestroy()
    {
        if(!isLocalPlayer)
        {
            onMessage -= newMessage;
        }
    }

    //RPC는 결국 ClientRpc 명령어 < Command(Server)명령어 < Client 명령어?
    //클라이언트가 서버에게 메세지를 요청하면 서버가 모든 클라이언트들에게 그 메세지를 보내버린다.
    //통제를 하냐?(명령) -> Rpc, 물어보고 실행하냐?(질문) -> 다른 네트워크?, 클라이언트가 요청하고 서버가 동기화해줌 // 이 차이라서 Rpc가 더 빠름
    //서버에 있는 스크립트와 클라이언트가 갖고있는 스크립트가 같아야만 실행이 된다,. 서버와 클라이언트의 스크립트가 다를경우 새로 빌드해야함

    [Client] // 본인 클라이언트 입장
    private void Send()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) return;
        if (string.IsNullOrWhiteSpace(inputfield.text)) return;
        cmdSendMessage(inputfield.text); //
        inputfield.text = string.Empty;
    }

    [Command] // 서버에서 다른 클라이언트들에게 보내는 명령 (서버입장)
    private void cmdSendMessage(string message)
    {
        RpcHandleMessage($"[{connectionToClient.connectionId}] : {message}");
        //클라이언트Rpc를 붙이지 않으면 메서드를 쓸 수 없다.
        // transform.position += 1; -> 가능
        // Debug(); -> 불가능
    }

    /* 예시로 ClientRpc를 붙이지 않은 Debug라는 메서드
     private void Debug()
    {
        Debug.Log(e.MEsagee);
    }
     */

    [ClientRpc] //이 메소드를 rpc로 쓸겁니다. 그냥 하나의 메서드임. 그냥 메서드인데 rpc로 쓴다
    private void RpcHandleMessage(string message) // string뿐만 아니라 rigidbody, transform 등 어떤 형식이든 가능하다.
    {
        onMessage?.Invoke($"\n{message}");
    }


}
