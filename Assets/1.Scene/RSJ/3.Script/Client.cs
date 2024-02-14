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

// ó�� �����ϴ� ��������(First), �����߾��� ��������(Continue) 
public enum ClientLoginStatus
{
    First,
    Continue
}

public class Client : MonoBehaviour
{
    // IP, Port ������
    [SerializeField] private string server_IP = "15.165.159.141"; // aws EC2 IP : 15.165.159.141
    [SerializeField] private int server_Port = 2421;

    bool socketReady;
    private TcpClient client;
    private NetworkStream stream;

    // login - license
    public static ClientLoginStatus loginStatus;
    public static string clientLicenseNumber;
    public string licensePath = string.Empty;

    // Login
    public InputField login_ID_Input;
    public InputField login_PW_Input;
    public Text loginLog;

    // Create Account
    public InputField create_ID_Input;
    public InputField create_PW_Input;

    private void Awake()
    {
        
    }

    private void Start()
    {
        ConnectToServer();
        ClientLoginSet();
    }

    // Ŭ���̾�Ʈ�� ������ �� ���� ���� �õ�
    public void ConnectToServer()
    {
        // �̹� ����Ǿ��ٸ� �Լ� ����
        if (socketReady) return;

        //// �⺻ ȣ��Ʈ / ��Ʈ��ȣ
        //string ip = IPInput.text == "" ? "127.0.0.1" : IPInput.text; // aws EC2 IP : 15.165.159.141
        //int port = PortInput.text == "" ? 2421 : int.Parse(PortInput.text);

        // ������ ����
        try
        {
            client = new TcpClient();
            client.Connect(server_IP, server_Port);
            Debug.Log("Success Connect to Server!");
            socketReady = true;

            stream = client.GetStream();// ���ῡ �����ϸ� stream�� ��� ������ �� �ֵ���
        }
        catch (Exception e)
        {
            Debug.Log($"Fail Connect to Server : {e.Message}");
        }

    }

    public void ClientLoginSet()
    {
        // ������� �ʾҴٸ� return
        if (!client.Connected) return;

        licensePath = Application.dataPath + "/License";

        Debug.Log($"File.Exists(licensePath) value ? {File.Exists(licensePath)}");
        // ��ο� ������ �������� �ʴ´ٸ� ���̼����ѹ��� ���ٴ°��̰�, ó�� �����Ѵٴ� ��
        if (!Directory.Exists(licensePath))
        {
            Directory.CreateDirectory(licensePath);
            loginStatus = ClientLoginStatus.First;
            // �������� ���̼��� �ѹ��� �޾ƿ;���, �׷��� ���� ������ ��û todo
            string requestName = "LicenseNumber";
            RequestToServer(requestName);
            Debug.Log($"[Client] Is Create licensenumber?");
            Debug.Log($"[Client] This client's licensenumber : {clientLicenseNumber}");
            return; // ó�� �����̶�� ���� �� ���� �����ϰ� return
        }

        // �ش� ��ο� �ִ� ������ �о� Ŭ���̾�Ʈ ���̼��� �ѹ��� �ҷ���
        string jsonStringFromFile = File.ReadAllText(licensePath + "/clientlicense.json");
        JsonData licenseNumber_JsonFile = JsonMapper.ToObject(jsonStringFromFile);
        clientLicenseNumber = licenseNumber_JsonFile["LicenseNumber"].ToString();
        Debug.Log($"[Client] Use created licensenumber?");
        Debug.Log($"[Client] This client's licensenumber : {clientLicenseNumber}");
    }

    //// ��ư ������ �α���
    //public void OnClickLogin()
    //{
    //    // id, password�� �Է����� �ʾ����� return;
    //    if (login_ID_Input.text == null || login_PW_Input.text == null)
    //    {
    //        loginLog.text = "���̵�� ��й�ȣ�� �Է��ϼ���";
    //        return;
    //    }

    //    // id, password�� DB�� �ִ� User_Name�� User_Password ��
    //    // id_Input.text == DBManager.instance.user_Info.user_Name && password_Input.text == DBManager.instance.user_Info.user_Password
    //    if (DBManager.instance.Login(login_ID_Input.text, login_PW_Input.text))
    //    {
    //        loginLog.text = "�α��ο� �����߽��ϴ�.";

    //        // todo.. ���̵��ϴ���, ��ȭ �ҷ������� ��� �Ұ� 

    //        User_Info user = DBManager.instance.user_Info;
    //        Debug.Log(user.user_Name + "|" + user.user_Password);
    //    }
    //    else // �α��� ����
    //    {
    //        loginLog.text = "���̵� �Ǵ� ��й�ȣ�� Ȯ�����ּ���";
    //    }
    //}

    //// ��ư ������ ��������
    //public void OnClickCreateAccount()
    //{
    //    DBManager.instance.CreateAccount(create_ID_Input.text, create_PW_Input.text);
    //}

    // ��ư ������ ������ �޽��� ������
    public void OnCilckSendMessage()
    {
        SendMessageToServer();
    }

    private void SendMessageToServer()
    {
        string sendMessage = "Hello World";
        try
        {
            
            byte[] data = Encoding.UTF8.GetBytes(sendMessage);
            stream.Write(data, 0, data.Length); // �����͸� ������ ���� ���? �׳� ������ ���ݾ�
            Debug.Log($"Sent message to server : {sendMessage}");

            ReceiveMessageFromServer(stream); // �޼��尡 ����ɶ� ���� ��� / ����Ű�� ���� �޼��� �տ� await�� �ٿ��� �����Ű���� �޼��尡 Task �ٿ����ϴµ�?
        }
        catch (Exception e)
        {
            Debug.Log($"Fail sending message to sever : + {e.Message}");
        }
    }

    private async void ReceiveMessageFromServer(NetworkStream stream)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �����͸� �о�ö����� ���
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ
            Debug.Log($"Received message from server : {receivedMessage}");
        }
        catch(Exception e)
        {
            Debug.Log($"Error receiving message from server : {e.Message}");
            Debug.LogError($"Error receiving message from server : {e.Message}");
        }

    }

    // ������ ��û�Ҷ� string���� �����µ�, �������� ���� �� string case�� �����ؼ� ó��
    private void RequestToServer(string requestName)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(requestName);
            stream.Write(data, 0, data.Length); // �����͸� ������ ���� ���? �׳� ������ ���ݾ�
            Debug.Log($"Request to server : {requestName}");

            ReceiveRequestFromServer(stream, requestName); // �޼��尡 ����ɶ� ���� ��� / ����Ű�� ���� �޼��� �տ� await�� �ٿ��� �����Ű���� �޼��尡 Task �ٿ����ϴµ�?
        }
        catch (Exception e)
        {
            Debug.Log($"Fail Request to sever : + {e.Message}");
        }
    }

    // ������ ��û������ ����, ���� string, case�� �����ؼ� ó��
    private async void ReceiveRequestFromServer(NetworkStream stream, string requestName)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �����͸� �о�ö����� ���
            string receivedRequestMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ
            Debug.Log($"Received request message from server : {receivedRequestMessage}");
            HandleRequestMessage(requestName, receivedRequestMessage);
        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving message from server : {e.Message}");
        }
    }

    // �����κ��� ���� �޼��� ó��
    private void HandleRequestMessage(string requestname, string requestmessage)
    {
        switch(requestname)
        {
            case "LicenseNumber":
                clientLicenseNumber = requestmessage;
                SaveLicenseNumberToJsonFile();
                break;
            default:
                Debug.Log("HandleRequestMessage Method Something Happend");
                break;
        }
    }

    // Json���Ͽ� LicenseNumber ��� / �񵿱�� ȣ��� ���� �������� ���������� ȣ���Ű�� ����
    private void SaveLicenseNumberToJsonFile()
    {
        // JsonData ����
        JsonData licenseNumber_Json = new JsonData();
        licenseNumber_Json["LicenseNumber"] = clientLicenseNumber;
        // Json �����͸� ���ڿ��� ��ȯ�Ͽ� ���Ͽ� ����
        string jsonString = JsonMapper.ToJson(licenseNumber_Json);
        File.WriteAllText(licensePath + "/clientlicense.json", jsonString);
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


    private void CloseSocket()
    {
        Debug.Log("Close Socket���� �����°�");
        if (!socketReady) return;

        //writer.Close();
        //reader.Close();
        client.Close();
        socketReady = false;
    }

    #region chatgpt
    //private TcpClient client;
    //private NetworkStream stream;
    //public Text clientLog;

    //// �÷��̾ ���� �����ų �� DB���� �����͸� �����;���
    //// �����;� �ϴ� ������. ����. 
    //// ������ �����ϱ� ���� �ʿ��� IP�� License�� ������? ��ũ��Ʈ?
    //// ��¶�� �����ص� ��ũ��Ʈ�� ���� �� ������. ���� �Ű����ϳ�?
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

    //    // �����κ��� ������ �ޱ� ����
    //    byte[] receiveBuffer = new byte[1024];
    //    stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleDataReceived, receiveBuffer);

    //    // ���� : ������ ��Ŷ ������
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

    //        // ���� : ���� ��Ŷ ������ȭ
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
