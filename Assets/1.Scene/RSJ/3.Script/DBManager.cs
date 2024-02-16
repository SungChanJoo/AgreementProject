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

    // LicenseNumber �⺻, Table ���Ӹ�(���Ǽ��� ����)
    private int clientLicenseNumber_Base = 10000;
    private string licenseNumber_Column = "User_LicenseNumber";
    public static string venezia_kor = "venezia_kor";
    public static string venezia_eng = "venezia_eng";
    public static string venezia_chn = "venezia_chn";
    public static string calculation = "calculation";
    public static string gugudan = "gugudan";

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
        string db = "test";
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
        table_List = new List<string>();
        table_List.Add("user_info");
    }

    // ������� // todo �񵿱�ȭ �����غ�����
    public int CreateLicenseNumber()
    {
        Debug.Log("[DB] CreateLicenseNumber method");

        if(!CheckConnection(connection))
        {
            Debug.Log("[DB] Didn't connect to DB");
            return -1;
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
        
        reader.Close(); // DataReader�� �����ִ� ���� �߰����� ��� ���� �Ұ�
        insert_SqlCmd.ExecuteNonQuery(); // command.ExecuteNonQuery()�� DB���� ���� �۾��� �����ϴ� SQL ��ɹ��� �����ϰ�, ������ ���� ���� ���� ��ȯ�ϴ� �޼���

        return clientLicenseNumber;
    }

    // �� �÷��̾� ������ ����
    public void CreateNewPlayerData(int clientlicensenumber)
    {
        // ClientLicenseNumber�� �߱޵Ǹ� �� �÷��̾� ������ ����� ���̹Ƿ�, ��� ���̺� data�� �߰��ؾ���
        string insertCreatePlayerData_Command;
        MySqlCommand insert_SqlCmd = new MySqlCommand();
        insert_SqlCmd.Connection = connection;

        string updatePlayerData_Command;
        MySqlCommand update_SqlCmd = new MySqlCommand();
        update_SqlCmd.Connection = connection;

        // rank table
        string rank_TableName = "rank";
        string[] rank_Columns = { "User_LicenseNumber", "User_Charactor", "TotalTime", "TotalScore" };
        int rank_valuepart;
        table_List.Add(rank_TableName);

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {rank_TableName} ({rank_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < rank_Columns.Length; i++)
        {
            if (i == 0) rank_valuepart = clientlicensenumber;
            else if (i == 1) rank_valuepart = 1;
            else rank_valuepart = 0;

            updatePlayerData_Command = $"UPDATE {rank_TableName} SET {rank_Columns[i]} = {rank_valuepart}";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] Rank table complete!");

        // achievement table
        string achievement_TableName = "achievement";
        string[] achievement_Columns = { "User_LicenseNumber", "User_Charactor" };
        int achievement_valuepart;
        table_List.Add(achievement_TableName);

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {achievement_TableName} ({achievement_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < achievement_Columns.Length; i++)
        {
            if (i == 0) achievement_valuepart = clientlicensenumber;
            else if (i == 1) achievement_valuepart = 1;
            else achievement_valuepart = 0;

            updatePlayerData_Command = $"UPDATE {achievement_TableName} SET {achievement_Columns[i]} = {achievement_valuepart}";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] achievement table complete!");

        // pet table
        string pet_TableName = "pet";
        string[] pet_Columns = { "User_LicenseNumber", "User_Charactor" };
        int pet_valuepart;
        table_List.Add(pet_TableName);

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {pet_TableName} ({pet_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < pet_Columns.Length; i++)
        {
            if (i == 0) pet_valuepart = clientlicensenumber;
            else if (i == 1) pet_valuepart = 1;
            else pet_valuepart = 0;

            updatePlayerData_Command = $"UPDATE {pet_TableName} SET {pet_Columns[i]} = {pet_valuepart}";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] pet table complete!");

        // game table
        string game_TableName;

        string[] game_Names = { "calculation", "venezia_chn" };
        int[] levels = { 1, 2, 3 };
        int[] steps = { 1, 2, 3, 4, 5, 6 };

        string[] game_Columns = { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };

        int user_Charactor = 1;

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
                        levelpart = "level";
                    }
                    game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";
                    game_TableList.Add(game_TableName);
                    table_List.Add(game_TableName);

                }
            }
        }

        for (int i = 0; i < game_TableList.Count; i++)
        {
            // insert row
            insertCreatePlayerData_Command = $"INSERT INTO {game_TableList[i]} (User_LicenseNumber, User_Charactor) VALUES ({clientlicensenumber}, {user_Charactor})";
            insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
            insert_SqlCmd.ExecuteNonQuery();

            // update row
            for (int j = 0; j < game_Columns.Length; j++)
            {
                updatePlayerData_Command = $"UPDATE {game_TableList[i]} SET {game_Columns[j]} = '0'";
                update_SqlCmd.CommandText = updatePlayerData_Command;
                update_SqlCmd.ExecuteNonQuery();
                //insertCreatePlayerData_Command = $"INSERT INTO {game_TableList[i]} ({game_Columns[j]}) VALUES (0)";
                //insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
                //insert_SqlCmd.ExecuteNonQuery();
            }
        }
        Debug.Log("[DB] game table complete!");
    }

    // �÷��̾� ������ �ҷ�����, DB���� �ҷ��ͼ� ������ Ŭ���̾�Ʈ���� ���ټ� �ְ� string���� ��� ��ȯ�� string
    public List<string> LoadPlayerData(ClientLoginStatus loginstatus, int clientlicensenumber)
    {
        List<string> return_TableData = new List<string>();
        string return_TempData;

        string selectTable_Command;
        MySqlCommand select_SqlCmd = new MySqlCommand();
        select_SqlCmd.Connection = connection;


        for(int i = 0; i < table_List.Count; i++)
        {
            selectTable_Command = $"SELECT User_LicenseNumber FROM {table_List[i]}";
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
                        switch(table_List[i])
                        {
                            case "user_info":
                                string selectColumn_Command = $"SELECT * FROM {table_List[i]}";
                                MySqlCommand selectColumn_SqlCmd = new MySqlCommand(selectColumn_Command, connection);
                                MySqlDataReader tempReader = selectColumn_SqlCmd.ExecuteReader();

                                Debug.Log($"[DB] tableName : {table_List[i]}, dbLicenseNumber : {dbLicenseNumber}");
                                Debug.Log("[DB] LoadPlayerData - user_info table");
                                string user_Name = tempReader.GetString("User_Name");
                                //string user_Name = reader["User_Name"].ToString();
                                Debug.Log("[DB] User_Name Get????");
                                // db���� binary Ÿ�� ������ �����ͼ� string���� ��ȯ
                                // ���� ���� �������� ����
                                int bytesRead = (int)tempReader.GetBytes(reader.GetOrdinal("User_Profile"), 0, null, 0, 1000);
                                // ���� ������ ���̸�ŭ �迭 ����
                                byte[] binaryData = new byte[bytesRead];
                                // �����͸� �о�ͼ� �迭�� ���� - binaryData(buffer)�� �����
                                tempReader.GetBytes(reader.GetOrdinal("User_Profile"), 0, binaryData, 0, 1000);
                                string user_Profile = Convert.ToBase64String(binaryData);
                                Debug.Log("[DB] User_Profile Get????");
                                string user_Coin = tempReader.GetInt32("User_Coin").ToString();
                                Debug.Log("[DB] User_Coin Get????");
                                return_TempData = $"{user_Name}|{user_Profile}|{user_Coin}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "rank":
                                Debug.Log("[DB] LoadPlayerData - rank table");
                                string rank_TotalTime = reader.GetString("TotalTime");
                                string rank_TotalScore = reader.GetString("TotalScore");
                                return_TempData = $"{rank_TotalTime}|{rank_TotalScore}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "achievement":
                                Debug.Log("[DB] LoadPlayerData - achievement table");
                                string achievement_Something = reader.GetInt32("Something").ToString();
                                return_TempData = $"{achievement_Something}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "pet":
                                Debug.Log("[DB] LoadPlayerData - pet table");
                                string pet_Something = reader.GetInt32("Something").ToString();
                                return_TempData = $"{pet_Something}";
                                return_TableData.Add(return_TempData);
                                break;
                            default:
                                Debug.Log("[DB] LoadPlayerData - game table");
                                string game_ReactionRate = reader.GetInt32("ReactionRate").ToString();
                                string game_AnswerCount = reader.GetInt32("AnswerCount").ToString();
                                string game_AnswerRate = reader.GetInt32("AnswerRate").ToString();
                                string game_Playtime = reader.GetInt32("Playtime").ToString();
                                string game_TotalScore = reader.GetInt32("TotalScore").ToString();
                                string game_StarPoint = reader.GetInt32("StarPoint").ToString();
                                return_TempData = $"{game_ReactionRate}|{game_AnswerCount}|{game_AnswerRate}|{game_Playtime}|{game_TotalScore}|{game_StarPoint}";
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
        // dataList index������ ���Ӹ�[0]/����[1]/����[2]/����[3]/�ð�[4]
        if (dataList[1] == "0") dataList[1] = null;
        string table_Name = $"{dataList[0]}_level{dataList[1]}_step{dataList[2]}";
        string column_Name = $"TotalScore";
        int score = int.Parse(dataList[3]);

        // DB table�� �ִ� score ���ؼ� �� ���� ���� DB�� ����
        string selectGameData_Command = $"SELECT {column_Name} FROM {table_Name}";
        MySqlCommand select_SqlCmd = new MySqlCommand(selectGameData_Command, connection);
       
        MySqlDataReader reader = select_SqlCmd.ExecuteReader();

        if (reader.HasRows)
        {
            while (reader.Read())
            {
                int dbScore = reader.GetInt32("TotalScore");
                score = score > dbScore ? score : dbScore;
            }
        }
        reader.Close();

        string insertGameData_Command = $"INSERT INTO {table_Name} ({column_Name}) VALUES ({score})";
        MySqlCommand insert_SqlCmd = new MySqlCommand(insertGameData_Command, connection);
        insert_SqlCmd.ExecuteNonQuery();
    }


    // Login, Client���� �ҷ��ð���, �α����Ҷ� ���̵�, ��� �Է�
    //public bool Login(string id, string pw)
    //{
    //    // �����͸� DB���� ������
    //    // ��ȸ�Ǵ� �����Ͱ� ������ False
    //    // ��ȸ�� �Ǵ� �����Ͱ� ������ True�ε�, �� user_Info���ٰ� ��������
    //    // 1. Connection�� Open���� Ȯ�� - �޼���
    //    // 2. Reader ���°� �а� �ִ��� Ȯ�� - �� �������� �ϳ�
    //    // 3. �����͸� �� �о����� Close();

    //    try
    //    {
    //        // 1��
    //        if (!CheckConnection(connection))
    //        {
    //            Debug.Log("DB�� ������� �ʾҽ��ϴ�.");
    //            return false;
    //        }

    //        string login_Command = string.Format(@"SELECT User_Name, User_Password FROM User_Info WHERE User_Name = '{0}' AND User_Password = '{1}';", id, pw);
    //        MySqlCommand cmd = new MySqlCommand(login_Command, connection);
    //        reader = cmd.ExecuteReader(); // SELECT

    //        // Reader�� ������ �ϳ��̻� �����ϸ�
    //        if(reader.HasRows)
    //        {
    //            // ���� �����͸� �ϳ��� ����
    //            while (reader.Read())
    //            {
    //                string name = (reader.IsDBNull(0)) ? string.Empty : (string)reader["User_Name"].ToString();
    //                string password = (reader.IsDBNull(1)) ? string.Empty : (string)reader["User_Password"].ToString();

    //                // �� �� ������� �ʴٸ� � ���� �Ǿ��� �����Ͱ� �ҷ������ٴ°�
    //                if (!name.Equals(string.Empty) || !password.Equals(string.Empty))
    //                {
    //                    user_Info = new User_Info(name, password); // DB���� �ش� ������ �̸��� ��й�ȣ�� ������

    //                    // �����Ⱑ �������� �ʴٸ� �ݱ�
    //                    if (!reader.IsClosed) reader.Close();
    //                    return true;
    //                }
    //                else
    //                {
    //                    break;
    //                }
    //            }
    //        }

    //        // ������ ����� ������ ���� ���ߴٸ� ������ �����ϰ� false ��ȯ
    //        if (!reader.IsClosed) reader.Close();
    //        return false;
    //    }
    //    catch(Exception e)
    //    {
    //        Debug.Log(e.Message);
    //        // �������� ������ ���� �� false��ȯ
    //        if (!reader.IsClosed) reader.Close();
    //        return false;
    //    }
    //}
}
