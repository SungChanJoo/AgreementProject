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
//using Unity.Android;

public class Client : MonoBehaviour
{
    // IP, Port 고정됨
    [SerializeField] private string server_IP = "15.164.166.133"; // aws EC2 IP : 15.164.166.133
    [SerializeField] private int server_Port = 2421;

    bool socketReady;
    // TCP 통신
    private TcpClient client;
    private static NetworkStream stream;

    // login - license, charactor / local(json)로 관리
    public static string clientLicenseNumber;
    public static string clientCharactor;
    public string licenseFolderPath = string.Empty;

    // 서버로 부터 받은 data를 1차적으로 거른 List
    public List<string> CharactorData_FromServer;

    // Charactor data를 사용하기 위한 Dictionary
    public Dictionary<string, List<string>> CharactorData_Dic;

    // 서버-클라이언트 string으로 data 주고받을때 구분하기 위한 문자열
    private const string separatorString = "E|";

    // DB Table Name
    private TableName table;

    // DB로부터 Load할 Data들을 담을 변수, 다른 곳에 반환할 수 있도록 class를 만듬
    // 서버에 Request한 후 받는 Received Data를 담을 변수
    private UserData clientUserData;
    private Player_DB clientPlayerData;
    private AnalyticsData clientAnalyticsData;
    private RankData clientRankData;
    private ExpenditionCrew clientExpenditionCrew;
    private LastPlayData clientLastPlayData;
    private AnalyticsProfileData clientAnalyticsProfileData;

    // timer
    private float transmissionTime = 1f;

    // 기타 데이터 처리용 Handler
    private ETCMethodHandler etcMethodHandler;

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
    }

    #region Start() Methods, Setting and Connect to Server
    // Start시 기타 멤버변수 초기화
    private void ETCInitSetting()
    {
        Debug.Log("[Client] ETCInitSetting");

        // DB TableName 생성
        table = new TableName();

        // CharactorData Load했을때 받아오는 List 생성
        CharactorData_FromServer = new List<string>();

        // Dictionary 생성
        CharactorData_Dic = new Dictionary<string, List<string>>();

        // 기타 데이터 처리용 Handler 생성
        etcMethodHandler = new ETCMethodHandler();
    }

    // 클라이언트가 실행할 때 서버 연결 시도
    public void ConnectToServer()
    {
        // 이미 연결되었다면 함수 무시
        if (socketReady) return;

        try
        {
            // 서버 연결
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


    // 서버에 연결할 때 클라이언트의 정보를 불러오기 위한 메서드
    public void ClientLoginSet()
    {
        // 연결되지 않았다면 return
        if (!client.Connected) return;
        //licenseFolderPath = Application.dataPath + "/License";

        // 나중에 안드로이드 할때 Application.persistentDataPath + "/License";

        if (Application.platform == RuntimePlatform.Android)
        {

            licenseFolderPath = Application.persistentDataPath + "/License";
        }
        else
        {
            licenseFolderPath = Application.dataPath + "/License";
        }

        string licenseFilePath = licenseFolderPath + "/clientlicense.json";
        Debug.Log($"제발요 파일경로를 알려주세요{licenseFilePath}");
        // 경로에 파일이 존재하지 않는다면 라이센스넘버가 없다는것이고, 처음 접속한다는 뜻
        if (!File.Exists(licenseFilePath))
        {

            Directory.CreateDirectory(licenseFolderPath);
            // 서버에서 라이센스 넘버를 받아와야함, 그러기 위해 서버에 요청 todo
            Debug.Log($"[Client] This client is entering game for the first time..");
            string requestName = "[Create]LicenseNumber|Finish";
            RequestToServer(requestName);
            return; // 처음 접속이라면 폴더 및 파일 저장하고 return
        }

            // 해당 경로에 있는 파일을 읽어 클라이언트 라이센스 넘버를 불러옴
            string jsonStringFromFile = File.ReadAllText(licenseFilePath);
            JsonData client_JsonFile = JsonMapper.ToObject(jsonStringFromFile);
            clientLicenseNumber = client_JsonFile["LicenseNumber"].ToString();
            clientCharactor = client_JsonFile["Charactor"].ToString();
            Debug.Log($"[Client] This client's licensenumber(have) : {clientLicenseNumber}");
            Debug.Log($"[Client] This client's charactor(last charactor) : {clientCharactor}");
        }
#endregion

#region Server-Client Communication
    // 서버 요청 - 매개변수로 string으로 받고, requestName으로 요청사항 구분
    private void RequestToServer(string requestData)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(requestData);
            stream.Write(data, 0, data.Length); // 데이터를 보낼때 까지 대기? 그냥 보내면 되잖어
            List<string> requestDataList = requestData.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
            string requestName = requestDataList[0];
            Debug.Log($"[Client] Request to server : {requestName}");

            ReceiveRequestDataFromServer(stream); // 메서드가 실행될때 까지 대기 / 대기시키기 위해 메서드 앞에 await을 붙여서 실행시키려면 메서드가 Task 붙여야하는듯?
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Error Request to server : + {e.Message}");
        }
    }

    // 서버 요청 후 Receive, 동기식으로 데이터 받는다 (비동기X, 서버는 비동기식, 클라이언트는 동기식)
    private void ReceiveRequestDataFromServer(NetworkStream stream)
    {
        try
        {
            List<string> receivedRequestData_List = new List<string>();

            while (true)
            {
                //byte[] buffer = new byte[1024]; // 일반적인 버퍼사이즈
                byte[] buffer = new byte[16000000]; // 16MiB 버퍼 사이즈 
                int bytesRead = stream.Read(buffer, 0, buffer.Length); // 데이터를 읽어올때까지 대기 (동기식)
                //int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 데이터를 읽어올때까지 대기 (비동기식)
                string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 데이터 변환

                receivedRequestData_List.Add(receivedRequestData);
                Debug.Log($"[Client] Receiving... request data from server : {receivedRequestData}");

                // todo 서버로부터 전송된 데이터를 제대로 받았는지 확인하는게 필요하다
                // 못받으면 재요청
                // 네트워크 패킷손실 확인하는 방법
                // requestName으로 요청한 데이터를 받을 때 , E| separtorString 개수를 파악해서 제대로 개수가 맞는지 확인후 Handling하고 개수가 맞지 않으면 재전송하는 식으로?

                // 서버로부터 데이터 전송이 끝나면(Finish) break;
                List<string> endCheck = receivedRequestData.Split('|').ToList();
                if (endCheck.Contains("Finish"))
                {
                    Debug.Log($"[Client] Received Finish Data from server : {receivedRequestData}");
                    // receivedRequestData에 Finish가 있는 경우 Finish를 제거
                    etcMethodHandler.RemoveFinish(receivedRequestData_List, endCheck);
                    //receivedRequestData_List.RemoveAt(receivedRequestData_List.Count - 1);
                    //endCheck.RemoveAt(endCheck.Count - 1);
                    //
                    //string fixLastIndexInList = null;
                    //
                    //for(int i = 0; i < endCheck.Count; i++)
                    //{
                    //    fixLastIndexInList += $"{endCheck[i]}|";
                    //}
                    //
                    //receivedRequestData_List.Add(fixLastIndexInList);

                    Debug.Log($"[Client] Finish Receive Data From Server");
                    break;
                }
            }

            HandleReceivedRequestData(receivedRequestData_List);
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Error receiving data from server : {e.Message}");
        }
    }
#endregion

#region Handle Data
    // 서버로부터 받은 데이터 처리
    private void HandleReceivedRequestData(List<string> dataList)
    {
        // requestname : 클라이언트-서버에서 데이터를 처리하기 위한 구분
        // requestdata : Server에서 requestName으로 처리된 결과를 보낸 data
        // LicenseNumber -> clientLicenseNumber를 server가 보내줌 (Client가 첫 접속인 경우 실행됨)
        // LoadNewCharactorData -> 새 플레이어의 경우 DB에 licenseNumber를 제외한 데이터가 없으므로 모든 테이블에 존재하는 열항목에 0값을 부여한 CharactorData를 가질 것임
        // LoadExistCharactorData -> 기존 플레이어의 경우 DB에 저장된 CharactorData를 가질 것임

        // dataList[0] = {requestName}|
        string requestName = dataList[0].Split('|')[0];
        Debug.Log($"[Client] HandleRequestData method, request name : {requestName}");

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                // dataList[1] = {clientLicenseNumber}|{clientCharactor}|
                string tempAllocate = null;
                for (int i = 0; i < dataList.Count; i++)
                {
                    tempAllocate += dataList[i];
                    Debug.Log($"[Client] Check User dataList{i} : {dataList[i]}");
                }
                //clientLicenseNumber = dataList[0].Split('|')[1];
                //clientCharactor = dataList[0].Split('|')[2];
                clientLicenseNumber = tempAllocate.Split('|')[1];
                clientCharactor = tempAllocate.Split('|')[2];
                SaveClientDataToJsonFile();
                Debug.Log($"[Client] RequestName : {requestName}, get and save licenseNumber to jsonfile");
                break;
            case "[Create]Charactor": // todo
                for (int i = 0; i < dataList.Count; i++)
                {
                    Debug.Log($"[Client] Check [Create]Charactor, dateList[{i}] : {dataList[i]}");
                }
                clientCharactor = dataList[0].Split('|')[1];
                Debug.Log($"[Client] RequestName : {requestName}, get new charactor number : {clientCharactor}");
                SaveClientDataToJsonFile();
                break;
            case "[Save]CharactorName":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]CharactorProfile":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]CharactorBirthday":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]CharactorData": // todo
                //clientCharactor = "22";
                SaveClientDataToJsonFile();
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]GameResult": // todo
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]venezia_kor":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]venezia_eng":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]venezia_chn":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]gugudan":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]calculation":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]UserData":
                HanlelLoadUserData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]CharactorData":
                // dataList[0] = "[Load]CharactorData|value|value..|value|E|";
                HandleLoadCharactorData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]AnalyticsData":
                // dataList[0] = "[Load]AnalyticsData|value|value|...|value|E|"
                Debug.Log($"[Client] RequestName : {requestName}, dataList[0] : {dataList[0]}");
                HandleLoadAnalyticsData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]RankData":
                HandleLoadRankData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]ExpenditionCrew":
                HandleLoadExpenditionCrew(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]LastPlayData":
                HandleLoadLastPlayData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]AnalyticsProfileData":
                HandleLoadAnalyticsProfileData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Change]Charactor":
                HandleChangeCharactorData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Delete]Charactor":
                // DB에 있는 data만 삭제하면 되므로 따로 클라이언트가 처리할 필요 없음
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Reset]CharactorProfile":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            default:
                Debug.Log("[Client] HandleRequestData Method Something Happend");
                break;
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
        File.WriteAllText(licenseFolderPath + "/clientlicense.json", jsonString);
    }

    // 공용Json파일에 LicenseNumber 등록 / 비동기로 호출된 것이 끝났을때 동기적으로 호출시키기 위해
    public void PublicSaveClientDataToJsonFile(int charactorNumber)
    {
        // JsonData 생성
        JsonData client_Json = new JsonData();
        client_Json["LicenseNumber"] = clientLicenseNumber;
        client_Json["Charactor"] = charactorNumber;

        // Json 데이터를 문자열로 변환하여 파일에 저장
        string jsonString = JsonMapper.ToJson(client_Json);
        File.WriteAllText(licenseFolderPath + "/clientlicense.json", jsonString);
    }

    // 유저 데이터 처리
    private void HanlelLoadUserData(List<string> dataList)
    {
        Debug.Log("[Client] Handle LoadUserData..");

        #region dataList Format / filterList Format
        // 들어오는 형태
        // dataList[0] = "[Load]UserData|createdCharactorCount|E|CharactorNumber|Name|Profile|E|"
        // dataList[1] = "CharactorNumber|Name|Profile|E|CharactorNumber|Name|Profile|E|"
        // ... dataList[Last] = "CharactorNumber|Name|Profile|E|

        // 하나의 string
        // string = "[Load]UserData|createdCharactorCount|E|CharactorNumber|Name|Profile|E|CharactorNumber|Name|Profile|E|CharactorNumber|Name|Profile|E| ... CharactorNumber|Name|Profile|E|";

        // 원하는 형태(filter)
        // filterList[0] = createdCharactorCount|
        // filterList[1] = CharactorNumber|Name|Profile|"
        // filterList[2] = CharactorNumber|Name|Profile|"
        // ... dataList[Last] = CharactorNumber|Name|Profile|
        // '|' Split후 각 데이터를 UserData_Value 생성자에 대입해서 사용
        #endregion

        // 하나의 string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // Filtering dataList
        List<string> filterList = new List<string>();

        // E|로 분할 및 RequestName 제거
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]UserData|".Length);

        // clientUserData Initiate
        clientUserData = new UserData();
        clientUserData.user = new List<UserData_Value>();

        // clientUserData에 data 할당
        for (int i = 0; i < filterList.Count; i++)
        {
            List<string> tempList = filterList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

            if(i == 0) clientUserData.createdCharactorCount = Int32.Parse(tempList[0]);
            else
            {
                int _charactorNumber = Int32.Parse(tempList[0]);
                string _name = tempList[1];
                byte[] _profile = Convert.FromBase64String(tempList[2]);
                UserData_Value userdata_value = new UserData_Value(_charactorNumber, _name, _profile);
                clientUserData.user.Add(userdata_value);
            }
        }
    }

    // 플레이어 데이터 처리
    private void HandleLoadCharactorData(List<string> dataList)
    {
        Debug.Log("[Client] Handle LoadCharactorData");

        #region dataList Format / filterList Format
        // dataList[0] = "[Load]CharactorData|user_info|value|value..|value|E|";
        // dataList[1] = "rank|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // dataList[2] = "{gameTableName}|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // ... dataList[Last] = "{gameTableName}|value|value|...|value|E|"

        // dataList를 필터링 했을 때 가질 형태
        // filterList[0] = user_info|name|profile|birthday|totalanswer|totaltime|coin|E|
        // filterList[1] = venezia_kor_level1_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // filterList[2] = venezia_kor_level1_step2|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // filterList[3] = venezia_kor_level1_step3|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[19] = venezia_eng_level1_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[37] = venezia_chn_level_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[43] = calculation_level1_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[61] = gugudan_level1_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[last=79] = gugudan_level3_step6|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        #endregion

        // 하나의 string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        for (int i = 0; i < dataList.Count; i++)
        {
            Debug.Log($"[Client] dataList[{i}] : {dataList[i]}");
        }

        Debug.Log(oneData);

        List<string> filterList = new List<string>();

        // E|로 분할 및 RequestName 제거
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]CharactorData|".Length);

        // clientPlayerData InitSetting
        clientPlayerData = new Player_DB();

        for (int i = 0; i < filterList.Count; i++)
        {
            List<string> tempList = filterList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (i == 0) // user_info table에서 가져온 data
            {
                clientPlayerData.playerName = tempList[1];
                clientPlayerData.image = Convert.FromBase64String(tempList[2]); // Convert.FromBase64String(CharactorData_Dic["user_info"][1]);
                clientPlayerData.Day = "";
                clientPlayerData.BirthDay = tempList[3];
                clientPlayerData.TotalAnswers = int.Parse(tempList[4]);
                clientPlayerData.TotalTime = float.Parse(tempList[5]);
                //clientPlayerData.Coin = int.Parser(tempList[6]);
            }
            else // game table에서 가져온 data
            {
                float reactionRate = float.Parse(tempList[1]);
                int answersCount = Int32.Parse(tempList[2]);
                int answers = Int32.Parse(tempList[3]);
                float playTime = float.Parse(tempList[4]);
                int totalScore = Int32.Parse(tempList[5]);
                int starPoint = Int32.Parse(tempList[6]);

                Data_value datavalue = new Data_value(reactionRate, answersCount, answers, playTime, totalScore, starPoint);

                // i == 1 부터 80까지 80회 반복됨 i == 1, 19, 37, 43, 61
                if (i <= 18) // i가 1~18까지 venezia_kor_level1_step1
                {
                    int level = ((i - 1) / 6) + 1;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.A, level, step), datavalue);
                }
                else if(i <=36) // i가 19~36까지 venezia_eng_level1_step1
                {
                    int level = ((i - 1) / 6) - 2;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.B, level, step), datavalue);
                }
                else if(i <= 42) // i가 37~42까지 venezia_chn_level_step1
                {
                    int level = 1;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.C, level, step), datavalue);
                }
                else if(i <= 60) // i가 43~60까지 calculation_level1_step1
                {
                    int level = ((i - 1) / 6) - 6;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.D, level, step), datavalue);
                }
                else // i가 61부터 gugudan_level1_step1
                {
                    int level = ((i - 1) / 6) - 9;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.E, level, step), datavalue);
                }
                
            }
        }

        Debug.Log("[Client] End HandleLoadCharactorData..");
    }

    // 분석 데이터 처리
    private void HandleLoadAnalyticsData(List<string> dataList)
    {
        Debug.Log("[Client] Handle LoadAnlayticsData..");

        #region dataList Format / filterList Format
        // 들어오는 형태
        // dataList[0] = "[Load]AnalyticsData|day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|...??"
        // dataList[1] = "day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|??"
        // ... dataList[Last] = "CharactorNumber|Name|Profile|E|

        // Filterd List or Array가 밑에처럼 구분되어야함
        // filterList[0] = "[Load]AnalyticsData|"day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[1] = "day1|venezia_kor_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[2] = "day1|venezia_kor_level3_analytics|ReactionRate|AnswerRate|E|"
        // filterList[3] = "day1|venezia_eng_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[4] = "day1|venezia_eng_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[5] = "day1|venezia_eng_level3_analytics|ReactionRate|AnswerRate|E|"
        // filterList[6] = "day1|venezia_chn_analytics|ReactionRate|AnswerRate|E|"
        // filterList[7] = "day1|calculation_level1_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[8] = "day1|calculation_level2_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[9] = "day1|calculation_level3_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[10] = "day1|gugudan_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[11] = "day1|gugudan_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[12] = "day1|gugudan_level3_analytics|ReactionRate|AnswerRate|E|"
        // 
        // filterList[78] = "day7|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[79] = "day7|venezia_kor_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[80] = "day7|venezia_kor_level3_analytics|ReactionRate|AnswerRate|E|"
        // filterList[81] = "day7|venezia_eng_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[82] = "day7|venezia_eng_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[83] = "day7|venezia_eng_level3_analytics|ReactionRate|AnswerRate|E|"
        // filterList[84] = "day7|venezia_chn_analytics|ReactionRate|AnswerRate|E|"
        // filterList[85] = "day7|calculation_level1_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[86] = "day7|calculation_level2_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[87] = "day7|calculation_level3_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[88] = "day7|gugudan_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[89] = "day7|gugudan_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[90] = "day7|gugudan_level3_analytics|ReactionRate|AnswerRate|E|"
        #endregion

        // 서버로부터 받아온 dataList를 하나의 string에 담아서 처리
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // Filtering dataList
        List<string> filterList = new List<string>();

        // E|로 분할 및 RequestName 제거
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]AnalyticsData|".Length);

        for (int i = 0; i < filterList.Count; i++)
        {
            Debug.Log($"[Client] AnalyticsData filterList[{i}] : {filterList[i]}");
        }

        // clientAnalyticsData InitSetting
        clientAnalyticsData = new AnalyticsData();

        // clientAnalyticsData의 멤버변수인 Data에 Value 추가
        etcMethodHandler.AddClientAnalyticsDataValue(filterList, clientAnalyticsData);

        Debug.Log($"[Client] Check clientAnalyticsData.Data.Count : {clientAnalyticsData.Data.Count}");

        int days = 7;
        int games = 5;
        int levels = 3;

        for(int i = 0; i < days; i++)
        {
            for(int j = 0; j < games; j++)
            {
                for(int k = 0; k < levels; k++)
                {
                    Debug.Log($"[Client] Check clientAnalyticsData.Data.Value(reactionRate), i,j,k : {i},{j}, {k} : {clientAnalyticsData.Data[(i + 1, (Game_Type)j, k + 1)].reactionRate}");
                }
            }
        }
        //clientAnalyticsData.Data[(1, 0, 1)].answerRate;

        Debug.Log("[Client] End HandleLoadAnalyticsData..");
    }

    // 랭크 데이터 처리
    private void HandleLoadRankData(List<string> dataList)
    {
        Debug.Log("[Client] Handle LoadRankData..");

        #region dataList Format / filterList Format
        // 들어오는 형식 == dataList
        // dataList[0] = "[Load]RankData|profile|name|score|E|profile|name|totalScore|E|";
        // dataList[1] = profile|name|totalScore|E|profile|name|totalScore|E|"profile|name|totalScore|E| ??

        // 원하는 형태 == filterList
        // filterList[0] = "[Load]RankData|profile|name|score|E|";
        // filterList[1] = "profile|name|totalScore|E|
        // filterList[2] = profile|name|totalScore|E|
        // ...
        // filterList[6] = profile|name|totalScore|scorePlace|highestScorePlace|E|
        // filterList[7] = profile|name|totalTime|E|
        // ...
        // filterList[last] = profile|name|totalTime|timePlace|highestTimePlace|E|
        #endregion

        // 하나의 string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // Filtering dataList
        List<string> filterList = new List<string>();

        // E| 분할 및 RequestName 제거
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]RankData|".Length);

        // clientRankData InitSetting
        clientRankData = new RankData();
        clientRankData.rankdata_score = new RankData_value[6];
        clientRankData.rankdata_time = new RankData_value[6];

        // clientRankData에 data 할당
        for (int i = 0; i < filterList.Count; i++)
        {
            List<string> part = filterList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

            if(i < 5) // score 1~5 등
            {
                clientRankData.rankdata_score[i].userProfile = Convert.FromBase64String(part[0]);
                clientRankData.rankdata_score[i].userName = part[1];
                clientRankData.rankdata_score[i].totalScore = Int32.Parse(part[2]);
            }
            else if(i == 5) // 클라이언트 score
            {
                clientRankData.rankdata_score[i].userProfile = Convert.FromBase64String(part[0]);
                clientRankData.rankdata_score[i].userName = part[1];
                clientRankData.rankdata_score[i].totalScore = Int32.Parse(part[2]);
                clientRankData.rankdata_score[i].scorePlace = Int32.Parse(part[3]);
                clientRankData.rankdata_score[i].highestScorePlace = Int32.Parse(part[4]);
            }
            else if(i < 11) // time 1~5등
            {
                clientRankData.rankdata_time[i - 6].userProfile = Convert.FromBase64String(part[0]);
                clientRankData.rankdata_time[i - 6].userName = part[1];
                clientRankData.rankdata_time[i - 6].totalTime = float.Parse(part[2]);
            }
            else // 클라이언트 time
            {
                clientRankData.rankdata_time[i - 6].userProfile = Convert.FromBase64String(part[0]);
                clientRankData.rankdata_time[i - 6].userName = part[1];
                clientRankData.rankdata_time[i - 6].totalTime = float.Parse(part[2]);
                clientRankData.rankdata_time[i - 6].timePlace = Int32.Parse(part[3]);
                clientRankData.rankdata_time[i - 6].highestTimePlace = Int32.Parse(part[4]);
            }
        }

        Debug.Log("[Client] End HandleLoadRankData..");
    }

    // 탐험대원 처리
    private void HandleLoadExpenditionCrew(List<string> dataList)
    {
        Debug.Log("[Client] HandleLoadExpenditionCrew..");

        #region dataList Format / filterList Format
        // dataList[0] = "[Load]ExpenditionCrew|LastSelectCrew|crew1|crew2|...|crew(n)|"

        // 사용하고자 하는 List 형태
        // filterList[0] = [Load]ExpenditionCrew|LastSelectCrew|crew1|crew2|...|crew(n)|
        #endregion

        // 하나의 string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // filterList
        List<string> filterList = new List<string>();

        // E| 분할 및 RequestName 제거
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]ExpenditionCrew|".Length);

        for (int i = 0; i < filterList.Count; i++)
        {
            Debug.Log($"[Client] ExpenditionCrew filterList[{i}] : {filterList[i]}");
        }

        // ExpenditionCrew's member variable
        int SelectedCrew; //선택한 대원
        List<bool> OwnedCrew = new List<bool>(); //보유한 대원 리스트

        List<string> tempList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        SelectedCrew = Int32.Parse(tempList[0]);

        bool haveThatCrew;
        for (int i = 1; i < tempList.Count; i++)
        {
            if (tempList[i] == "0") haveThatCrew = false;
            else haveThatCrew = true;

            OwnedCrew.Add(haveThatCrew);
        }

        // clientExpenditionCrew Initiate and Setting
        clientExpenditionCrew = new ExpenditionCrew(SelectedCrew, OwnedCrew);

        Debug.Log("[Client] End HandleLoadExpendtionCrew...");
    }

    // 마지막 플레이 게임 처리
    private void HandleLoadLastPlayData(List<string> dataList)
    {
        Debug.Log("[Client] Come in HandleLoadLastPlayData..");

        #region dataList Format / filterList Format
        // dataList[0] = [Load]LastPlayData|(game1_level1)의 value|(game1_level2)의 value| ... |(game5_level3)의 value|
        // dataList[1] = ... |(game5_level3)의 value| ??

        // filterList 형태
        // filterList[0] = [Load]LastPlayData|(game1_level1)의 value|(game1_level2)의 value| ... |(game5_level3)의 value|

        // tempList
        // tempList[0] = (game1_level1)의 value
        // tempList[1] = (game1_level2)의 value
        // ...
        // tempList[n] = (game5_level3)의 value
        #endregion

        // 하나의 string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // filterList
        List<string> filterList = new List<string>();

        // E| 분할 및 RequestName 제거
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]LastPlayData|".Length);

        List<string> tempList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        for (int i = 0; i < tempList.Count; i++)
        {
            Debug.Log($"[Client] LastPlayData tempList[{i}] : {tempList[i]}");
        }

        // clientLastPlayData Initiate
        clientLastPlayData = new LastPlayData();

        // clientLastPlayData Setting
        for(int i = 0; i < tempList.Count; i++)
        {
            if (i < 3) clientLastPlayData.Step.Add((Game_Type.A, i + 1), Int32.Parse(tempList[i])); // venezia_kor
            else if (i < 6) clientLastPlayData.Step.Add((Game_Type.B, (i + 1) - 3), Int32.Parse(tempList[i])); // venezia_eng
            else if (i == 6) clientLastPlayData.Step.Add((Game_Type.C, (i + 1) - 6), Int32.Parse(tempList[i])); // venezia_chn
            else if (i < 10) clientLastPlayData.Step.Add((Game_Type.D, (i + 1) - 7), Int32.Parse(tempList[i])); // calculation
            else clientLastPlayData.Step.Add((Game_Type.E, (i + 1) - 10), Int32.Parse(tempList[i])); // gugudan;
        }

        Debug.Log("[Client] End Handle LoadLastPlayData...");
    }

    // 프로필용 분석데이터 처리
    private void HandleLoadAnalyticsProfileData(List<string> dataList)
    {
        Debug.Log("[Client] Come in HandleLoadAnalyticsProfileData..");

        // 들어오는 dataList
        // dataList[0] = [Load]AnalyticsProfileData|Level1_게임명|Level1_평균반응속도|Level1_평균정답률|
        //                + Level2_게임명|Leve2_평균반응속도|Level2_평균정답률|Level3_게임명|Leve3_평균반응속도|Level3_평균정답률|

        // filterList 형태
        // filterList[0] = [Load]AnalyticsProfileData|Level1_게임명|Level1_평균반응속도|Level1_평균정답률|Level2_게임명|Leve2_평균반응속도|Level2_평균정답률|Level3_게임명|Leve3_평균반응속도|Level3_평균정답률|

        // tempList 형태
        // tempList[0] = Level1_게임명
        // tempList[1] = Level1_평균반응속도
        // tempList[2] = Level1_평균정답률
        // ...
        // tempList[n] = Level3_평균정답률

        // 하나의 string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // filterList
        List<string> filterList = new List<string>();

        // E| 분할 및 RequestName 제거
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]ExpenditionCrew|".Length);

        List<string> tempList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        for (int i = 0; i < tempList.Count; i++)
        {
            Debug.Log($"[Client] AnalyticsProfileData tempList[{i}] : {tempList[i]}");
        }

        // clientAnalyticsProfileData Initiate
        clientAnalyticsProfileData = new AnalyticsProfileData();

        // clientAnalyticsProfileData Setting
        for (int i = 0; i < tempList.Count; i++)
        {
            if (i % 3 == 0)
            {
                string mostPlayedGame = tempList[i];
                float reactionRate = float.Parse(tempList[i + 1]);
                int answerRate = Int32.Parse(tempList[i + 2]);
                clientAnalyticsProfileData.Data[i / 3] = new Tuple<string, float, int>(mostPlayedGame, reactionRate, answerRate);
            }
        }

        Debug.Log("[Client] End HandleLoadAnalyticsProfileData..");
    }

    // 캐릭터 변경시 데이터 처리 - clientCharactor만 변경
    private void HandleChangeCharactorData(List<string> dataList)
    {
        Debug.Log("[Client] HandleChangeCharactorData..");

        for (int i = 0; i < dataList.Count; i++)
        {
            Debug.Log($"[Client] ChangeCharactorData[{i}] : {dataList[i]}");
        }

        // dataList[0] = [Change]Charactor|ChangeCharctorNumber|
        List<string> filterList = dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        // requestName 제거
        filterList.RemoveAt(0);

        // clientCharactor 변경
        clientCharactor = filterList[0];

        Debug.Log("[Client] End ChangeCharactorData..");
    }
#endregion

#region 클라이언트-서버요청

    /*
    Client가 DB에 있는 파일을 Save Load 하는 경우
    1. 앱 시작 -> UserData Load (User Charactors) // Only DB
    1. 앱 시작 -> Player_DB Load (Charactor Info, GameData) // DB <-> Client
    1. 앱 시작 -> AnalyticsData Load (개인 분석표) // Only DB
    1. 앱 시작 -> RankData Load (랭킹) // Only DB
    1. 앱 시작 -> ExpenditionCrew Load (탐험대원) // DB <-> Client
    1. 앱 시작 -> LastPlay Load (마지막으로 한 게임) // DB <-> Client
    1. 앱 시작 -> AnalyticsProfileData Load (프로필용 분석표) // Only DB
    ------------------------------------------------------------------------------------
    2. 플레이어(charactor) 생성 -> Save, Load
    2-1. Player_DB Save (기존 CharactorData)
    2-1. ExpendtionCrew Save (기존 CharactorData)
    2-1. LastPlay Save (기존 CharactorData)
    2-2. New Charactor Create (새 ChractorData 생성)
    2-3. UserData Load (새 CharactorData 기준으로)
    2-3. Player_DB Load (새 CharactorData)
    2-3. GameAnalytics Load (새 CharactorData)
    2-3. RankData Load (새 CharactorData)
    2-3. ExpenditionCrew Load (새 CharactorData)
    2-3. LastPlay Load (새 CharactorData)
    2-3. AnalyticsProfileData Load (새 CharactorData)
    -------------------------------------------------------------------------------------
    3. 플레이어(Charactor) 변경 -> Save, Load
    3-1. Player_DB Save (기존 CharactorData)
    3-1. ExpendtionCrew Save (기존 CharactorData)
    3-1. LastPlay Save (기존 CharactorData)
    3-2. UserData Load (변경 CharactorData 기준으로)
    3-2. Player_DB Load (변경 CharactorData)
    3-2. GameAnalytics Load (변경 CharactorData)
    3-2. RankData Load (변경 CharactorData)
    3-2. ExpenditionCrew Load (변경 CharactorData)
    3-2. LastPlay Load (변경 CharactorData)
    3-2. AnalyticsProfileData (변경 CharactorData)
    ---------------------------------------------------------------------------------------
    4. 플레이어(Charactor) 삭제 -> UserData Save(Update) and Load
    5. Charactor Name 등록
    6. Charactor Profile 등록
    7. Charactor Birthday 등록
    8. 게임 끝났을 때 Game Data(reactionrate, score, time 등) DB에 저장
    9. Reset Charactor Profile -> PlayerDB, AnalyticsProfileData Load
    ---------------------------------------------------------------------------------------
    10. 앱 종료 -> Player_DB Save 
    10. 앱 종료 -> ExpenditionCrew Save 
    10. 앱 종료 -> LastPlay Save
    
    7. 앱 종료 -> 마지막 접속한 Charactor - LitJson파일로 로컬 저장(clientlicense)

##. 앱 게임 시작 -> TotalScore(int형) Load (game_type, level, step 받음) -> Save GameData
    --. 랭킹 UI 접속 시(or 랭킹새로고침) -> rank Load  // 일단 테스트용
    */

#region 앱 시작시
    // 앱 시작시 UserData Load
    public UserData AppStart_LoadUserDataFromDB()
    {
        string requestData = $"[Load]UserData|{clientLicenseNumber}|Finish";
        RequestToServer(requestData);

        return clientUserData;
    }

    // 앱 시작시 Player_DB Load
    public Player_DB AppStart_LoadAllDataFromDB()
    {
        //Player_DB playerDB = new Player_DB();

        string requestData = $"[Load]CharactorData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        //playerDB = clientPlayerData;

        return clientPlayerData;
    }

    // 앱 시작시 AnalyticsData Load
    public AnalyticsData AppStart_LoadAnalyticsDataFromDB()
    {
        //AnalyticsData return_AnalyticsData = new AnalyticsData();

        string requestData = $"[Load]AnalyticsData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        //return_AnalyticsData = ;

        return clientAnalyticsData;
    }

    // 앱 시작시 RankData Load
    public RankData AppStart_LoadRankDataFromDB()
    {
        //RankData return_RankData = new RankData();

        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        //return_RankData = clientRankData;

        return clientRankData;
    }

    // 앱 시작시 ExpenditionCrew Load
    public ExpenditionCrew AppStart_LoadExpenditionCrewFromDB()
    {
        string requestData = $"[Load]ExpenditionCrew|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientExpenditionCrew;
    }

    // 앱 시작시 LastPlay Load
    public LastPlayData AppStart_LoadLastPlayFromDB()
    {
        string requestData = $"[Load]LastPlayData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientLastPlayData;
    }

    // 앱 시작시 AnalyticsProfileData Load
    public AnalyticsProfileData AppStart_LoadAnalyticsProfileDataFromDB()
    {
        string requestData = $"[Load]AnalyticsProfileData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsProfileData;
    }

#endregion

#region 캐릭터 생성
    // Charactor 생성시 Player_DB Save (기존 CharactorData)
    public void CreateCharactor_SaveCharactorDataToDB(Player_DB playerdb)
    {
        SaveCharactorDataToDB(playerdb);
    }

    // Charactor 생성시 ExpenditionCrew Save (기존 CharactorData)
    public void CreateCharactor_SaveExpenditionCrewDataToDB(ExpenditionCrew crew)
    {
        SaveExpenditionCrewDataToDB(crew);
    }

    // Charactor 생성시 LastPlay Save (기존 CharactorData)
    public void CreateCharactor_SaveLastPlayDataToDB(LastPlayData lastplaydata)
    {
        SaveLastPlayDataToDB(lastplaydata);
    }

    // Charactor 생성 요청 -> Server에서 보낸 데이터 받았을 때 clientCharactor -> 생성한 charactor번호로 변경
    public void CreateCharactorData()
    {
        string reqeustData = $"[Create]Charactor|{clientLicenseNumber}|{clientCharactor}|Finish";

        RequestToServer(reqeustData);
    }

    // Charactor 생성시 UserData Load (새 CharactorData 기준으로)
    public UserData CreateCharactor_LoadUserDataFromDB()
    {
        string requestData = $"[Load]UserData|{clientLicenseNumber}|Finish";
        RequestToServer(requestData);

        return clientUserData;
    }

    // Charactor 생성시 Player_DB Load (새 CharactorData)
    public Player_DB CreateCharactor_LoadCharactorDataFromDB()
    {
        string requestData = $"[Load]CharactorData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientPlayerData;
    }

    // Charactor 생성시 GameAnalytics Load (새 CharactorData)
    public AnalyticsData CreateCharactor_LoadAnalyticsDataFromDB()
    {
        string requestData = $"[Load]AnalyticsData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsData;
    }

    // Charactor 생성시 RankData Load (새 CharactorData)
    public RankData CreateCharactor_LoadRankDataFromDB()
    {
        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientRankData;
    }

    // Charactor 생성시 ExpenditionCrew Load (새 CharactorData) 
    public ExpenditionCrew CreateCharactor_LoadExpenditionCrewFromDB()
    {
        string requestData = $"[Load]ExpenditionCrew|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientExpenditionCrew;
    }

    // Charactor 생성시 LastPlay Load (새 CharactorData) 
    public LastPlayData CreateCharactor_LoadLastPlayFromDB()
    {
        string requestData = $"[Load]LastPlayData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientLastPlayData;
    }

    // Charactor 생성시 AnalyticsProfileData Load (새 CharactorData)
    public AnalyticsProfileData CreateCharactor_LoadAnalyticsProfileDataFromDB()
    {
        string requestData = $"[Load]AnalyticsProfileData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsProfileData;
    }
#endregion

#region 캐릭터 변경
    // Charactor 변경시 Player_DB Save (기존 CharactorData)
    public void ChangeCharactor_SaveCharactorDataToDB(Player_DB playerdb)
    {
        SaveCharactorDataToDB(playerdb);
    }

    // Charactor 변경시 ExpenditionCrew Save (기존 CharactorData)
    public void ChangeCharactor_SaveExpenditionCrewDataToDB(ExpenditionCrew crew)
    {
        SaveExpenditionCrewDataToDB(crew);
    }

    // Charactor 변경시 LastPlay Save (기존 CharactorData)
    public void ChangeCharactor_SaveLastPlayDataToDB(LastPlayData lastplaydata)
    {
        SaveLastPlayDataToDB(lastplaydata);
    }

    // Charactor 변경 요청 -> Server에서 보낸 데이터 받았을 때 clientCharactor -> 변경한 charactor번호로 변경, todo
    public void ChangeCharactorData(int changeCharactor)
    {
        string reqeustData = $"[Change]Charactor|{clientLicenseNumber}|{changeCharactor}|Finish";

        RequestToServer(reqeustData);
    }

    // Charactor 변경시 UserData Load (변경 CharactorData 기준으로)
    public UserData ChangeCharactor_LoadUserDataFromDB()
    {
        string requestData = $"[Load]UserData|{clientLicenseNumber}|Finish";
        RequestToServer(requestData);

        return clientUserData;
    }

    // Charactor 변경시 Player_DB Load (변경 CharatrorData)
    public Player_DB ChangeCharactor_LoadCharactorDataFromDB()
    {
        string requestData = $"[Load]CharactorData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientPlayerData;
    }

    // Charactor 변경시 GameAnalytics Load (변경 CharactorData)
    public AnalyticsData ChangeCharactor_LoadAnalyticsDataFromDB()
    {
        string requestData = $"[Load]AnalyticsData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsData;
    }

    // Charactor 변경시 RankData Load (변경 CharactorData)
    public RankData ChangeCharactor_LoadRankDataFromDB()
    {
        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientRankData;
    }

    // Charactor 변경시 ExpenditionCrew Load (변경 CharactorData) 
    public ExpenditionCrew ChangeCharactor_LoadExpenditionCrewFromDB()
    {
        string requestData = $"[Load]ExpenditionCrew|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientExpenditionCrew;
    }

    // Charactor 변경시 LastPlay Load (변경 CharactorData) 
    public LastPlayData ChangeCharactor_LoadLastPlayFromDB()
    {
        string requestData = $"[Load]LastPlayData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientLastPlayData;
    }

    // Charactor 변경시 AnalyticsProfileData Load
    public AnalyticsProfileData ChangeCharactor_LoadAnalyticsProfileDataFromDB()
    {
        string requestData = $"[Load]AnalyticsProfileData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsProfileData;
    }
#endregion

    // Charactor Delete
    public void DeleteCharactor(int deleteCharactor)
    {
        string requestData = $"[Delete]Charactor|{clientLicenseNumber}|{deleteCharactor}|Finish";

        RequestToServer(requestData);
    }

    // Charactor 삭제 후 UserData Load
    public UserData DeleteCharactor_LoadUserData()
    {
        string requestData = $"[Load]UserData|{clientLicenseNumber}|Finish";
        RequestToServer(requestData);

        return clientUserData;
    }

    // Charactor Name 등록
    public void RegisterCharactorName_SaveDataToDB(string name)
    {
        // DB에 저장할 때 user_licensenumber와 user_charactor의 value에 맞는 row에 넣어야 하므로 저 값들이 필요
        // requestData = 요청제목|라이센스|캐릭터번호|{name}|
        string requestData = $"[Save]CharactorName|{clientLicenseNumber}|{clientCharactor}|{name}|Finish";

        RequestToServer(requestData);
    }

    // Charactor Profile 등록
    public void RegisterCharactorProfile_SaveDataToDB(byte[] image)
    {
        /*
         string base64 = "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";
        byte[] profile = Convert.FromBase64String(base64);

        RegisterCharactorProfile_SaveDataToDB(profile);
         */


        // DB에 저장할 때 user_licensenumber와 user_charactor의 value에 맞는 row에 넣어야 하므로 저 값들이 필요
        // requestData = 요청제목|라이센스|캐릭터번호|{image}|

        // byte[] image -> Base64 문자열로 변환
        string imageBase64 = Convert.ToBase64String(image);
        string requestData = $"[Save]CharactorProfile|{clientLicenseNumber}|{clientCharactor}|{imageBase64}|Finish";

        RequestToServer(requestData);
    }

    // Charactor Birthday 등록
    public void RegisterCharactorBirthday_SaveDataToDB(string birthday)
    {
        string requestData = $"[Save]CharactorBirthday|{clientLicenseNumber}|{clientCharactor}|{birthday}|Finish";

        RequestToServer(requestData);
    }

    // 게임 시작시 TotalScore Load
    public int AppGame_LoadTotalScoreFromDB(Game_Type game_type, int level, int step)
    {

        return 0;
    }

    // 재백이가 만든 Result_Data를 매개변수로 받아서 DB에 저장하는 메서드(server에 요청 -> RequestServer)
    public void AppGame_SaveResultDataToDB(Player_DB resultdata, Game_Type game_type, int level, int step)
    {
        // requestData = RequestName[0]/User_Licensenumber[1]/User_Charactor[2]/ReactionRate[3]/.../StarPoint[8]
        string requestData;
        string requestName;
        string values;
        string gameName;

        switch(game_type)
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

        Data_value datavalue = resultdata.Data[(game_type, level, step)];

        requestName = $"[Save]{gameName}";
        values = $"{level}|{step}|{clientLicenseNumber}|{clientCharactor}|{datavalue.ReactionRate}|{datavalue.AnswersCount}|{datavalue.Answers}|{datavalue.PlayTime}|{datavalue.TotalScore}|";

        requestData = $"{requestName}|{values}Finish";

        RequestToServer(requestData);
    }

    // 캐릭터 프로필 일부 데이터 초기화
    public void ResetCharactorProfile()
    {
        string requestData = $"[Reset]CharactorProfile|{clientLicenseNumber}|{clientCharactor}|Finish";

        RequestToServer(requestData);
    }

    // 랭킹 UI 접속 시 Rank Load
    public RankData Ranking_LoadRankDataFromDB()
    {
        RankData return_RankData = new RankData();

        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return_RankData = clientRankData;

        return return_RankData;
    }

    // 앱 종료시 Player_DB Save
    public void AppExit_SaveCharactorDataToDB(Player_DB playerdb)
    {
        SaveCharactorDataToDB(playerdb);
    }

    // 앱 종료시 ExpendtionCrew Save
    public void AppExit_SaveExpenditionCrewDataToDB(ExpenditionCrew crew)
    {
        SaveExpenditionCrewDataToDB(crew);
    }

    // 앱 종료시 LastPlay Save
    public void AppExit_SaveLastPlayDataToDB(LastPlayData lastplaydata)
    {
        SaveLastPlayDataToDB(lastplaydata);
    }

#endregion

#region 중복 기능 Method (Save)
    // Save Charactor(Player_DB) Data To DB
    private void SaveCharactorDataToDB(Player_DB playerdb)
    {
        string requestData;
        string requestName = $"[Save]CharactorData";
        requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|{separatorString}";

        string base64 = "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";
        string imagebase64 = Convert.ToBase64String(playerdb.image);

        /*
         list.Add("user_info");
        list.Add("rank");
        list.Add("crew");
        list.Add("lastplaygame");

        // AnalyticsProfile table
        list.Add("analyltics_level1_profile");
        list.Add("analyltics_level2_profile");
        list.Add("analyltics_level3_profile");
         */

        // table.list[0] == user_info
        requestData += $"{table.list[0]}|{playerdb.playerName}|{imagebase64}|{playerdb.BirthDay}|{playerdb.TotalAnswers}|{playerdb.TotalTime}|{separatorString}";
        //requestData += $"{table.list[0]}|{separatorString}";
        // table.list[1] == rank
        //requestData += $"{table.list[1]}|{}";
        // table.list[2] == crew
        //requestData += $"{}";
        // table.list[3] == lastplaygame
        //requestData += $"{}";
        // table.list[4] == analyltics_level1_profile
        // table.list[5] == analyltics_level2_profile
        // table.list[6] == analyltics_level3_profile
        // table.list[7~n] == gametable
        // gametable's column : reactionrate/answercount/answerrate/playtime/totalscore/starpoint
        List<float> reactionRate_List = new List<float>();
        List<int> answersCount_List = new List<int>();
        List<int> answers_List = new List<int>();
        List<float> playTime_List = new List<float>();
        List<int> totalScore_List = new List<int>();
        List<int> starCount_List = new List<int>();

        ICollection<Data_value> allvalues = playerdb.Data.Values;
        foreach (Data_value datavalue in allvalues)
        {
            reactionRate_List.Add(datavalue.ReactionRate);
            answersCount_List.Add(datavalue.AnswersCount);
            answers_List.Add(datavalue.Answers);
            playTime_List.Add(datavalue.PlayTime);
            totalScore_List.Add(datavalue.TotalScore);
            starCount_List.Add(datavalue.StarCount);
        }

        for (int i = 0; i < reactionRate_List.Count; i++)
        {
            Debug.Log($"reactionRate_List[i] : {reactionRate_List[i]}, count : {i}");
        }

        // gameTable(i=4부터)
        for (int i = 7; i < table.list.Count; i++)
        {
            requestData += $"{table.list[i]}|{reactionRate_List[i - 7]}|{answersCount_List[i - 7]}|{answers_List[i - 7]}|{playTime_List[i - 7]}|{totalScore_List[i - 7]}|{starCount_List[i - 7]}|{separatorString}";
        }
        requestData += "Finish";
        Debug.Log($"[Client] before request to server savedata {requestData}");

        RequestToServer(requestData);
    }

    // Save ExpendtionCrew Data To DB
    private void SaveExpenditionCrewDataToDB(ExpenditionCrew crew)
    {
        //requestData = [Save]ExpenditionCrew|license|charactor|LastSelectCrew|Crew1|Crew2|... |Crew(n)|Finish
        string requestName = "[Save]ExpenditionCrew";
        string requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|";

        // SelectedCrew 추가
        requestData += $"{crew.SelectedCrew}|";

        // True(보유) -> 1 / False(미보유) -> 0
        for(int i = 0; i < crew.OwnedCrew.Count; i++)
        {
            if (crew.OwnedCrew[i]) requestData += "1|";
            else requestData += "0|";
        }

        requestData += "Finish";

        RequestToServer(requestData);
    }

    // Save LastPlay Data To DB
    private void SaveLastPlayDataToDB(LastPlayData lastplaydata)
    {
        //requestData = [Save]LastPlayData|license|charactor| (game1_level1)의 value | (game1_level2)의 value | ... | (game5_level3)의 value |Finish
        string requestName = "[Save]LastPlayData";
        string requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|";

        for(int i = 0; i < lastplaydata.Step.Values.Count; i++)
        {
            if (i < 3) requestData += $"{lastplaydata.Step[(Game_Type.A, i + 1)]}|"; // venezia_kor
            else if (i < 6) requestData += $"{lastplaydata.Step[(Game_Type.B, (i + 1) - 3)]}|"; // venezia_kor
            else if (i == 6) requestData += $"{lastplaydata.Step[(Game_Type.C, (i + 1) - 6)]}|"; // venezia_chn
            else if (i < 10) requestData += $"{lastplaydata.Step[(Game_Type.D, (i + 1) - 7)]}|"; // calculation
            else requestData += $"{lastplaydata.Step[(Game_Type.E, (i + 1) - 10)]}|"; // gugudan
        }

        requestData += "Finish";

        RequestToServer(requestData);
    }

#endregion

#region TestMethods
    public void OnClickSaveGameDataTest()
    {
        Game_Type game_Type = Game_Type.A;
        int level = 1;
        int step = 2;
        Player_DB result_DB = new Player_DB();
        Data_value data_Value = new Data_value(234.21f, 23, 12, 22.44f, 18000,3);
        //Reult_DB가 Null일 때 처리
        if (!result_DB.Data.ContainsKey((game_Type, level, step)))
        {
            result_DB.Data.Add((game_Type, level, step), data_Value);
        }
        else
        {
            //만약 totalScore가 DB에 있는 점수보다 크다면 다시 할당
            if (result_DB.Data[(game_Type, level, step)].TotalScore < 20000)
            {
                result_DB.Data[(game_Type, level, step)] = data_Value;
            }
        }

        AppGame_SaveResultDataToDB(result_DB, game_Type, level, step);

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

    public void OnClickLoadCharactorDataFromServerTest()
    {
        Debug.Log($"[Client] Test... {CharactorData_FromServer}");
        for (int i = 0; i < CharactorData_FromServer.Count; i++)
        {
            Debug.Log($"CharactorData_FromServer[{i}]'s value : {CharactorData_FromServer[i]}");
        }
    }

    public void OnClickLoadFilterCharactorDataTest()
    {
        foreach (KeyValuePair<string, List<string>> item in CharactorData_Dic)
        {
            Debug.Log($"item.Key : {item.Key}, item.Value : {item.Value}");
            foreach (string value in item.Value)
            {
                Debug.Log($"item.Key : {item.Key}, value(string) in item.Value : {value}");
            }
        }
    }

    public void OnClickRanking_LoadRankDataTest()
    {
        RankData rankDataFromDB = Ranking_LoadRankDataFromDB();
    }

    public void OnClickLoadRankDataTest()
    {
        for(int i = 0; i< clientRankData.rankdata_score.Length; i++)
        {
            Debug.Log($"clientRankData.rankdata_score[i].place : {clientRankData.rankdata_score[i].scorePlace}");
            Debug.Log($"clientRankData.rankdata_score[i].userProfile : {clientRankData.rankdata_score[i].userProfile}");
            Debug.Log($"clientRankData.rankdata_score[i].userName : {clientRankData.rankdata_score[i].userName}");
            Debug.Log($"clientRankData.rankdata_score[i].totalScore : {clientRankData.rankdata_score[i].totalScore}");
        }

        for (int i = 0; i < clientRankData.rankdata_time.Length; i++)
        {
            Debug.Log($"clientRankData.rankdata_time[i].place : {clientRankData.rankdata_time[i].timePlace}");
            Debug.Log($"clientRankData.rankdata_time[i].userProfile : {clientRankData.rankdata_time[i].userProfile}");
            Debug.Log($"clientRankData.rankdata_time[i].userName : {clientRankData.rankdata_time[i].userName}");
            Debug.Log($"clientRankData.rankdata_time[i].totalScore : {clientRankData.rankdata_time[i].totalTime}");
        }

    }

    public void OnClickSaveNameTest()
    {
        string name = "재백홍";

        RegisterCharactorName_SaveDataToDB(name);
    }

    public void OnClickSaveProfileTest()
    {
        string base64 = "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";
        byte[] profile = Convert.FromBase64String(base64);

        RegisterCharactorProfile_SaveDataToDB(profile);
    }

    public void OnClickSavePlayerDataTest()
    {
        Player_DB player_DB = new Player_DB();
        player_DB.playerName = "테스트";
        string base64 = "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";
        player_DB.image = Convert.FromBase64String(base64);
        player_DB.Day = "monday";
        player_DB.BirthDay = "95.03.21";
        player_DB.TotalAnswers = 44;
        player_DB.TotalTime = 123f;

        Game_Type game_Type = Game_Type.A;
        int level = 1;
        int step = 2;
        Data_value data_Value = new Data_value(234.21f, 23, 12, 22.44f, 18000, 3);
        //Reult_DB가 Null일 때 처리
        if (!player_DB.Data.ContainsKey((game_Type, level, step)))
        {
            player_DB.Data.Add((game_Type, level, step), data_Value);
        }
        else
        {
            //만약 totalScore가 DB에 있는 점수보다 크다면 다시 할당
            if (player_DB.Data[(game_Type, level, step)].TotalScore < 20000)
            {
                player_DB.Data[(game_Type, level, step)] = data_Value;
            }
        }

        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.A, i+1, j+1)] = data_Value;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.B, i + 1, j + 1)] = data_Value;
            }
        }

        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.C, i + 1, j + 1)] = data_Value;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.D, i + 1, j + 1)] = data_Value;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.E, i + 1, j + 1)] = data_Value;
            }
        }

        AppExit_SaveCharactorDataToDB(player_DB);
    }

    public void OnClickCreateDateDBTest()
    {
        string requestData = "[Test]CreateDB|Finish";

        RequestToServer(requestData);
    }
#endregion

    private void CloseSocket()
    {
        Debug.Log("Close Socket으로 들어오는가");
        if (!socketReady) return;

        //writer.Close();
        //reader.Close();
        client.Close();
        socketReady = false;
        Application.Quit();
    }
}
