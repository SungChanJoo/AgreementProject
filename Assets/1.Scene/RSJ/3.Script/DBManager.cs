using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using UnityEngine.UI;
using System;

// �̱��� ����
// Ŭ���̾�Ʈ�� ������ ����������? Ŭ���̾�Ʈ�� �̹� ��ü������ IP�� Port�� �˾Ƽ� ������ ������
// Ŭ���̾�Ʈ�� �α��� �Ҷ� ID�� Password�� �Է����ٵ�, �׶� ������ �ִ� DB���� login table�� ���ؼ� true�� �α���
// ȸ����ϵ� �ϴ� �������´�
// DBManager�� �������� �����ϴ°� �´�

// ���� ����
public class User_Info
{
    public string user_Name { get; private set; }
    public string user_Password { get; private set; }

    public User_Info(string _name, string _password)
    {
        user_Name = _name;
        user_Password = _password;
    }
        
}

public class DBManager : MonoBehaviour
{
    public User_Info user_Info;

    MySqlConnection connection; // DB�� �����ϴ� Ŭ����
    MySqlDataReader reader;

    //public string path = string.Empty;

    // �����Ҷ� �ʿ��� ����
    private string str_Connection;

    // Table List
    private List<string> table_List;
    private TableName table;

    // Table - Columns
    private string[] userinfo_Columns;
    private string[] rank_Columns;
    private string[] achievement_Columns;
    private string[] pet_Columns;
    private string[] game_Columns;

    // ����-Ŭ���̾�Ʈ string���� data �ְ������ �����ϱ� ���� ���ڿ�
    private const string separatorString = "E|";

    // LicenseNumber �⺻, Table ���Ӹ�(���Ǽ��� ����)
    private int clientLicenseNumber_Base = 10000;

    public static DBManager instance = null; // �̱��� ������

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

    // DB ���ǿ� ������ �� �ʿ��� ���� �ʱ�ȭ
    private void SetStringConnection()
    {
        string ip = "127.0.0.1"; // �켱 ����(127.0.0.1)��, aws EC2 IP : 15.165.159.141
        string db = "present";
        string uid = "root"; //string.IsNullOrEmpty(user_Info.user_Name)? "" : user_Info.user_Name;
        string pw = "12345678"; //string.IsNullOrEmpty(user_Info.user_Password)? "" : user_Info.user_Password;
        string port = "3306";
        //str_Connection = $"Server={ip};Database={db};Uid={uid};Pwd={pw};Port={port};Charset=utf8;";
        str_Connection = $"Server={ip};" + $"Database={db};" + $"Uid={uid};" + $"Pwd={pw};" + $"Port={port};" + "CharSet=utf8;"; // ; �����ݷ� �����ض�
        Debug.Log($"[DB] DB connect info : {str_Connection}");
    }

    // DB�� ����
    private void ConnectDB()
    {
        try
        {
            connection = new MySqlConnection(str_Connection);
            connection.Open(); // DB ����
            Debug.Log("[DB] Success connect to db!");
        }
        catch(Exception e)
        {
            Debug.Log($"[DB] Fail connect to db + {e.Message}");
        }
    }

    private bool CheckConnection(MySqlConnection con)
    {
        // open�� �ȵǾ��ִٸ�
        if(con.State != System.Data.ConnectionState.Open)
        {
            con.Open(); // �ѹ� �� �����
            if (con.State != System.Data.ConnectionState.Open)
            {
                return false; //������� �ߴµ��� �ȵǸ� �̻��� ����
            }
        }
        return true;
    }

    // DB Table�� data�� ������ �� �ʿ��� ���� ����
    private void InitSetting()
    {
        // Tables / TableName ��ü ������ table.list �ڵ� ����
        table = new TableName(); 

        // Table - Columns
        userinfo_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Coin" };
        rank_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "User_Profile", "User_Name", "TotalTime", "TotalScore" };
        achievement_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "Something" };
        pet_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "White" };
        game_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };
    }

    // ������� // todo �񵿱�ȭ �����غ�����
    public string CreateLicenseNumber()
    {
        Debug.Log("[DB] CreateLicenseNumber method");

        if(!CheckConnection(connection))
        {
            Debug.Log("[DB] Didn't connect to DB");
            return "";
        }

        int count = 0;

        // DB user_info ���̺��� User_LicenseNumber�� � �ִ��� üũ
        string checkUserInfo_Command = $"SELECT User_LicenseNumber FROM user_info";
        MySqlCommand check_SqlCmd = new MySqlCommand(checkUserInfo_Command, connection);
        reader = check_SqlCmd.ExecuteReader();

        // table üũ
        if (reader.HasRows)
        {
            while(reader.Read())
            {
                count++;
            }
        }
        reader.Close(); // DataReader�� �����ִ� ���� �߰����� ��� ���� �Ұ�

        // user_info ����
        string table_Name = "user_info";
        string[] columns = { "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Coin" };
        int clientLicenseNumber = clientLicenseNumber_Base + count;
        int user_Charactor = 1;
        string user_Name = "Guest";
        int user_Profile = 1;
        int user_Coin = 0;

        // Binary Parameter
        //MySqlParameter binaryParameter = new MySqlParameter

        // �����ϴ� ������
        string createAccount_Command = $"INSERT INTO {table_Name} ({columns[0]}, {columns[1]}, {columns[2]}, {columns[3]}, {columns[4]}) " +
                                        $"VALUES (@clientLicenseNumber, @user_Charactor, @user_Name, @user_Profile, @user_Coin)"; // `(����ǥ), '(��ƽ) �����ϱ�
        MySqlCommand insert_SqlCmd = new MySqlCommand(createAccount_Command, connection);
        insert_SqlCmd.Parameters.Add("@clientLicenseNumber", MySqlDbType.Int32).Value = clientLicenseNumber;
        insert_SqlCmd.Parameters.Add("@user_Charactor", MySqlDbType.Int32).Value = user_Charactor;
        insert_SqlCmd.Parameters.Add("@user_Name", MySqlDbType.VarChar).Value = user_Name;
        insert_SqlCmd.Parameters.Add("@user_Profile", MySqlDbType.Int32).Value = user_Profile;
        insert_SqlCmd.Parameters.Add("@user_Coin", MySqlDbType.Int32).Value = user_Coin;
        //insert_SqlCmd.ExecuteNonQuery(); 
        
        insert_SqlCmd.ExecuteNonQuery(); // command.ExecuteNonQuery()�� DB���� ���� �۾��� �����ϴ� SQL ��ɹ��� �����ϰ�, ������ ���� ���� ���� ��ȯ�ϴ� �޼���

        string returnData = $"{clientLicenseNumber}|{user_Charactor}";
        return returnData;
    }

    // �� �÷��̾� ������ ����
    public void CreateNewPlayerData(int clientlicensenumber)
    {
        Debug.Log($"[DB] Create new Player data, client's licensenumber : {clientlicensenumber}");
        // ClientLicenseNumber�� �߱޵Ǹ� �� �÷��̾� ������ ����� ���̹Ƿ�, ��� ���̺� data�� �߰��ؾ���
        string insertCreatePlayerData_Command;
        MySqlCommand insert_SqlCmd = new MySqlCommand();
        insert_SqlCmd.Connection = connection;

        // Update Query
        string updatePlayerData_Command;
        MySqlCommand update_SqlCmd = new MySqlCommand();
        update_SqlCmd.Connection = connection;

        // table.list[0] -> userinfo, [1]->rank, [2]->achievement, [3]->pet

        // rank table
        //string rank_TableName = "rank";
        //string[] rank_Columns = { "User_LicenseNumber", "User_Charactor", "TotalTime", "TotalScore" };
        int rank_valuepart;

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {table.list[1]} ({rank_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < rank_Columns.Length; i++) // 0->licensenumber, 1->charactor
        {
            if (i == 0) rank_valuepart = clientlicensenumber;
            else if (i == 1) rank_valuepart = 1;
            else rank_valuepart = 0;

            updatePlayerData_Command = $"UPDATE {table.list[1]} SET {rank_Columns[i]} = {rank_valuepart}";
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

            updatePlayerData_Command = $"UPDATE {table.list[2]} SET {achievement_Columns[i]} = {achievement_valuepart}";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] achievement table complete!");

        // pet table
        //string pet_TableName = "pet";
        //string[] pet_Columns = { "User_LicenseNumber", "User_Charactor", "White" };
        int pet_valuepart;

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {table.list[3]} ({pet_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < pet_Columns.Length; i++)
        {
            if (i == 0) pet_valuepart = clientlicensenumber;
            else if (i == 1) pet_valuepart = 1;
            else pet_valuepart = 0;

            updatePlayerData_Command = $"UPDATE {table.list[3]} SET {pet_Columns[i]} = {pet_valuepart}";
            if (i==2) // float
            {
                updatePlayerData_Command = $"UPDATE {table.list[3]} SET {pet_Columns[i]} = {(float)pet_valuepart}";
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

        for (int i = 0; i < game_Names.Length; i++)
        {
            for (int j = 0; j < levels.Length; j++)
            {
                for (int k = 0; k < steps.Length; k++)
                {
                    string levelpart = $"level{levels[j]}";
                    if (game_Names[i] == "venezia_chn")
                    {
                        j = 2; // venezia_chn ������ level�� 1�����̹Ƿ� �ѹ��� ���ƾ���.
                        levelpart = "level";
                    }
                    game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";
                    game_TableList.Add(game_TableName);

                }
            }
        }

        for (int i = 0; i < game_TableList.Count; i++)
        {
            // insert row
            insertCreatePlayerData_Command = $"INSERT INTO {game_TableList[i]} (`User_LicenseNumber`, `User_Charactor`) VALUES ({clientlicensenumber}, {user_Charactor})";
            insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
            insert_SqlCmd.ExecuteNonQuery();

            // update rowm j=2���� �����ϴ� ������ 0�� licensenumber�� 1�� charactor��ȣ�̰� insert�� �־���� ������ ���� �� ������Ʈ�� �� �ʿ䰡 ����
            for (int j = 2; j < game_Columns.Length; j++)
            {
                updatePlayerData_Command = $"UPDATE {game_TableList[i]} SET {game_Columns[j]} = @value";
                update_SqlCmd.CommandText = updatePlayerData_Command;

                // parameter �ʱ�ȭ
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

                /*
                 string createAccount_Command = $"INSERT INTO {table_Name} ({columns[0]}, {columns[1]}, {columns[2]}, {columns[3]}, {columns[4]}) " +
                                        $"VALUES (@clientLicenseNumber, @user_Charactor, @user_Name, @user_Profile, @user_Coin)"; // `(����ǥ), '(��ƽ) �����ϱ�
        MySqlCommand insert_SqlCmd = new MySqlCommand(createAccount_Command, connection);
        insert_SqlCmd.Parameters.Add("@clientLicenseNumber", MySqlDbType.Int32).Value = clientLicenseNumber;
        insert_SqlCmd.Parameters.Add("@user_Charactor", MySqlDbType.Int32).Value = user_Charactor;
        insert_SqlCmd.Parameters.Add("@user_Name", MySqlDbType.VarChar).Value = user_Name;
        insert_SqlCmd.Parameters.Add("@user_Profile", MySqlDbType.Int32).Value = user_Profile;
        insert_SqlCmd.Parameters.Add("@user_Coin", MySqlDbType.Int32).Value = user_Coin;
        //insert_SqlCmd.ExecuteNonQuery(); 
                 */
            }
        }
        Debug.Log("[DB] game table complete!");
    }

    // �÷��̾� ������ �ҷ�����, DB���� �ҷ��ͼ� ������ Ŭ���̾�Ʈ���� ���ټ� �ְ� string���� ��� ��ȯ�� string
    public List<string> LoadPlayerData(int clientlicensenumber, int clientcharactor)
    {
        List<string> return_TableData = new List<string>();
        string return_TempData;

        string selectTable_Command;
        MySqlCommand select_SqlCmd = new MySqlCommand();
        select_SqlCmd.Connection = connection;


        for(int i = 0; i < table.list.Count; i++) // table_List
        {
            // �� ���̺� �ִ� licensenumber�� �о���� �ٸ� �÷��� �о���� �ʾҴµ� �ٸ� �÷��� �ִ� ���� ���������� �翬�� ������ ���� ������?
            //selectTable_Command = $"SELECT User_LicenseNumber FROM {table.list[i]}";
            selectTable_Command = $"SELECT * FROM {table.list[i]}";
            select_SqlCmd.CommandText = selectTable_Command;
            reader = select_SqlCmd.ExecuteReader();

            // table, User_LicenseNumber������ ���� �ִٸ�
            if (reader.HasRows)
            {
                // �д´�. �дµ���
                while (reader.Read())
                {
                    int dbLicenseNumber = reader.GetInt32("User_LicenseNumber");
                    // int dbcharactor = reader.GetInt32("User_Charactor"); ���߿� ĳ���� ����� �ʿ��� ����. todo

                    // Ŭ���̾�Ʈ ���̼����� db�� �ִ� ���̼����� ���ٸ� �����͸� �ҷ��´�
                    if (clientlicensenumber == dbLicenseNumber)
                    {
                        switch(table.list[i])
                        {
                            case "user_info":
                                //string selectColumn_Command = $"SELECT * FROM {table.list[i]}";
                                //MySqlCommand selectColumn_SqlCmd = new MySqlCommand(selectColumn_Command, connection);
                                //MySqlDataReader tempReader = selectColumn_SqlCmd.ExecuteReader();

                                Debug.Log($"[DB] tableName : {table.list[i]}, dbLicenseNumber : {dbLicenseNumber}");
                                Debug.Log("[DB] LoadPlayerData - user_info table");
                                string user_Name = reader.GetString("User_Name");
                                //string user_Name = reader["User_Name"].ToString();
                                Debug.Log("[DB] User_Name Get????");
                                // db���� binary Ÿ�� ������ �����ͼ� string���� ��ȯ
                                //// ���� ���� �������� ����
                                //int bytesRead = (int)reader.GetBytes(reader.GetOrdinal("User_Profile"), 0, null, 0, 1000);
                                //// ���� ������ ���̸�ŭ �迭 ����
                                //byte[] binaryData = new byte[bytesRead];
                                //// �����͸� �о�ͼ� �迭�� ���� - binaryData(buffer)�� �����
                                //reader.GetBytes(reader.GetOrdinal("User_Profile"), 0, binaryData, 0, 1000);
                                //string user_Profile = Convert.ToBase64String(binaryData);
                                string user_Profile = reader.GetInt32("User_Profile").ToString();
                                Debug.Log("[DB] User_Profile Get????");
                                string user_Coin = reader.GetInt32("User_Coin").ToString();
                                Debug.Log("[DB] User_Coin Get????");
                                return_TempData = $"{table.list[i]}|{user_Name}|{user_Profile}|{user_Coin}|{separatorString}"; // �� �տ� tagó�� ������ �� �ְ� Ư���� ����(User)
                                return_TableData.Add(return_TempData);
                                break;
                            case "rank":
                                Debug.Log("[DB] LoadPlayerData - rank table");
                                string rank_TotalTime = reader.GetString("TotalTime");
                                string rank_TotalScore = reader.GetString("TotalScore");
                                return_TempData = $"{table.list[i]}|{rank_TotalTime}|{rank_TotalScore}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "achievement":
                                Debug.Log("[DB] LoadPlayerData - achievement table");
                                string achievement_Something = reader.GetInt32("Something").ToString();
                                return_TempData = $"{table.list[i]}|{achievement_Something}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "pet":
                                Debug.Log("[DB] LoadPlayerData - pet table");
                                string pet_White = reader.GetInt32("White").ToString();
                                return_TempData = $"{table.list[i]}|{pet_White}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            default:
                                Debug.Log("[DB] LoadPlayerData - game table");
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

    public void SaveGameResultData(List<string> dataList)
    {
        // DB gametable column�� : User_Licensenumber/User_Charactor/ReactionRate/AnswerCount/AnswerRate/Playtime/TotalScore/StarPoint
        // dataList�� [0]�� �����ϰ� value(int)�� ����. index������ RequestName[0]/level[1]/step[2]/User_Licensenumber[3]/User_Charactor[4]/ReactionRate[5]/.../TotalScore[9]
        Debug.Log($"[DB] SaveGameResultData, gameName : {dataList[0]}"); // [Save]gameName
        //string gameName = dataList[0].Split("[Save]", StringSplitOptions.RemoveEmptyEntries).ToString();
        string gameName = dataList[0].Substring("[Save]".Length);
        Debug.Log($"[DB] gameName : {gameName}");
        string table_Name = $"{gameName}_level{dataList[1]}_step{dataList[2]}";
        if(gameName == "venezia_chn") table_Name = $"{gameName}_level_step{dataList[2]}";

        // gametable�� ����� totalscore �����ͼ� �� (�� ������ �ְ�����)
        int gameresult_Score = Int32.Parse(dataList[9]);
        int db_Score = 0;

        // Score�� StarPoint(0,1,2,3) �Ҵ�
        int starPoint = 0;

        // DB ��ȸ
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
                // DB�� �ִ� licensenumber�� charactor�� �����Ϸ��� �������� licensenumber�� charactor�� ���ٸ� (�ش� �࿡ �ִ� �� ������)
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

        // �� ���������� db �������� ��
        Debug.Log($"[DB] gameresult_Score, DB_Score : {gameresult_Score}, {db_Score}");
        if (gameresult_Score >= db_Score) // ����
        {
            // gameresult_Score�� StarPoint �Ҵ���
            if (gameresult_Score >= 25000) starPoint = 3;
            else if (gameresult_Score >= 12500) starPoint = 2;
            else if (gameresult_Score >= 6500) starPoint = 1;

            // rank table�� TotalTime�� TotalScore�� ���� ���� 
            UpdateRankTable(Int32.Parse(dataList[3]), Int32.Parse(dataList[4]), float.Parse(dataList[8]), gameresult_Score);
        }
        else // ���� X
        {
            // ������ �ʿ䰡 �����Ƿ� rank table���� TotalTime�� TotalScore�� ���� ���� �� return
            UpdateRankTable(Int32.Parse(dataList[3]), Int32.Parse(dataList[4]), float.Parse(dataList[8]), gameresult_Score);
            return;
        }

        Debug.Log($"[DB] starPoint : {starPoint}");
        // starpoint value�� list�� �߰�
        dataList.Add(starPoint.ToString());
        Debug.Log($"[DB] dataList's Count : {dataList.Count}");
        Debug.Log($"[DB] dataList[9]'s value : {dataList[9]}");
        Debug.Log($"[DB] dataList[10]'s value : {dataList[10]}");

        //// �÷���-�� Tuple
        //Tuple<string, int>[] columns_Tuple = new Tuple<string, int>[8];
        //for(int i = 0; i < columns_Tuple.Length; i++)
        //{
        //    columns_Tuple[i] = new Tuple<string, int>(game_Columns[i], Int32.Parse(dataList[i+3]));
        //}

        //// �������̺� values update
        //// i=0 -> licensenumber / i=1 -> charactor ==> table���� Ư������(���̼���,ĳ����)�� �����ϴ� column�� ������Ʈ��
        //for(int i = 2; i < columns_Tuple.Length; i++)
        //{
        //    Debug.Log($"[DB] Update Tuple problem?? ....");
        //    string updateGameData_Command = $"UPDATE `{table_Name}` SET `{columns_Tuple[i].Item1}` = '{columns_Tuple[i].Item2}' WHERE `{columns_Tuple[0].Item1}` = '{columns_Tuple[0].Item2}' AND `{columns_Tuple[1].Item1}` = '{columns_Tuple[1].Item2}'";
        //    MySqlCommand update_SqlCmd = new MySqlCommand(updateGameData_Command, connection);
        //    update_SqlCmd.ExecuteNonQuery();
        //}

        for (int i = 2; i < game_Columns.Length; i++) // game_Columns = [0]~[7]
        {
            Debug.Log($"[DB] Update Tuple problem?? ....");
            Debug.Log($"[DB] Check dataList[10] is ? : {dataList[10]}");
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

    // ������ ���������� rank table�� �ð�, ���� ����
    private void UpdateRankTable(int licensenumber, int charactor, float time, int score)
    {
        // rank table -> [0]:User_LicenseNumber, [1]:User_Charactor, [2]:User_Profile, [3]:User_Name, [4]:TotalTime, [5]"TotalScore
        Debug.Log($"[DB] Updating... rank table, licensenumber : {licensenumber}, characator : {charactor}");
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

    // �Ϸ簡 ������ �� presentDB gamedata -> ����DB �����ϰ� gamedata ����
    public void CreateDaysExGameDataDB()
    {
        Debug.Log("[DB] CreateDaysExGameDataDB");

        // ������ DB �̸�
        string DBName = $"{DateTime.Now.Year}.{DateTime.Now.Month}.{DateTime.Now.Day}";
        Debug.Log($"DBName : {DBName}");
        // ���� ���̺� ��, TableName�� �����Ǹ� �˾Ƽ� ���̺���� ���. ������ ������ ������ ���̺� ����
        TableName gameTable = new TableName();
        string[] deleteTable = { "user_info", "rank", "achievement", "pet"};
        for(int i =0; i < deleteTable.Length; i++)
        {
            gameTable.list.Remove(deleteTable[i]);
        }

        // DB���� ��ɹ�
        string createDB_Command = $"CREATE DATABASE IF NOT EXISTS `{DBName}`;";
        // DB��� ��ɹ�
        string useDB_Command = $"USE `{DBName}`;";

        // DB����
        MySqlCommand createDB_SqlCmd = new MySqlCommand(createDB_Command, connection);
        createDB_SqlCmd.ExecuteNonQuery();

        // DB���
        MySqlCommand useDB_SqlCmd = new MySqlCommand(useDB_Command, connection);
        useDB_SqlCmd.ExecuteNonQuery();

        // Table���� ��ɹ�
        // �� ���̺�� �����ϰ� �÷��� ����
        for(int i = 0; i < gameTable.list.Count; i++)
        {
            string createTable_Command = $"CREATE TABLE IF NOT EXISTS {gameTable.list[i]} (";
            for(int j = 0; j < game_Columns.Length; j++)
            {
                if(j == 2 || j ==5) // float
                {
                    createTable_Command += $"{game_Columns[j]} float DEFAULT NULL,";
                }
                else // int
                {
                    createTable_Command += $"{game_Columns[j]} int(11) DEFAULT NULL,";
                }
            }
            createTable_Command = createTable_Command.TrimEnd(','); // ������ ��ǥ ����
            createTable_Command += $") ENGINE = InnoDB DEFAULT CHARSET = latin1 COLLATE = latin1_swedish_ci;";
            //ENGINE = InnoDB DEFAULT CHARSET = latin1 COLLATE = latin1_swedish_ci;

            // Table ����
            MySqlCommand createTable_SqlCmd = new MySqlCommand(createTable_Command, connection);
            createTable_SqlCmd.ExecuteNonQuery();
        }

        // PresentDB ���
        useDB_SqlCmd.CommandText = $"USE `PresentDB`";
        useDB_SqlCmd.ExecuteNonQuery();

        // PresentDB�� �ִ� gamedata�� �ҷ�����
        for(int i=0; i<gameTable.list.Count; i++)
        {
            // table�� �� ���� �ִ���
            string rowCount_Command = $"SELECT COUNT(*) FROM `{gameTable.list[i]}`";
            MySqlCommand rowCount_SqlCmd = new MySqlCommand(rowCount_Command, connection);
            int rowCountInTable = (int)rowCount_SqlCmd.ExecuteScalar(); // ExcuteScalar() �޼���� ������ �����ϰ� ��� ������ ù ��° ���� ù ��° ���� ���� ��ȯ

            string select_Command = $"SELECT * FROM {gameTable.list[i]}";
            MySqlCommand select_SqlCmd = new MySqlCommand(select_Command, connection);
            MySqlDataReader reader = select_SqlCmd.ExecuteReader();

            // PresentDB �� table�� �ִ� data�� �ޱ����� List
            List<List<string>> table_Values = new List<List<string>>();

            while(reader.Read())
            {
                for(int j = 0; j < rowCountInTable; j++)
                {
                    List<string> columns_Value = new List<string>();

                    for(int k = 0; k < game_Columns.Length; k++)
                    {
                        if(k == 2 || k == 5) // float
                        {
                            columns_Value.Add(reader.GetFloat(game_Columns[k]).ToString());
                        }
                        else // int
                        {
                            columns_Value.Add(reader.GetInt32(game_Columns[k]).ToString());
                        }
                    }

                    table_Values.Add(columns_Value);
                }
            }
            reader.Close();

            // �� ���̺� ����
            

        }


        /*
        
        ��, �½��ϴ�. CREATE DATABASE�� CREATE TABLE�� ������ ��ɹ��̹Ƿ�, �Ѳ����� �Ѱ��־�� �� �˴ϴ�. 
        MariaDB�� MySQL�� ���� �����ͺ��̽� �ý��ۿ����� ������ ��ɹ��� �����Ͽ� �����ؾ� �մϴ�.
        ���� CREATE DATABASE�� USE �׸��� CREATE TABLE �� ���� ���� SQL ��ɹ��� �ϳ��� ���ڿ��� ���ļ� �����ϸ� ���� ������ �߻��� �� �ֽ��ϴ�. 
         */
        /*
         CREATE DATABASE IF NOT EXISTS `test` /*!40100 DEFAULT CHARACTER SET latin1 COLLATE latin1_swedish_ci
        USE `test`;

        CREATE TABLE IF NOT EXISTS `calculation_level1_step1` (
          `User_LicenseNumber` int(11) DEFAULT NULL,
          `User_Charactor` int(11) DEFAULT NULL,
          `ReactionRate` int(11) DEFAULT NULL,
          `AnswerCount` int(11) DEFAULT NULL,
          `AnswerRate` int(11) DEFAULT NULL,
          `Playtime` int(11) DEFAULT NULL,
          `TotalScore` int(11) DEFAULT NULL,
          `StarPoint` int(11) DEFAULT NULL
        ) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci; */

    }

    // �ӽ÷� PresentDB�� �ִ� Rank Table ������ ��� / Score, Time �� Rank 1~5�� �� 6��° �ڱ� �ڽ��� ������ 
    public List<string> RankOrderByUserData(int licensenumber, int charactor)
    {
        // rank table -> [0]:User_LicenseNumber, [1]:User_Charactor, [2]:User_Profile, [3]:User_Name, [4]:TotalTime, [5]"TotalScore
        List<string> return_List = new List<string>();

        // rank table's row count
        string rowCount_Command = $"SELECT COUNT(*) FROM `rank`";
        MySqlCommand rowCount_SqlCmd = new MySqlCommand(rowCount_Command, connection);
        int rowCountInTable = (int)rowCount_SqlCmd.ExecuteScalar(); // ExcuteScalar() �޼���� ������ �����ϰ� ��� ������ ù ��° ���� ù ��° ���� ���� ��ȯ

        // Score ����
        string selectScore_Command = $"SELECT * FROM `rank`";
        MySqlCommand selectScore_SqlCmd = new MySqlCommand(selectScore_Command, connection);
        MySqlDataReader reader = selectScore_SqlCmd.ExecuteReader();

        List<List<string>> rankdata = new List<List<string>>(); 

        // table�� �ִ� data�� rankdata�� ���
        while(reader.Read())
        {
            for(int i = 0; i < rowCountInTable; i++)
            {
                List<string> valuesInColumn = new List<string>();
                
                for (int j = 0; j < rank_Columns.Length; j++)
                {
                    if(j == 2) // byte[]
                    {
                        byte[] binarydata = reader[$"{rank_Columns[j]}"] as byte[]; // column�� �ִ� binary data �޾ƿ���
                        valuesInColumn.Add(System.Text.Encoding.UTF8.GetString(binarydata));
                    }
                    else if(j == 3) // Varchar
                    {
                        valuesInColumn.Add(reader.GetString(rank_Columns[j]));
                    }
                    else if(j == 4) // Float
                    {
                        valuesInColumn.Add(reader.GetFloat(rank_Columns[j]).ToString());
                    }
                    else // int
                    {
                        valuesInColumn.Add(reader.GetInt32(rank_Columns[j]).ToString());
                    }
                }
                rankdata.Add(valuesInColumn);
            }
        }
        reader.Close();


        // �������� ������
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

        // totalScore�� �������� ����
        scoreList.Sort((a, b) => b.totalScore.CompareTo(a.totalScore));

        // ���ĵ� List������ 1~5�� ������ ����,���� ����
        for(int i = 0; i < scoreList.Count; i++)
        {
            //���� ������(value) place/Profile/Name/total
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

        // ����ó��, rank data�� 5���� ����� ������ return_List�� index[5]���� �󳻿����� ����
        if(scoreList.Count < 5)
        {
            int user_Count = scoreList.Count;

            while(user_Count < 5)
            {
                //���� ������(value) place/Profile/Name/total
                return_List.Add($"{user_Count}|0|None|0|{separatorString}");
                user_Count++;
            }
        }

        // return_List index[5]�� ���� ����,���� ����
        for (int i = 0; i < scoreList.Count; i++)
        {
            if (scoreList[i].userlicensenumber == licensenumber && scoreList[i].usercharactor == charactor)
            {
                string return_string;
                return_string = $"{i}|{scoreList[i].userProfile}|{scoreList[i].userName}|{scoreList[i].totalScore}|{separatorString}";
                return_List.Add(return_string);
            }
        }

        // Time ����
        List<Rank_Time> timeList = new List<Rank_Time>();

        for (int i = 0; i < rankdata.Count; i++)
        {
            Rank_Time time = new Rank_Time();

            time.place = 0;
            time.userlicensenumber = Int32.Parse(rankdata[i][0]);
            time.usercharactor = Int32.Parse(rankdata[i][1]);
            time.userProfile = System.Text.Encoding.UTF8.GetBytes(rankdata[i][2]);
            time.userName = rankdata[i][3];
            time.totalTime = float.Parse(rankdata[i][5]);

            timeList.Add(time);
        }

        // totalTime���� �������� ����
        timeList.Sort((a, b) => b.totalTime.CompareTo(a.totalTime));

        // ���ĵ� List������ 1~5�� ������ ����, �ð� ����
        for (int i = 0; i < timeList.Count; i++)
        {
            //���� ������(value) place/Profile/Name/total
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

        // ����ó��, rank data�� 5���� ����� ������ return_List�� index[5]���� �󳻿����� ����
        if (timeList.Count < 5)
        {
            int user_Count = timeList.Count;

            while (user_Count < 5)
            {
                //���� ������(value) place/Profile/Name/total
                return_List.Add($"{user_Count}|0|None|0|{separatorString}");
                user_Count++;
            }
        }

        // return_List index[11]�� ���� ����,���� ����
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



}
