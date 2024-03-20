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

    // 서버-클라이언트 string으로 data 주고받을때 구분하기 위한 문자열
    private const string separatorString = "E|";

    // 서버 일일타이머(자정) 계산하기 위한 변수
    private DateTime nextMidnight;
    private WaitForSeconds waitUntilMidnight;
    // 서버 한 주를 체크하기 위한 카운트
    private DateTime standardDay;

    // 기타 데이터 처리용 Handler
    private ETCMethodHandler etcMethodHandler = new ETCMethodHandler();

    // Rank용 Timer 변수, 5분마다 실시간 갱신(DB데이터 불러와서) 그 후 클라이언트한테 쏴줘야함(UDP) todo
    private float rankTime = 300f;

    // Debug용
    private float debugTime = 5f;
    private float debugTimer;

    // Test용 bool
    private bool testBool = true;

    // 서버는 한번만 실행
    private void Start()
    {
        Debug.Log("[Server] Server start callback function");
        ServerCreate();
        //Invoke("TimerSet", 5f);
    }

    private void Update()
    {
        if (!isServerStarted) return;
        //Debug.Log("Client가 들어오지 않으면 실행이 되지 않을것");
        //DayTimer();
        if (clients.Count == 0) return;

        CheckClientsState();

        //if (testBool)
        //{   
        //    Debug.Log("[Server] TestCreatDBTest");
        //    testBool = false;
        //    Debug.Log($"[Server] TestCreatDBTest, testBool value : {testBool}");
        //    Invoke("CreateDBTest", 5f);
        //}
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
        ReceiveRequestFromClient(client);

        StartListening(); // 클라이언트 받고 다시 실행    
    }

    // 클라이언트로부터 요청받음
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

                List<string> receivedRequestData_List = new List<string>();

                while (true)
                {
                    // 클라이언트로부터 요청메세지(이름)을 받음
                    //byte[] buffer = new byte[1024]; // 일반적으로 받는 버퍼사이즈 1024byte
                    byte[] buffer = new byte[327680];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 메세지 받을때까지 대기
                    string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 데이터 변환

                    receivedRequestData_List.Add(receivedRequestData);

                    // 클라이언트로부터 데이터 전송이 끝나면(Finish) break;
                    List<string> endCheck = receivedRequestData.Split('|').ToList();
                    if (endCheck.Contains("Finish"))
                    {
                        //Debug.Log($"[Server] Received Finish Data from Client : {receivedRequestData}");
                        // receivedRequestData에 Finish가 있는 경우 Finish를 제거
                        etcMethodHandler.RemoveFinish(receivedRequestData_List, endCheck);

                        Debug.Log($"[Server] Finish Receive Data From Client");
                        break;
                    }
                    //if (receivedRequestData.Contains("Finish"))
                    //{
                    //    etcMethodHandler.RemoveFinish(dataList);
                    //    Debug.Log("[Server] Received data contains Finish from client");
                    //    break;
                    //}
                }

                // 클라이언트에게 온 요청을 처리해서 클라이언트에게 회신
                if (receivedRequestData_List.Count > 0) HandleRequestData(stream, receivedRequestData_List);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"[Server] Error in ReceiveRequestFromClient Method : {e.Message}");
        }

    }

    // 클라이언트로부터 받은 요청을 제목에 맞게 분류해서 처리함
    private void HandleRequestData(NetworkStream stream, List<string> dataList)
    {
        Debug.Log($"[Server] Received request name from client : {dataList[0]}");

        // 클라이언트로부터 온 데이터 하나의 string으로 통합
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // E| 분할
        List<string> filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();

        Debug.Log($"[Server] HandleRequestData filterList.Count : {filterList.Count}");

        for(int i = 0; i <filterList.Count; i ++)
        {
            //Debug.Log($"[Server] Received Data From Client to filterList[{i}] : {filterList[i]}");
        }

        //string requestName = $"[Save]CharactorData";
        //requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|{separatorString}";

        // filterList[0] '|' 분할 -> requestName, clientLicenseNumber, clientCharactor을 얻기 위해서
        List<string> baseDataList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        string requestName = baseDataList[0];
        int clientLicenseNumber = 0;
        int clientCharactor = 0;

        if(baseDataList.ElementAtOrDefault(1) != null && baseDataList.ElementAtOrDefault(2) != null) // 일반적으로 clientLicenseNumber와 clientCharactor는 같이 들어옴
        {
            clientLicenseNumber = Int32.Parse(baseDataList[1]);
            clientCharactor = Int32.Parse(baseDataList[2]);
        }
        else if(baseDataList.ElementAtOrDefault(1) != null) // UserData, clientLicenseNumber만 있을 때
        {
            clientLicenseNumber = Int32.Parse(baseDataList[1]);
        }

        // Reply -> Client에게 보낼 List<string>, [0]은 requestName
        List<string> replyRequestData_List = new List<string>();
        replyRequestData_List.Add($"{requestName}|");

        // TempAllocate -> DB에서 반환하는 List<string> 임시로 담기
        List<string> tempAllocate = new List<string>();

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                // 클라이언트가 LicenseNumber를 요청하는건 처음 접속하기때문에 LicenseNumber가 없는것
                // 따라서 DB에 연결해 LicenseNumber가 몇개있는지 확인(Count)하고 클라이언트에게 LicenseNumber 부여

                // 새 라이센스 발급 (유저(플레이어), 로컬에 저장되는 한 개의 라이센스)
                Debug.Log($"[Server] Creating... User_LicenseNumber");
                string clientdata = DBManager.instance.CreateLicenseNumber(); 
                clientLicenseNumber = Int32.Parse(clientdata.Split('|')[0]);

                // 새 캐릭터 정보 생성 (유저 한명당 가지는 첫 캐릭터)
                Debug.Log($"[Server] Creating... new Charactor Data");
                DBManager.instance.CreateNewCharactorData(clientLicenseNumber, 1); 

                Debug.Log($"[Server] Finish Create LicenseNumber and new CharactorData");
                replyRequestData_List.Add($"{clientdata}");
                break;
            case "[Create]Charactor":
                string newClientCharactor = DBManager.instance.CreateNewCharactorData(clientLicenseNumber, clientCharactor);
                Debug.Log($"[Server] Finish Create new CharactorData");
                //replyRequestData_List.Add($"{newClientCharactor}|");
                break;
            case "[Save]CharactorName":
                DBManager.instance.SaveCharactorName(filterList);
                break;
            case "[Save]CharactorProfile":
                DBManager.instance.SaveCharactorProfile(filterList);
                break;
            case "[Save]CharactorBirthday":
                DBManager.instance.SaveCharactorBirthday(filterList);
                break;
            case "[Save]CharactorData":
                DBManager.instance.SaveCharactorData(filterList);
                break;
            case "[Save]ExpenditionCrew":
                DBManager.instance.SaveCrewData(filterList);
                break;
            case "[Save]LastPlayData":
                DBManager.instance.SaveLastPlayData(filterList);
                break;
            case "[Save]GameResult":
                break;
            case "[Save]venezia_kor":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Save]venezia_eng":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Save]venezia_chn":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Save]gugudan":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Save]calculation":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Load]UserData":
                tempAllocate = DBManager.instance.LoadUserData(clientLicenseNumber);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]CharactorData":
                // dataList[1] = user_LicenseNumber, dataList[2] = user_Charactor
                tempAllocate = DBManager.instance.LoadCharactorData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data)); // TempList의 각 요소(data = string value) ReplyList에 추가
                break;
            case "[Load]AnalyticsData":
                tempAllocate = DBManager.instance.LoadAnalyticsData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]RankData":
                tempAllocate = DBManager.instance.LoadRankData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]ExpenditionCrew":
                tempAllocate = DBManager.instance.LoadExpenditionCrew(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]LastPlayData":
                tempAllocate = DBManager.instance.LoadLastPlayData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]AnalyticsProfileData":
                tempAllocate = DBManager.instance.LoadAnalyticsProfileData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Change]Charactor":
                tempAllocate = DBManager.instance.ChangeCharactor(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Delete]Charactor":
                DBManager.instance.DeleteCharactor(clientLicenseNumber, clientCharactor);
                break;
            case "[Reset]CharactorProfile":
                DBManager.instance.ResetCharactorProfile(clientLicenseNumber, clientCharactor);
                break;
            case "[Test]CreateDB":
                DBManager.instance.CreateDateDB();
                break;
            case "[Test]UpdateWeeklyRankDB":
                DBManager.instance.UpdateWeeklyRankDB();
                break;
            default:
                Debug.Log($"[Server] Handling error that request from client, request name : {requestName}");
                break;
        }

        for (int i = 0; i < replyRequestData_List.Count; i++)
        {
            byte[] data = Encoding.UTF8.GetBytes(replyRequestData_List[i]); // string -> byte[] 데이터 형식 변환
            stream.Write(data, 0, data.Length);
            //Debug.Log("[Server] Replying... request data to client");
        }

        byte[] finishData = Encoding.UTF8.GetBytes("Finish");
        stream.Write(finishData, 0, finishData.Length);
        Debug.Log("[Server] Finish reply request data to client");
    }

    // Timer 세팅
    private void TimerSet()
    {
        try
        {
            // 서버를 실행 한 시점부터 다음 자정까지의 시간 계산
            nextMidnight = DateTime.Today.AddDays(1);
            TimeSpan timeUntilMidnight = nextMidnight - DateTime.Now;

            // Test용
            DateTime testAfterOneMinute = DateTime.Now.AddSeconds(20);
            timeUntilMidnight = testAfterOneMinute - DateTime.Now;

            // 자정까지 대기할 WaitforSeconds 설정
            waitUntilMidnight = new WaitForSeconds((float)timeUntilMidnight.TotalSeconds);

            // DB에 저장되어있는 weeklyrank 갱신용 시작 기준일
            // 그 갱신일로부터 일주일이 지나면 weeklyrankDB table update
            standardDay = DBManager.instance.LoadStandardDay();

            TimeSpan timeSpan = DateTime.Now.Date - standardDay.Date;

            Debug.Log($"[Server] standardDay : {standardDay}");
            Debug.Log($"[Server] standardDay.Date : {standardDay.Date}");
            Debug.Log($"[Server] DateTime.Now : {DateTime.Now}");
            Debug.Log($"[Server] DateTime.Now.Date : {DateTime.Now.Date}");
            Debug.Log($"[Server] timeSpan : {timeSpan}");
            Debug.Log($"[Server] timeSpan.Days : {timeSpan.Days}");

            if(timeSpan.Days == 7)
            {

            }

            // 43200
            Debug.Log($"[Server] Waited time until next midnight, timeUntilMidnight : {timeUntilMidnight.TotalSeconds} ");

            // 자정 체크 실행
            StartCoroutine(CheckMidnight_Co());
        }
        catch(Exception e)
        {
            Debug.Log($"[Server] Can't execute timerset method, {e.Message}");
            // 실행이 안되면 5초후에 다시 실행
            Invoke("TimerSet", 5f);
        }
    }

    // 하루가 지났을 때 PresentDB에 있는 gamedata들 새 DateDB(ex) 24.02.21)에 저장
    private IEnumerator CheckMidnight_Co()
    {
        while(true)
        {
            Debug.Log($"[Server] Come in CheckMidnight Coroutine");
            yield return waitUntilMidnight;

            Debug.Log("[Server] A day has passed. Start creating new DateDB for save data");

            // 자정이 되면 DateDB 생성
            DBManager.instance.CreateDateDB();

            Debug.Log("[Server] Complete Create DateDB");

            // 매주 월요일이 되면 weeklyrankDB에 한주간 rank table 생성
            TimeSpan timeSpan = DateTime.Now.Date - standardDay.Date;

            if (timeSpan.Days == 2)
            {
                Debug.Log("[Server] Start Update WeeklyRankDB");
                DBManager.instance.UpdateWeeklyRankDB();
                Debug.Log("[Server] Complete Update WeeklyRankDB");
            }

            // 자정이 지나고 나서 DateDB를 생성한 후 자정 체크 시간 갱신
            // CreateDateDB()를 수행하는데 시간이 어느정도 걸리므로, 몇초의 오차가 있을 수 있다.
            nextMidnight = DateTime.Today.AddDays(1);
            TimeSpan timeUntilNextMidnight = nextMidnight - DateTime.Now;
            waitUntilMidnight = new WaitForSeconds((float)timeUntilNextMidnight.TotalSeconds);

            Debug.Log($"[Server] Waited time until next midnight, timeUntilNextMidnight : {timeUntilNextMidnight.TotalSeconds} ");
        }
    }

    // 연결된 클라이언트 확인
    private void CheckClientsState()
    {
        debugTimer += Time.deltaTime;

        while (debugTimer > debugTime)
        {
            Debug.Log($"[Server] Present Connected Clients : {clients.Count}");

            foreach (TcpClient client in clients)
            {
                Debug.Log($"[Server] Connected Clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            }

            //DayTimer();

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

    // 하루가 지났을 때 PresentDB에 있는 gamedata들 새 DB(ex) 24.02.21)에 저장
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

    private void CreateDBTest()
    {
        //DBManager.instance.CreateDaysExGameDataDB();
    }

    private void SendMessageToClients(TcpClient client)
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