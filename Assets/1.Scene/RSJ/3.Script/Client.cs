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
                SaveClientDataToJsonFile();
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
    private void SaveClientDataToJsonFile()
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

    // 재백이가 만든 Result_Data를 매개변수로 받아서 DB에 저장하는 메서드(server에 요청 -> RequestServer)
    public void SaveResultDataToDB(Result_DB resultdata)
    {
        // requestData = RequestName[0]/User_Licensenumber[1]/User_Charactor[2]/ReactionRate[3]/.../StarPoint[8]
        string requestData;
        string requestName;
        string values;
        string gameName;

        switch(resultdata.game_type)
        {
            case Game_Type.A:
                gameName = "venezia_kor";
                break;
            case Game_Type.B:
                gameName = "venezia_eng";
                break;
            case Game_Type.C:
                gameName = "venezia_chn";
                break;
            case Game_Type.D:
                gameName = "calculation";
                break;
            case Game_Type.E:
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
        for (int i = 0; i < playerdata_FromServer.Count; i++)
        {
            Debug.Log($"playerdata_FromServer[{i}]'s value : {playerdata_FromServer[i]}");
        }
    }

    public void OnClickLoadFilterPlayerDataTest()
    {
        foreach (KeyValuePair<string, List<string>> item in playerdata_Dic)
        {
            Debug.Log($"item.Key : {item.Key}, item.Value : {item.Value}");
            foreach (string value in item.Value)
            {
                Debug.Log($"item.Key : {item.Key}, value(string) in item.Value : {value}");
            }
        }
    }

    public void OnClickLoadGameDataForRankTest()
    {

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
}
