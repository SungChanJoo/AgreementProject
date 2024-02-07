using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Client : MonoBehaviour
{
    // IP, Port 고정됨
    [SerializeField] private string server_IP = "15.165.159.141";
    [SerializeField] private int server_Port = 2421;

    bool socketReady;
    private TcpClient client;

    // Login
    public InputField login_ID_Input;
    public InputField login_PW_Input;
    public Text loginLog;

    // Create Account
    public InputField create_ID_Input;
    public InputField create_PW_Input;

    // 클라이언트가 실행할 때 서버 연결 시도
    private void Start()
    {
        ConnectedToServer();
    }

    public void ConnectedToServer()
    {
        // 이미 연결되었다면 함수 무시
        if (socketReady) return;

        //// 기본 호스트 / 포트번호
        //string ip = IPInput.text == "" ? "127.0.0.1" : IPInput.text; // aws EC2 IP : 15.165.159.141
        //int port = PortInput.text == "" ? 2421 : int.Parse(PortInput.text);

        // 서버에 연결
        try
        {
            client = new TcpClient();
            client.Connect(server_IP, server_Port);
            Debug.Log("Success Connect to Server!");
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log($"Fail Connect to Server : {e.Message}");
        }

    }

    // 버튼 눌러서 로그인
    public void OnClickLogin()
    {
        // id, password를 입력하지 않았으면 return;
        if (login_ID_Input.text == null || login_PW_Input.text == null)
        {
            loginLog.text = "아이디와 비밀번호를 입력하세요";
            return;
        }

        // id, password를 DB에 있는 User_Name과 User_Password 비교
        // id_Input.text == DBManager.instance.user_Info.user_Name && password_Input.text == DBManager.instance.user_Info.user_Password
        if (DBManager.instance.Login(login_ID_Input.text, login_PW_Input.text))
        {
            loginLog.text = "로그인에 성공했습니다.";

            // todo.. 씬이동하던지, 재화 불러오던지 등등 할거 

            User_Info user = DBManager.instance.user_Info;
            Debug.Log(user.user_Name + "|" + user.user_Password);
        }
        else // 로그인 실패
        {
            loginLog.text = "아이디 또는 비밀번호를 확인해주세요";
        }
    }

    // 버튼 눌러서 계정생성
    public void OnClickCreateAccount()
    {
        DBManager.instance.CreateAccount(create_ID_Input.text, create_PW_Input.text);
    }

    // 버튼 눌러서 서버에 메시지 보내기
    public void OnCilckSendMessage()
    {
        string hello = "Hello World";
        try
        {

            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(hello);
            stream.Write(data, 0, data.Length);
            Debug.Log($"Sent message to server : {hello}");
        }
        catch (Exception e)
        {
            Debug.Log($"Fail sending message to sever : + {e.Message}");
        }
    }

    //private void OnDestroy()
    //{
    //    if(client != null)
    //    {
    //        client.Close();
    //    }
    //}

    private void OnApplicationQuit()
    {
        CloseSocket();
    }


    private void CloseSocket()
    {
        if (!socketReady) return;

        //writer.Close();
        //reader.Close();
        client.Close();
        socketReady = false;
    }

    #region chatgpt
    //private TcpClient client;
    //private NetworkStream stream;
    //public Text clientLog;

    //// 플레이어가 앱을 실행시킬 때 DB에서 데이터를 가져와야함
    //// 가져와야 하는 데이터. 많음. 
    //// 서버에 접속하기 위해 필요한 IP는 License로 관리해? 스크립트?
    //// 어쨋든 빌드해도 스크립트상에 남길 수 있으니. 보안 신경써야하나?
    //// need Fix

    //public void StartClient()
    //{
    //    client = new TcpClient();
    //    client.BeginConnect("127.0.0.1", 8888, HandleClientConnect, null);
    //}

    //private void HandleClientConnect(IAsyncResult result)
    //{
    //    client.EndConnect(result);
    //    stream = client.GetStream();

    //    Debug.Log("Connected to server!");
    //    clientLog.text += "\nConnected to server!";

    //    // 서버로부터 데이터 받기 시작
    //    byte[] receiveBuffer = new byte[1024];
    //    stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleDataReceived, receiveBuffer);

    //    // 예시 : 서버에 패킷 보내기
    //    Packet packetToSend = new Packet("Player2", 150);
    //    SendPacket(packetToSend);
    //}

    //private void HandleDataReceived(IAsyncResult result)
    //{
    //    int bytesRead = stream.EndRead(result);

    //    if(bytesRead > 0)
    //    {
    //        byte[] receivedData = new byte[bytesRead];
    //        Array.Copy((result.AsyncState as byte[]), receivedData, bytesRead);

    //        // 예시 : 받은 패킷 역직렬화
    //        Packet receivedPacket = PacketFromBytes(receivedData);
    //        Debug.Log("Received data from server: " + receivedPacket.playerName + " - " + receivedPacket.playerScore);

    //        // Handle the received data as needed

    //        // Continue listening for more data
    //        byte[] newBuffer = new byte[1024];
    //        stream.BeginRead(newBuffer, 0, newBuffer.Length, HandleDataReceived, newBuffer);
    //    }
    //}

    //private void SendPacket(Packet packet)
    //{
    //    byte[] data = PacketToBytes(packet);
    //    stream.Write(data, 0, data.Length);
    //}

    //private byte[] PacketToBytes(Packet packet)
    //{
    //    string jsonData = JsonUtility.ToJson(packet);
    //    return Encoding.ASCII.GetBytes(jsonData);
    //}

    //private Packet PacketFromBytes(byte[] data)
    //{
    //    string jsonData = Encoding.ASCII.GetString(data);
    //    return JsonUtility.FromJson<Packet>(jsonData);
    //}

    //// need Fix
    //private void OnDestroy()
    //{
    //    if(client != null)
    //    {
    //        client.Close();
    //    }
    //}
    #endregion
}
