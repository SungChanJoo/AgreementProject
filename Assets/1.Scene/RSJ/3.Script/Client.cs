using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using LitJson;

// 처음 접속하는 유저인지(First), 접속했었던 유저인지(Continue) 
public enum ClientLoginStatus
{
    First,
    Continue
}

public class Client : MonoBehaviour
{
    // IP, Port 고정됨
    [SerializeField] private string server_IP = "15.165.159.141"; // aws EC2 IP : 15.165.159.141
    [SerializeField] private int server_Port = 2421;

    bool socketReady;
    private TcpClient client;
    private NetworkStream stream;

    // login - license
    public static ClientLoginStatus loginStatus;
    public static string clientLicenseNumber;
    public string licensePath = string.Empty;

    // Login
    public InputField login_ID_Input;
    public InputField login_PW_Input;
    public Text loginLog;

    // Create Account
    public InputField create_ID_Input;
    public InputField create_PW_Input;

    private void Awake()
    {
        
    }

    private void Start()
    {
        ConnectToServer();
        ClientLoginSet();
    }

    // 클라이언트가 실행할 때 서버 연결 시도
    public void ConnectToServer()
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

            stream = client.GetStream();// 연결에 성공하면 stream도 계속 연결할 수 있도록
        }
        catch (Exception e)
        {
            Debug.Log($"Fail Connect to Server : {e.Message}");
        }

    }

    public void ClientLoginSet()
    {
        // 연결되지 않았다면 return
        if (!client.Connected) return;

        licensePath = Application.dataPath + "/License";

        Debug.Log($"File.Exists(licensePath) value ? {File.Exists(licensePath)}");
        // 경로에 파일이 존재하지 않는다면 라이센스넘버가 없다는것이고, 처음 접속한다는 뜻
        if (!Directory.Exists(licensePath))
        {
            Directory.CreateDirectory(licensePath);
            loginStatus = ClientLoginStatus.First;
            // 서버에서 라이센스 넘버를 받아와야함, 그러기 위해 서버에 요청 todo
            string requestName = "LicenseNumber";
            RequestToServer(requestName);
            Debug.Log($"[Client] Is Create licensenumber?");
            Debug.Log($"[Client] This client's licensenumber : {clientLicenseNumber}");
            return; // 처음 접속이라면 폴더 및 파일 저장하고 return
        }

        // 해당 경로에 있는 파일을 읽어 클라이언트 라이센스 넘버를 불러옴
        string jsonStringFromFile = File.ReadAllText(licensePath + "/clientlicense.json");
        JsonData licenseNumber_JsonFile = JsonMapper.ToObject(jsonStringFromFile);
        clientLicenseNumber = licenseNumber_JsonFile["LicenseNumber"].ToString();
        Debug.Log($"[Client] Use created licensenumber?");
        Debug.Log($"[Client] This client's licensenumber : {clientLicenseNumber}");
    }

    //// 버튼 눌러서 로그인
    //public void OnClickLogin()
    //{
    //    // id, password를 입력하지 않았으면 return;
    //    if (login_ID_Input.text == null || login_PW_Input.text == null)
    //    {
    //        loginLog.text = "아이디와 비밀번호를 입력하세요";
    //        return;
    //    }

    //    // id, password를 DB에 있는 User_Name과 User_Password 비교
    //    // id_Input.text == DBManager.instance.user_Info.user_Name && password_Input.text == DBManager.instance.user_Info.user_Password
    //    if (DBManager.instance.Login(login_ID_Input.text, login_PW_Input.text))
    //    {
    //        loginLog.text = "로그인에 성공했습니다.";

    //        // todo.. 씬이동하던지, 재화 불러오던지 등등 할거 

    //        User_Info user = DBManager.instance.user_Info;
    //        Debug.Log(user.user_Name + "|" + user.user_Password);
    //    }
    //    else // 로그인 실패
    //    {
    //        loginLog.text = "아이디 또는 비밀번호를 확인해주세요";
    //    }
    //}

    //// 버튼 눌러서 계정생성
    //public void OnClickCreateAccount()
    //{
    //    DBManager.instance.CreateAccount(create_ID_Input.text, create_PW_Input.text);
    //}

    // 버튼 눌러서 서버에 메시지 보내기
    public void OnCilckSendMessage()
    {
        SendMessageToServer();
    }

    private void SendMessageToServer()
    {
        string sendMessage = "Hello World";
        try
        {
            
            byte[] data = Encoding.UTF8.GetBytes(sendMessage);
            stream.Write(data, 0, data.Length); // 데이터를 보낼때 까지 대기? 그냥 보내면 되잖어
            Debug.Log($"Sent message to server : {sendMessage}");

            ReceiveMessageFromServer(stream); // 메서드가 실행될때 까지 대기 / 대기시키기 위해 메서드 앞에 await을 붙여서 실행시키려면 메서드가 Task 붙여야하는듯?
        }
        catch (Exception e)
        {
            Debug.Log($"Fail sending message to sever : + {e.Message}");
        }
    }

    private async void ReceiveMessageFromServer(NetworkStream stream)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 데이터를 읽어올때까지 대기
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 데이터 변환
            Debug.Log($"Received message from server : {receivedMessage}");
        }
        catch(Exception e)
        {
            Debug.Log($"Error receiving message from server : {e.Message}");
            Debug.LogError($"Error receiving message from server : {e.Message}");
        }

    }

    // 서버에 요청할때 string으로 보내는데, 서버에서 받을 때 string case로 구분해서 처리
    private void RequestToServer(string requestName)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(requestName);
            stream.Write(data, 0, data.Length); // 데이터를 보낼때 까지 대기? 그냥 보내면 되잖어
            Debug.Log($"Request to server : {requestName}");

            ReceiveRequestFromServer(stream, requestName); // 메서드가 실행될때 까지 대기 / 대기시키기 위해 메서드 앞에 await을 붙여서 실행시키려면 메서드가 Task 붙여야하는듯?
        }
        catch (Exception e)
        {
            Debug.Log($"Fail Request to sever : + {e.Message}");
        }
    }

    // 서버에 요청보낸거 받음, 받은 string, case로 구분해서 처리
    private async void ReceiveRequestFromServer(NetworkStream stream, string requestName)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 데이터를 읽어올때까지 대기
            string receivedRequestMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 데이터 변환
            Debug.Log($"Received request message from server : {receivedRequestMessage}");
            HandleRequestMessage(requestName, receivedRequestMessage);
        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving message from server : {e.Message}");
        }
    }

    // 서버로부터 받은 메세지 처리
    private void HandleRequestMessage(string requestname, string requestmessage)
    {
        switch(requestname)
        {
            case "LicenseNumber":
                clientLicenseNumber = requestmessage;
                SaveLicenseNumberToJsonFile();
                break;
            default:
                Debug.Log("HandleRequestMessage Method Something Happend");
                break;
        }
    }

    // Json파일에 LicenseNumber 등록 / 비동기로 호출된 것이 끝났을때 동기적으로 호출시키기 위해
    private void SaveLicenseNumberToJsonFile()
    {
        // JsonData 생성
        JsonData licenseNumber_Json = new JsonData();
        licenseNumber_Json["LicenseNumber"] = clientLicenseNumber;
        // Json 데이터를 문자열로 변환하여 파일에 저장
        string jsonString = JsonMapper.ToJson(licenseNumber_Json);
        File.WriteAllText(licensePath + "/clientlicense.json", jsonString);
    }

    private void OnDestroy()
    {
        CloseSocket();
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    public void OnClickCheckCloseSocket()
    {
        CloseSocket();
    }


    private void CloseSocket()
    {
        Debug.Log("Close Socket으로 들어오는가");
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
