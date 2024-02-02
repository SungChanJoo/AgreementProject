using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server : MonoBehaviour
{
    // 클라이언트로부터 연결 요청 기다림(서버만 가짐)
    private TcpListener server; 
    // 서버, 클라이언트 양쪽에서 사용 
    // 서버에서는 클라이언트의 요청을 수락하면 TcpClient를 반환(통신에 사용가능한)
    private TcpClient client;
    private NetworkStream stream; // 데이터 주고받기

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

    // 비동기하는건데, 문법상 어떻게 가능한지 공부 필요함
    private void HandleClientConnect(IAsyncResult result)
    {
        client = server.EndAcceptTcpClient(result);
        stream = client.GetStream();

        Debug.Log("Client connected!"); // if문 예외처리해야하함

        // 연결이 되었다면 
        // 클라이언트한테 패킷을 전송하는 예시
        Packet packetToSend = new Packet("Player1", 100);
        SendPacket(packetToSend);

        // 클라이언트로부터 데이터 받기 시작
        // 데이터 크기는 알아서 정할 수 있지만 일반적으로 1024 크기로 송수신함
        byte[] receiveBuffer = new byte[1024];
        stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleDataReceived, null);
    }

    private void HandleDataReceived(IAsyncResult result)
    {
        int bytesRead = stream.EndRead(result); // 읽어들인 데이터의 크기?

        if(bytesRead > 0)
        {
            byte[] receivedData = new byte[bytesRead];
            Array.Copy((result.AsyncState as byte[]), receivedData, bytesRead); // 카피하는 이유?

            // Deserialized : 포맷형태(포맷 상태의 데이터)에서 객체로 변환하는 과정을 역직렬화
            // 예시, 받은 패킷을 역직렬화
            Packet receivedPacket = PacketFromBytes(receivedData);
            Debug.Log("Received data from client : " + receivedPacket.playerName + " - " + receivedPacket.playerScore);

            // 필요한대로 받은 데이터를 다뤄라(가공해라)

            // 데이터를 더 받기 위해 리스닝 계속하기
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

    // 클라이언트가 앱을 종료함과 상관 없이 서버는 계속 돌아가야함
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
