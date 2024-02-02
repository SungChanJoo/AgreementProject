using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server : MonoBehaviour
{
    // Ŭ���̾�Ʈ�κ��� ���� ��û ��ٸ�(������ ����)
    private TcpListener server; 
    // ����, Ŭ���̾�Ʈ ���ʿ��� ��� 
    // ���������� Ŭ���̾�Ʈ�� ��û�� �����ϸ� TcpClient�� ��ȯ(��ſ� ��밡����)
    private TcpClient client;
    private NetworkStream stream; // ������ �ְ�ޱ�

    private void Start()
    {
        StartServer();
    }

    private void StartServer()
    {
        server = new TcpListener(IPAddress.Any, 8888);
        server.Start();

        Debug.Log("Server is listening");

        server.BeginAcceptTcpClient(HandleClientConnect, null);
    }

    // �񵿱��ϴ°ǵ�, ������ ��� �������� ���� �ʿ���
    private void HandleClientConnect(IAsyncResult result)
    {
        client = server.EndAcceptTcpClient(result);
        stream = client.GetStream();

        Debug.Log("Client connected!"); // if�� ����ó���ؾ�����

        // ������ �Ǿ��ٸ� 
        // Ŭ���̾�Ʈ���� ��Ŷ�� �����ϴ� ����
        Packet packetToSend = new Packet("Player1", 100);
        SendPacket(packetToSend);

        // Ŭ���̾�Ʈ�κ��� ������ �ޱ� ����
        // ������ ũ��� �˾Ƽ� ���� �� ������ �Ϲ������� 1024 ũ��� �ۼ�����
        byte[] receiveBuffer = new byte[1024];
        stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleDataReceived, null);
    }

    private void HandleDataReceived(IAsyncResult result)
    {
        int bytesRead = stream.EndRead(result); // �о���� �������� ũ��?

        if(bytesRead > 0)
        {
            byte[] receivedData = new byte[bytesRead];
            Array.Copy((result.AsyncState as byte[]), receivedData, bytesRead); // ī���ϴ� ����?

            // Deserialized : ��������(���� ������ ������)���� ��ü�� ��ȯ�ϴ� ������ ������ȭ
            // ����, ���� ��Ŷ�� ������ȭ
            Packet receivedPacket = PacketFromBytes(receivedData);
            Debug.Log("Received data from client : " + receivedPacket.playerName + " - " + receivedPacket.playerScore);

            // �ʿ��Ѵ�� ���� �����͸� �ٷ��(�����ض�)

            // �����͸� �� �ޱ� ���� ������ ����ϱ�
            byte[] newBuffer = new byte[1024];
            stream.BeginRead(newBuffer, 0, newBuffer.Length, HandleDataReceived, newBuffer);
        }
    }

    private void SendPacket(Packet packet)
    {
        byte[] data = PacketToBytes(packet);
        stream.Write(data, 0, data.Length);
    }

    private byte[] PacketToBytes(Packet packet)
    {
        string jsonData = JsonUtility.ToJson(packet);
        return Encoding.ASCII.GetBytes(jsonData);
    }

    private Packet PacketFromBytes(byte[] data)
    {
        string jsonData = Encoding.ASCII.GetString(data);
        return JsonUtility.FromJson<Packet>(jsonData);
    }

    // Ŭ���̾�Ʈ�� ���� �����԰� ��� ���� ������ ��� ���ư�����
    // need Fix
    private void OnDestroy()
    {
        if(client != null)
        {
            client.Close();
        }

        if(server != null)
        {
            server.Stop();
        }
    }
}
