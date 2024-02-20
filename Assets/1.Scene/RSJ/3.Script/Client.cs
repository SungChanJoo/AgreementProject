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
using System.Linq;

// 처음 접속하는 유저인지(First), 접속했었던 유저인지(Continue) 
public enum ClientLoginStatus
{
    New,
    Exist
}

public class Client : MonoBehaviour
{
    // IP, Port 고정됨
    [SerializeField] private string server_IP = "15.165.159.141"; // aws EC2 IP : 15.165.159.141
    [SerializeField] private int server_Port = 2421;

    bool socketReady;
    private TcpClient client;
    private static NetworkStream stream;

    // login - license
    public static ClientLoginStatus loginStatus;
    public static string clientLicenseNumber;
    public string licensePath = string.Empty;
    public string clientCharactor;

    // 서버로 부터 받은 data를 1차적으로 거른 List
    public List<string> playerdata_FromServer = new List<string>();

    // DB Table Name
    private TableName table;

    // Player data를 사용하기 위한 Dictionary
    public Dictionary<string, List<string>> playerdata_Dic;

    // 서버-클라이언트 string으로 data 주고받을때 구분하기 위한 문자열
    private const string separatorString = "E|";

    // timer
    private float transmissionTime = 1f;

    public Client(NetworkStream _stream)
    {
        stream = _stream;
    }

    public static Client instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ETCInitSetting();
        ConnectToServer();
        ClientLoginSet();
        Invoke("LoadPlayerDataFromDB", 5f);
    }

    // 클라이언트가 실행할 때 서버 연결 시도
    public void ConnectToServer()
    {
        // 이미 연결되었다면 함수 무시
        if (socketReady) return;

        // 서버에 연결
        try
        {
            client = new TcpClient();
            client.Connect(server_IP, server_Port);
            Debug.Log("[Client] Success Connect to Server!");
            socketReady = true;

            stream = client.GetStream();// 연결에 성공하면 stream도 계속 연결할 수 있도록
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Fail Connect to Server : {e.Message}");
        }

    }

    public void ClientLoginSet()
    {
        // 연결되지 않았다면 return
        if (!client.Connected) return;

        licensePath = Application.dataPath + "/License";

        Debug.Log($"[Client] Directory.Exists(licensePath) value ? {Directory.Exists(licensePath)}");
        // 경로에 파일이 존재하지 않는다면 라이센스넘버가 없다는것이고, 처음 접속한다는 뜻
        if (!Directory.Exists(licensePath))
        {
            loginStatus = ClientLoginStatus.New;
            Directory.CreateDirectory(licensePath);
            // 서버에서 라이센스 넘버를 받아와야함, 그러기 위해 서버에 요청 todo
            string requestName = "[Create]LicenseNumber";
            RequestToServer(requestName);
            Debug.Log($"[Client] Create licensenumber?");
            Debug.Log($"[Client] This client's licensenumber(first) : {clientLicenseNumber}");
            return; // 처음 접속이라면 폴더 및 파일 저장하고 return
        }

        loginStatus = ClientLoginStatus.Exist;
        // 해당 경로에 있는 파일을 읽어 클라이언트 라이센스 넘버를 불러옴
        string jsonStringFromFile = File.ReadAllText(licensePath + "/clientlicense.json");
        JsonData client_JsonFile = JsonMapper.ToObject(jsonStringFromFile);
        clientLicenseNumber = client_JsonFile["LicenseNumber"].ToString();
        clientCharactor = client_JsonFile["Charactor"].ToString();
        Debug.Log($"[Client] Use already existed licensenumber?");
        Debug.Log($"[Client] This client's licensenumber(existing) : {clientLicenseNumber}");
    }

    // 게임 시작할 때 유저정보 불러오기 - 서버에 요청(서버-DB) // 나중에 Player Class 정리되면 수정해야함. todo
    private void LoadPlayerDataFromDB()
    {
        Debug.Log("[Clinet] Request LoadPlayerDataFromDB");
        string requestData; // 이 메서드에서 requestData는 requestName/clientLicenseNumber/clientCharactor
        requestData = $"[Load]PlayerData|{clientLicenseNumber}|{clientCharactor}";
        RequestToServer(requestData);
    }

    // 서버에 요청할때 string으로 보내는데, 서버에서 받을 때 string case로 구분해서 처리
    private void RequestToServer(string requestData)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(requestData);
            stream.Write(data, 0, data.Length); // 데이터를 보낼때 까지 대기? 그냥 보내면 되잖어
            List<string> requestDataList = requestData.Split('|').ToList();
            string requestName = requestDataList[0];
            Debug.Log($"[Client] Request to server : {requestData}");

            // MassiveData인지 아닌지 초기에 플레이어 데이터를 받을때 많은 양의 데이터를 받아야해서
            // 따로 처리해야함
            if(requestName == "[Load]PlayerData")
            {
                ReceiveMassiveRequestFromServer(stream);
            }
            else
            {
                ReceiveRequestFromServer(stream, requestName); // 메서드가 실행될때 까지 대기 / 대기시키기 위해 메서드 앞에 await을 붙여서 실행시키려면 메서드가 Task 붙여야하는듯?
            }
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Fail Request to sever : + {e.Message}");
        }
    }

    // 서버에 요청보낸거 받음, 받은 string, case로 구분해서 처리
    private async void ReceiveRequestFromServer(NetworkStream stream, string requestName)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 데이터를 읽어올때까지 대기
            string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 데이터 변환
            Debug.Log($"[Client] Received request message from server : {receivedRequestData}");
            List<string> dataList = receivedRequestData.Split('|').ToList();
            HandleRequestData(dataList);
        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving message from server : {e.Message}");
        }
    }

    // 서버로부터 받은 데이터 처리
    private void HandleRequestData(List<string> dataList)
    {
        // requestname : 클라이언트-서버에서 데이터를 처리하기 위한 구분
        // requestdata : Server에서 requestName으로 처리된 결과를 보낸 data
        // LicenseNumber -> clientLicenseNumber를 server가 보내줌 (Client가 첫 접속인 경우 처리됨)
        // LoadNewPlayerData -> 새 플레이어의 경우 DB에 licenseNumber를 제외한 데이터가 없으므로 모든 테이블에 존재하는 열항목에 0값을 부여한 PlayerData를 가질 것임
        // LoadExistPlayerData -> 기존 플레이어의 경우 DB에 저장된 PlayerData를 가질 것임
        string requestName = dataList[0];
        Debug.Log($"[Client] HandleRequestData method, request name : {requestName}");

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                clientLicenseNumber = dataList[1];
                clientCharactor = dataList[2];
                SaveLicenseNumberToJsonFile();
                Debug.Log($"[Client] RequestName : LicenseNumber, get and save licenseNumber to jsonfile");
                break;
            //case "[Save]venezia_kor":
            //    break;
            //case "[Save]venezia_eng":
            //    break;
            //case "[Save]venezia_chn":
            //    Debug.Log($"[Client] RequestName : venezia_chn, End handling data");
            //    DBManager.instance.SaveGameResultData(dataList);
            //    break;
            //case "[Save]gugudan":
            //    break;
            //case "[Save]calculation":
            //    break;
            default:
                Debug.Log("[Client] HandleRequestMessage Method Something Happend");
                break;
        }
    }

    // 서버에서 대량으로 데이터를 받는 메서드
    private async void ReceiveMassiveRequestFromServer(NetworkStream stream)
    {
        Debug.Log($"[Client] ReceiveMassiveRequestFromServer");

        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 데이터를 읽어올때까지 대기
                string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 데이터 변환

                Debug.Log($"[Client] Receiving... massive request message from server : {receivedRequestData}");
                HandleMassiveRequestData(receivedRequestData);

                // 데이터가 다 불러와졌으면 break;
                List<string> endCheck = receivedRequestData.Split('|').ToList();
                if (endCheck.Contains("Finish"))
                {
                    FilterPlayerData(); // 플레이어 데이터 정리
                    //ClientPlayerDataToPlayerClassVariable();
                    break;
                }
            }

        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving message from server : {e.Message}");
        }
    }

    // 서버로부터 받은 대량 데이터 처리
    private void HandleMassiveRequestData(string requestdata)
    {
        Debug.Log("[Client] Handling... massive request data");
        List<string> parts = requestdata.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();

        foreach(string part in parts)
        {
            playerdata_FromServer.Add(part);
        }
    }

    // 최종적으로 사용할 수 있는 player data
    // 서버로부터 받고 1차적으로 정리한 data중 table name을 가지고 최종적으로 data 정리 
    private void FilterPlayerData()
    {
        Debug.Log("[Client] Filtering... player data");
        for(int i = 0; i < playerdata_FromServer.Count; i++)
        {
            for(int j = 0; j < table.list.Count; j++)
            {
                if(playerdata_FromServer[i].Contains(table.list[j]))
                {
                    List<string> values = playerdata_FromServer[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList(); // User테이블기준으로 User|User_Name|User_Profile|User_Coin이 있을것임
                    values.RemoveAt(0); // 0번째 인덱스는 테이블명이므로 values에 필요하지 않다.
                    playerdata_Dic.Add(table.list[j], values);
                }
            }
        }
    }

    // Json파일에 LicenseNumber 등록 / 비동기로 호출된 것이 끝났을때 동기적으로 호출시키기 위해
    private void SaveLicenseNumberToJsonFile()
    {
        // JsonData 생성
        JsonData client_Json = new JsonData();
        client_Json["LicenseNumber"] = clientLicenseNumber;
        client_Json["Charactor"] = clientCharactor;
        // Json 데이터를 문자열로 변환하여 파일에 저장
        string jsonString = JsonMapper.ToJson(client_Json);
        File.WriteAllText(licensePath + "/clientlicense.json", jsonString);
    }

    // Start DBTable 세팅
    private void ETCInitSetting()
    {
        Debug.Log("[Client] ETCInitSetting");
        // DB TableName 생성
        table = new TableName();

        // Dictionary 생성
        playerdata_Dic = new Dictionary<string, List<string>>();
    }


    // 서버로부터 받은 PlayerData를 게임에서 사용하는 Player Class에 설정
    private void SetPlayer(User_Info user)
    {

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

    public void OnClickLoadPlayerDataFromServerTest()
    {
        Debug.Log($"[Client] Test... {playerdata_FromServer}");
        for(int i=0; i<playerdata_FromServer.Count;i++)
        {
            Debug.Log($"playerdata_FromServer[{i}]'s value : {playerdata_FromServer[i]}");
        }
    }

    public void OnClickLoadFilterPlayerDataTest()
    {
        foreach(KeyValuePair<string, List<string>> item in playerdata_Dic)
        {
            Debug.Log($"item.Key : {item.Key}, item.Value : {item.Value}");
            foreach(string value in item.Value)
            {
                Debug.Log($"item.Key : {item.Key}, value(string) in item.Value : {value}");
            }
        }
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

    // 재백이가 만든 Result_Data를 매개변수로 받아서 DB에 저장하는 메서드(server에 요청 -> RequestServer)
    public void SaveResultDataToDB(Result_Data resultdata)
    {
        // requestData = RequestName[0]/User_Licensenumber[1]/User_Charactor[2]/ReactionRate[3]/.../StarPoint[8]
        string requestData;
        string requestName;
        string values;
        string gameName;

        switch(resultdata.game_type)
        {
            case Game_Type.A:
                gameName = "calculation";
                break;
            case Game_Type.B:
                gameName = "venezia_chn";
                break;
            case Game_Type.C:
                gameName = "gugudan";
                break;
            default:
                Debug.Log("[Client] Game_Type error");
                gameName = "error";
                break;
        }

        Data_value datavalue = resultdata.Data[(resultdata.game_type, resultdata.Level, resultdata.Step)];

        requestName = $"[Save]{gameName}";
        values = $"{resultdata.Level}|{resultdata.Step}|{clientLicenseNumber}|{clientCharactor}|{datavalue.ReactionRate}|{datavalue.AnswersCount}|{datavalue.Answers}|{datavalue.PlayTime}|{datavalue.TotalScore}|";

        requestData = $"{requestName}|{values}";

        RequestToServer(requestData);
    }

    //// 게임 접속시 Result_Data에 있는 변수(인게임에서 사용할 변수들)에 Client PlayerData 값을 넣는 메서드
    //public Result_Data ClientPlayerDataToResultData()
    //{
    //    // tablename :  "user_info", "rank", "achievement", "pet", "calculation", "venezia_chn");
    //    // playerdata_Dic에서 데이터 가져옴
    //    Result_Data result_data = new Result_Data();

    //    result_data.playerName = playerdata_Dic("user_info", List<string>)

    //    return result_data;
    //}

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

    //// 버튼 눌러서 서버에 메시지 보내기
    //public void OnCilckSendMessage()
    //{
    //    SendMessageToServer();
    //}

    //private void SendMessageToServer()
    //{
    //    string sendMessage = "Hello World";
    //    try
    //    {

    //        byte[] data = Encoding.UTF8.GetBytes(sendMessage);
    //        stream.Write(data, 0, data.Length); // 데이터를 보낼때 까지 대기? 그냥 보내면 되잖어
    //        Debug.Log($"Sent message to server : {sendMessage}");

    //        ReceiveMessageFromServer(stream); // 메서드가 실행될때 까지 대기 / 대기시키기 위해 메서드 앞에 await을 붙여서 실행시키려면 메서드가 Task 붙여야하는듯?
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log($"Fail sending message to sever : + {e.Message}");
    //    }
    //}

    //private async void ReceiveMessageFromServer(NetworkStream stream)
    //{
    //    try
    //    {
    //        byte[] buffer = new byte[1024];
    //        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 데이터를 읽어올때까지 대기
    //        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 데이터 변환
    //        Debug.Log($"Received message from server : {receivedMessage}");
    //    }
    //    catch(Exception e)
    //    {
    //        Debug.Log($"Error receiving message from server : {e.Message}");
    //        Debug.LogError($"Error receiving message from server : {e.Message}");
    //    }
    //}

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
