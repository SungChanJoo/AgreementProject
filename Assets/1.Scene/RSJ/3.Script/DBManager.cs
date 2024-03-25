using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using UnityEngine.UI;
using System;
using System.Text;
using System.Linq;

public class DBManager : MonoBehaviour
{
    MySqlConnection connection; // DB에 연결하는 클래스
    MySqlDataReader reader;

    //public string path = string.Empty;

    // 연결할때 필요한 정보
    private string str_Connection;

    // DataBase Name
    private string presentDB;
    private string weeklyRankDB;

    // Table List, AnalyticsTable List
    private TableName table;
    private AnalyticsTableName analyticsTable;

    // Table - Columns
    private string[] userinfo_Columns;
    private string[] rank_Columns;
    private string[] weeklyRank_Columns;
    private string[] achievement_Columns;
    private string[] crew_Columns;
    private string[] game_Columns;
    private string[] analytics_Columns;
    private string[] lastplaygame_Columns;
    private string[] analyticsProfile1_Columns; // Level 1 (venezia_chn 포함)
    private string[] analyticsProfile2_Columns; // Level 2,3 (venezia_chn 미포함)
    
    // RankData Load시 조회할 WeeklyRank Table
    // WeeklyRankDB에 어떤 테이블도 없으면(랭킹데이터가없으면) 클라이언트에게 보낼 RankData는 0으로
    private string referenceWeeklyRankTableName;
    // DB에 기준으로 삼을 일자 저장하고 불러올 변수
    private DateTime dbStandardDay;

    // 서버-클라이언트 string으로 data 주고받을때 구분하기 위한 문자열
    private const string separatorString = "E|";

    // LicenseNumber 기본, Table 게임명(편의성을 위해)
    private int clientLicenseNumber_Base = 10000;

    // 기타 데이터 처리용 Handler
    private ETCMethodHandler etcMethodHandler;

    public static DBManager instance = null;

    private void Awake()
    {
        if(instance == null)
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
        SetStringConnection();
        ConnectDB();
        InitSetting();
    }

    #region Start() Methods, Setting
    // DB 세션에 연결할 때 필요한 정보 초기화
    private void SetStringConnection()
    {
        string ip = "127.0.0.1"; // 우선 로컬(127.0.0.1)로, aws EC2 IP : 15.165.159.141
        string db = "present";
        string uid = "root"; //string.IsNullOrEmpty(user_Info.user_Name)? "" : user_Info.user_Name;
        string pw = "12345678"; //string.IsNullOrEmpty(user_Info.user_Password)? "" : user_Info.user_Password;
        string port = "3306";
        //str_Connection = $"Server={ip};Database={db};Uid={uid};Pwd={pw};Port={port};Charset=utf8;";
        str_Connection = $"Server={ip};" + 
                         $"Database={db};" + 
                         $"Uid={uid};" + 
                         $"Pwd={pw};" + 
                         $"Port={port};" +
                         $"Allow User Variables=True;" + // Parameter @Something must be defined 에러
                         $"CharSet=utf8;"; // ; 세미콜론 주의해라
        Debug.Log($"[DB] DB connect info : {str_Connection}");
    }

    // DB에 연결
    private void ConnectDB()
    {
        try
        {
            connection = new MySqlConnection(str_Connection);
            connection.Open(); // DB 열음
            Debug.Log("[DB] Success connect to db!");
        }
        catch(Exception e)
        {
            Debug.Log($"[DB] Fail connect to db + {e.Message}");
        }
    }

    // DB 재연결
    private bool CheckConnection(MySqlConnection con)
    {
        // open이 안되어있다면
        if(con.State != System.Data.ConnectionState.Open)
        {
            con.Open(); // 한번 더 열어보기
            if (con.State != System.Data.ConnectionState.Open)
            {
                return false; //재오픈을 했는데도 안되면 이상함 감지
            }
        }
        return true;
    }

    // DB Table등 data에 조작할 때 필요한 변수 설정
    private void InitSetting()
    {
        // DataBase Name
        presentDB = "present";
        weeklyRankDB = "weeklyrank";

        // Tables / TableName 객체 생성시 table.list 자동 생성
        table = new TableName();

        // analyticsTable 객체 생성
        analyticsTable = new AnalyticsTableName();

        // Table - Columns
        userinfo_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin" };
        rank_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "User_Profile", "User_Name", "TotalTime", "TotalScore" };
        weeklyRank_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "User_Profile", "User_Name", "TotalScore", "TotalTime", "ScorePlace", "TimePlace", "HighestScorePlace", "HighestTimePlace" };
        achievement_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "Something" };
        crew_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "LastSelectCrew", "루즈비", "블루비", "베르비", "로조비", "블랙비", "똘이", "고릴라", "고슴도치", "고양이", "까마귀", "남생이", "늑대", "다람쥐", "독수리", "돌고래", "라쿤", "물범", "방울뱀", "범고래", "병아리", "북극여우", "쇠족제비", "수탉", "악어", "양", "여우", "오리너구리", "치타", "캥거루", "코뿔소", "쿼카", "팬더", "펭귄", "해달", "호랑이", "흰토끼", "황금독수리" };
        game_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };
        analytics_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerRate" };
        lastplaygame_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "venezia_kor_level1", "venezia_kor_level2", "venezia_kor_level3", "venezia_eng_level1", "venezia_eng_level2", "venezia_eng_level3", "venezia_chn_level", "calculation_level1", "calculation_level2", "calculation_level3", "gugudan_level1", "gugudan_level2", "gugudan_level3" };
        analyticsProfile1_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "Venezia_Kor_PlayCount", "Venezia_Kor_ReactionRate", "Venezia_Kor_AnswerRate", "Venezia_Eng_PlayCount", "Venezia_Eng_ReactionRate", "Venezia_Eng_AnswerRate", "Venezia_Chn_PlayCount", "Venezia_Chn_ReactionRate", "Venezia_Chn_AnswerRate", "Calculation_PlayCount", "Calculation_ReactionRate", "Calculation_AnswerRate", "Gugudan_PlayCount", "Gugudan_ReactionRate", "Gugudan_AnswerRate", "LastPlayGame" };
        analyticsProfile2_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "Venezia_Kor_PlayCount", "Venezia_Kor_ReactionRate", "Venezia_Kor_AnswerRate", "Venezia_Eng_PlayCount", "Venezia_Eng_ReactionRate", "Venezia_Eng_AnswerRate", "Calculation_PlayCount", "Calculation_ReactionRate", "Calculation_AnswerRate", "Gugudan_PlayCount", "Gugudan_ReactionRate", "Gugudan_AnswerRate", "LastPlayGame" };

        // DB에 저장된 standard day
        dbStandardDay = LoadStandardDay();

        // RankData Load시 조회할 WeeklyRank Table
        referenceWeeklyRankTableName = SetWeeklyRankTableName(dbStandardDay);

        // 기타 데이터 처리용 Handler
        etcMethodHandler = new ETCMethodHandler();
    }
    #endregion

    #region Server-Client Data Communication
    // 계정등록, 단 한번 실행 
    public string CreateLicenseNumber()
    {
        Debug.Log("[DB] Come in CreateLicenseNumber method");

        if(!CheckConnection(connection))
        {
            Debug.Log("[DB] Didn't connect to DB");
            return "";
        }

        int count = 0;

        // DB user_info 테이블에서 User_LicenseNumber가 몇개 있는지 체크
        string checkUserInfo_Command = $"SELECT `User_LicenseNumber` FROM `user_info`";
        MySqlCommand check_SqlCmd = new MySqlCommand(checkUserInfo_Command, connection);
        reader = check_SqlCmd.ExecuteReader();

        // table 체크
        if (reader.HasRows)
        {
            while(reader.Read())
            {
                count++;
            }
        }
        reader.Close(); // DataReader가 열려있는 동안 추가적인 명령 실행 불가

        int clientLicenseNumber = clientLicenseNumber_Base + count;
        int defaultClientCharactor = 1;
        
        string returnData = $"{clientLicenseNumber}|{defaultClientCharactor}|";

        return returnData;
    }

    // 새 캐릭터 데이터 생성
    public string CreateNewCharactorData(int clientlicensenumber, int clientcharactor, string defaultName = null)
    {
        Debug.Log($"[DB] Create new Charactor data, client's licensenumber : {clientlicensenumber}, clients's charactor : {clientcharactor}");

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;
        
        // Insert Query
        string insertCharactorData_Command;

        // Update Query
        string updateCharactorData_Command;

        // Select Query -> clientCharactor Count
        string selectCharactorCount_Command;

        // table.list[0] -> user_info, [1] -> rank, [2] -> crew, [3] -> lastplaygame, [4]~ -> gameTables~

        // User가 Charactor를 얼마나 보유하고 있는지 체크
        selectCharactorCount_Command = $"SELECT `{userinfo_Columns[1]}` FROM `{table.list[0]}` WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}';";
        mySqlCommand.CommandText = selectCharactorCount_Command;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();

        // Check charactor count 
        int count = 0;

        while(reader.Read())
        {
            count++;
        }
        reader.Close();

        // 처음 라이센스 발급 받아서 캐릭터 생성시 count == 0
        // 프로필 화면에서 캐릭터 생성시 적어도 count는 1이상
        // 추가로 생성하는 캐릭터 번호는 db에 저장되어있는 총 캐릭터의 개수(count) + 1
        clientcharactor = count + 1; 

        // user_info table
        // userinfo_Columns = { "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin" };
        insertCharactorData_Command = $"INSERT INTO `{table.list[0]}` (`{userinfo_Columns[0]}`, `{userinfo_Columns[1]}`) VALUES ({clientlicensenumber},{clientcharactor})";
        mySqlCommand.CommandText = insertCharactorData_Command;
        mySqlCommand.ExecuteNonQuery();

        MySqlParameter valueParameter = mySqlCommand.Parameters.Add("@value", MySqlDbType.VarChar);
        string defaultProfile = "/9j/4AAQSkZJRgABAQAAAQABAAD/4gIoSUNDX1BST0ZJTEUAAQEAAAIYAAAAAAQwAABtbnRyUkdCIFhZWiAAAAAAAAAAAAAAAABhY3NwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAA9tYAAQAAAADTLQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAlkZXNjAAAA8AAAAHRyWFlaAAABZAAAABRnWFlaAAABeAAAABRiWFlaAAABjAAAABRyVFJDAAABoAAAAChnVFJDAAABoAAAAChiVFJDAAABoAAAACh3dHB0AAAByAAAABRjcHJ0AAAB3AAAADxtbHVjAAAAAAAAAAEAAAAMZW5VUwAAAFgAAAAcAHMAUgBHAEIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFhZWiAAAAAAAABvogAAOPUAAAOQWFlaIAAAAAAAAGKZAAC3hQAAGNpYWVogAAAAAAAAJKAAAA+EAAC2z3BhcmEAAAAAAAQAAAACZmYAAPKnAAANWQAAE9AAAApbAAAAAAAAAABYWVogAAAAAAAA9tYAAQAAAADTLW1sdWMAAAAAAAAAAQAAAAxlblVTAAAAIAAAABwARwBvAG8AZwBsAGUAIABJAG4AYwAuACAAMgAwADEANv/bAEMAAgEBAQEBAgEBAQICAgICBAMCAgICBQQEAwQGBQYGBgUGBgYHCQgGBwkHBgYICwgJCgoKCgoGCAsMCwoMCQoKCv/bAEMBAgICAgICBQMDBQoHBgcKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCv/AABEIBDkEOAMBIgACEQEDEQH/xAAdAAEAAQUBAQEAAAAAAAAAAAAABwECBAUGAwgJ/8QAQRABAAEDAgMGBQIEAgkEAwEAAAECAwQFEQYHIQkSGTFBWRNRcZfUImEUMoGRCFIVIzNCYpKhscEkU3KTFjRD8P/EABwBAQABBQEBAAAAAAAAAAAAAAAGAQMEBQcCCP/EADYRAQACAgIBAwMCBQIFBAMAAAABAgMEBREGEiExB0FREyIUIzJhcRWBFjORobE0QkNSweHw/9oADAMBAAIRAxEAPwD9a/Cl7Ln22uQX2d0T8U8KXsufba5BfZ3RPxU+jE7ll9QgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FPCl7Ln22uQX2d0T8VPodydQgLwpey59trkF9ndE/FE+h3J1AAoqAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH//Z";

        for(int i = 2; i < userinfo_Columns.Length; i++) // 0 -> licenseNumber, 1 -> charactor
        {
            updateCharactorData_Command = $"UPDATE `{table.list[0]}` SET `{userinfo_Columns[i]}` = @value " +
                                          $"WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}' AND `{userinfo_Columns[1]}` = '{clientcharactor}';";
            mySqlCommand.CommandText = updateCharactorData_Command;

            if (i == 2) // User_Name
            {
                valueParameter.MySqlDbType = MySqlDbType.VarChar;
                valueParameter.Value = defaultName == null? "Guest" : defaultName;
            }
            else if (i == 3) // User_Profile
            {
                valueParameter.MySqlDbType = MySqlDbType.MediumBlob;
                valueParameter.Value = defaultProfile;
            }
            else if (i == 4) // User_Birthday
            {
                valueParameter.MySqlDbType = MySqlDbType.VarChar;
                valueParameter.Value = "2015.02.02";
            }
            else if (i == 6) // float
            {
                valueParameter.MySqlDbType = MySqlDbType.Float;
                valueParameter.Value = 0;
            }
            else // int
            {
                valueParameter.MySqlDbType = MySqlDbType.Int32;
                valueParameter.Value = 0;
            }
            mySqlCommand.ExecuteNonQuery();
        }
        Debug.Log("[DB] User_info table complete!");
        
        // rank table
        // rank_Columns = { "User_LicenseNumber", "User_Charactor", "User_Profile", "User_Name", "TotalTime", "TotalScore" };
        insertCharactorData_Command = $"INSERT INTO `{table.list[1]}` (`{rank_Columns[0]}`, `{rank_Columns[1]}`) VALUES ({clientlicensenumber},{clientcharactor})";
        mySqlCommand.CommandText = insertCharactorData_Command;
        mySqlCommand.ExecuteNonQuery();
        int rank_valuepart = 0;

        for (int i = 2; i < rank_Columns.Length; i++) // 0->licensenumber, 1->charactor
        {
            updateCharactorData_Command = $"UPDATE `{table.list[1]}` SET `{rank_Columns[i]}` = '{rank_valuepart}' " +
                                          $"WHERE `{rank_Columns[0]}` = '{clientlicensenumber}' AND `{rank_Columns[1]}` = '{clientcharactor}';";
            mySqlCommand.CommandText = updateCharactorData_Command;
            mySqlCommand.ExecuteNonQuery();
        }
        Debug.Log("[DB] Rank table complete!");

        // crew table
        // crew_Columns { "User_LicenseNumber", "User_Charactor", "FinallySelectedCrew", "루즈비", "블루비", "베르비", "로조비", "블랙비", "똘이", "고릴라", "고슴도치", "고양이", "까마귀", "남생이", "늑대", "다람쥐", "독수리", "돌고래", "라쿤", "물범", "방울뱀", "범고래", "병아리", "북극여우", "쇠족제비", "수탉", "악어", "양", "여우", "오리너구리", "치타", "캥거루", "코뿔소", "쿼카", "팬더", "펭귄", "해달", "호랑이", "흰토끼", "황금독수리" };
        insertCharactorData_Command = $"INSERT INTO `{table.list[2]}` (`{crew_Columns[0]}`, `{crew_Columns[1]}`) VALUES ({clientlicensenumber},{clientcharactor})";
        mySqlCommand.CommandText = insertCharactorData_Command;
        mySqlCommand.ExecuteNonQuery();
        int crew_valuepart;

        for (int i = 2; i < crew_Columns.Length; i++)
        {
            if (i == 2) crew_valuepart = 0; // 0번째 crew는 루즈비, crew들 중 0번째 index를 선택했음을 Default로
            else if (2 < i && i <= 7) crew_valuepart = 1; // value == 1 -> 해당 crew들을 보유하고 있음. Default로 보유한 탐사대원 5마리
            else crew_valuepart = 0; // value == 0 -> 해당 crew들을 보유하고 있지 않음

            updateCharactorData_Command = $"UPDATE `{table.list[2]}` SET `{crew_Columns[i]}` = '{crew_valuepart}' " +
                                          $"WHERE `{crew_Columns[0]}` = '{clientlicensenumber}' AND `{crew_Columns[1]}` = '{clientcharactor}';"; 
            mySqlCommand.CommandText = updateCharactorData_Command;
            mySqlCommand.ExecuteNonQuery();
        }
        Debug.Log("[DB] Crew table complete!");

        // lastplaygame table
        // lastplaygame_Columns = { "User_LicenseNumber", "User_Charactor", "venezia_kor_level1", "venezia_kor_level2", "venezia_kor_level3", "venezia_eng_level1", "venezia_eng_level2", "venezia_eng_level3", "venezia_chn_level", "calculation_level1", "calculation_level2", "calculation_level3", "gugudan_level1", "vgugudan_level2", "gugudan_level3" };
        // 마지막으로 플레이 한 게임이 무엇인지 저장하기 위한 테이블 / 게임,레벨의 밸류(1~6)가 해당 스텝을 플레이함
        insertCharactorData_Command = $"INSERT INTO `{table.list[3]}` (`{lastplaygame_Columns[0]}`, `{lastplaygame_Columns[1]}`) VALUES ({clientlicensenumber}, {clientcharactor})";
        mySqlCommand.CommandText = insertCharactorData_Command;
        mySqlCommand.ExecuteNonQuery();
        int lastplaygame_valuepart = 1; // default 값 1 -> 스텝 1을 플레이했음

        for (int i = 2; i < lastplaygame_Columns.Length; i++)
        {
            updateCharactorData_Command = $"UPDATE `{table.list[3]}` SET `{lastplaygame_Columns[i]}` = '{lastplaygame_valuepart}' " +
                                          $"WHERE `{lastplaygame_Columns[0]}` = '{clientlicensenumber}' AND `{lastplaygame_Columns[1]}` = '{clientcharactor}';";
            mySqlCommand.CommandText = updateCharactorData_Command;
            mySqlCommand.ExecuteNonQuery();
        }
        Debug.Log("[DB] Lastplaygame table complete!");

        // analytics_level1_profile table
        // 캐릭터 프로필 화면에서 보여주기위한 테이블
        insertCharactorData_Command = $"INSERT INTO `{table.list[4]}` (`{analyticsProfile1_Columns[0]}`, `{analyticsProfile1_Columns[1]}`) VALUES ({clientlicensenumber}, {clientcharactor});";
        mySqlCommand.CommandText = insertCharactorData_Command;
        mySqlCommand.ExecuteNonQuery();
        int analyticsProfile1_valuepart = 0; // default 값 0

        for (int i = 2; i < analyticsProfile1_Columns.Length; i++)
        {
            updateCharactorData_Command = $"UPDATE `{table.list[4]}` SET `{analyticsProfile1_Columns[i]}` = {analyticsProfile1_valuepart} " +
                                          $"WHERE `{analyticsProfile1_Columns[0]}` = '{clientlicensenumber}' AND `{analyticsProfile1_Columns[1]}` = '{clientcharactor}';";
            mySqlCommand.CommandText = updateCharactorData_Command;
            mySqlCommand.ExecuteNonQuery();
        }
        Debug.Log("[DB] Analytics_level1_profile table complete!");

        // analytics_level(2,3)_profile table
        for (int i = 0; i < 2; i++)
        {
            insertCharactorData_Command = $"INSERT INTO `{table.list[5+i]}` (`{analyticsProfile2_Columns[0]}`, `{analyticsProfile2_Columns[1]}`) VALUES ({clientlicensenumber}, {clientcharactor});";
            mySqlCommand.CommandText = insertCharactorData_Command;
            mySqlCommand.ExecuteNonQuery();
            int analyticsProfile2_valuepart = 0; // default 값 0

            for (int j = 2; j < analyticsProfile2_Columns.Length; j++)
            {
                updateCharactorData_Command = $"UPDATE `{table.list[5+i]}` SET `{analyticsProfile2_Columns[j]}` = {analyticsProfile2_valuepart} " +
                                              $"WHERE `{analyticsProfile2_Columns[0]}` = '{clientlicensenumber}' AND `{analyticsProfile2_Columns[1]}` = '{clientcharactor}';";
                mySqlCommand.CommandText = updateCharactorData_Command;
                mySqlCommand.ExecuteNonQuery();
            }
        }
        Debug.Log("[DB] Analytics_level2,3_profile table complete!");

        // game table
        string game_TableName;

        string[] game_Names = { "venezia_kor", "venezia_eng", "venezia_chn", "calculation", "gugudan"};
        int[] levels = { 1, 2, 3 };
        int[] steps = { 1, 2, 3, 4, 5, 6 };

        //string[] game_Columns = { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };

        float init_Float = 0f;
        int init_Int = 0;

        List<string> game_TableList = new List<string>();

        // game_TableList 추가
        for (int i = 0; i < game_Names.Length; i++)
        {
            for (int j = 0; j < levels.Length; j++)
            {
                for (int k = 0; k < steps.Length; k++)
                {
                    string levelpart = $"level{levels[j]}";
                    if (game_Names[i] == "venezia_chn")
                    {
                        j = 2; // venezia_chn 게임은 level이 1개뿐이므로 한번만 돌아야함.
                        levelpart = "level";
                    }
                    game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";
                    game_TableList.Add(game_TableName);
                }
            }
        }

        // 게임 테이블에 따라 DB에 insert 및 update
        for (int i = 0; i < game_TableList.Count; i++)
        {
            // insert row
            insertCharactorData_Command = $"INSERT INTO {game_TableList[i]} (`User_LicenseNumber`, `User_Charactor`) VALUES ({clientlicensenumber}, {clientcharactor})";
            mySqlCommand.CommandText = insertCharactorData_Command;
            mySqlCommand.ExecuteNonQuery();

            // update rowm j=2부터 시작하는 이유는 0은 licensenumber고 1은 charactor번호이고 insert로 넣어줬기 때문에 굳이 또 업데이트를 할 필요가 없음
            for (int j = 2; j < game_Columns.Length; j++)
            {
                updateCharactorData_Command = $"UPDATE {game_TableList[i]} SET {game_Columns[j]} = @value  " +
                                              $"WHERE `{game_Columns[0]}` = '{clientlicensenumber}' AND `{game_Columns[1]}` = '{clientcharactor}'" ;
                mySqlCommand.CommandText = updateCharactorData_Command;

                // parameter 초기화
                mySqlCommand.Parameters.Clear();

                if (j == 2 || j == 5)
                {
                    mySqlCommand.Parameters.Add("@value", MySqlDbType.Float).Value = init_Float;
                }
                else
                {
                    mySqlCommand.Parameters.Add("@value", MySqlDbType.Int32).Value = init_Int;
                }
                mySqlCommand.ExecuteNonQuery();
            }
        }
        Debug.Log("[DB] Game table complete!");

        Debug.Log($"[DB] End Create new Charactor data, client's licensenumber : {clientlicensenumber}, clients's charactor : {clientcharactor}");

        return clientcharactor.ToString();
    }

    #region Data Save
    // DB에 캐릭터 이름 저장
    public void SaveCharactorName(List<string> filterList)
    {
        // user_info table과 rank table에 업데이트
        // dataList -> [0]requestName / [1]license / [2]charactor / [3]name
        Debug.Log("[DB] Come in SaveCharactorName method");

        //        [Server] Received Data From Client to filterList[0] : [Save]CharactorName | 10001 | 1 | name
        //      [DB] Come in SaveCharactorName method
        //      [Server] Error in ReceiveRequestFromClient Method : Index was out of range. Must be non - negative and less than the size of the collection.
        //Parameter name: index

        // '|' 분할
        List<string> dataList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        // 지역변수
        string update_Command;
        MySqlCommand update_sqlCmd;

        // user_info == table.list[0]
        update_Command = $"UPDATE `{table.list[0]}` SET `{userinfo_Columns[2]}` = '{dataList[3]}' " +
                         $"WHERE `{userinfo_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{userinfo_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        // rank == table.list[1]
        update_Command = $"UPDATE `{table.list[1]}` SET `{rank_Columns[3]}` = '{dataList[3]}' " +
                         $"WHERE `{rank_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{rank_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        Debug.Log("[DB] Complete save charactor name to DB");
    }

    // DB에 캐릭터 프로필(이미지) 저장
    public void SaveCharactorProfile(List<string> filterList)
    {
        // user_info table과 rank table에 업데이트
        // dataList -> [0]requestName / [1]license / [2]charactor / [3]profile(Base64)
        // Profile Base64 형식으로 DB에 저장
        Debug.Log("[DB] Come in SaveCharactorProfile method");

        // '|' 분할
        List<string> dataList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        // 지역변수
        string update_Command;
        MySqlCommand update_sqlCmd;

        // user_info == table.list[0]
        update_Command = $"UPDATE `{table.list[0]}` SET `{userinfo_Columns[3]}` = '{dataList[3]}' " +
                         $"WHERE `{userinfo_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{userinfo_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        //Debug.Log(update_Command);
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        // rank == table.list[1]
        update_Command = $"UPDATE `{table.list[1]}` SET `{rank_Columns[2]}` = '{dataList[3]}' " +
                         $"WHERE `{rank_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{rank_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        Debug.Log("[DB] Complete save charactor profile to DB");
    }

    // DB에 캐릭터 생년월일 저장
    public void SaveCharactorBirthday(List<string> filterList)
    {
        Debug.Log("[DB] Come in SaveCharactorBirthday method");

        // userinfo table에 저장
        // dataList-> [0]requestName / [1]license / [2]charactor / [3]birthday

        // '|' 분할
        List<string> dataList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        for(int i = 0; i < dataList.Count; i++)
        {
            Debug.Log($"[DB] dataList[{i}] : {dataList[i]}");
        }

        // Update할 table -> userinfo
        string update_Command = $"UPDATE `{table.list[0]}` SET `{userinfo_Columns[4]}` = '{dataList[3]}' " +
                                $"WHERE `{userinfo_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{userinfo_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        mySqlCommand.CommandText = update_Command;
        mySqlCommand.ExecuteNonQuery();

        Debug.Log("[DB] Complete save charactor birthday to DB");
    }

    // DB에 캐릭터 데이터(Charactor info, GameData) 저장 (캐릭터 변경 또는 게임 종료시)
    public void SaveCharactorData(List<string> filterList)
    {
        Debug.Log("[DB] Come in save charactor data");
        // filterList[0] : $"{requestName}|{clientLicenseNumber}|{clientCharactor}|"
        // filterList[1] : $"{table.list[0]}|{playerdb.playerName}|{Convert.ToBase64String(playerdb.image)}|{playerdb.BirthDay}|{playerdb.TotalAnswers}|{playerdb.TotalTime}|";
        // filterList[2~n-1] : $"{table.list[i]}|{reactionRate_List[i - 4]}|{answersCount_List[i - 4]}|{answers_List[i - 4]}|{playTime_List[i - 4]}|{totalScore_List[i - 4]}|{starCount_List[i - 4]}|}";
        // filterList[n] : $"Finish"

        for(int i = 0; i<filterList.Count; i++)
        {
            Debug.Log($"[DB] charactor filterList{i} : {filterList[i]}");
        }
        
        string update_Command;
        MySqlCommand update_SqlCmd = new MySqlCommand();
        update_SqlCmd.Connection = connection;
        Debug.Log($"[DB] Check filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[1] : {Int32.Parse(filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[1])}");
        Debug.Log($"[DB] Check filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[2] : {Int32.Parse(filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[2])}");
        int clientLicenseNumber = Int32.Parse(filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[1]);
        int clientCharactor = Int32.Parse(filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[2]);

        //userinfo_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin" };
        //game_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };

        for(int i = 1; i < filterList.Count; i++)
        {
            List<string> tempAllocate = filterList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (tempAllocate.Contains("Finish")) break;

            if (i == 1) // userinfo table
            {
                update_Command = $"UPDATE `{tempAllocate[0]}` SET ";
                tempAllocate.RemoveAt(0);
                for(int j = 2; j < userinfo_Columns.Length-1; j++) // 0,1 - license, charactor , todo 나중에 코인 playerdata에 추가되면 길이 원래대로 Length-1 -> Length
                {
                    if (j == 2) update_Command += $"`{userinfo_Columns[j]}` = '{tempAllocate[j - 2]}', "; // name, string
                    else if(j == 3) update_Command += $"`{userinfo_Columns[j]}` = '{tempAllocate[j - 2]}', "; // profile, Base64 (MediumBlob)
                    else if(j == 4) update_Command += $"`{userinfo_Columns[j]}` = '{tempAllocate[j - 2]}', "; // birthday, string
                    else if (j == 5) update_Command += $"`{userinfo_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])}, "; // totalanswers, int
                    else if (j == 6) update_Command += $"`{userinfo_Columns[j]}` = {float.Parse(tempAllocate[j - 2])} "; // totaltime, float
                }
                update_Command += $"WHERE `{userinfo_Columns[0]}` = {clientLicenseNumber} AND `{userinfo_Columns[1]}` = {clientCharactor};";
            }
            else // game table
            {
                update_Command = $"UPDATE `{tempAllocate[0]}` SET ";
                tempAllocate.RemoveAt(0);
                for (int j = 2; j < game_Columns.Length; j++) // 0,1 - license, charactor
                {   
                    if (j == 2) update_Command += $"`{game_Columns[j]}` = {float.Parse(tempAllocate[j - 2])}, "; //reactionrate, float
                    else if(j == 3) update_Command += $"`{game_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])}, "; // answercount, int
                    else if(j == 4) update_Command += $"`{game_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])}, "; // answerrate, int
                    else if(j == 5) update_Command += $"`{game_Columns[j]}` = {float.Parse(tempAllocate[j - 2])}, "; // playtime, float
                    else if(j == 6) update_Command += $"`{game_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])}, "; // totalscore, int
                    else if(j == 7) update_Command += $"`{game_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])} "; // starpoint, int
                }
                update_Command += $"WHERE `{game_Columns[0]}` = {clientLicenseNumber} AND `{game_Columns[1]}` = {clientCharactor};";
            }

            Debug.Log($"[DB] update_Command : {update_Command}");
            update_SqlCmd.CommandText = update_Command;
            update_SqlCmd.ExecuteNonQuery();
            Debug.Log("[DB] In progress save charactor data");
        }

        Debug.Log("[DB] Complete save charactor data");
    }

    // DB에 탐험대원 데이터 저장
    public void SaveCrewData(List<string> filterList)
    {
        Debug.Log("[DB] Come in save crew data");

        //filterList = [Save]ExpenditionCrew|license|charactor|LastSelectCrew|Crew1|Crew2|... |Crew(n)|
        //dataList[0] = [Save]ExpenditionCrew
        //dataList[1] = license

        List<string> dataList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        string update_Command = $"UPDATE `{table.list[2]}` SET";
        for(int i = 2; i < crew_Columns.Length; i++) // license, charactor 제외
        {
            update_Command += $" `{crew_Columns[i]}` = '{Int32.Parse(dataList[i + 1])}',";
        }
        update_Command = update_Command.TrimEnd(',');
        update_Command += $" WHERE `{crew_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{crew_Columns[1]}` = '{Int32.Parse(dataList[2])}'";

        mySqlCommand.CommandText = update_Command;
        mySqlCommand.ExecuteNonQuery();

        Debug.Log("[DB] Complete save crew data");
    }

    // DB에 마지막으로 플레이한 스텝 저장
    public void SaveLastPlayData(List<string> filterList)
    {
        Debug.Log("[DB] Come in save lastplay data");

        // requestData = [Save]LastPlayData|license|charactor| (game1_level1)의 value | (game1_level2)의 value | ... | (game5_level3)의 value |Finish
        // filterList[0] = [Save]LastPlayData|license|charactor| (game1_level1)의 value | (game1_level2)의 value | ... | (game5_level3)의 value |Finish
        // dataList[0] = [Save]LastPlayData
        // dataList[1] = license
        // dataList[2] = charactor
        // ...
        // dataList[9] = venezia_chn_level (step : 4) 
        // ...
        // dataList[15] = gugudan_lev

        List<string> dataList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        string update_Command = $"UPDATE `{table.list[3]}` SET";
        for(int i = 2; i < lastplaygame_Columns.Length; i++) // license, charactor 제외
        {
            update_Command += $" `{lastplaygame_Columns[i]}` = '{Int32.Parse(dataList[i+1])}',";
        }
        update_Command = update_Command.TrimEnd(',');
        update_Command += $" WHERE `{lastplaygame_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{lastplaygame_Columns[1]}` = '{Int32.Parse(dataList[2])}'";

        Debug.Log($"[DB] LastPlayGame Save, update_Command Query : {update_Command}");

        mySqlCommand.CommandText = update_Command;
        mySqlCommand.ExecuteNonQuery();

        Debug.Log("[DB] Complete save lastplay data");
    }

    // DB에 게임결과 저장
    public void SaveGameResultData(List<string> filterList)
    {
        // DB gametable column순 : User_Licensenumber/User_Charactor/ReactionRate/AnswerCount/AnswerRate/Playtime/TotalScore/StarPoint
        // requestName = $"[Save]{gameName}";
        // values = $"{level}|{step}|{clientLicenseNumber}|{clientCharactor}|{datavalue.ReactionRate}|{datavalue.AnswersCount}|{datavalue.Answers}|{datavalue.PlayTime}|{datavalue.TotalScore}|";
        // requestData = $"{requestName}|{values}Finish";
        // filterList[0] => "RequestName|level|step|User_Licensenumber|User_Charactor|ReactionRate|...|TotalScore
        // dataList[0] = [Save]venezia_kor
        // dataList[1] = level
        // dataList[2] = step
        // dataList[3] = User_LicenseNumber
        // dataList[4] = User_Charactor
        // dataList[5] = ReactionRate
        // dataList[6] = AnswerCount
        // dataList[7] = AnswerRate
        // dataList[8] = PlayTime
        // dataList[9] = TotalScore
        // dataList[10] = StarPoint

        Debug.Log("[DB] Come in SaveGameResultData method");

        List<string> dataList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        for (int i = 0; i < dataList.Count; i++)
        {
            Debug.Log($"[DB] Check dataList[{i}] : {dataList[i]}");
        }

        // [Save] 제거
        string gameName = dataList[0].Substring("[Save]".Length);
        int level = Int32.Parse(dataList[1]);
        int step = Int32.Parse(dataList[2]);
        
        // licenseNumber, charactor
        int licenseNumber = Int32.Parse(dataList[3]);
        int charactor = Int32.Parse(dataList[4]);

        // game datas
        float reactionRate = float.Parse(dataList[5]); // 보여줄 분석 데이터(개인분석표, 프로필), 갱신
        int answerCount = Int32.Parse(dataList[6]); // 갱신
        int answerRate = Int32.Parse(dataList[7]); // 보여줄 분석 데이터(개인분석표, 프로필), 갱신
        float playTime = float.Parse(dataList[8]); // 보여줄 데이터(프로필분석, 랭킹), 갱신
        int totalScore = Int32.Parse(dataList[9]); // 보여줄 데이터(프로필분석, 랭킹), 조건부 갱신
        int starPoint = Int32.Parse(dataList[10]); // 조건부 갱신

        // table 이름
        string table_Name = $"{gameName}_level{level}_step{step}";
        if(gameName == "venezia_chn") table_Name = $"{gameName}_level_step{step}";

        // AnalyticsProfile table에 사용하기 위한 변수
        Game_Type gameType;
        if (gameName == "venezia_kor") gameType = Game_Type.A;
        else if (gameName == "venezia_eng") gameType = Game_Type.B;
        else if (gameName == "venezia_chn") gameType = Game_Type.C;
        else if (gameName == "calculation") gameType = Game_Type.D;
        else gameType = Game_Type.E;

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // DB gametable column순 : User_Licensenumber/User_Charactor/ReactionRate/AnswerCount/AnswerRate/Playtime/TotalScore/StarPoint
        string selectScoreStarQuery = $"SELECT `{game_Columns[6]}`, `{game_Columns[7]}` FROM `{table_Name}` " +
                                      $"WHERE `{game_Columns[0]}` = '{licenseNumber}' AND `{game_Columns[1]}` = '{charactor}';";
        mySqlCommand.CommandText = selectScoreStarQuery;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();

        // DB에 있는 최고점수(TotalScore)와 별포인트(starPoint) 갱신할 때 db에 있는 value보다 커야 갱신
        int dbTotalScore = 0;
        int dbStarPoint = 0;

        while(reader.Read())
        {
            dbTotalScore = reader.GetInt32(0);
            dbStarPoint = reader.GetInt32(1);
        }
        reader.Close();

        string updateScoreStartQuery;

        // Score
        if(totalScore > dbTotalScore)
        {
            updateScoreStartQuery = $"UPDATE `{table_Name}` SET `{game_Columns[6]}` = '{totalScore}' " +
                                    $"WHERE `{game_Columns[0]}` = '{licenseNumber}' AND `{game_Columns[1]}` = '{charactor}';";
            mySqlCommand.CommandText = updateScoreStartQuery;
            mySqlCommand.ExecuteNonQuery();
        }

        // StarPoint
        if (starPoint > dbStarPoint)
        {
            updateScoreStartQuery = $"UPDATE `{table_Name}` SET `{game_Columns[7]}` = '{totalScore}' " +
                                    $"WHERE `{game_Columns[0]}` = '{licenseNumber}' AND `{game_Columns[1]}` = '{charactor}';";
            mySqlCommand.CommandText = updateScoreStartQuery;
            mySqlCommand.ExecuteNonQuery();
        }

        MySqlParameter valueParameter = mySqlCommand.Parameters.Add("@value", MySqlDbType.Float);

        // Game Datas Update
        for (int i = 2; i < game_Columns.Length-2; i++) // game_Columns = [0]~[5]
        {
            string updateGameDataQuery = $"UPDATE `{table_Name}` SET `{game_Columns[i]}` = @value ";

            if (i == 2) // reactionRate, float
            {
                valueParameter.MySqlDbType = MySqlDbType.Float;
                valueParameter.Value = reactionRate;
            }
            else if (i == 3) // answerCount, int
            {
                valueParameter.MySqlDbType = MySqlDbType.Int32;
                valueParameter.Value = answerCount;
            }
            else if (i == 4) // answerRate, int
            {
                valueParameter.MySqlDbType = MySqlDbType.Int32;
                valueParameter.Value = answerRate;
            }
            else // playTime, float
            {
                valueParameter.MySqlDbType = MySqlDbType.Float;
                valueParameter.Value = playTime;
            }
            updateGameDataQuery += $"WHERE `{game_Columns[0]}` = '{licenseNumber}' AND `{game_Columns[1]}` = '{charactor}'";
            mySqlCommand.CommandText = updateGameDataQuery;
            mySqlCommand.ExecuteNonQuery();
        }

        // user_info table에 AnswerCount, TotalTime 누적
        UpdateUserInfoTableForScoreTime(licenseNumber, charactor, answerCount, playTime);
        // rank table에 TotalTime과 TotalScore에 점수 누적 
        UpdateRankTable(licenseNumber, charactor, playTime, totalScore);
        // analytics_level(1,2,3)_Profile table Update
        UpdateAnalyticsProfileTable(licenseNumber, charactor, gameType, level, reactionRate, answerRate);

        Debug.Log($"[DB] Complete SaveGameResultData To DB");
    }

    // 게임이 끝날때마다 user_info table에 TotalScore, TotalTime 누적
    private void UpdateUserInfoTableForScoreTime(int licensenumber, int charactor, int answercount, float totaltime)
    {
        Debug.Log($"[DB] Come in UpdateUserInfoTableForScoreTime Method..");

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // userinfo_Columns = "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin"
        string selectQuery = $"SELECT `{userinfo_Columns[5]}`, `{userinfo_Columns[6]}` FROM `{table.list[0]}` " +
                             $"WHERE `{userinfo_Columns[0]}` = '{licensenumber}' AND `{userinfo_Columns[1]}` = '{charactor}';";
        mySqlCommand.CommandText = selectQuery;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();

        // DB user_info table에 있는 answerCount, TotalTime
        int dbAnswerCount = 0;
        float dbTotalTime = 0;

        while(reader.Read())
        {
            dbAnswerCount = reader.GetInt32(0);
            dbTotalTime = reader.GetFloat(1);
        }
        reader.Close();

        dbAnswerCount += answercount;
        dbTotalTime += totaltime;

        // DB user_info table Update
        string updateQuery = $"UPDATE `{table.list[0]}` SET `{userinfo_Columns[5]}` = '{dbAnswerCount}', `{userinfo_Columns[6]}` = '{dbTotalTime}' " +
                             $"WHERE `{userinfo_Columns[0]}` = '{licensenumber}' AND `{userinfo_Columns[1]}` = '{charactor}';";
        mySqlCommand.CommandText = updateQuery;
        mySqlCommand.ExecuteNonQuery();

        Debug.Log($"[DB] Complete UpdateUserInfoTableForScoreTime Method..");
    }

    // 게임이 끝날때마다 rank table에 시간, 점수 누적
    private void UpdateRankTable(int licensenumber, int charactor, float time, int score)
    {
        // rank table -> [0]:User_LicenseNumber, [1]:User_Charactor, [2]:User_Profile, [3]:User_Name, [4]:TotalTime, [5]"TotalScore
        Debug.Log("[DB] Come in UpdateRankTable method");
        Debug.Log($"[DB] licensenumber : {licensenumber}, characator : {charactor}");
        string rankTable = "rank";
        float totalTime = 0;
        int totalScore = 0;

        string selectRankData_Command = $"SELECT * FROM {rankTable}";
        MySqlCommand select_SqlCmd = new MySqlCommand(selectRankData_Command, connection);
        MySqlDataReader reader = select_SqlCmd.ExecuteReader();

        while(reader.Read())
        {
            if ((reader.GetInt32(rank_Columns[0]) == licensenumber) && (reader.GetInt32(rank_Columns[1]) == charactor))
            {
                totalTime = reader.GetFloat(rank_Columns[4]);
                totalScore = reader.GetInt32(rank_Columns[5]);
            }
        }
        reader.Close();

        totalTime += time;
        totalScore += score;

        string updateRankData_Command = $"UPDATE {rankTable} SET {rank_Columns[4]} = {totalTime} WHERE {rank_Columns[0]} = {licensenumber} AND {rank_Columns[1]} = {charactor}";
        MySqlCommand update_SqlCmd = new MySqlCommand(updateRankData_Command, connection);
        update_SqlCmd.ExecuteNonQuery();
        update_SqlCmd.CommandText = $"UPDATE {rankTable} SET {rank_Columns[5]} = {totalScore} WHERE {rank_Columns[0]} = {licensenumber} AND {rank_Columns[1]} = {charactor}";
        update_SqlCmd.ExecuteNonQuery();
    }

    // 게임이 끝날때마다 analytics_level(1,2,3)_Profile table에 플레이 횟수, reactionRate, AnswerRate, LastPlayGame 업데이트(누적)
    private void UpdateAnalyticsProfileTable(int licensenumber, int charactor, Game_Type gametype, int level, float reactionrate, int answerrate)
    {
        Debug.Log("[DB] Come in UpdateAnalyticsProfileTable Method...");

        string tableName = $"analytics_level{level}_profile";
        string[] tableColumns = new string[3];
        string lastPlayGame = "0";
        int dbPlayCount = 0;
        float dbReactionRate = 0;
        int dbAnswerRate = 0;

        switch (gametype)
        {
            case Game_Type.A:
                tableColumns[0] = "Venezia_Kor_PlayCount"; 
                tableColumns[1] = "Venezia_Kor_ReactionRate"; 
                tableColumns[2] = "Venezia_Kor_AnswerRate"; 
                lastPlayGame = "Venezia_Kor";
                break;
            case Game_Type.B:
                tableColumns[0] = "Venezia_Eng_PlayCount";
                tableColumns[1] = "Venezia_Eng_ReactionRate";
                tableColumns[2] = "Venezia_Eng_AnswerRate";
                lastPlayGame = "Venezia_Eng";
                break;
            case Game_Type.C:
                tableColumns[0] = "Venezia_Chn_PlayCount";
                tableColumns[1] = "Venezia_Chn_ReactionRate";
                tableColumns[2] = "Venezia_Chn_AnswerRate";
                lastPlayGame = "Venezia_Chn";
                break;
            case Game_Type.D:
                tableColumns[0] = "Calculation_PlayCount";
                tableColumns[1] = "Calculation_ReactionRate";
                tableColumns[2] = "Calculation_AnswerRate";
                lastPlayGame = "Calculation";
                break;
            case Game_Type.E:
                tableColumns[0] = "Gugudan_PlayCount";
                tableColumns[1] = "Gugudan_ReactionRate";
                tableColumns[2] = "Gugudan_AnswerRate";
                lastPlayGame = "Gugudan";
                break;
            default:
                Debug.Log("[DB] Something wrong");
                break;
        }

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        string select_Command = $"SELECT `{tableColumns[0]}`, `{tableColumns[1]}`, `{tableColumns[2]}` FROM `{tableName}` " +
                                $"WHERE `User_LicenseNumber` = '{licensenumber}' AND `User_Charactor` = '{charactor}';";
        mySqlCommand.CommandText = select_Command;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();

        while(reader.Read())
        {
            dbPlayCount = reader.GetInt32(0);
            dbReactionRate = reader.GetFloat(1);
            dbAnswerRate = reader.GetInt32(0);
        }
        reader.Close();

        int updatePlayCount = dbPlayCount + 1;
        float updateReacitonRate = dbReactionRate + reactionrate;
        int updateAnswerRate = dbAnswerRate + answerrate;

        //string update_Command = $"
        //`{table.list[0]}` SET `{userinfo_Columns[1]}` = `{userinfo_Columns[1]}` - 1 " +
        //$"WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}' AND `{userinfo_Columns[1]}` > '{deletecharactornumber}';";

        string update_Command = $"UPDATE `{tableName}` SET `{tableColumns[0]}`= '{updatePlayCount}', `{tableColumns[1]}` = '{updateReacitonRate}', " +
                                $"`{tableColumns[2]}` = '{updateAnswerRate}', `LastPlayGame` = '{lastPlayGame}' " +
                                $"WHERE `User_LicenseNumber` = '{licensenumber}' AND `User_Charactor` = '{charactor}';";
        mySqlCommand.CommandText = update_Command;
        mySqlCommand.ExecuteNonQuery();

        Debug.Log("[DB] Complete UpdateAnalyticsProfileTable Method...");
    }
    #endregion

    #region Data Load
    // UserData 불러오기
    public List<string> LoadUserData(int clientlicensenumber)
    {
        Debug.Log("[DB] Come in LoadUserData Method");

        // 클라이언트에게 보낼 형태
        // dataList[0] = "[Load]UserData|createdCharactorCount|E|CharactorNumber|Name|Profile|E|"
        // dataList[1] = "CharactorNumber|Name|Profile|E|CharactorNumber|Name|Profile|E|"
        // ... dataList[Last] = "CharactorNumber|Name|Profile|E|

        List<string> return_List = new List<string>();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;
        //Debug.Log("[DB] LoadUserData Make SQL Error");
        // presentDB의 user_info테이블에 있는 Charactor컬럼을 Count, clientlicensenumber가 일치하는곳에서
        string select_Command = $"SELECT `{userinfo_Columns[1]}` FROM `{table.list[0]}` WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}';";
        mySqlCommand.CommandText = select_Command;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();

        int charactorCount = 0;
        while(reader.Read())
        {
            charactorCount++;
        }
        reader.Close();

        return_List.Add($"{charactorCount}|{separatorString}");

        // CharactorNumber|Name|Profile 선택
        select_Command = $"SELECT * FROM `{table.list[0]}` WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}';";
        mySqlCommand.CommandText = select_Command;
        reader = mySqlCommand.ExecuteReader();

        while(reader.Read())
        {
            string charactorNumber = reader.GetInt32($"{userinfo_Columns[1]}").ToString();
            string charactorName = reader.GetString($"{userinfo_Columns[2]}");
            string charactorProfile = reader.GetString($"{userinfo_Columns[3]}");
            string tempData = $"{charactorNumber}|{charactorName}|{charactorProfile}|{separatorString}";
            return_List.Add(tempData);
        }
        reader.Close();

        Debug.Log("[DB] Complete Load UserData From DB");

        return return_List;
    }

    // Player_DB(플레이어 데이터) 불러오기, DB에서 불러와서 서버가 클라이언트한테 쏴줄수 있게 string으로 묶어서 반환형 string
    public List<string> LoadCharactorData(int clientlicensenumber, int clientcharactor)
    {
        Debug.Log("[DB] Come in LoadCharactorData method");

        List<string> return_TableData = new List<string>();
        string return_TempData;

        string selectTable_Command;
        MySqlCommand select_SqlCmd = new MySqlCommand();
        select_SqlCmd.Connection = connection;

        // table.list에서 analyltics_level(1,2,3)_profile 빼야함
        TableName selectTable = new TableName();
        string[] deleteTable = { "rank", "crew", "lastplaygame", "analytics_level1_profile", "analytics_level2_profile", "analytics_level3_profile" };
        /*
         list.Add("rank");
        list.Add("crew");
        list.Add("lastplaygame");
         */

        for (int i = 0; i < deleteTable.Length; i++)
        {
            selectTable.list.Remove(deleteTable[i]);
        }

        for (int i = 0; i < selectTable.list.Count; i++) // table_List
        {
            selectTable_Command = $"SELECT * FROM {selectTable.list[i]}";
            select_SqlCmd.CommandText = selectTable_Command;
            reader = select_SqlCmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int dbLicenseNumber = reader.GetInt32("User_LicenseNumber");
                    int dbcharactor = reader.GetInt32("User_Charactor"); 

                    // client license와 db에 있는 license가 같고 client charactor와 db에 있는 charactor가 같다면 데이터를 불러온다
                    if ((clientlicensenumber == dbLicenseNumber) && (clientcharactor == dbcharactor))
                    {
                        //Debug.Log($"[DB] tableName : {selectTable.list[i]}, dbLicenseNumber : {dbLicenseNumber}, dbcharactor : {dbcharactor}");
                        switch (selectTable.list[i])
                        {
                            case "user_info":
                                Debug.Log("[DB] LoadCharactorData - user_info table");
                                string user_Name = reader.GetString("User_Name");
                                // DB에서 MediumBlob 타입 데이터(Base64 형식으로 저장됨) string으로 가져오기
                                string user_Profile = reader.GetString("User_Profile");
                                //string user_Profile = "0";
                                string user_Birthday = reader.GetString("User_Birthday");
                                string user_TotalAnswers = reader.GetInt32("User_TotalAnswers").ToString();
                                string user_TotalTime = reader.GetFloat("User_TotalTime").ToString();
                                string user_Coin = reader.GetInt32("User_Coin").ToString();
                                // table 구분을 위해 맨 앞에 {selectTable.list[i]} 추가
                                return_TempData = $"{selectTable.list[i]}|{user_Name}|{user_Profile}|{user_Birthday}|{user_TotalAnswers}|{user_TotalTime}|{user_Coin}|{separatorString}"; 
                                return_TableData.Add(return_TempData);
                                break;
                            case "rank": // todo presentdb에 있는 rank table이 아니라 `rank` db에 있는 데이터들을 가져와야함
                                Debug.Log("[DB] LoadCharactorData - rank table");
                                string rank_TotalTime = reader.GetString("TotalTime");
                                string rank_TotalScore = reader.GetString("TotalScore");
                                return_TempData = $"{selectTable.list[i]}|{rank_TotalTime}|{rank_TotalScore}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "crew":
                                Debug.Log("[DB] LoadCharactorData - crew table");
                                return_TempData = $"{selectTable.list[i]}|";
                                for (int j = 2; j < crew_Columns.Length; j++) // 0->licensenumber, 1->charactor
                                {
                                    string tempStr = reader.GetInt32(crew_Columns[j]).ToString();
                                    return_TempData += $"{tempStr}|";
                                    //if(j == 2) // finallyselectedcrew column
                                    //{
                                    //    tempStr = reader.GetInt32(crew_Columns[j]).ToString(); INT
                                    //}
                                    //else
                                    //{
                                    //    tempStr = reader.GetInt32 DB에 저장된 타입이 TINYINT인데, 제대로 꺼내와지나 체크해봐야함
                                    //}
                                }
                                return_TempData += $"{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "lastplaygame":
                                Debug.Log("[DB] LoadCharactorData - lastplaygame table");
                                return_TempData = $"{selectTable.list[i]}";
                                for(int j = 2; j < lastplaygame_Columns.Length; j ++) // 0->licensenumber, 1->charactor
                                {
                                    string tempStr = reader.GetInt32(lastplaygame_Columns[j]).ToString();
                                    return_TempData += $"{tempStr}|";
                                }
                                return_TempData += $"{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            default:
                                //Debug.Log("[DB] LoadCharactorData - game table");
                                string game_ReactionRate = reader.GetFloat("ReactionRate").ToString();
                                string game_AnswerCount = reader.GetInt32("AnswerCount").ToString();
                                string game_AnswerRate = reader.GetInt32("AnswerRate").ToString();
                                string game_Playtime = reader.GetFloat("Playtime").ToString();
                                string game_TotalScore = reader.GetInt32("TotalScore").ToString();
                                string game_StarPoint = reader.GetInt32("StarPoint").ToString();
                                return_TempData = $"{selectTable.list[i]}|{game_ReactionRate}|{game_AnswerCount}|{game_AnswerRate}|{game_Playtime}|{game_TotalScore}|{game_StarPoint}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                        }
                    }
                }
            }
            reader.Close();
        }

        Debug.Log("[DB] End LoadCharactorData method");
        return return_TableData;
    }

    // AnalyticsData(분석 데이터) 불러오기 
    public List<string> LoadAnalyticsData(int licensenumber, int charactor)
    {
        // 클라이언트에서 사용할 List 형식
        // dataList[0] = "[Load]AnalyticsData|E|"
        // dataList[1] = "day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[2] = "day1|venezia_kor_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[3] = "day1|venezia_kor_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[4] = "day1|venezia_eng_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[5] = "day1|venezia_eng_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[6] = "day1|venezia_eng_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[7] = "day1|venezia_chn_analytics|ReactionRate|AnswerRate|E|"
        // dataList[8] = "day1|calculation_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[9] = "day1|calculation_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[10] = "day1|calculation_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[11] = "day1|gugudan_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[12] = "day1|gugudan_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[13] = "day1|gugudan_level3_analytics|ReactionRate|AnswerRate|E|"

        // 가장 최근 날짜부터 찾아들어가야 하므로(가정 및 정의) day1 = 가장 최근 날짜 (List index 앞순서에 배치되므로)
        // day1 = 가장 최근 날짜 ... day7 = 가장 오래된 날짜
        // 해당하는 날짜에 찾고자 하는 게임의 데이터가 없다면 그 앞 날짜
        // 예를 들어 24.03.05 DB에 venezia_kor 게임의 level 1,2를 플레이한 데이터가 있고 level 3을 플레이한 데이터가 없고
        // 24.03.04 DB에 veneiza_kor 게임의 level 1,2,3을 플레이한 데이터가 있다면
        // 개인분석표에서 보여줄 때 24.03.05 일자에는 level 1,2 보여주고 level 3은 보여줄수없음 / 24.03.04 일자에는 level 1,2,3 모두 보여줌
        // DB Name : "24.02.25", ... , "24.03.04", "24.03.05", "24.03.06" ...
        // DB를 조회할 때 가장 오래 저장된 날짜까지(maximum 30일) 찾았는데도 저장된 데이터가 없어서 7일까지 채울수 없다면 default값으로 0을 넣는다.

        Debug.Log("[DB] Come in LoadAnalyticsData Method");

        List<string> return_List = new List<string>();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // 조회할 DB Array
        string[] DBName_Array = new string[30];
        DateTime currentDate = DateTime.Now; // 오늘
        for(int i =0; i < DBName_Array.Length; i++)
        {
            currentDate = currentDate.AddDays(-1); // 전날
            DBName_Array[i] = $"{currentDate.Year.ToString().Substring(2,2)}.{currentDate.Month:00}.{currentDate.Day:00}";
            Debug.Log($"[DB] AnalyticsData, DBName_Array[{i}] : {DBName_Array[i]} ");
        }

        // 조회할 테이블 수만큼 String Type을 받는 List를 생성, 각 배열은 테이블을 의미하고, 해당 테이블의 value가 유의미 한지 아닌지 판단해서 Add로 추가할지 안할지 선택
        List<string>[] tempList_Array = new List<string>[13];

        // 조회할 테이블 Array
        string[] tableName_Array = new string[13];
        for(int i = 0; i < tableName_Array.Length; i++)
        {
            if(i < 3) // venezia_kor
            {
                tableName_Array[i] = $"venezia_kor_level{i+1}_analytics";
            }   
            else if(i < 6) // venezia_eng
            {
                tableName_Array[i] = $"venezia_eng_level{i-2}_analytics";
            }
            else if(i == 6) // venezia_chn
            {
                tableName_Array[i] = $"venezia_chn_analytics";
            }
            else if(i < 10) // calculation
            {
                tableName_Array[i] = $"calculation_level{i-6}_analytics";
            }
            else // gugudan
            {
                tableName_Array[i] = $"gugudan_level{i-9}_analytics";
            }

            // tempList_Array 객체 생성
            tempList_Array[i] = new List<string>();
        }

        // 선택할 컬럼 Array
        string[] columnName_Array = new string[4] { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerRate"};

        // 30일 동안, 최근 날짜(어제)부터 DB조회
        MySqlDataReader reader;
        for (int i = 0; i < DBName_Array.Length; i++)
        {
            Debug.Log($"[DB] Load AnalyticsData, DBName_Array[{i}] : {DBName_Array[i]}");
            try
            {
                // DB내에 있는 라이센스와 캐릭터를 만족하는 테이블 컬럼 조회
                for (int j = 0; j < tableName_Array.Length; j++)
                {
                    string selectTable_Command = $"SELECT `{columnName_Array[2]}`, `{columnName_Array[3]}` FROM `{DBName_Array[i]}`.`{tableName_Array[j]}` " +
                                                 $"WHERE `{columnName_Array[0]}` = {licensenumber} AND `{columnName_Array[1]}` = {charactor};";
                    mySqlCommand.CommandText = selectTable_Command;
                    reader = mySqlCommand.ExecuteReader();
                    float reactionRate = 0;
                    int answerRate = 0;

                    while (reader.Read())
                    {
                        reactionRate = reader.GetFloat(0);
                        answerRate = reader.GetInt32(1);
                    }
                    reader.Close();

                    if (reactionRate == 0 || answerRate == 0) continue; // 저장되어있는 데이터가 0이면(해당 날짜에 게임을 플레이한적없다면) continue

                    // 배열안의 List index를 7개 까지만 채운다. -> 데이터를 가지고 있는 날짜중에 최근 날짜 7일만 
                    if(tempList_Array[j].Count < 7)
                    {
                        // DB로부터 불러온 데이터가 유효하다면 최근 날짜대로 index 순서를 가지게 될것
                        tempList_Array[j].Add($"{DBName_Array[i]}|{tableName_Array[j]}|{reactionRate}|{answerRate}|{separatorString}");
                    }
                }
            }
            catch(Exception e)
            {
                // 일자별로 생성되고 저장되는 DB가 없다면 바로 break / 어떤 날에 DB가 없다면 해당 일자의 이전 날도 DB도 없다(DB는 일자별로 매일 생성되기때문)
                //reader = mySqlCommand.ExecuteReader();
                //reader.Close();
                Debug.Log($"[DB] Select DB is not efficient, Maybe there is not existed database by date : {e.Message}");
                break;
            }
        }

        // 임시로 만든 List의 배열의 길이동안 -> 각 테이블마다
        for(int i = 0; i < tempList_Array.Length; i++)
        {
            // DB에 저장된 날짜가 7일 미만이라서 일의 데이터가 없다면 또는
            // 30일까지 기록된 일자별 DB에 해당 게임의 플레이 데이터가 없다면
            // 등등의 이유로 7일간의 데이터기록이 없다면 나머지 일수(index)를 0으로 채워서 판단할수 있게 채운다
            while (tempList_Array[i].Count < 7)
            {
                // day|game_name|reactionRate|answerRate|E| / value 값들에 들어가는 것이기 때문에 0000으로 보내도 상관없음
                tempList_Array[i].Add($"0|0|0|0|{separatorString}");
            }
        }

        // 일자별로 return_List에 옮겨 담기
        //tempList_Array[0][0]
        //tempList_Array[1][0]
        //tempList_Array[2][0]
        for(int i = 0; i < tempList_Array[0].Count; i++) 
        {
            for (int j = 0; j < tempList_Array.Length; j++)
            {
                return_List.Add(tempList_Array[j][i]);
                Debug.Log($"[DB] Load AnalyticsData, tempList_Array[{j}][{i}] : {tempList_Array[j][i]}");
            }
        }

        Debug.Log("[DB] Complete Load AnalyticsData From DB");

        return return_List;
    }

    // RankData(랭크 데이터) 불러오기 
    public List<string> LoadRankData(int licensenumber, int charactor)
    {
        Debug.Log("[DB] Come in LoadRankData Method..");
        //////////////////////////////// SELECT * FROM `present`.`user_info` ORDER BY `User_LicenseNumber` ASC LIMIT 1000; DB ORDER BY 생각할것
        ///// ASC = Ascend, DESC = Descend

        // weeklyrank DB에 있는 table 사용
        // Score, Time 별 Rank 1~5위 및 6번째 자기 자신의 데이터 

        /*
         public class RankData
        {
        // 0~4 -> 1~5등 / 5 -> 개인 순위 및 점수
        public RankData_value[] rankdata_score;
        public RankData_value[] rankdata_time;
        }

        public class RankData_value
        {
        public int userlicensenumber;
        public int usercharactor;
        public byte[] userProfile;
        public string userName;
        public int totalScore;
        public float totalTime;
        // 개인(index 5)용 순위
        public int scorePlace;
        public int timePlace;
        public int highScorePlace;
        public int highTimePlace;
        }
         */

        // 클라이언트에서 사용할 List 형식
        // dataList[0] = "[Load]RankData|place|profile|name|time or score|E|";
        // dataList[1] = "profile|name|time or score|E|"; // parts.Count == 1
        // or dataList[1] = "place|profile|name|time or score|E|place|profile|name|time or score|E|"; // parts.Count == 2
        // dataList[2] = profile|name|totalScore|E|
        // dataList[6] = profile|name|totalScore|scorePlace|highestScorePlace|E|
        // dataList[7] = profile|name|totalTime|E|

        // dataList[last] = profile|name|totalTime|timePlace|highestTimePlace|E|

        // weeklyRank_Columns = { "User_LicenseNumber", "User_Charactor", "User_Profile", "User_Name", "TotalScore", "TotalTime", "ScorePlace", "TimePlace", "HighestScorePlace", "HighestTimePlace" };
        List<string> return_List = new List<string>();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // Load할 rank Table이 없다면
        string showQuery = $"USE `{weeklyRankDB}`;" +
                           $"SHOW TABLES LIKE '{referenceWeeklyRankTableName}'";
        mySqlCommand.CommandText = showQuery;
        object result = mySqlCommand.ExecuteScalar();
        
        if(result == null)
        {
            Debug.Log($"[DB] WeeklyRankTable is not exist on DB, referenceWeeklyRankTableName : {referenceWeeklyRankTableName}");
            // rankTable이 없다는 것을 표시
            return_List.Add("NOTHING|");

            // presentDB로 변경
            mySqlCommand.CommandText = $"USE `{presentDB}`;";
            mySqlCommand.ExecuteNonQuery();

            return return_List;
        }

        // Score 1~5등 
        string rankScoreSelectQuery = $"SELECT * FROM `{weeklyRankDB}`.`{referenceWeeklyRankTableName}` ORDER BY `ScorePlace` DESC LIMIT 5;";
        mySqlCommand.CommandText = rankScoreSelectQuery;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();
        //if(!reader.IsClosed)
        //{
        //    reader.Close
        //}

        while(reader.Read())
        {
            string profile = reader.GetString("User_Profile");
            string name = reader.GetString("User_Name");
            int totalScore = reader.GetInt32("TotalScore");

            return_List.Add($"{profile}|{name}|{totalScore}|{separatorString}");
        }
        reader.Close();

        // 예외처리, return_List에 5명의 기록이 없으면 return_List에 index[4]까지 빈내용으로 저장
        if (return_List.Count < 5)
        {
            int user_Count = return_List.Count;

            while (user_Count < 5)
            {
                //보낼 데이터(value) Profile/Name/totalScore
                return_List.Add($"0|None|0|{separatorString}");
                user_Count++;
            }
        }

        // Score 6등 - 요청한 client
        string clientRankScoreSelectQuery = $"SELECT * FROM `{weeklyRankDB}`.`{referenceWeeklyRankTableName}` " +
                                            $"WHERE `{weeklyRank_Columns[0]}` = '{licensenumber}' AND `{weeklyRank_Columns[1]}` = '{charactor}';";
        mySqlCommand.CommandText = clientRankScoreSelectQuery;
        reader = mySqlCommand.ExecuteReader();

        if(!reader.Read())
        {
            // 요청한 클라이언트의 데이터가 weeklyrank table에 없다면 (신규유저 등)
            return_List.Add($"0|None|0|0|0|{separatorString}");
        }
        else
        {
            string profile = reader.GetString("User_Profile");
            string name = reader.GetString("User_Name");
            int totalScore = reader.GetInt32("TotalScore");
            int scorePlace = reader.GetInt32("ScorePlace");
            int highestScorePlace = reader.GetInt32("HighestScorePlace"); // test후 변경 todo

            return_List.Add($"{profile}|{name}|{totalScore}|{scorePlace}|{highestScorePlace}|{separatorString}");
        }
        reader.Close();

        // Time 1~5등 
        string rankTimeSelectQuery = $"SELECT * FROM `{weeklyRankDB}`.`{referenceWeeklyRankTableName}` ORDER BY `TimePlace` DESC LIMIT 5;";
        mySqlCommand.CommandText = rankTimeSelectQuery;
        reader = mySqlCommand.ExecuteReader();

        while (reader.Read())
        {
            string profile = reader.GetString("User_Profile");
            string name = reader.GetString("User_Name");
            float totalTime = reader.GetFloat("TotalTime");

            return_List.Add($"{profile}|{name}|{totalTime}|{separatorString}");
        }
        reader.Close();

        // 예외처리, return_List에 (score 6) + 5명의 기록이 없으면 return_List에 index[10]까지 빈내용으로 저장
        if (return_List.Count < 11)
        {
            int user_Count = return_List.Count;

            while (user_Count < 11)
            {
                //보낼 데이터(value) Profile/Name/totalTime
                return_List.Add($"0|None|0|{separatorString}");
                user_Count++;
            }
        }

        // Time 6등 - 요청한 client
        string clientRankTimeSelectQuery = $"SELECT * FROM `{weeklyRankDB}`.`{referenceWeeklyRankTableName}` " +
                                            $"WHERE `{weeklyRank_Columns[0]}` = '{licensenumber}' AND `{weeklyRank_Columns[1]}` = '{charactor}';";
        mySqlCommand.CommandText = clientRankTimeSelectQuery;
        reader = mySqlCommand.ExecuteReader();

        if (!reader.Read())
        {
            // 요청한 클라이언트의 데이터가 weeklyrank table에 없다면 (신규유저 등)
            return_List.Add($"0|None|0|0|0|{separatorString}");
        }
        else
        {
            string profile = reader.GetString("User_Profile");
            string name = reader.GetString("User_Name");
            float totalTime = reader.GetFloat("TotalTime");
            int timePlace = reader.GetInt32("TimePlace");
            int highestTimePlace = reader.GetInt32("HighestTimePlace"); // test후 변경 todo

            return_List.Add($"{profile}|{name}|{totalTime}|{timePlace}|{highestTimePlace}|{separatorString}");
        }
        reader.Close();

        Debug.Log("[DB] Complete LoadRankData Method!!");

        // presentDB로 변경
        mySqlCommand.CommandText = $"USE `{presentDB}`;";
        mySqlCommand.ExecuteNonQuery();

        // Rank 주간 날짜 보내주기
        return_List.Add($"{referenceWeeklyRankTableName}|{separatorString}");

        return return_List;
    }

    // ExpenditionData(탐험대원 데이터) 불러오기
    public List<string> LoadExpenditionCrew(int licensenumber, int charactor)
    {
        Debug.Log("[DB] Come in LoadExpenditionCrew Method");

        // 클라이언트에서 사용할 데이터 형식 
        // dataList[0] = "[Load]ExpenditionCrew|LastSelectCrew|crew1|crew2|...|crew(n)|"
        // DB가 보내줘야할 형식
        // List = [0] -> LastSelectCrew|, [1] -> crew1|, [2] -> crew2|, ...|, [n] -> crew(n)|
        // presentdb 사용

        List<string> return_List = new List<string>();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // 조회할 table
        string select_Command = $"SELECT * FROM `{table.list[2]}` WHERE `{crew_Columns[0]}` = {licensenumber} AND `{crew_Columns[1]}` = {charactor};";
        mySqlCommand.CommandText = select_Command;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();

        while(reader.Read())
        {
            for(int i = 2; i < crew_Columns.Length; i++) // licensenumber, charactor 제외
            {
                return_List.Add($"{reader.GetInt32(crew_Columns[i])}|");
            }
        }
        reader.Close();

        Debug.Log("[DB] Complete Load ExpenditionCrew From DB");

        return return_List;
    }

    // LastPlayData(마지막 플레이 데이터) 불러오기
    public List<string> LoadLastPlayData(int licensenumber, int charactor)
    {
        Debug.Log("[DB] Come in LoadLastPlayData Method");

        // 클라이언트에서 사용할 데이터 형식 
        // dataList[0] = [Load]LastPlayData|(game1_level1)의 value|(game1_level2)의 value| ... |(game5_level3)의 value|
        // DB가 보내줘야할 형식
        // List = [0] -> (game1_level1)의 value|, [1] -> (game1_level2)의 value, ...|, [n] -> (game5_level3)의 value|
        // presentdb 사용

        List<string> return_List = new List<string>();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // 조회할 table -> lastplaygame
        string select_Command = $"SELECT * FROM `{table.list[3]}` WHERE `{lastplaygame_Columns[0]}` = {licensenumber} AND `{lastplaygame_Columns[1]}` = {charactor};";
        mySqlCommand.CommandText = select_Command;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();

        while (reader.Read())
        {
            for (int i = 2; i < lastplaygame_Columns.Length; i++) // licensenumber, charactor 제외
            {
                return_List.Add($"{reader.GetInt32(lastplaygame_Columns[i])}|");
            }
        }
        reader.Close();

        Debug.Log("[DB] Complete Load LastPlayData From DB");

        return return_List;
    }

    // AnalyticsProfileData(프로필용 분석데이터) 불러오기
    public List<string> LoadAnalyticsProfileData(int licensenumber, int charactor)
    {
        Debug.Log("[DB] Come in LoadAnalyticsProfileData Method");

        // 클라이언트에서 사용할 데이터 형식 
        // dataList[0] = [Load]AnalyticsProfileData|Level1_게임명|Level1_평균반응속도|Level1_평균정답률|
        //                + Level2_게임명|Leve2_평균반응속도|Level2_평균정답률|Level3_게임명|Leve3_평균반응속도|Level3_평균정답률|
        // DB가 Server에 반환할 List
        // List = [0] -> Level1_게임명| / [1] -> Level1_평균반응속도| / ... / [n] -> Level3_평균정답률|
        // presentdb 사용

        // 테이블 1,2,3에 접근해서 analyltics_level1_profile, analyltics_level2_profile, analyltics_level3_profile
        // 각 게임별 playCount를 비교한 뒤 가장 많이 플레이한 게임을 정하고 해당 게임의 반응속도와 정답률을 가져온다.
        // playCount가 동일할 경우 LastPlayGame Column을 참고해서 가장 많이 플레이한 게임을 정한다.

        List<string> return_List = new List<string>();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // 조회할 table -> analytics_level(1,2,3)_profile
        for(int i = 0; i < 3; i ++)
        {
            string tableName;
            string[] tableColumns;
            string[] gameNames;

            if (i == 0)
            {
                tableName = table.list[4];
                tableColumns = analyticsProfile1_Columns;
                gameNames = new string[] { "Venezia_Kor", "Venezia_Eng", "Venezia_Chn", "Calculation", "Gugudan" };
            }
            else if (i == 1)
            {
                tableName = table.list[5];
                tableColumns = analyticsProfile2_Columns;
                gameNames = new string[] { "Venezia_Kor", "Venezia_Eng", "Calculation", "Gugudan" };
            }
            else
            {
                tableName = table.list[6];
                tableColumns = analyticsProfile2_Columns;
                gameNames = new string[] { "Venezia_Kor", "Venezia_Eng", "Calculation", "Gugudan" };
            }

            string select_Command = $"SELECT * FROM `{tableName}` WHERE `{tableColumns[0]}` = {licensenumber} AND `{tableColumns[1]}` = {charactor};";
            mySqlCommand.CommandText = select_Command;
            MySqlDataReader reader = mySqlCommand.ExecuteReader();

            // 게임별로 게임명 / PlayCount / ReactionRate / AnswerRate 을 담을 List<Tuple>
            List<Tuple<string, int, float, int>> tuple_List = new List<Tuple<string, int, float, int>>();

            string lastPlayGame = "0";

            while (reader.Read())
            {
                for (int j = 2; j < tableColumns.Length; j++) // licensenumber, charactor 제외
                {
                    if (j % 3 == 2 && (j != tableColumns.Length - 1))
                    {
                        string gameName = gameNames[j / 3];
                        int playCount = reader.GetInt32(j);
                        float reactionRate = reader.GetFloat(j + 1);
                        int answerRate = reader.GetInt32(j + 2);

                        Tuple<string, int, float, int> temp_Tuple = new Tuple<string, int, float, int>(gameNames[j / 3], playCount, reactionRate, answerRate);
                        tuple_List.Add(temp_Tuple);
                    }
                    else if (j == tableColumns.Length - 1) lastPlayGame = reader.GetString(j);
                }
            }
            reader.Close();

            // PlayCount를 기준으로 내림차순 정렬
            tuple_List.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            // return_List에 보낼 데이터 추가
            if (lastPlayGame == "0") // 해당 레벨의 어떤 게임도 플레이 하지 않음
            {
                return_List.Add($"{lastPlayGame}|0|0|");
            }
            else
            {
                if(tuple_List[0].Item2 == tuple_List[1].Item2) // 두 게임의 PlayCount가 같다면
                {
                    for(int j = 0; j < tuple_List.Count; j++)
                    {
                        if(tuple_List[j].Item1 == lastPlayGame)
                        {
                            float reactionRate = tuple_List[j].Item3 / tuple_List[j].Item2;
                            int answerRate = (int)(tuple_List[j].Item4 / tuple_List[j].Item2);

                            return_List.Add($"{tuple_List[j].Item1}|{reactionRate}|{answerRate}|");
                        }
                    }
                }
                else // PlayCount가 가장 많은 게임
                {
                    float reactionRate = tuple_List[0].Item3 / tuple_List[0].Item2;
                    int answerRate = (int)(tuple_List[0].Item4 / tuple_List[0].Item2);

                    return_List.Add($"{tuple_List[0].Item1}|{reactionRate}|{answerRate}|");
                }
            }

        }
        
        Debug.Log("[DB] Complete Load AnalyticsProfileData From DB");

        return return_List;
    }

    // 캐릭터 변경시 ClientCharactorNumber 불러오기
    public List<string> ChangeCharactor(int licensenumber, int charactor)
    {
        Debug.Log("[DB] Come in ChangeCharactor Method");

        // 클라이언트에서 사용할 데이터 형식 
        // dataList[0] = [Load]LastPlayData|ChangedCharactorNumber|
        // DB가 보내줘야할 형식
        // List = [0] -> ChangedCharactorNumber|
        // presentdb 사용

        List<string> return_List = new List<string>();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // 조회할 table -> user_info
        string select_Command = $"SELECT {userinfo_Columns[1]} FROM `{table.list[0]}` WHERE `{userinfo_Columns[0]}` = {licensenumber} AND `{userinfo_Columns[1]}` = {charactor};";
        mySqlCommand.CommandText = select_Command;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();

        while (reader.Read())
        {
            return_List.Add($"{reader.GetInt32(userinfo_Columns[1])}|");
        }
        reader.Close();

        Debug.Log("[DB] Complete Changer Charactor From DB");

        return return_List;
    }
    #endregion

    // 캐릭터 제거
    public void DeleteCharactor(int clientlicensenumber, int deletecharactornumber)
    {
        Debug.Log("[DB] Come in DeleteCharactor Method..");

        // 캐릭터를 제거한 후 해당 캐릭터 번호보다 높은 번호를 가진 캐릭터들의 번호 -1씩 내려줌
        // 예를 들어 1~5번 캐릭터가 존재하는데, 3번캐릭터를 제거하면 4,5번 캐릭터 -> 3,4번 캐릭터번호로
        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        //// userinfo table
        //string delete_Command = $"DELETE FROM `{table.list[0]}` WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}' AND `{userinfo_Columns[1]}` = '{deletecharactornumber}';";
        //mySqlCommand.CommandText = delete_Command;
        //mySqlCommand.ExecuteNonQuery();
        //
        //// Shift charactor number in the table that match the clientlicensenumber and deletecharactornumber
        //string update_Command = $"UPDATE `{table.list[0]}` SET `{userinfo_Columns[1]}` = `{userinfo_Columns[1]}` - 1 " +
        //                        $"WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}' AND `{userinfo_Columns[1]}` > '{deletecharactornumber}';";
        //mySqlCommand.CommandText = update_Command;
        //mySqlCommand.ExecuteNonQuery();

        for(int i = 0; i < table.list.Count; i++)
        {
            string delete_Command = $"DELETE FROM `{table.list[i]}` WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}' AND `{userinfo_Columns[1]}` = '{deletecharactornumber}';";
            mySqlCommand.CommandText = delete_Command;
            mySqlCommand.ExecuteNonQuery();

            string update_Command = $"UPDATE `{table.list[i]}` SET `{userinfo_Columns[1]}` = `{userinfo_Columns[1]}` - 1 " +
                                $"WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}' AND `{userinfo_Columns[1]}` > '{deletecharactornumber}';";
            mySqlCommand.CommandText = update_Command;
            mySqlCommand.ExecuteNonQuery();
        }

        Debug.Log("[DB] Complete DeleteCharactor Method..");
    }

    // 캐릭터 프로필 일부 데이터 초기화
    public void ResetCharactorProfile(int clientlicensenumber, int clientcharactor)
    {
        Debug.Log("[DB] Come in ResetCharactorProfile Method..");

        // user_info table의 User_TotalAnswer(누적정답개수), User_TotalTime(누적플레이시간) 초기화
        // analytics_level(1,2,3)_profile table 초기화
        // 게임데이터의 starpoint가 초기화되지는 모름, 사진도 모름, 음 정확하게 잘 모르겠는거는 내일 물어본다. todo

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        string update_Command = $"UPDATE `{table.list[0]}` SET `User_TotalAnswers` = '0', `User_TotalTime` = '0' " +
                                $"WHERE `{userinfo_Columns[0]}` = '{clientlicensenumber}' AND `{userinfo_Columns[1]}` = '{clientcharactor}';";
        mySqlCommand.CommandText = update_Command;
        mySqlCommand.ExecuteNonQuery();

        // 조회할 table -> analytics_level(1,2,3)_profile
        for (int i = 0; i < 3; i++)
        {
            string tableName;
            string[] tableColumns;
            string[] gameNames;

            if (i == 0)
            {
                tableName = table.list[4];
                tableColumns = analyticsProfile1_Columns;
                gameNames = new string[] { "Venezia_Kor", "Venezia_Eng", "Venezia_Chn", "Calculation", "Gugudan" };
            }
            else if (i == 1)
            {
                tableName = table.list[5];
                tableColumns = analyticsProfile2_Columns;
                gameNames = new string[] { "Venezia_Kor", "Venezia_Eng", "Calculation", "Gugudan" };
            }
            else
            {
                tableName = table.list[6];
                tableColumns = analyticsProfile2_Columns;
                gameNames = new string[] { "Venezia_Kor", "Venezia_Eng", "Calculation", "Gugudan" };
            }

            update_Command = $"UPDATE `{tableName}` SET ";
            for(int j = 2; j < tableColumns.Length; j++) // licensenumber, charactor 제외
            {
                update_Command += $"`{tableColumns[j]}` = '0', ";
            }
            update_Command = update_Command.TrimEnd(',');
            update_Command += $"WHERE `{tableColumns[0]}` = '{clientlicensenumber}' AND `{tableColumns[1]}` = '{clientcharactor}';";

            mySqlCommand.CommandText = update_Command;
            mySqlCommand.ExecuteNonQuery();
        }

        Debug.Log("[DB] Complete ResetCharactorProfile Method!");
    }

    #endregion

    #region Server only use
    // DateDB 생성 / 하루가 지났을 때 presentDB gamedata -> 요일DB 생성하고 gamedata 저장
    public void CreateDateDB()
    {
        Debug.Log("[DB] Come in CreateDateDB");

        // 생성할 DB 이름
        Debug.Log($"[DB] presentTime, DateTime.Now.AddDays : {DateTime.Now}");
        DateTime presentTime = DateTime.Now.AddDays(-1);
        string DBName = $"{presentTime.Year.ToString().Substring(2,2)}.{presentTime.Month:00}.{presentTime.Day:00}";
        Debug.Log($"DBName : {DBName}");
        // 게임 테이블 명, TableName은 생성되면 알아서 테이블들이 담김. 게임을 제외한 나머지 테이블 제거
        TableName gameTable = new TableName();
        string[] deleteTable = { "user_info", "rank", "crew", "lastplaygame", "analytics_level1_profile", "analytics_level2_profile", "analytics_level3_profile" };
        for (int i = 0; i < deleteTable.Length; i++)
        {
            gameTable.list.Remove(deleteTable[i]);
        }

        // todo 그럴일은 없겠지만 나중에 DBName이 중복되는 DateDB가 이미 있다면 바로 return;
        Debug.Log("[DB] Already existed DateDB So return");

        // DateDB생성
        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.CommandText = $"CREATE DATABASE IF NOT EXISTS `{DBName}`;"; // DB생성 명령문
        mySqlCommand.Connection = connection;
        mySqlCommand.ExecuteNonQuery();
        Debug.Log("[DB] Complete Create DateDB");
            

        // DateDB사용
        mySqlCommand.CommandText = $"USE `{DBName}`;";
        mySqlCommand.ExecuteNonQuery();

        // Table생성 명령문
        // 한 테이블당 생성하고 컬럼들 생성
        // game_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };
        for (int i = 0; i < gameTable.list.Count; i++)
        {
            string createTable_Command = $"CREATE TABLE IF NOT EXISTS {gameTable.list[i]} (";
            for (int j = 0; j < game_Columns.Length; j++)
            {
                if (j == 2 || j == 5) // float -> ReactionRate, Playtime
                {
                    createTable_Command += $"{game_Columns[j]} float DEFAULT NULL,";
                }
                else // int -> 나머지
                {
                    createTable_Command += $"{game_Columns[j]} int(11) DEFAULT NULL,";
                }
            }
            createTable_Command = createTable_Command.TrimEnd(','); // 마지막 쉼표 제거
            createTable_Command += $") ENGINE = InnoDB DEFAULT CHARSET = latin1 COLLATE = latin1_swedish_ci;";
            //ENGINE = InnoDB DEFAULT CHARSET = latin1 COLLATE = latin1_swedish_ci;

            // Table 생성
            MySqlCommand createTable_SqlCmd = new MySqlCommand(createTable_Command, connection);
            createTable_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] Complete Create DateDB GameData table");

        // PresentDB 사용
        mySqlCommand.CommandText = $"USE `present`;";
        mySqlCommand.ExecuteNonQuery();
        Debug.Log($"[DB] 1)");

        // PresentDB의 (table에 있는 (한 행의 Column Data를 담은 List)들(행 / 유저,캐릭터)으로 담은 List)들을 Table개수만큼 Array를 가지는 변수
        List<List<string>>[] valuesInColumnsInTable_Array = new List<List<string>>[gameTable.list.Count];
        Debug.Log($"[DB] 2)");

        // PresentDB에 있는 gamedata들 불러와서 위 변수에 저장
        for (int i = 0; i < gameTable.list.Count; i++)
        {
            Debug.Log($"[DB] 3)");
            // table에 몇 행이 있는지
            string rowCount_Command = $"SELECT COUNT(*) FROM `{gameTable.list[i]}`";
            MySqlCommand rowCount_SqlCmd = new MySqlCommand(rowCount_Command, connection);
            // Int 캐스팅
            object result = rowCount_SqlCmd.ExecuteScalar(); // ExcuteScalar() 메서드는 쿼리를 실행하고 결과 집합의 첫 번째 행의 첫 번째 열의 값을 반환
            result = (result == DBNull.Value) ? null : result;
            int rowCountInTable = Convert.ToInt32(result);
            Debug.Log($"[DB] rowCountInTable : {rowCountInTable}");

            string select_Command = $"SELECT * FROM {gameTable.list[i]}";
            MySqlCommand select_SqlCmd = new MySqlCommand(select_Command, connection);
            MySqlDataReader reader = select_SqlCmd.ExecuteReader();
            Debug.Log($"[DB] reader open? ");

            // List<List<string>> i번째 index 초기화
            valuesInColumnsInTable_Array[i] = new List<List<string>>();

            List<List<string>> column_Values_List = new List<List<string>>();
            int count = 0;

            while (reader.Read()) //reader.Read()는 모든 행을 읽는데, 1행->2행->3행순으로.. 해당 테이블에 있는 모든 데이터를 읽는다
            {
                //for (int j = 0; j < rowCountInTable; j++) // 근데 모든 행을 8번 더 읽는다. 그러니까 8*8하니까 64행을 읽는거지.
                
                // table에 있는 Column들의 Data를 담은 List
                List<string> column_Values = new List<string>();

                for (int k = 0; k < game_Columns.Length; k++)
                {
                    if (k == 2 || k == 5) // float
                    {
                        column_Values.Add(reader.GetFloat(game_Columns[k]).ToString());
                    }
                    else // int
                    {
                        column_Values.Add(reader.GetInt32(game_Columns[k]).ToString());
                    }
                    Debug.Log($"[DB] i:{i}, j:{count}, k:{k}, column_Values[k]:{column_Values[k]} ");
                }

                column_Values_List.Add(column_Values);
            }
            reader.Close();

            valuesInColumnsInTable_Array[i] = column_Values_List;
        }
        Debug.Log("[DB] Complete copy presentDB");

        // DataBase 새로고침 또는 갱신
        mySqlCommand.CommandText = $"SHOW DATABASES";
        MySqlDataReader showDBReader = mySqlCommand.ExecuteReader();
        while(showDBReader.Read())
        {
            string showDBName = showDBReader.GetString(0);
            Debug.Log($"[DB] showDBName = {showDBName}");
            if (showDBName == DBName)
            {
                Debug.Log($"[DB] Date DB is Exist : {DBName}");
            }
        
        }
        showDBReader.Close();

        // 생성한 DateDB 사용
        mySqlCommand.CommandText = $"USE `{DBName}`;"; // 24.03.07과 `24.03.07`은 SQL언어에서 다르게 받아들인다
        mySqlCommand.ExecuteNonQuery();
        Debug.Log($"[DB] 1");
        // @value Parameter 생성
        MySqlParameter valueParameter = mySqlCommand.Parameters.Add("@value", MySqlDbType.Float);
        Debug.Log($"[DB] 1.5");
        

        // DateDB에 presentDB에서 가져온 데이터(게임데이터만) 복사 생성
        for (int i = 0; i < gameTable.list.Count; i ++) // Table
        {
            Debug.Log($"[DB] 2");
            for (int j=0; j < valuesInColumnsInTable_Array[i].Count; j ++) // Columns
            {
                Debug.Log($"[DB] maybe count is 8, valuesInColumnsInTable_Array[i].Count : {valuesInColumnsInTable_Array[i].Count}");
                // 0 -> licenseNumber, 1 -> charactor
                string insert_Command = $"INSERT INTO {gameTable.list[i]} (`{game_Columns[0]}`,`{game_Columns[1]}`) " +
                                        $"VALUES ('{Int32.Parse(valuesInColumnsInTable_Array[i][j][0])}','{Int32.Parse(valuesInColumnsInTable_Array[i][j][1])}');";
                Debug.Log($"[DB] insert_Command : {insert_Command}");
                Debug.Log($"[DB] Int32.Parse(valuesInColumnsInTable_Array[i][j][0]) : {Int32.Parse(valuesInColumnsInTable_Array[i][j][0])}");
                Debug.Log($"[DB] Int32.Parse(valuesInColumnsInTable_Array[i][j][1]) : {Int32.Parse(valuesInColumnsInTable_Array[i][j][1])}");
                mySqlCommand.CommandText = insert_Command;
                mySqlCommand.ExecuteNonQuery();
                Debug.Log($"[DB] Insert i:{i}, j:{j}");

                for (int k = 2; k < valuesInColumnsInTable_Array[i][j].Count; k++) // Column
                {
                    string update_Command = $"UPDATE {gameTable.list[i]} SET `{game_Columns[k]}` = @value " +
                                            $"WHERE `{game_Columns[0]}` = '{Int32.Parse(valuesInColumnsInTable_Array[i][j][0])}' AND `{game_Columns[1]}` = '{Int32.Parse(valuesInColumnsInTable_Array[i][j][1])}';";
                    mySqlCommand.CommandText = update_Command;

                    Debug.Log($"[DB] Update i:{i}, j:{j}, k:{k}");

                    if (k == 2 || k == 5) // float
                    {
                        valueParameter.MySqlDbType = MySqlDbType.Float;
                        valueParameter.Value = float.Parse(valuesInColumnsInTable_Array[i][j][k]);
                        //mySqlCommand.Parameters.Add("@value", MySqlDbType.Float).Value = float.Parse(valuesInColumnsInTable_Array[i][j][k]);
                    }
                    else // int
                    {
                        valueParameter.MySqlDbType = MySqlDbType.Int32;
                        valueParameter.Value = Int32.Parse(valuesInColumnsInTable_Array[i][j][k]);
                        //mySqlCommand.Parameters.Add("@value", MySqlDbType.Int32).Value = Int32.Parse(valuesInColumnsInTable_Array[i][j][k]);
                    }
                    mySqlCommand.ExecuteNonQuery();
                }
            }
        }
        Debug.Log("[DB] Complete paste presentDB to DateDB only gamedatas");

        // 분석테이블에 있는 반응속도, 정답률은 각 게임의 레벨별 스텝에 있는 반응속도, 정답률 갖고와서 평균을 낸 후 저장
        // 예를들면 베네치아 게임 레벨 1의 6스텝중에 3개의 스텝만 플레이 했다면, 3개 스텝의 반응속도, 정답률 평균을 낸다. 즉 (x1+x2+x3)/3 해버린다.
        // (∑n)/n
        // 게임테이블에서 각 게임의 레벨별 스텝1~6의 Column중 ReactionRate와 AnswerRate만 참조한다. 값이 0이면 무시 
        //string selectTable_Command = $"SELECT * FROM `{tableName_Array[j]}` WHERE `{columnName_Array[0]}` = {licensenumber} AND `{columnName_Array[1]}` = {charactor};"

        // 클래스로 만들기 
        //List<Tuple<int, int, float, int>>[][] tuple_Structure = new List<Tuple<int, int, float, int>>[analyticsTable.list.Count][];
        // 13개의 anlyticsTable -> Array, 테이블의 개수를 알고 있음
        // 그 테이블의 행 개수 valuesInColumnsInTable_Array[i].Count -> Array, 위에서 presentDB의 데이터를 가져올 때 행의 개수를 알고 있음
        // 한 행의 컬럼 수 -> List, List로 한 이유는 해당 게임을 플레이 하지 않았다면(값이 0) 데이터를 사용할 이유가 없어 추가할 필요가 없음
        List<AnalyticsColumnValue>[][] valueList_ColumnArray_TableArray = new List<AnalyticsColumnValue>[analyticsTable.list.Count][];

        Debug.Log($"[DB] Check etcMethodHandler is null? : {(etcMethodHandler == null ? "it's null" : "it's initialized")}");

        for (int i = 0; i < gameTable.list.Count; i++)
        {
            switch(i/6) // 스텝1~6
            {
                case 0: // venezia_kor_level1_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    //Debug.Log($"[DB] i:{i}, creating analyticsTable");
                    //Debug.Log($"[DB] List<AnalyticsColumnValue>[][] valueList_ColumnArray_TableArray.Length = {valueList_ColumnArray_TableArray.Length}");
                    //Debug.Log($"[DB] valueList_ColumnArray_TableArray[i/6] = {(valueList_ColumnArray_TableArray[i/6] == null? "null":"have")}");
                    break;
                case 1: // venezia_kor_level2_analytics
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 2: // venezia_kor_level3_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 3: // venezia_eng_level1_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;         
                case 4: // venezia_eng_level2_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;         
                case 5: // venezia_eng_level3_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 6: // venezia_chn_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 7: // calculation_level1_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break; 
                case 8: // calculation_level2_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break; 
                case 9: // calculation_level3_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 10: // gugudan_level1_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;  
                case 11: // gugudan_level2_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;  
                case 12: // gugudan_level3_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                default:
                    Debug.Log("[DB] index i is unexpected value In CreateDB");
                    break;
            }
        }
        Debug.Log("[DB] Complete etcMethodHandler.AddAnalyticsColumnValueInDB");

        // 반복문 밖에서 매개변수 추가
        mySqlCommand.Parameters.Clear();
        mySqlCommand.Parameters.Add("@licenseNumber", MySqlDbType.Int32);
        mySqlCommand.Parameters.Add("@charactor", MySqlDbType.Float);
        mySqlCommand.Parameters.Add("@reactionRate", MySqlDbType.Float);
        mySqlCommand.Parameters.Add("@answerRate", MySqlDbType.Int32);

        // 평균 반응속도, 정답률 구하고 분석 table 생성
        for (int i = 0; i < valueList_ColumnArray_TableArray.Length; i++) // 13개 table
        {
            // 테이블을 만들고, 컬럼을 만든 후 데이터를 Insert하자.
            string analyticsTableName = $"{analyticsTable.list[i]}";
            string createAnalyticsTable_Command = $"CREATE TABLE IF NOT EXISTS {analyticsTable.list[i]} (";

            for(int j = 0; j < analytics_Columns.Length; j++)
            {
                if(j ==2) // float -> ReactionRate
                {
                    createAnalyticsTable_Command += $"{analytics_Columns[j]} float DEFAULT NUll,";
                }
                else
                {
                    createAnalyticsTable_Command += $"{analytics_Columns[j]} int(11) DEFAULT NULL,";
                }
            }
            createAnalyticsTable_Command = createAnalyticsTable_Command.TrimEnd(',');
            createAnalyticsTable_Command += $") ENGINE = InnoDB DEFAULT CHARSET = latin1 COLLATE = latin1_swedish_ci;";

            mySqlCommand.CommandText = createAnalyticsTable_Command;
            mySqlCommand.ExecuteNonQuery();

            // Insert Column's value in Rows
            for (int j = 0; j < valueList_ColumnArray_TableArray[i].Length; j ++) // 각 table안에 존재하는 행의 개수(유저마다 가지고 있는 캐릭터의 합)
            {
                int licenseNumber = Int32.Parse(valuesInColumnsInTable_Array[i][j][0]);
                int charactor = Int32.Parse(valuesInColumnsInTable_Array[i][j][1]);
                float reactionRate;
                int answerRate;

                if (valueList_ColumnArray_TableArray[i][j].Count == 0) // 하루에 해당 게임(레벨,스텝)을 한번도 하지 않았다면
                {
                    // 해당 j행에 default 값 == 0 을 넣는다.
                    reactionRate = 0;
                    answerRate = 0;
                }
                else
                {
                    reactionRate = valueList_ColumnArray_TableArray[i][j].Sum(item => item.reactionRate) / valueList_ColumnArray_TableArray[i][j].Count;
                    answerRate = (int)(valueList_ColumnArray_TableArray[i][j].Sum(item => item.answerRate) / valueList_ColumnArray_TableArray[i][j].Count);
                }

                //for (int k = 0; j < valueList_ColumnArray_TableArray[i][j].Count; k++)
                //{
                //
                //}

                // insertColumnsValueInAnalyticsTable_Command
                string insert_Command = $"INSERT INTO `{analyticsTable.list[i]}` ({analytics_Columns[0]}, {analytics_Columns[1]}, {analytics_Columns[2]}, {analytics_Columns[3]}) " +
                                        $"VALUES (@licenseNumber, @charactor, @reactionRate, @answerRate);";
                mySqlCommand.CommandText = insert_Command;
                // 반복문 안에서 매개변수 업데이트
                mySqlCommand.Parameters["@licenseNumber"].Value = licenseNumber;
                mySqlCommand.Parameters["@charactor"].Value = charactor;
                mySqlCommand.Parameters["@reactionRate"].Value = reactionRate;
                mySqlCommand.Parameters["@answerRate"].Value = answerRate;
                mySqlCommand.ExecuteNonQuery();
            }

            Debug.Log($"[DB] Continue create valueList_ColumnArray_TableArray{i}");
        }

        Debug.Log("[DB] Finish create AnalayticsTable and columns");
        mySqlCommand.CommandText = "USE `present`";
        mySqlCommand.ExecuteNonQuery();
    }

    // WeeklyRank DataBase Update
    public void UpdateWeeklyRankDB()
    {
        Debug.Log("[DB] Come in UpdateWeeklyRankDB..");

        // 매주 월요일 00시 00분 업데이트(초기화)
        // 매일 present DB에 있는 rank table에 게임 데이터들을 누적시키다가
        // 월요일(일요일->월요일)이 되었을 때 presentDB.rankTable에 있는 data들 값을 weeklyRankDB에 있는 table에 저장
        // 나중에(todo) weeklyrank DB에 있는 table들의 이름은 주간별로 가지게 될 것임 ex) 24.3.11-24.3.17 / 24.3.18-24.3.31
        // presentDB.rank table에서 데이터 가져올 때 개인의 Scoreplace, Timeplace (주간순위) / HighestScorePlace, HighestTimePlace (최고순위) 판별해서 WeeklyDB에 저장
        // WeeklyDB에 저장 후 presentDB.rankTable 초기화

        //////////////////////////////// SELECT * FROM `present`.`user_info` ORDER BY `User_LicenseNumber` ASC LIMIT 1000; DB ORDER BY 생각할것
        ///// ASC = Ascend, DESC = Descend

        // rank table -> [0]:User_LicenseNumber, [1]:User_Charactor, [2]:User_Profile, [3]:User_Name, [4]:TotalTime, [5]"TotalScore
        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // 오늘(today, 월요일)과 일주일전(week ago, 월요일)
        // 월요일이 오늘인 때에 실행되므로, 하루와 일주일을 뺸 요일로 이름을 저장한다.
        DateTime currentDate = DateTime.Now;
        currentDate = dbStandardDay;
        currentDate = currentDate.AddDays(-1);
        string yesterday = $"{currentDate.Year.ToString().Substring(2, 2)}.{currentDate.Month:00}.{currentDate.Day:00}";
        currentDate = currentDate.AddDays(-6);
        string weekago = $"{currentDate.Year.ToString().Substring(2, 2)}.{currentDate.Month:00}.{currentDate.Day:00}";

        string newTableName = $"{weekago}_{yesterday}";

        referenceWeeklyRankTableName = newTableName;

        /*
         CREATE TABLE `weeklyrank`.`24.03.13_24.03.20` LIKE `present`.rank;
         INSERT INTO `weeklyrank`.`24.03.13_24.03.20` SELECT * FROM `present`.rank;
         ALTER TABLE `weeklyrank`.`24.03.13_24.03.20` ADD COLUMN `sadf` INT;
         */

        // present DB에 있는 rank table의 컬럼과 데이터를 복사한 후 순위 컬럼 추가
        string createModifiedTableQuery = $"CREATE TABLE `{weeklyRankDB}`.`{newTableName}` LIKE `present`.rank;" +
                                          $"INSERT INTO `{weeklyRankDB}`.`{newTableName}` SELECT * FROM `present`.rank;" +
                                          $"ALTER TABLE `{weeklyRankDB}`.`{newTableName}` ADD COLUMN `ScorePlace` INT DEFAULT 0;" +
                                          $"ALTER TABLE `{weeklyRankDB}`.`{newTableName}` ADD COLUMN `TimePlace` INT DEFAULT 0;" +
                                          $"ALTER TABLE `{weeklyRankDB}`.`{newTableName}` ADD COLUMN `HighestScorePlace` INT DEFAULT 0;" +
                                          $"ALTER TABLE `{weeklyRankDB}`.`{newTableName}` ADD COLUMN `HighestTimePlace` INT DEFAULT 0;";
        mySqlCommand.CommandText = createModifiedTableQuery;
        mySqlCommand.ExecuteNonQuery();

        /*
         SET @rank=0;
         UPDATE `weeklyrank`.`24.03.13_24.03.19` SET `ScorePlace` = (@rank:=@rank+1) ORDER BY `TotalScore` DESC;
         */

        // TotalScore를 기준으로 ScorePlace 순위 Update
        string updateScorePlace = $"SET @rank = 0;" +
                                  $"UPDATE `weeklyrank`.`{newTableName}` SET `ScorePlace` = (@rank:=@rank+1) ORDER BY `TotalScore` DESC;";

        /*
         string updateScorePlace = $"SET @rank = 0; " + 
                          $"UPDATE `weeklyrank`.`{newTableName}` SET `ScorePlace` = (@rank:=@rank+1) ORDER BY `TotalScore` DESC;";
mySqlCommand.CommandText = updateScorePlace;
         */
        mySqlCommand.CommandText = updateScorePlace;
        mySqlCommand.ExecuteNonQuery();

        // TotalTime을 기준으로 TimePlace 순위 Update
        string updateTimePlace = $"SET @rank = 0;" +
                                 $"UPDATE `weeklyrank`.`{newTableName}` SET `TimePlace` = (@rank:=@rank+1) ORDER BY `TotalTime` DESC;";
        mySqlCommand.CommandText = updateTimePlace;
        mySqlCommand.ExecuteNonQuery();

        // HighestScorePlace, HighestTimePlace Update
        // tableName : 2주 전_1주 전, 저번주의 ranktable에 기록된 HighestScore, Time 비교해서 더 높으면 갱신, 0이거나 null이거나 또는 낮거나 같으면 그대로 기록 
        currentDate = currentDate.AddDays(-1);
        string yesterday_weekago = $"{currentDate.Year.ToString().Substring(2, 2)}.{currentDate.Month:00}.{currentDate.Day:00}";
        currentDate = currentDate.AddDays(-6);
        string twoweekago = $"{currentDate.Year.ToString().Substring(2, 2)}.{currentDate.Month:00}.{currentDate.Day:00}";

        string lastWeekTableName = $"{twoweekago}_{yesterday_weekago}";

        // lastWeekTable이 없다면 예외처리
        string checkTableQuery = $"USE `{weeklyRankDB}`;" +
                                 $"SHOW TABLES LIKE '{lastWeekTableName}'";
        mySqlCommand.CommandText = checkTableQuery;
        object result = mySqlCommand.ExecuteScalar();

        Debug.Log($"[DB] checkTableQuery : {checkTableQuery}");
        Debug.Log($"[DB] result : {result}");

        string updateCompareQuery;

        if(result == null)  // lastWeekTable이 없다면
        {
            Debug.Log($"[DB] dosen't exist lastWeekTable");
            // highestScorePlace와 highestTimePlace는 새로 생긴 rank table에 있는 ScorePlace와 TimePlace로 갱신
            updateCompareQuery = $"UPDATE `{weeklyRankDB}`.`{newTableName}` AS current " +
                                 $"SET current.HighestScorePlace = current.ScorePlace, " +
                                 $"    current.HighestTimePlace = current.TimePlace";
        }
        else // lastWeekTable이 있다면
        {
            Debug.Log($"[DB] It exist lastWeekTable");
            // 1주전(최근) ranktable에 있는 scorePlace와 timePlace를 2주전 ranktable에 있는 highestScorePlace와 highestTimePlace하고 비교해서 갱신
            updateCompareQuery = $"UPDATE `{weeklyRankDB}`.`{newTableName}` AS current " +
                                 $"LEFT JOIN `{weeklyRankDB}`.`{lastWeekTableName}` AS lastweek " +
                                 $"ON current.User_LicenseNumber = lastweek.User_LicenseNumber " +
                                 $"AND current.User_Charactor = lastweek.User_Charactor " +
                                 $"SET current.HighestScorePlace = IF(lastweek.HighestScorePlace < current.ScorePlace OR lastweek.HighestScorePlace = '0', current.ScorePlace, lastweek.HighestScorePlace), " +
                                 $"    current.HighestTimePlace = IF(lastweek.HighestTimePlace < current.TimePlace OR lastweek.HighestTimePlace = '0', current.TimePlace, lastweek.HighestTimePlace) " +
                                 $"WHERE current.ScorePlace IS NOT NULL AND current.TimePlace IS NOT NULL";
        }
        Debug.Log($"[DB] updateCompareQuery : {updateCompareQuery}");
        mySqlCommand.CommandText = updateCompareQuery;
        mySqlCommand.ExecuteNonQuery();

        // presentDB로 변경
        mySqlCommand.CommandText = $"USE `{presentDB}`;";
        mySqlCommand.ExecuteNonQuery();

        Debug.Log("[DB] Complete UpdateWeeklyRankDB!!");

        ResetRankTableInPresentDB();
    }

    // Reset PresentDB.rankTable After Update WeeklyDB
    private void ResetRankTableInPresentDB()
    {
        Debug.Log("[DB] Come in ResetRankTableInPresentDB..");
        
        // PresentDB에 있는 rank table의 TotalScore, TotalTime 0으로 리셋

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        string updateQueryForReset = $"UPDATE `{presentDB}`.`{table.list[1]}` SET `TotalScore` = '0', `TotalTime` = '0'";
        mySqlCommand.CommandText = updateQueryForReset;
        mySqlCommand.ExecuteNonQuery();

        Debug.Log("[DB] Complete ResetRankTableInPresentDB!!");

        SaveStandardDay();
    }

    // After update ranktable, Save StandardDay
    private void SaveStandardDay()
    {
        Debug.Log("[DB] Come in SaveStandardDay..");

        DateTime saveStandardDay = dbStandardDay.AddDays(7);

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        Debug.Log($"[DB] saveStandardDay : {saveStandardDay}");
        Debug.Log($"[DB] saveStandardDay.Date : {saveStandardDay.Date}");
        Debug.Log($"[DB] saveStandardDay.Date.Day : {saveStandardDay.Date.Day}");
        Debug.Log($"[DB] saveStandardDay.Date.Date : {saveStandardDay.Date.Date}");
        Debug.Log($"[DB] saveStandardDay:yyyy-MM-dd : {saveStandardDay:yyyy-MM-dd}");

        string updateQuery = $"UPDATE `{presentDB}`.`standardday` SET `StandardDay` = '{saveStandardDay:yyyy-MM-dd}'";
        mySqlCommand.CommandText = updateQuery;
        mySqlCommand.ExecuteNonQuery();

        Debug.Log("[DB] Complete SaveStandardDay!!");
    }

    // Load StandardDay to calculate the week
    public DateTime LoadStandardDay()
    {
        Debug.Log("[DB] Come in LoadStandardDay..");

        DateTime dbStandardTime = DateTime.Now;

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        string selectQuery = $"SELECT * FROM `standardday`";
        mySqlCommand.CommandText = selectQuery;
        MySqlDataReader reader = mySqlCommand.ExecuteReader();

        while(reader.Read())
        {
            dbStandardTime = reader.GetDateTime(0);
        }
        reader.Close();
        Debug.Log($"[DB] dbStandardTime, it should be 2024.03.18 : {dbStandardTime}");

        Debug.Log("[DB] Comeplete LoadStandardDay..");

        return dbStandardTime;
    }

    // Set Rank Table Name In WeeklyRankDB
    private string SetWeeklyRankTableName(DateTime dbStandardDay)
    {
        Debug.Log($"[DB] Come in SetWeeklyRankTableName..");

        dbStandardDay = dbStandardDay.AddDays(-1);
        string standardDay_yesterday = $"{dbStandardDay.Year.ToString().Substring(2, 2)}.{dbStandardDay.Month:00}.{dbStandardDay.Day:00}";
        dbStandardDay = dbStandardDay.AddDays(-6);
        string standardDay_weekago = $"{dbStandardDay.Year.ToString().Substring(2, 2)}.{dbStandardDay.Month:00}.{dbStandardDay.Day:00}";

        string rankTableName = $"{standardDay_weekago}_{standardDay_yesterday}";

        Debug.Log($"[DB] standardDay_yesterday = {standardDay_yesterday}");
        Debug.Log($"[DB] standardDay_weekago = {standardDay_weekago}");
        Debug.Log($"[DB] rankTableName = {rankTableName}");

        Debug.Log($"[DB] Complete SetWeeklyRankTableName..");

        return rankTableName;
    }

    #endregion
}
