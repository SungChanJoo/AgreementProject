using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

public class Server : MonoBehaviour
{
    private const int port = 2421;
                      
    // 클라이언트 여러명 받을 수 있도록 리스트
    private List<TcpClient> clients;
    private List<TcpClient> disconnectList;

    private TcpListener server;
    private bool isServerStarted;

    // 클라이언트에게 라이센스번호를 부여하기위한 변수
    private int clientLicenseNumber;

    // Rank용 Timer 변수, 5분마다 실시간 갱신(DB데이터 불러와서) 그 후 클라이언트한테 쏴줘야함(UDP) todo
    private float rankTime = 300f;

    // Debug용
    private float debugTime = 5f;
    private float debugTimer;

    // 서버는 한번만 실행
    private void Start()
    {
        Debug.Log("[Server] Server start callback function");
        ServerCreate();
    }

    private void Update()
    {
        if (!isServerStarted) return;
        //Debug.Log("Client가 들어오지 않으면 실행이 되지 않을것");
        //DayTimer();
        if (clients.Count == 0) return;

        CheckClientsState();
        ReceiveDataFromClients();
        
    }

    // 서버 생성
    public void ServerCreate()
    {
        clients = new List<TcpClient>();
        disconnectList = new List<TcpClient>();

        try
        {
            Debug.Log("[Server] Try server create");
            server = new TcpListener(IPAddress.Any, port);
            server.Start(); // 서버 시작
            Debug.Log("[Server] server.Start()");

            StartListening();
            isServerStarted = true;
        }
        catch(Exception e)
        {
            Debug.Log($"[Server] Server-Client Connect Error! : {e.Message}");
        }
    }

    // 비동기로 클라이언트 연결요청 받기 시작
    private async void StartListening()
    {
        Debug.Log("[Server] Server is listening until client is comming");
        // 비동기 클라이언트 연결 / await 키워드는 비동기 호출이 완료될 때까지 대기
        TcpClient client = await server.AcceptTcpClientAsync();
        Debug.Log("[Server] Asynchronously server accept client's request");

        // 클라이언트가 연결되면 다음 메서드 호출
        HandleClient(client);
    }

    // 재귀 메서드, 클라이언트 연결되면 다시 연결요청 받음
    private void HandleClient(TcpClient client)
    {
        Debug.Log("[Server] Client connected");
        clients.Add(client);

        for (int i = 0; i < clients.Count; i++)
        {
            Debug.Log($"[Server] Connected client's IP : {((IPEndPoint)clients[i].Client.RemoteEndPoint).Address}");
        }

        // 연결된 클라이언트 -> 데이터받을준비
        //ReceiveDataFromClient(client);
        ReceiveRequestFromClient(client);

        StartListening(); // 클라이언트 받고 다시 실행    
    }

    private void CheckClientsState()
    {
        debugTimer += Time.deltaTime;

        while(debugTimer > debugTime)
        {
            Debug.Log($"[Server] Present Connected Clients : {clients.Count}");
            
            foreach(TcpClient client in clients)
            {
                Debug.Log($"[Server] Connected Clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            }

            DayTimer();

            debugTimer = 0f;
        }

        foreach (TcpClient client in clients)
        {
            if (!IsConnected(client))
            {
                Debug.Log("[Server] Client connection is closed");
                Debug.Log($"[Server] Disconnected clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                client.Close();
                disconnectList.Add(client);
                Debug.Log($"[Server] After client.Close(), Test check client.Connected bool value : {client.Connected}");
                clients.Remove(client);
                continue;
            }
            #region etc
            //// 클라이언트 연결이 끊어졌다면
            //if (!CheckConnectState(client))
            //{
            //    Debug.Log($"Disconnected client's IP {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            //    client.Close();
            //    disconnectList.Add(client);
            //    continue;
            //}
            // 클라이언트로부터 체크 메시지 받기
            //else
            //{

            //    //NetworkStream stream = client.tcp.GetStream();
            //    //if(stream.DataAvailable)
            //    //{
            //    //    string data = new StreamReader(stream, true).ReadLine();

            //    //    if(data != null)
            //    //    {
            //    //        On
            //    //    }
            //    //}
            //}
            #endregion
        }
    }

    private async void ReceiveRequestFromClient(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                // 연결이 끊겼으면 break
                if (!IsConnected(client))
                {
                    Debug.Log("[Server] Client connection is closed");
                    break;
                }

                // 클라이언트로부터 요청메세지(이름)을 받음
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 메세지 받을때까지 대기
                string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 데이터 변환
                List<string> dataList = receivedRequestData.Split('|').ToList(); // 받은 data 분할해서 list에 담음
                string receivedRequestName = dataList[0]; // dataList[0]은 클라이언트가 요청한 주제, 게임 등의 이름
                Debug.Log($"[Server] Received request name from client : {receivedRequestName}");

                // 클라이언트로부터 요청 받은 제목에 대한 내용을 처리해서 클라이언트에게 보내줘야함
                if(receivedRequestName == "[Load]PlayerData")
                {
                    HandleRequestDataForLoad(stream, dataList);
                }
                else
                {
                    HandleRequestData(stream, dataList);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"[Server] Data Communicate error : {e.Message}");
        }

    }

    // 클라이언트로부터 받은 요청을 제목에 맞게 분류해서 처리함
    private void HandleRequestData(NetworkStream stream, List<string> dataList)
    {
        // dataList -> 0 : requestName, 1~ : values
        string requestName = dataList[0];
        string replyRequestMessage = "";
        Debug.Log($"[Server] HandleRequestData requestName : {dataList[0]}");
        

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                // 클라이언트가 LicenseNumber를 요청하는건 처음 접속하기때문에 LicenseNumber가 없는것
                // 따라서 DB에 연결해 LicenseNumber가 몇개있는지 확인(Count)하고 클라이언트에게 LicenseNumber 부여
                Debug.Log($"[Server] Creating... User_LicenseNumber");
                string clientdata = DBManager.instance.CreateLicenseNumber(); // 새 라이센스 발급
                List<string> clientdata_List = clientdata.Split('|').ToList();
                clientLicenseNumber = Int32.Parse(clientdata_List[0]);
                Debug.Log($"[Server] Creating... new PlayerData");
                DBManager.instance.CreateNewPlayerData(clientLicenseNumber); // 새 플레이어 정보 생성
                Debug.Log($"[Server] Finish Create LicenseNumber and new PlayerData");
                replyRequestMessage =  "[Create]LicenseNumber|" + clientdata;
                break;
            case "[Create]Charactor":
                break;
            case "[Save]venezia_kor":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            case "[Save]venezia_eng":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            case "[Save]venezia_chn":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            case "[Save]gugudan":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            case "[Save]calculation":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            default:
                Debug.Log($"[Server] Handling error that request from client, request name : {requestName}");
                break;
        }

        byte[] data = Encoding.UTF8.GetBytes(replyRequestMessage); // 데이터 변환
        stream.Write(data, 0, data.Length); // 메세지 보냄
        Debug.Log($"[Server] Reply request message to client");

    }

    private void HandleRequestDataForLoad(NetworkStream stream, List<string> dataList)
    {
        Debug.Log($"[Server] receivedRequestName : {dataList[0]}");

        int newClientLicenseNumber = Int32.Parse(dataList[1]);
        List<string> replyMassiveRequestMessage = DBManager.instance.LoadPlayerData(ClientLoginStatus.New, newClientLicenseNumber); // 새 플레이어 정보 불러오기

        for (int i = 0; i < replyMassiveRequestMessage.Count; i++)
        {
            byte[] data = Encoding.UTF8.GetBytes(replyMassiveRequestMessage[i]);
            stream.Write(data, 0, data.Length);
            Debug.Log("[Server] Replying... massive request message to client");
        }

        byte[] finishData = Encoding.UTF8.GetBytes("Finish");
        stream.Write(finishData, 0, finishData.Length);
        Debug.Log("[Server] End reply massive request message to client");
    }

    // 하루가 지났을 때 PresentDB에 있는 gamedata들 새 DB(ex) 24-02-21)에 저장
    private void DayTimer()
    {
        // 현재 시간
        DateTime currentTime = DateTime.Now;
        // 초기화가 되는 시간 (23시 59분 55초)
        DateTime criterionTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 11, 50, 55);

        // TimeSpan, 현재시간과 초기화시간 차이
        TimeSpan timeDiff = criterionTime - currentTime;

        // uint 양수만 사용
        uint diffHours = (uint)(timeDiff.Hours >= 0? timeDiff.Hours : 0);
        uint diffMinutes = (uint)(timeDiff.Minutes >= 0? timeDiff.Minutes : 0);
        uint diffSeconds = (uint)(timeDiff.Seconds >= 0? timeDiff.Seconds : 0);

        // 현재 시간이 초기화 되는 시간보다 크다면, timeDiff가 음수가 아니고, 0이 아니면
        if (diffHours == 0 && diffMinutes == 0 && diffSeconds > 0 && diffSeconds <= 5)
        {
            // 새 DB 생성 및 저장 / presentDB gamedata초기화
            Debug.Log("[Server] A day has passed. Create new DB for save data and Reset presentDB some datas");

        }

        Debug.Log($"[Server] currenTime = {currentTime}");
        Debug.Log($"[Server] criterionTime = {criterionTime}");
        Debug.Log($"[Server] timeDiff = {timeDiff}");

    }

    // 일주일이 지났을 때 PresentDB에 있는 rankTable data들 새 DB(주간 랭킹 출력용)? or 어떤 DB에 저장
    private void WeekTimer()
    {

    }

    private void SendMessageToClients(TcpClient client)
    {

    }

    private void ReceiveDataFromClients()
    {

    }

    private bool IsConnected(TcpClient client)
    {
        // 해석 필요
        bool isConnCheck = !((client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0)) || !client.Client.Connected);
        //bool isConnCheck = client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0) && client.Client.Connected;

        return isConnCheck;
    }

    // 서버 종료
    private void OnApplicationQuit()
    {
        server.Stop();
    }


    
}