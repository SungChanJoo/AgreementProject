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

public class Client : MonoBehaviour
{
    // IP, Port 고정됨
    [SerializeField] private string server_IP = "43.201.77.212"; // aws EC2 IP : 43.201.77.212
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
    private Player_DB clientPlayerData;
    private AnalyticsData clientAnalyticsData;
    private RankData clientRankData;

    // timer
    private float transmissionTime = 1f;

    // 기타 데이터 처리용 Handler
    private IETCMethodHandler etcMethodHandler;

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
        Invoke("LoadCharactorDataFromDB", 6f);
    }

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

        licenseFolderPath = Application.dataPath + "/License";
        string licenseFilePath = licenseFolderPath + "/clientlicense.json";

        // 경로에 파일이 존재하지 않는다면 라이센스넘버가 없다는것이고, 처음 접속한다는 뜻
        if (!File.Exists(licenseFilePath))
        {
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

    // 게임 시작할 때 유저정보 불러오기 - 서버에 요청(서버-DB) // 나중에 Player Class 정리되면 수정해야함. todo
    private void LoadCharactorDataFromDB()
    {
        Debug.Log("[Clinet] Request LoadCharactorDataFromDB");
        string requestData; // 이 메서드에서 requestData는 requestName/clientLicenseNumber/clientCharactor
        requestData = $"[Load]CharactorData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);
    }

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

                // 서버로부터 데이터 전송이 끝나면(Finish) break;
                List<string> endCheck = receivedRequestData.Split('|').ToList();
                if (endCheck.Contains("Finish"))
                {
                    // receivedRequestData에 Finish가 있는 경우 Finish를 제거
                    etcMethodHandler.RemoveFinish(receivedRequestData_List, endCheck);
                   // receivedRequestData_List.RemoveAt(receivedRequestData_List.Count - 1);
                   // endCheck.RemoveAt(endCheck.Count - 1);
                   //
                   // string fixLastIndexInList = null;
                   //
                   // for(int i = 0; i < endCheck.Count; i++)
                   // {
                   //     fixLastIndexInList += $"{endCheck[i]}|";
                   // }
                   //
                   // receivedRequestData_List.Add(fixLastIndexInList);

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
                for(int i = 0; i < dataList.Count; i ++)
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
                clientCharactor = "22";
                SaveClientDataToJsonFile();
                break;
            case "[Save]CharactorName":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]CharactorProfile":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]CharactorData": // todo
                //clientCharactor = "22";
                //SaveClientDataToJsonFile();
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

    // 플레이어 데이터 처리
    private void HandleLoadCharactorData(List<string> dataList)
    {
        // dataList[0] = "[Load]CharactorData|user_info|value|value..|value|E|";
        // dataList[1] = "rank|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // dataList[2] = "{gameTableName}|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // ... dataList[Last] = "{gameTableName}|value|value|...|value|E|"

        Debug.Log("[Client] Handling LoadCharactorData");

        for (int i = 0; i < dataList.Count; i++)
        {
            // requestName 제거
            if (i == 0) dataList[0] = dataList[0].Substring("[Load]CharactorData".Length);

            // "E|" 제거 -> DB table별로 List에 나눠서 담음
            List<string> parts = new List<string>();
            parts = dataList[i].Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList(); 

            // List[index]에 string
            foreach (string part in parts) // part : rank|value|value|...|value|
            {
                //part.Split('|', StringSplitOptions.RemoveEmptyEntries);
                CharactorData_FromServer.Add(part);
            }

        }

        FilterCharactorData();
    }

    // 최종적으로 사용할 수 있는 Player_DB(clientPlayerData)
    // 서버로부터 받고 1차적으로 정리한 data중 table name을 가지고 최종적으로 data 정리 
    private void FilterCharactorData()
    {
        Debug.Log("[Client] Filtering... player data");
        for (int i = 0; i < CharactorData_FromServer.Count; i++)
        {
            for (int j = 0; j < table.list.Count; j++)
            {
                if (CharactorData_FromServer[i].Contains(table.list[j]))
                {
                    List<string> values = CharactorData_FromServer[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList(); // User테이블기준으로 User|User_Name|User_Profile|User_Coin이 있을것임
                    values.RemoveAt(0); // 0번째 인덱스는 테이블명이므로 values에 필요하지 않다.
                    CharactorData_Dic.Add(table.list[j], values);
                }
            }
        }

        Debug.Log("[Client] Store CharactorData to clientPlayerData variable");
        // clientPlayerData InitSetting
        clientPlayerData = new Player_DB();
        
        // user_info columns -> "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin"
        
        // CharactorData_Dic에 담긴 값
        // CharactorData_Dic["user_info"] -> [0]:name / [1]:profile / [2]:birthday / [3]:totalanswers / [4]:totaltime / [5]:coin
        clientPlayerData.playerName = CharactorData_Dic["user_info"][0];
        clientPlayerData.image = Convert.FromBase64String(CharactorData_Dic["user_info"][1]);
        clientPlayerData.Day = "";
        clientPlayerData.BirthDay = CharactorData_Dic["user_info"][2];
        clientPlayerData.TotalAnswers = int.Parse(CharactorData_Dic["user_info"][3]);
        clientPlayerData.TotalTime = float.Parse(CharactorData_Dic["user_info"][4]);

        string[] game_Names = { "venezia_kor", "venezia_eng", "venezia_chn", "calculation", "gugudan" };
        int[] levels = { 1, 2, 3 };
        int[] steps = { 1, 2, 3, 4, 5, 6 };

        for (int i = 0; i < game_Names.Length; i++)
        {
            for (int j = 0; j < levels.Length; j++)
            {
                for (int k = 0; k < steps.Length; k++)
                {
                    Game_Type game_type;
                    switch (i)
                    {
                        case 0:
                            game_type = Game_Type.A;
                            break;
                        case 1:
                            game_type = Game_Type.B;
                            break;
                        case 2:
                            game_type = Game_Type.C;
                            break;
                        case 3:
                            game_type = Game_Type.D;
                            break;
                        case 4:
                            game_type = Game_Type.E;
                            break;
                        default:
                            game_type = Game_Type.A;
                            Debug.Log("[Client] AppStart_LoadAllDataFromDB() Game_Type default Problem");
                            break;
                    }

                    string levelpart = $"level{levels[j]}";
                    if (game_Names[i] == "venezia_chn")
                    {
                        j = 2; // venezia_chn 게임은 level이 1개뿐이므로 한번만 돌아야함.
                        levelpart = "level";
                    }

                    string game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";

                    float reactionRate = float.Parse(CharactorData_Dic[$"{game_TableName}"][0]);
                    int answersCount = Int32.Parse(CharactorData_Dic[$"{game_TableName}"][1]);
                    int answers = Int32.Parse(CharactorData_Dic[$"{game_TableName}"][2]);
                    float playTime = float.Parse(CharactorData_Dic[$"{game_TableName}"][3]);
                    int totalScore = Int32.Parse(CharactorData_Dic[$"{game_TableName}"][4]);
                    int starCount = Int32.Parse(CharactorData_Dic[$"{game_TableName}"][5]);

                    Data_value datavalue = new Data_value(reactionRate, answersCount, answers, playTime, totalScore, starCount);

                    clientPlayerData.Data.Add((game_type, j, k), datavalue);
                }
            }
        }
        Debug.Log("[Client] End HandleLoadCharactorData and Filter..");
    }

    // 분석 데이터 처리
    private void HandleLoadAnalyticsData(List<string> dataList)
    {
        Debug.Log("[Client] HandleLoadAnlayticsData..");

        // clientAnalyticsData InitSetting
        clientAnalyticsData = new AnalyticsData();

        for(int i = 0; i < dataList.Count; i++)
        {
            Debug.Log($"[Client] AnalyticsDataList[{i}] : {dataList[i]}");
        }

        // Filterd List or Array가 밑에처럼 구분되어야함
        // dataList[0] = "[Load]AnalyticsData|E|"
        // dataList[1] = "day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[2] = "day1|venezia_kor_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[3] = "day1|venezia_kor_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[4] = "day1|venezia_eng_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[5] = "day1|venezia_eng_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[6] = "day1|venezia_eng_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[7] = "day1|venezia_chn_analytics|ReactionRate|AnswerRate|E|"
        // dataList[8] = "day1|calculation_level1_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[9] = "day1|calculation_level2_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[10] = "day1|calculation_level3_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[11] = "day1|gugudan_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[12] = "day1|gugudan_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[13] = "day1|gugudan_level3_analytics|ReactionRate|AnswerRate|E|"
        // ...
        // dataList[79] = "day7|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[80] = "day7|venezia_kor_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[81] = "day7|venezia_kor_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[82] = "day7|venezia_eng_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[83] = "day7|venezia_eng_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[84] = "day7|venezia_eng_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[85] = "day7|venezia_chn_analytics|ReactionRate|AnswerRate|E|"
        // dataList[86] = "day7|calculation_level1_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[87] = "day7|calculation_level2_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[88] = "day7|calculation_level3_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[89] = "day7|gugudan_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[90] = "day7|gugudan_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[91] = "day7|gugudan_level3_analytics|ReactionRate|AnswerRate|E|"

        // Filtering dataList 
        List<string> filterDataList = new List<string>();
        foreach(string str in dataList)
        {
            List<string> tempList = str.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
            tempList.ForEach(data => filterDataList.Add(data));
        }
        //dataList.ForEach(data => filterDataList.Add(data.Split(separatorString, StringSplitOptions.RemoveEmptyEntries)[0]));

        // clientAnalyticsData의 멤버변수인 Data에 Value 추가
        etcMethodHandler.AddClientAnalyticsDataValue(filterDataList, clientAnalyticsData);

        //string day = "24.03.04";
        //float reactionRate = 34.5f;
        //int answerRate = 33;
        //AnalyticsData_Value analyticsData_value = new AnalyticsData_Value(day, reactionRate, answerRate);

        // AnalyticsData_Value의 Key 개수 : 보여줘야 하는 일수 7일(없으면 null or 0) * GameType 5개 * level 3개 = 105개 (- venezia_chn=14) 91개?
        // clientAnalyticsData.Data.Add((1, Game_Type.A, 1), analyticsData_value);
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
                    Debug.Log($"[Client] Check clientAnalyticsData.Data.Value(reactionRate) : {clientAnalyticsData.Data[(i + 1, (Game_Type)j, k + 1)].reactionRate}");
                }
            }
        }
        //clientAnalyticsData.Data[(1, 0, 1)].answerRate;

        Debug.Log("[Client] End HandleLoadAnalyticsData..");
    }

    // 랭크 데이터 처리
    private void HandleLoadRankData(List<string> dataList)
    {
        // return_string = $"{i}|{timeList[i].userProfile}|{timeList[i].userName}|{timeList[i].totalTime}|{separatorString}";
        // dataList[0] = "[Load]RankData|place|profile|name|time or score|E|";
        // dataList[1] = "place|profile|name|time or score|E|"; // parts.Count == 1
        // or dataList[1] = "place|profile|name|time or score|E|place|profile|name|time or score|E|"; // parts.Count == 2

        Debug.Log("[Client] HandleLoadRankData..");

        // clientRankData InitSetting
        clientRankData = new RankData();
        clientRankData.rank_Score = new Rank_Score[6];
        clientRankData.rank_Time = new Rank_Time[6];

        // separatorString ( "E|" )을 기준으로 Split 한 List
        List<string> filterDataList = new List<string>();

        for(int i =0; i < dataList.Count; i++)
        {
            // requestName 제거
            if (i == 0) dataList[0] = dataList[0].Substring("[Load]RankData|".Length);

            List<string> parts = new List<string>();
            parts = dataList[i].Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
            parts.ForEach(data => filterDataList.Add(data)); // 인덱스(string) 옮김
        }
        Debug.Log("[Client] Check HandleLoadRankData..");
        for(int i = 0; i < filterDataList.Count; i++)
        {
            Debug.Log($"{filterDataList[i]}");
        }

        for (int i = 0; i < filterDataList.Count; i++)
        {
            List<string> parts = filterDataList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

            if(i < 6) // score
            {
                for (int j = 0; j < parts.Count; j++)
                {
                    if (j == 0) clientRankData.rank_Score[i].place = Int32.Parse(parts[j]);
                    else if (j == 1) clientRankData.rank_Score[i].userProfile = Convert.FromBase64String(parts[j]);
                    else if (j == 2) clientRankData.rank_Score[i].userName = parts[j];
                    else if (j == 3) clientRankData.rank_Score[i].totalScore = Int32.Parse(parts[j]);
                }

                // 자기 자신의 userlicensenumber와 usercharactor를 저장해둬야 개인의 데이터를 가져다 쓸수있다.
                if(i == 5)
                {
                    clientRankData.rank_Score[i].userlicensenumber = Int32.Parse(clientLicenseNumber);
                    clientRankData.rank_Score[i].usercharactor = Int32.Parse(clientCharactor);
                }
                else
                {
                    clientRankData.rank_Score[i].userlicensenumber = 0;
                    clientRankData.rank_Score[i].usercharactor = 0;
                }
            }
            else // time
            {
                for (int j = 0; j < parts.Count; j++)
                {
                    if (j == 0) clientRankData.rank_Time[i].place = Int32.Parse(parts[j]);
                    else if (j == 1) clientRankData.rank_Time[i].userProfile = Convert.FromBase64String(parts[j]);
                    else if (j == 2) clientRankData.rank_Time[i].userName = parts[j];
                    else if (j == 3) clientRankData.rank_Time[i].totalTime = float.Parse(parts[j]);
                }

                // 자기 자신의 userlicensenumber와 usercharactor를 저장해둬야 개인의 데이터를 가져다 쓸수있다.
                if (i == 11)
                {
                    clientRankData.rank_Score[i].userlicensenumber = Int32.Parse(clientLicenseNumber);
                    clientRankData.rank_Score[i].usercharactor = Int32.Parse(clientCharactor);
                }
                else
                {
                    clientRankData.rank_Score[i].userlicensenumber = 0;
                    clientRankData.rank_Score[i].usercharactor = 0;
                }
            }
        }

        Debug.Log("[Client] End HandleLoadRankData..");
    }

    #region 클라이언트-서버요청

    /*
    Client가 DB에 있는 파일을 Save Load 하는 경우
    1. 앱 시작 -> Player_DB Load (모든 데이터를 한번에 받아오는 것이 맞으나, 반환타입별로 나눠서 받음.)
    2. 앱 시작 -> GameAnalytics Load (개인 분석표)
    3. 앱 시작 -> RankData Load (랭킹)
    4. 플레이어(charactor) 생성 -> Save, Load
    5. Charactor Name 등록
    6. Charactor Profile 등록
    7. 앱 게임 시작 -> TotalScore(int형) Load (game_type, level, step 받음) -> Save GameData
    8. 랭킹 UI 접속 시(or 랭킹새로고침) -> rank Load  // 일단 테스트용
    9. 앱 종료 -> Save (모든 데이터)
    */

    // 앱 시작시 Player_DB Load
    public Player_DB AppStart_LoadAllDataFromDB()
    {
        //Player_DB playerDB = new Player_DB();

        string requestData = $"[Load]CharactorData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        //playerDB = clientPlayerData;

        return clientPlayerData;
    }

    // 앱 시작시 GameAnalytics Load
    public AnalyticsData AppStart_LoadGameAnalyticsDataFromDB()
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

    // Charactor 생성시, 사용중인 Charactor Data Save
    public void CreateCharactor_SaveCharactorDataToDB(Player_DB resultdb)
    {
        string requestData = $"[Save]CharactorData|Finish";

        RequestToServer(requestData);
    }

    // Charactor 생성 Load (새로 생성한 Charatror Data)
    public Player_DB CreateCharactor_LoadCharactorDataFromDB()
    {
        Player_DB resultdb = new Player_DB();

        return resultdb;
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

    // 랭킹 UI 접속 시 Rank Load
    public RankData Ranking_LoadRankDataFromDB()
    {
        RankData return_RankData = new RankData();

        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return_RankData = clientRankData;

        return return_RankData;
    }

    // 앱 종료시 Charactor Data Save
    public void AppExit_SaveCharactorDataToDB(Player_DB playerdb)
    {
        string requestData;
        string requestName = $"[Save]CharactorData";
        requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|{separatorString}";

        string base64 = "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";
        string imagebase64 = Convert.ToBase64String(playerdb.image);

        // table.list[0] == user_info
        requestData += $"{table.list[0]}|{playerdb.playerName}|{imagebase64}|{playerdb.BirthDay}|{playerdb.TotalAnswers}|{playerdb.TotalTime}|{separatorString}";
        //requestData += $"{table.list[0]}|{separatorString}";
        // table.list[1] == rank
        //requestData += $"{table.list[1]}|{}";
        // table.list[2] == achievement
        //requestData += $"{}";
        // table.list[3] == pet
        //requestData += $"{}";
        // table.list[4~n] == gametable
        // gametable's column : reactionrate/answercount/answerrate/playtime/totalscore/starpoint
        List<float> reactionRate_List = new List<float>();
        List<int> answersCount_List = new List<int>();
        List<int> answers_List = new List<int>();
        List<float> playTime_List = new List<float>();
        List<int> totalScore_List = new List<int>();
        List<int> starCount_List = new List<int>();

        ICollection<Data_value> allvalues = playerdb.Data.Values;
        foreach(Data_value datavalue in allvalues)
        {
            reactionRate_List.Add(datavalue.ReactionRate);
            answersCount_List.Add(datavalue.AnswersCount);  
            answers_List.Add(datavalue.Answers);
            playTime_List.Add(datavalue.PlayTime);
            totalScore_List.Add(datavalue.TotalScore);
            starCount_List.Add(datavalue.StarCount);
        }

        for(int i = 0; i < reactionRate_List.Count; i++)
        {
            Debug.Log($"reactionRate_List[i] : {reactionRate_List[i]}, count : {i}");
        }

        // gameTable(i=4부터)
        for(int i = 4; i < table.list.Count; i++)
        {
            requestData += $"{table.list[i]}|{reactionRate_List[i - 4]}|{answersCount_List[i - 4]}|{answers_List[i - 4]}|{playTime_List[i - 4]}|{totalScore_List[i - 4]}|{starCount_List[i - 4]}|{separatorString}";
        }
        requestData += "Finish";
        Debug.Log($"[Client] before request to server savedata {requestData}");

        RequestToServer(requestData);
    }

    #endregion

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
        for(int i = 0; i< clientRankData.rank_Score.Length; i++)
        {
            Debug.Log($"clientRankData.rank_Score[i].place : {clientRankData.rank_Score[i].place}");
            Debug.Log($"clientRankData.rank_Score[i].userProfile : {clientRankData.rank_Score[i].userProfile}");
            Debug.Log($"clientRankData.rank_Score[i].userName : {clientRankData.rank_Score[i].userName}");
            Debug.Log($"clientRankData.rank_Score[i].totalScore : {clientRankData.rank_Score[i].totalScore}");
        }

        for (int i = 0; i < clientRankData.rank_Time.Length; i++)
        {
            Debug.Log($"clientRankData.rank_Time[i].place : {clientRankData.rank_Time[i].place}");
            Debug.Log($"clientRankData.rank_Time[i].userProfile : {clientRankData.rank_Time[i].userProfile}");
            Debug.Log($"clientRankData.rank_Time[i].userName : {clientRankData.rank_Time[i].userName}");
            Debug.Log($"clientRankData.rank_Time[i].totalScore : {clientRankData.rank_Time[i].totalTime}");
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
