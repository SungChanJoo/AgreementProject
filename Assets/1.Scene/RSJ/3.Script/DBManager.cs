using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using UnityEngine.UI;
using System;
using System.Text;
using System.Linq;

// 싱글톤 쓰자
// 클라이언트가 서버에 연결했을때? 클라이언트는 이미 자체적으로 IP와 Port를 알아서 서버에 연결함
// 클라이언트가 로그인 할때 ID와 Password를 입력할텐데, 그때 서버에 있는 DB에서 login table과 비교해서 true면 로그인
// 회원등록도 일단 만들어놓는다
// DBManager는 서버에서 관리하는게 맞다
public class DBManager : MonoBehaviour
{
    MySqlConnection connection; // DB에 연결하는 클래스
    MySqlDataReader reader;

    //public string path = string.Empty;

    // 연결할때 필요한 정보
    private string str_Connection;

    // Table List, AnalyticsTable List
    private TableName table;
    private AnalyticsTableName analyticsTable;

    // Table - Columns
    private string[] userinfo_Columns;
    private string[] rank_Columns;
    private string[] achievement_Columns;
    private string[] pet_Columns;
    private string[] game_Columns;
    private string[] analytics_Columns;

    // 서버-클라이언트 string으로 data 주고받을때 구분하기 위한 문자열
    private const string separatorString = "E|";

    // LicenseNumber 기본, Table 게임명(편의성을 위해)
    private int clientLicenseNumber_Base = 10000;

    // 기타 데이터 처리용 Handler
    private IETCMethodHandler etcMethodHandler;

    public static DBManager instance = null; // 싱글톤 쓸거임

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

    // DB 세션에 연결할 때 필요한 정보 초기화
    private void SetStringConnection()
    {
        string ip = "127.0.0.1"; // 우선 로컬(127.0.0.1)로, aws EC2 IP : 15.165.159.141
        string db = "present";
        string uid = "root"; //string.IsNullOrEmpty(user_Info.user_Name)? "" : user_Info.user_Name;
        string pw = "12345678"; //string.IsNullOrEmpty(user_Info.user_Password)? "" : user_Info.user_Password;
        string port = "3306";
        //str_Connection = $"Server={ip};Database={db};Uid={uid};Pwd={pw};Port={port};Charset=utf8;";
        str_Connection = $"Server={ip};" + $"Database={db};" + $"Uid={uid};" + $"Pwd={pw};" + $"Port={port};" + "CharSet=utf8;"; // ; 세미콜론 주의해라
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
        // Tables / TableName 객체 생성시 table.list 자동 생성
        table = new TableName(); 

        // Table - Columns
        userinfo_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin" };
        rank_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "User_Profile", "User_Name", "TotalTime", "TotalScore" };
        achievement_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "Something" };
        pet_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "White" };
        game_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };
        analytics_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerRate" };
    }

    // 계정등록 // todo 비동기화 생각해봐야함
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

        // user_info 변수
        string table_Name = "user_info";
        string[] columns = { "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin" };
        int clientLicenseNumber = clientLicenseNumber_Base + count;
        int user_Charactor = 1;
        string user_Name = "Guest";
        byte[] user_Profile = { 0x00};
        string user_Birthday = "96.02.01";
        int user_TotalAnswers = 0;
        float user_TotalTime = 0f;
        int user_Coin = 0;

        // Binary Parameter
        //MySqlParameter binaryParameter = new MySqlParameter

        // 생성하는 쿼리문
        string createAccount_Command = $"INSERT INTO {table_Name} ({columns[0]}, {columns[1]}, {columns[2]}, {columns[3]}, {columns[4]}, {columns[5]}, {columns[6]}, {columns[7]}) " +
                                        $"VALUES (@clientLicenseNumber, @user_Charactor, @user_Name, @user_Profile, @user_Birthday, @user_TotalAnswers, @user_TotalTime, @user_Coin)"; // `(따옴표), '(백틱) 구분하기
        MySqlCommand insert_SqlCmd = new MySqlCommand(createAccount_Command, connection);
        insert_SqlCmd.Parameters.Add("@clientLicenseNumber", MySqlDbType.Int32).Value = clientLicenseNumber;
        insert_SqlCmd.Parameters.Add("@user_Charactor", MySqlDbType.Int32).Value = user_Charactor;
        insert_SqlCmd.Parameters.Add("@user_Name", MySqlDbType.VarChar).Value = user_Name;
        insert_SqlCmd.Parameters.Add("@user_Profile", MySqlDbType.MediumBlob).Value = user_Profile;
        insert_SqlCmd.Parameters.Add("@user_Birthday", MySqlDbType.MediumBlob).Value = user_Birthday;
        insert_SqlCmd.Parameters.Add("@user_TotalAnswers", MySqlDbType.MediumBlob).Value = user_TotalAnswers;
        insert_SqlCmd.Parameters.Add("@user_TotalTime", MySqlDbType.MediumBlob).Value = user_TotalTime;
        insert_SqlCmd.Parameters.Add("@user_Coin", MySqlDbType.Int32).Value = user_Coin;
        
        insert_SqlCmd.ExecuteNonQuery(); // command.ExecuteNonQuery()은 DB에서 변경 작업을 수행하는 SQL 명령문을 실행하고, 영향을 받은 행의 수를 반환하는 메서드

        string returnData = $"{clientLicenseNumber}|{user_Charactor}";
        return returnData;
    }

    // 새 캐릭터 데이터 생성
    public void CreateNewCharactorData(int clientlicensenumber, int clientcharactor)
    {
        Debug.Log($"[DB] Create new Charactor data, client's licensenumber : {clientlicensenumber}");

        // ClientLicenseNumber가 발급되면 새 플레이어 정보를 만드는 것이므로, 모든 테이블에 data를 추가해야함
        string insertCreatePlayerData_Command;
        MySqlCommand insert_SqlCmd = new MySqlCommand();
        insert_SqlCmd.Connection = connection;

        // Update Query
        string updatePlayerData_Command;
        MySqlCommand update_SqlCmd = new MySqlCommand();
        update_SqlCmd.Connection = connection;

        // table.list[0] -> userinfo, [1]->rank, [2]->achievement, [3]->pet

        // rank table
        // table.list[1] = "rank"
        // rank_Columns = { "User_LicenseNumber", "User_Charactor", "User_Profile", "User_Name", "TotalTime", "TotalScore" };
        int rank_valuepart;

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {table.list[1]} ({rank_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < rank_Columns.Length; i++) // 0->licensenumber, 1->charactor
        {
            if (i == 0) rank_valuepart = clientlicensenumber;
            else if (i == 1) rank_valuepart = clientcharactor;
            else rank_valuepart = 0;

            updatePlayerData_Command = $"UPDATE `{table.list[1]}` SET `{rank_Columns[i]}` = '{rank_valuepart}' WHERE `{rank_Columns[0]}` = '{clientlicensenumber}'";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] Rank table complete!");

        // achievement table
        //string achievement_TableName = "achievement";
        //string[] achievement_Columns = { "User_LicenseNumber", "User_Charactor", "Something"};
        int achievement_valuepart;

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {table.list[2]} ({achievement_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < achievement_Columns.Length; i++)
        {
            if (i == 0) achievement_valuepart = clientlicensenumber;
            else if (i == 1) achievement_valuepart = 1;
            else achievement_valuepart = 0;

            updatePlayerData_Command = $"UPDATE `{table.list[2]}` SET `{achievement_Columns[i]}` = '{achievement_valuepart}' WHERE `{achievement_Columns[0]}` = '{clientlicensenumber}'";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] achievement table complete!");

        // pet table
        //string pet_TableName = "pet";
        //string[] pet_Columns = { "User_LicenseNumber", "User_Charactor", "White" };
        int pet_valuepart;

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO `{table.list[3]}` (`{pet_Columns[0]}`) VALUES ('{clientlicensenumber}')";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < pet_Columns.Length; i++)
        {
            if (i == 0) pet_valuepart = clientlicensenumber;
            else if (i == 1) pet_valuepart = 1;
            else pet_valuepart = 0;

            updatePlayerData_Command = $"UPDATE `{table.list[3]}` SET `{pet_Columns[i]}` = '{pet_valuepart}' WHERE `{pet_Columns[0]}` = '{clientlicensenumber}'";
            if (i==2) // float
            {
                updatePlayerData_Command = $"UPDATE `{table.list[3]}` SET `{pet_Columns[i]}` = '{(float)pet_valuepart}'";
            }
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] pet table complete!");

        // game table
        string game_TableName;

        string[] game_Names = { "venezia_kor", "venezia_eng", "venezia_chn", "calculation", "gugudan"};
        int[] levels = { 1, 2, 3 };
        int[] steps = { 1, 2, 3, 4, 5, 6 };

        //string[] game_Columns = { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };

        int user_Charactor = 1;
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
            insertCreatePlayerData_Command = $"INSERT INTO {game_TableList[i]} (`User_LicenseNumber`, `User_Charactor`) VALUES ({clientlicensenumber}, {user_Charactor})";
            insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
            insert_SqlCmd.ExecuteNonQuery();

            // update rowm j=2부터 시작하는 이유는 0은 licensenumber고 1은 charactor번호이고 insert로 넣어줬기 때문에 굳이 또 업데이트를 할 필요가 없음
            for (int j = 2; j < game_Columns.Length; j++)
            {
                updatePlayerData_Command = $"UPDATE {game_TableList[i]} SET {game_Columns[j]} = @value  WHERE `{game_Columns[0]}` = '{clientlicensenumber}'" ;
                update_SqlCmd.CommandText = updatePlayerData_Command;

                // parameter 초기화
                update_SqlCmd.Parameters.Clear();

                if (j == 2 || j == 5)
                {
                    update_SqlCmd.Parameters.Add("@value", MySqlDbType.Float).Value = init_Float;
                }
                else
                {
                    update_SqlCmd.Parameters.Add("@value", MySqlDbType.Int32).Value = init_Int;
                }
                update_SqlCmd.ExecuteNonQuery();
            }
        }

        Debug.Log("[DB] game table complete!");
    }

    // DB에 캐릭터 이름 저장
    public void SaveCharactorName(List<string> dataList)
    {
        // user_info table과 rank table에 업데이트
        // dataList -> [0]requestName / [1]license / [2]charactor / [3]name
        Debug.Log("[DB] Come in SaveCharactorName method");

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
    public void SaveCharactorProfile(List<string> dataList)
    {
        // user_info table과 rank table에 업데이트
        // dataList -> [0]requestName / [1]license / [2]charactor / [3]profile(Base64)
        // Profile Base64 형식으로 DB에 저장
        Debug.Log("[DB] Come in SaveCharactorProfile method");


        // 지역변수
        string update_Command;
        MySqlCommand update_sqlCmd;

        // user_info == table.list[0]
        update_Command = $"UPDATE `{table.list[0]}` SET `{userinfo_Columns[3]}` = '{dataList[3]}' " +
                         $"WHERE `{userinfo_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{userinfo_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        Debug.Log(update_Command);
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        // rank == table.list[1]
        update_Command = $"UPDATE `{table.list[1]}` SET `{rank_Columns[2]}` = '{dataList[3]}' " +
                         $"WHERE `{rank_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{rank_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        Debug.Log("[DB] Complete save charactor profile to DB");
    }

    // DB에 캐릭터 데이터 저장 (캐릭터 변경 또는 게임 종료시)
    public void SaveCharactorData(List<string> dataList)
    {
        Debug.Log("[DB] Come in save charactor data");
        // dataList[0] : $"{requestName}|{clientLicenseNumber}|{clientCharactor}|"
        // dataList[1] : $"{table.list[0]}|{playerdb.playerName}|{Convert.ToBase64String(playerdb.image)}|{playerdb.BirthDay}|{playerdb.TotalAnswers}|{playerdb.TotalTime}|";
        // dataList[2~n-1] : $"{table.list[i]}|{reactionRate_List[i - 4]}|{answersCount_List[i - 4]}|{answers_List[i - 4]}|{playTime_List[i - 4]}|{totalScore_List[i - 4]}|{starCount_List[i - 4]}|}";
        // dataList[n] : $"Finish"

        for(int i = 0; i<dataList.Count; i++)
        {
            Debug.Log($"[DB] charactor dataList{i} : {dataList[i]}");
        }
        string update_Command;
        MySqlCommand update_SqlCmd = new MySqlCommand();
        update_SqlCmd.Connection = connection;
        Debug.Log($"[DB] Check dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[1] : {Int32.Parse(dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[1])}");
        Debug.Log($"[DB] Check dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[2] : {Int32.Parse(dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[2])}");
        int clientLicenseNumber = Int32.Parse(dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[1]);
        int clientCharactor = Int32.Parse(dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[2]);

        //userinfo_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin" };
        //game_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };

        for(int i = 1; i < dataList.Count; i++)
        {
            List<string> tempAllocate = dataList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
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

    // DB에 게임결과 저장
    public void SaveGameResultData(List<string> dataList)
    {
        // DB gametable column순 : User_Licensenumber/User_Charactor/ReactionRate/AnswerCount/AnswerRate/Playtime/TotalScore/StarPoint
        // dataList는 [0]을 제외하고 value(int)만 있음. index순으로 RequestName[0]/level[1]/step[2]/User_Licensenumber[3]/User_Charactor[4]/ReactionRate[5]/.../TotalScore[9]
        Debug.Log("[DB] Come in SaveGameResultData method");
        Debug.Log($"[DB] SaveGameResultData, gameName : {dataList[0]}"); // [Save]gameName
        string gameName = dataList[0].Substring("[Save]".Length); // [Save] 제거
        Debug.Log($"[DB] gameName : {gameName}");

        string table_Name = $"{gameName}_level{dataList[1]}_step{dataList[2]}";
        if(gameName == "venezia_chn") table_Name = $"{gameName}_level_step{dataList[2]}";

        // gametable에 저장된 totalscore 가져와서 비교 (한 게임의 최고점수)
        int gameresult_Score = Int32.Parse(dataList[9]);
        int db_Score = 0;

        // Score로 StarPoint(0,1,2,3) 할당
        int starPoint = 0;

        // DB 조회
        Debug.Log($"[DB] SelectGameTable ....");
        string selectGameTable_Command = $"SELECT `{game_Columns[0]}`, `{game_Columns[1]}`, `{game_Columns[6]}` FROM `{table_Name}`";
        Debug.Log($"[DB] selectGameTable_Command : {selectGameTable_Command}");
        MySqlCommand select_SqlCmd = new MySqlCommand(selectGameTable_Command, connection);
        MySqlDataReader reader = select_SqlCmd.ExecuteReader();
        Debug.Log($"[DB] reader is alright?");

        while(reader.Read())
        {
            Debug.Log($"[DB] reader has come While()?");
            try
            {
                // DB에 있는 licensenumber와 charactor가 저장하려는 데이터의 licensenumber와 charactor가 같다면 (해당 행에 있는 값 가져옴)
                if (reader.GetInt32(0) == Int32.Parse(dataList[3]) && reader.GetInt32(1) == Int32.Parse(dataList[4]))
                {
                    db_Score = reader.GetInt32(game_Columns[6]);
                    Debug.Log($"[DB] db_Score = {db_Score}");
                }
            }
            catch(Exception e)
            {
                Debug.Log($"[DB] reader error : {e.Message}");
            }
        }
        reader.Close();

        // 새 게임점수와 db 게임점수 비교
        Debug.Log($"[DB] gameresult_Score, DB_Score : {gameresult_Score}, {db_Score}");
        if (gameresult_Score >= db_Score) // 저장
        {
            // gameresult_Score로 StarPoint 할당함
            if (gameresult_Score >= 25000) starPoint = 3;
            else if (gameresult_Score >= 12500) starPoint = 2;
            else if (gameresult_Score >= 6500) starPoint = 1;

            // rank table에 TotalTime과 TotalScore에 점수 누적 
            UpdateRankTable(Int32.Parse(dataList[3]), Int32.Parse(dataList[4]), float.Parse(dataList[8]), gameresult_Score);
        }
        else // 저장 X
        {
            // 저장할 필요가 없으므로 rank table에만 TotalTime과 TotalScore에 점수 누적 후 return
            UpdateRankTable(Int32.Parse(dataList[3]), Int32.Parse(dataList[4]), float.Parse(dataList[8]), gameresult_Score);
            return;
        }

        Debug.Log($"[DB] starPoint : {starPoint}");
        // starpoint value값 list에 추가
        dataList.Add(starPoint.ToString());
        Debug.Log($"[DB] dataList's Count : {dataList.Count}");
        Debug.Log($"[DB] dataList[9]'s value : {dataList[9]}");
        Debug.Log($"[DB] dataList[10]'s value : {dataList[10]}");

        for (int i = 2; i < game_Columns.Length; i++) // game_Columns = [0]~[7]
        {
            string updateGameData_Command = "";
            if (i == 2 || i == 5) // float
            {
                updateGameData_Command = $"UPDATE `{table_Name}` SET `{game_Columns[i]}` = '{float.Parse(dataList[i + 3])}' WHERE `{game_Columns[0]}` = '{Int32.Parse(dataList[3])}' AND `{game_Columns[1]}` = '{Int32.Parse(dataList[4])}'";
            }
            else
            {
                updateGameData_Command = $"UPDATE `{table_Name}` SET `{game_Columns[i]}` = '{Int32.Parse(dataList[i + 3])}' WHERE `{game_Columns[0]}` = '{Int32.Parse(dataList[3])}' AND `{game_Columns[1]}` = '{Int32.Parse(dataList[4])}'";
            }
            MySqlCommand update_SqlCmd = new MySqlCommand(updateGameData_Command, connection);
            update_SqlCmd.ExecuteNonQuery();
        }
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

    

    // 플레이어 데이터 불러오기, DB에서 불러와서 서버가 클라이언트한테 쏴줄수 있게 string으로 묶어서 반환형 string
    public List<string> LoadCharactorData(int clientlicensenumber, int clientcharactor)
    {
        Debug.Log("[DB] Come in LoadCharactorData method");

        List<string> return_TableData = new List<string>();
        string return_TempData;

        string selectTable_Command;
        MySqlCommand select_SqlCmd = new MySqlCommand();
        select_SqlCmd.Connection = connection;


        for (int i = 0; i < table.list.Count; i++) // table_List
        {
            selectTable_Command = $"SELECT * FROM {table.list[i]}";
            select_SqlCmd.CommandText = selectTable_Command;
            reader = select_SqlCmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int dbLicenseNumber = reader.GetInt32("User_LicenseNumber");
                    // int dbcharactor = reader.GetInt32("User_Charactor"); 나중에 캐릭터 변경시 필요할 변수. todo

                    // 클라이언트 라이센스와 db에 있는 라이센스가 같다면 데이터를 불러온다
                    if (clientlicensenumber == dbLicenseNumber)
                    {
                        Debug.Log($"[DB] tableName : {table.list[i]}, dbLicenseNumber : {dbLicenseNumber},");
                        switch (table.list[i])
                        {
                            case "user_info":
                                Debug.Log("[DB] LoadCharactorData - user_info table");
                                string user_Name = reader.GetString("User_Name");
                                // DB에서 MediumBlob 타입 데이터(Base64 형식으로 저장됨) string으로 가져오기
                                string user_Profile = reader.GetString("User_Profile");
                                string user_Birthday = reader.GetString("User_Birthday");
                                string user_TotalAnswers = reader.GetInt32("User_TotalAnswers").ToString();
                                string user_TotalTime = reader.GetFloat("User_TotalTime").ToString();
                                string user_Coin = reader.GetInt32("User_Coin").ToString();
                                // table 구분을 위해 맨 앞에 {table.list[i]} 추가
                                return_TempData = $"{table.list[i]}|{user_Name}|{user_Profile}|{user_Birthday}|{user_TotalAnswers}|{user_TotalTime}|{user_Coin}|{separatorString}"; 
                                return_TableData.Add(return_TempData);
                                break;
                            case "rank":
                                Debug.Log("[DB] LoadCharactorData - rank table");
                                string rank_TotalTime = reader.GetString("TotalTime");
                                string rank_TotalScore = reader.GetString("TotalScore");
                                return_TempData = $"{table.list[i]}|{rank_TotalTime}|{rank_TotalScore}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "achievement":
                                Debug.Log("[DB] LoadCharactorData - achievement table");
                                string achievement_Something = reader.GetInt32("Something").ToString();
                                return_TempData = $"{table.list[i]}|{achievement_Something}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "pet":
                                Debug.Log("[DB] LoadCharactorData - pet table");
                                string pet_White = reader.GetInt32("White").ToString();
                                return_TempData = $"{table.list[i]}|{pet_White}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            default:
                                Debug.Log("[DB] LoadCharactorData - game table");
                                string game_ReactionRate = reader.GetFloat("ReactionRate").ToString();
                                string game_AnswerCount = reader.GetInt32("AnswerCount").ToString();
                                string game_AnswerRate = reader.GetInt32("AnswerRate").ToString();
                                string game_Playtime = reader.GetFloat("Playtime").ToString();
                                string game_TotalScore = reader.GetInt32("TotalScore").ToString();
                                string game_StarPoint = reader.GetInt32("StarPoint").ToString();
                                return_TempData = $"{table.list[i]}|{game_ReactionRate}|{game_AnswerCount}|{game_AnswerRate}|{game_Playtime}|{game_TotalScore}|{game_StarPoint}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                        }
                    }
                }
            }
            reader.Close();
        }

        return return_TableData;
    }

    // 분석 데이터 불러오기 
    public List<string> LoadAnalyticsData(int licensenumber, int charactor)
    {
        // 클라이언트에서 받아야할 List 형식
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
        DateTime currentDate = DateTime.Now.AddDays(-1); // 오늘 전날
        for(int i =0; i < DBName_Array.Length; i++)
        {
            currentDate = currentDate.AddDays(-1);
            DBName_Array[i] = $"{currentDate.Year.ToString().Substring(2,2)}.{currentDate.Month:00}.{currentDate.Day:00}";
        }

        // 조회할 테이블 수만큼 String Type을 받는 List를 생성, 각 배열은 테이블을 의미하고, 해당 테이블의 value가 유의미 한지 아닌지 판단해서 Add로 추가할지 안할지 선택
        List<string>[] tempList_Array = new List<string>[13];

        // 조회할 테이블 Array
        string[] tableName_Array = new string[13];
        for(int i = 0; i < tableName_Array.Length; i++)
        {
            if(i < 3) // venezia_kor
            {
                tableName_Array[i] = $"venezia_kor_level{i+1}_anlaytics";
            }   
            else if(i < 6) // venezia_eng
            {
                tableName_Array[i] = $"venezia_eng_level{i-2}_anlaytics";
            }
            else if(i == 6) // venezia_chn
            {
                tableName_Array[i] = $"venezia_chn_anlaytics";
            }
            else if(i < 10) // calculation
            {
                tableName_Array[i] = $"calculation_level{i-6}_anlaytics";
            }
            else // gugudan
            {
                tableName_Array[i] = $"gugudan_level{i-9}_anlaytics";
            }

            // tempList_Array 객체 생성
            tempList_Array[i] = new List<string>();
        }

        // 선택할 컬럼 Array
        string[] columnName_Array = new string[4] { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerRate"};

        // 30일 동안, 최근 날짜(어제)부터 DB조회
        for(int i = 0; i < DBName_Array.Length; i++)
        {
            try
            {
                string useDB_Command = $"USE {DBName_Array[i]};";
                mySqlCommand.CommandText = useDB_Command;
                mySqlCommand.ExecuteNonQuery();

                // DB내에 있는 라이센스와 캐릭터를 만족하는 테이블 컬럼 조회
                for (int j = 0; j < tableName_Array.Length; j++)
                {
                    string selectTable_Command = $"SELECT * FROM `{tableName_Array[j]}` WHERE `{columnName_Array[0]}` = {licensenumber} AND `{columnName_Array[1]}` = {charactor};";
                    mySqlCommand.CommandText = selectTable_Command;
                    MySqlDataReader reader = mySqlCommand.ExecuteReader();
                    float reactionRate = 0;
                    int answerRate = 0;

                    while (reader.Read())
                    {
                        reactionRate = reader.GetFloat(columnName_Array[2]);
                        answerRate = reader.GetInt32(columnName_Array[3]);
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
            }
        }

        Debug.Log("[DB] Complete Load AnalyticsData From DB");

        return return_List;
    }

    // 임시로 PresentDB에 있는 Rank Table 데이터 사용 / Score, Time 별 Rank 1~5위 및 6번째 자기 자신의 데이터 
    public List<string> RankOrderByUserData(int licensenumber, int charactor)
    {
        Debug.Log("[DB] Come in RankOrderByUserData Method");

        // rank table -> [0]:User_LicenseNumber, [1]:User_Charactor, [2]:User_Profile, [3]:User_Name, [4]:TotalTime, [5]"TotalScore
        List<string> return_List = new List<string>();

        // rank table's row count
        string rowCount_Command = $"SELECT COUNT(*) FROM `rank`";
        MySqlCommand rowCount_SqlCmd = new MySqlCommand(rowCount_Command, connection);
        // Int 캐스팅
        object result = rowCount_SqlCmd.ExecuteScalar(); // ExcuteScalar() 메서드는 쿼리를 실행하고 결과 집합의 첫 번째 행의 첫 번째 열의 값을 반환
        Debug.Log($"[DB] Check result: {result}");
        result = (result == DBNull.Value) ? null : result;
        int rowCountInTable = Convert.ToInt32(result); 
        Debug.Log($"[DB] Check rowCount, rowCountInTable : {rowCountInTable}");

        // Score 기준
        string selectScore_Command = $"SELECT * FROM `rank`";
        MySqlCommand selectScore_SqlCmd = new MySqlCommand(selectScore_Command, connection);
        MySqlDataReader reader = selectScore_SqlCmd.ExecuteReader();
        Debug.Log("[DB] Check selectScore");

        List<List<string>> rankdata = new List<List<string>>(); 

        // table에 있는 data들 rankdata에 담기
        while(reader.Read())
        {
            for(int i = 0; i < rowCountInTable; i++)
            {
                List<string> valuesInColumn = new List<string>();
                
                for (int j = 0; j < rank_Columns.Length; j++)
                {
                    Debug.Log($"[DB] Check Type Conversion, j : {j}");
                    if (j == 2) // byte[]
                    {
                        // column에 있는 MediumBlob Type value -> byte[] type으로 받아오기
                        byte[] profileData = reader[$"{rank_Columns[j]}"] as byte[];
                        // Image Data를 Base64 문자열로 변환
                        string profileDataBase64 = Convert.ToBase64String(profileData);
                        // client에 보낼 문자열에 Base64 문자열 추가 (나중에 client에서 decoding해야함)
                        valuesInColumn.Add(profileDataBase64);
                        Debug.Log($"[DB] Check Type Conversion, Where: byte[] profileData, {profileData}, j : {j}");
                        Debug.Log($"[DB] Check Type Conversion, Where: string profileDataBase64, {profileDataBase64}, j : {j}");
                        Debug.Log($"[DB] Check Type Conversion, Where: string Encoding.UTF8.Getstring(profileData), {System.Text.Encoding.UTF8.GetString(profileData)}, j : {j}");
                    }
                    else if(j == 3) // Varchar
                    {
                        valuesInColumn.Add(reader.GetString(rank_Columns[j]));
                        Debug.Log($"[DB] Check Type Conversion, Where: Varchar[], j : {j}");
                    }
                    else if(j == 4) // Float
                    {
                        valuesInColumn.Add(reader.GetFloat(rank_Columns[j]).ToString());
                        Debug.Log($"[DB] Check Type Conversion, Where: Float[], j : {j}");
                        Debug.Log($"[DB] Check float type? : {reader.GetFloat(rank_Columns[j]).GetType()}");
                        Debug.Log($"[DB] Check float value : {reader.GetFloat(rank_Columns[j])}");
                    }
                    else // int
                    {
                        valuesInColumn.Add(reader.GetInt32(rank_Columns[j]).ToString());
                        Debug.Log($"[DB] Check Type Conversion, Where: Int[], j : {j}");
                    }
                }
                rankdata.Add(valuesInColumn);
            }
        }
        reader.Close();

        // 직접적인 순위비교
        // Score - rankdata[i][5]
        List<Rank_Score> scoreList = new List<Rank_Score>();

        for(int i= 0; i < rankdata.Count; i++)
        {
            Rank_Score score = new Rank_Score();

            score.place = 0;
            score.userlicensenumber = Int32.Parse(rankdata[i][0]);
            score.usercharactor = Int32.Parse(rankdata[i][1]);
            score.userProfile = System.Text.Encoding.UTF8.GetBytes(rankdata[i][2]);
            score.userName = rankdata[i][3];
            score.totalScore = Int32.Parse(rankdata[i][5]);

            scoreList.Add(score);
        }

        // totalScore로 내림차순 정렬
        scoreList.Sort((a, b) => b.totalScore.CompareTo(a.totalScore));

        // 정렬된 List가지고 1~5등 유저의 순위,점수 저장
        for(int i = 0; i < scoreList.Count; i++)
        {
            //보낼 데이터(value) place/Profile/Name/total
            string return_string;

            if(i >= 0 && i < 5)
            {
                return_string = $"{i}|{scoreList[i].userProfile}|{scoreList[i].userName}|{scoreList[i].totalScore}|{separatorString}";
            }
            else
            {
                continue;
            }

            return_List.Add(return_string);
        }

        // 예외처리, rank data에 5명의 기록이 없으면 return_List에 index[5]까지 빈내용으로 저장
        if(scoreList.Count < 5)
        {
            int user_Count = scoreList.Count;

            while(user_Count < 5)
            {
                //보낼 데이터(value) place/Profile/Name/total
                return_List.Add($"{user_Count}|0|None|0|{separatorString}");
                user_Count++;
            }
        }

        // return_List index[5]에 개인 순위,점수 저장
        for (int i = 0; i < scoreList.Count; i++)
        {
            if (scoreList[i].userlicensenumber == licensenumber && scoreList[i].usercharactor == charactor)
            {
                string return_string;
                return_string = $"{i}|{scoreList[i].userProfile}|{scoreList[i].userName}|{scoreList[i].totalScore}|{separatorString}";
                return_List.Add(return_string);
            }
        }

        // Time 기준
        List<Rank_Time> timeList = new List<Rank_Time>();

        for (int i = 0; i < rankdata.Count; i++)
        {
            Rank_Time time = new Rank_Time();

            time.place = 0;
            time.userlicensenumber = Int32.Parse(rankdata[i][0]);
            time.usercharactor = Int32.Parse(rankdata[i][1]);
            time.userProfile = System.Text.Encoding.UTF8.GetBytes(rankdata[i][2]);
            time.userName = rankdata[i][3];
            time.totalTime = float.Parse(rankdata[i][4]);

            timeList.Add(time);
        }

        // totalTime으로 내림차순 정렬
        timeList.Sort((a, b) => b.totalTime.CompareTo(a.totalTime));

        // 정렬된 List가지고 1~5등 유저의 순위, 시간 저장
        for (int i = 0; i < timeList.Count; i++)
        {
            //보낼 데이터(value) place/Profile/Name/total
            string return_string;

            if (i >= 0 && i < 5)
            {
                return_string = $"{i}|{timeList[i].userProfile}|{timeList[i].userName}|{timeList[i].totalTime}|{separatorString}";
            }
            else
            {
                continue;
            }

            return_List.Add(return_string);
        }

        // 예외처리, rank data에 5명의 기록이 없으면 return_List에 index[5]까지 빈내용으로 저장
        if (timeList.Count < 5)
        {
            int user_Count = timeList.Count;

            while (user_Count < 5)
            {
                //보낼 데이터(value) place/Profile/Name/total
                return_List.Add($"{user_Count}|0|None|0|{separatorString}");
                user_Count++;
            }
        }

        // return_List index[11]에 개인 순위,점수 저장
        for (int i = 0; i < timeList.Count; i++)
        {
            if (timeList[i].userlicensenumber == licensenumber && timeList[i].usercharactor == charactor)
            {
                string return_string;
                return_string = $"{i}|{timeList[i].userProfile}|{timeList[i].userName}|{timeList[i].totalTime}|{separatorString}";
                return_List.Add(return_string);
            }
        }

        return return_List;
    }

    // 하루가 지났을 때 presentDB gamedata -> 요일DB 생성하고 gamedata 저장
    public void CreateDateDB()
    {
        Debug.Log("[DB] CreateDateDB");

        // 생성할 DB 이름
        string DBName = $"{DateTime.Now.Year.ToString().Substring(2,2)}.{DateTime.Now.Month:00}.{DateTime.Now.Day:00}";
        Debug.Log($"DBName : {DBName}");
        // 게임 테이블 명, TableName은 생성되면 알아서 테이블들이 담김. 게임을 제외한 나머지 테이블 제거
        TableName gameTable = new TableName();
        string[] deleteTable = { "user_info", "rank", "achievement", "pet" };
        for (int i = 0; i < deleteTable.Length; i++)
        {
            gameTable.list.Remove(deleteTable[i]);
        }

        // 분석표 테이블 -> analyticsTable

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
        Debug.Log("[DB] Complete Create DateDB table");

        // PresentDB 사용
        mySqlCommand.CommandText = $"USE `PresentDB`";
        mySqlCommand.ExecuteNonQuery();

        // PresentDB의 (table에 있는 (한 행의 Column Data를 담은 List)들을 여러 행으로 담은 List)들을 Table개수만큼 Array를 가지는 변수
        List<List<string>>[] valuesInColumnsInTable_Array = new List<List<string>>[gameTable.list.Count];

        // PresentDB에 있는 gamedata들 불러와서 위 변수에 저장
        for (int i = 0; i < gameTable.list.Count; i++)
        {
            // table에 몇 행이 있는지
            string rowCount_Command = $"SELECT COUNT(*) FROM `{gameTable.list[i]}`";
            MySqlCommand rowCount_SqlCmd = new MySqlCommand(rowCount_Command, connection);
            int rowCountInTable = (int)rowCount_SqlCmd.ExecuteScalar(); // ExcuteScalar() 메서드는 쿼리를 실행하고 결과 집합의 첫 번째 행의 첫 번째 열의 값을 반환

            string select_Command = $"SELECT * FROM {gameTable.list[i]}";
            MySqlCommand select_SqlCmd = new MySqlCommand(select_Command, connection);
            MySqlDataReader reader = select_SqlCmd.ExecuteReader();

            List<List<string>> column_Values_List = new List<List<string>>();

            while (reader.Read())
            {
                for (int j = 0; j < rowCountInTable; j++)
                {
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
                    }

                    column_Values_List.Add(column_Values);
                }
            }
            reader.Close();

            valuesInColumnsInTable_Array[i] = column_Values_List;
        }
        Debug.Log("[DB] Complete copy presentDB");

        // 생성한 DateDB 사용
        mySqlCommand.CommandText = $"USE {DBName};";
        mySqlCommand.ExecuteNonQuery();

        // DateDB에 presentDB에서 가져온 데이터(게임데이터만) 복사 생성
        for(int i = 0; i < gameTable.list.Count; i ++) // Table
        {
            for(int j=0; j < valuesInColumnsInTable_Array[i].Count; j ++) // Columns
            {
                // 0 -> licenseNumber, 1 -> charactor
                string insert_Command = $"INSERT INTO {gameTable.list[i]} (`{game_Columns[0]}`,`{game_Columns[1]}`) " +
                                        $"VALUES ({valuesInColumnsInTable_Array[i][j][0]},{valuesInColumnsInTable_Array[i][j][1]});";
                mySqlCommand.CommandText = insert_Command;
                mySqlCommand.ExecuteNonQuery();

                for (int k = 2; k < valuesInColumnsInTable_Array[i][j].Count; k++) // Column
                {
                    string update_Command = $"UPDATE {gameTable.list[i]} SET `{game_Columns[k]}` = @value " +
                                            $"WHERE `{game_Columns[0]}` = {valuesInColumnsInTable_Array[i][j][0]} AND `{game_Columns[1]}` = {valuesInColumnsInTable_Array[i][j][1]};";
                    mySqlCommand.CommandText = update_Command;

                    if(k == 2 || k == 5) // float
                    {
                        mySqlCommand.Parameters.Add("@value", MySqlDbType.Float).Value = float.Parse(valuesInColumnsInTable_Array[i][j][k]);
                    }
                    else // int
                    {
                        mySqlCommand.Parameters.Add("@value", MySqlDbType.Int32).Value = Int32.Parse(valuesInColumnsInTable_Array[i][j][k]);
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

        for (int i = 0; i < gameTable.list.Count; i++)
        {
            switch(i/6) // 스텝1~6
            {
                case 0: // venezia_kor_level1_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    //// 중복해서 초기화되지 않도록 예외처리
                    //if (valueList_ColumnArray_TableArray[i / 6] == null)
                    //{
                    //    valueList_ColumnArray_TableArray[i / 6] = new List<AnalyticsColumnValue>[valuesInColumnsInTable_Array[i].Count];
                    //}
                    //
                    //for (int j = 0; j < valuesInColumnsInTable_Array[i].Count; i ++) // 테이블에 저장된 행수(유저와 캐릭터들)
                    //{
                    //    // 중복해서 초기화되지 않도록 예외처리
                    //    if (valueList_ColumnArray_TableArray[i / 6][j] == null)
                    //    {
                    //        valueList_ColumnArray_TableArray[i / 6][j] = new List<AnalyticsColumnValue>();
                    //    }
                    //
                    //    // 데이터가 있다면(0이 아니라면) 추가
                    //    if(float.Parse(valuesInColumnsInTable_Array[i][j][2]) != 0 && Int32.Parse(valuesInColumnsInTable_Array[i][j][4]) != 0)
                    //    {
                    //        int _licenseNumber = Int32.Parse(valuesInColumnsInTable_Array[i][j][0]);
                    //        int _charactor = Int32.Parse(valuesInColumnsInTable_Array[i][j][1]);
                    //        float _reactionRate = float.Parse(valuesInColumnsInTable_Array[i][j][2]);
                    //        int _answerRate = Int32.Parse(valuesInColumnsInTable_Array[i][j][4]);
                    //        //AnalyticsColumnValue data = new AnalyticsColumnValue(_licenseNumber, _charactor, _reactionRate, _answerRate);
                    //        valueList_ColumnArray_TableArray[i / 6][j].Add(new AnalyticsColumnValue(_licenseNumber, _charactor, _reactionRate, _answerRate));
                    //    }
                    //}
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
        mySqlCommand.Parameters.Add("@reactionRate", MySqlDbType.Int32);
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
    }

}
