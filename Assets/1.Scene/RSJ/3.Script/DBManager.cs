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
        SetStrCon();
        ConnectDB();
    }

    // DB ���ǿ� ������ �� �ʿ��� ���� �ʱ�ȭ
    private void SetStrCon()
    {
        string ip = "15.165.159.141"; // �켱 ����(127.0.0.1)��, aws EC2 IP : 15.165.159.141
        string db = "test";
        string uid = "root"; //string.IsNullOrEmpty(user_Info.user_Name)? "" : user_Info.user_Name;
        string pw = "12345678"; //string.IsNullOrEmpty(user_Info.user_Password)? "" : user_Info.user_Password;
        string port = "3306";
        //str_Connection = $"Server={ip};Database={db};Uid={uid};Pwd={pw};Port={port};Charset=utf8;";
        str_Connection = $"Server={ip};" + $"Database={db};" + $"Uid={uid};" + $"Pwd={pw};" + $"Port={port};" + "CharSet=utf8;"; // ; �����ݷ� �����ض�
        Debug.Log(str_Connection);
    }

    // DB�� ����
    private void ConnectDB()
    {
        try
        {
            connection = new MySqlConnection(str_Connection);
            connection.Open(); // DB ����
            Debug.Log("Success connect to db!");
        }
        catch(Exception e)
        {
            Debug.Log($"Fail connect to db + {e.Message}");
        }

    }

    // Login, Client���� �ҷ��ð���, �α����Ҷ� ���̵�, ��� �Է�
    public bool Login(string id, string pw)
    {
        // �����͸� DB���� ������
        // ��ȸ�Ǵ� �����Ͱ� ������ False
        // ��ȸ�� �Ǵ� �����Ͱ� ������ True�ε�, �� user_Info���ٰ� ��������
        // 1. Connection�� Open���� Ȯ�� - �޼���
        // 2. Reader ���°� �а� �ִ��� Ȯ�� - �� �������� �ϳ�
        // 3. �����͸� �� �о����� Close();

        try
        {
            // 1��
            if (!CheckConnection(connection))
            {
                Debug.Log("DB�� ������� �ʾҽ��ϴ�.");
                return false;
            }

            string login_Command = string.Format(@"SELECT User_Name, User_Password FROM User_Info WHERE User_Name = '{0}' AND User_Password = '{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(login_Command, connection);
            reader = cmd.ExecuteReader(); // SELECT

            // Reader�� ������ �ϳ��̻� �����ϸ�
            if(reader.HasRows)
            {
                // ���� �����͸� �ϳ��� ����
                while (reader.Read())
                {
                    string name = (reader.IsDBNull(0)) ? string.Empty : (string)reader["User_Name"].ToString();
                    string password = (reader.IsDBNull(1)) ? string.Empty : (string)reader["User_Password"].ToString();

                    // �� �� ������� �ʴٸ� � ���� �Ǿ��� �����Ͱ� �ҷ������ٴ°�
                    if (!name.Equals(string.Empty) || !password.Equals(string.Empty))
                    {
                        user_Info = new User_Info(name, password); // DB���� �ش� ������ �̸��� ��й�ȣ�� ������

                        // �����Ⱑ �������� �ʴٸ� �ݱ�
                        if (!reader.IsClosed) reader.Close();
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // ������ ����� ������ ���� ���ߴٸ� ������ �����ϰ� false ��ȯ
            if (!reader.IsClosed) reader.Close();
            return false;
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            // �������� ������ ���� �� false��ȯ
            if (!reader.IsClosed) reader.Close();
            return false;
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

    // ������� // todo �񵿱�ȭ �����غ�����
    public void CreateAccount(string user_name, string user_password)
    {
        if(!CheckConnection(connection))
        {
            Debug.Log("DB�� ���� �ȵ�");
            return;
        }

        // ���� ������ �ߺ��Ǹ� ������ ���� ���� ��ȸ�� �ʿ���, ���̵�
        string checkUserInfo_Command = $"SELECT User_Name FROM user_info WHERE User_Name='{user_name}';";
        MySqlCommand check_SqlCmd = new MySqlCommand(checkUserInfo_Command, connection);
        reader = check_SqlCmd.ExecuteReader();

        // �����ϴ� ������
        string createAccount_Command = $"INSERT INTO user_info (User_Name, User_Password) VALUES ('{user_name}','{user_password}');";
        MySqlCommand insert_SqlCmd = new MySqlCommand(createAccount_Command, connection);
        //insert_SqlCmd.ExecuteNonQuery(); // command.ExecuteNonQuery()�� �����ͺ��̽����� ���� �۾��� �����ϴ� SQL ��ɹ��� �����ϰ�, ������ ���� ���� ���� ��ȯ�ϴ� �޼����Դϴ�.

        // table üũ
        if (reader.HasRows)
        {
            while(reader.Read())
            {
                // ������ �����Ϸ��� id�� �ߺ��ȴٸ�
                if(user_name == reader[$"{user_name}"].ToString()) // && user_password == reader[$"{user_password}"].ToString()
                {
                    Debug.Log("�ش��ϴ� ���̵� �����մϴ�");
                    reader.Close(); // maybe need fix
                    return;
                }
            }
        }

        // �� while������ return�� ���� �ʾҴٸ� �ش��ϴ� ���̵� ���ٴ� ���̹Ƿ�
        reader.Close(); // DataReader�� �����ִ� ���� �߰����� ��� ���� �Ұ�
        insert_SqlCmd.ExecuteNonQuery();
    }

}
