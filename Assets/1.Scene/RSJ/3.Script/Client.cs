using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Client : MonoBehaviour
{
    // IP, Port ������
    [SerializeField] private string server_IP = "15.165.159.141";
    [SerializeField] private int server_Port = 2421;

    bool socketReady;
    private TcpClient client;

    // Login
    public InputField login_ID_Input;
    public InputField login_PW_Input;
    public Text loginLog;

    // Create Account
    public InputField create_ID_Input;
    public InputField create_PW_Input;

    // Ŭ���̾�Ʈ�� ������ �� ���� ���� �õ�
    private void Start()
    {
        ConnectedToServer();
    }

    public void ConnectedToServer()
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
        }
        catch (Exception e)
        {
            Debug.Log($"Fail Connect to Server : {e.Message}");
        }

    }

    // ��ư ������ �α���
    public void OnClickLogin()
    {
        // id, password�� �Է����� �ʾ����� return;
        if (login_ID_Input.text == null || login_PW_Input.text == null)
        {
            loginLog.text = "���̵�� ��й�ȣ�� �Է��ϼ���";
            return;
        }

        // id, password�� DB�� �ִ� User_Name�� User_Password ��
        // id_Input.text == DBManager.instance.user_Info.user_Name && password_Input.text == DBManager.instance.user_Info.user_Password
        if (DBManager.instance.Login(login_ID_Input.text, login_PW_Input.text))
        {
            loginLog.text = "�α��ο� �����߽��ϴ�.";

            // todo.. ���̵��ϴ���, ��ȭ �ҷ������� ��� �Ұ� 

            User_Info user = DBManager.instance.user_Info;
            Debug.Log(user.user_Name + "|" + user.user_Password);
        }
        else // �α��� ����
        {
            loginLog.text = "���̵� �Ǵ� ��й�ȣ�� Ȯ�����ּ���";
        }
    }

    // ��ư ������ ��������
    public void OnClickCreateAccount()
    {
        DBManager.instance.CreateAccount(create_ID_Input.text, create_PW_Input.text);
    }

    // ��ư ������ ������ �޽��� ������
    public void OnCilckSendMessage()
    {
        string hello = "Hello World";
        try
        {

            NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(hello);
            stream.Write(data, 0, data.Length);
            Debug.Log($"Sent message to server : {hello}");
        }
        catch (Exception e)
        {
            Debug.Log($"Fail sending message to sever : + {e.Message}");
        }
    }

    //private void OnDestroy()
    //{
    //    if(client != null)
    //    {
    //        client.Close();
    //    }
    //}

    private void OnApplicationQuit()
    {
        CloseSocket();
    }


    private void CloseSocket()
    {
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
