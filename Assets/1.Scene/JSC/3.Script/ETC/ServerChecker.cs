using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using kcp2k;
using LitJson;
using System;
using System.IO;

public enum Type
{
    Empty = 0,
    Server,
    Client
}
//IP가 바뀔수 있기 때문에 Json으로 관리
public class Item
{
    //변수명을 키값으로 Json 파일 저장
    public string License;
    public string ServerIP;
    public string Port;

    public Item(string licenseIndex, string ipValue, string port)
    {
        License = licenseIndex;
        ServerIP = ipValue;
        Port = port;
    }
}

//서버인지 클라이언트인지 판별해주는 클래스
public class ServerChecker : MonoBehaviour
{
    public Type type = Type.Client;
    //public string InitServerIP = "54.180.92.129";
    public string InitServerIP = "127.0.0.1";
    private NetworkManager _manager;
    private KcpTransport _kcp;

    private string _path = string.Empty;
    public string ServerIp { get; private set; }
    public string ServerPort { get; private set; }

    private void Awake()
    {
        
        if(_path.Equals(string.Empty))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                _path = Application.persistentDataPath + "/License";
            }
            else
                _path = Application.dataPath + "/License";
        }
        if(!File.Exists(_path))//폴더 검사
        {
            Directory.CreateDirectory(_path);
        }
        if(!File.Exists(_path + "/License.json"))// 파일 검사
        {
            DefaultData(_path);
        }
        _manager = GetComponent<NetworkManager>();
        _kcp = (KcpTransport)_manager.transport;
    }
    private void DefaultData(string path)
    {
        List<Item> item = new List<Item>();
        item.Add(new Item($"{type}", InitServerIP, "7777"));

        JsonData data = JsonMapper.ToJson(item);
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    //License.json을 읽어와 서버인지 클라이언트인지 판별
    private Type LicenseType()
    {
        Type type = Type.Empty;

        try
        {
            string jsonString = File.ReadAllText(_path + "/License.json");
            JsonData itemData = JsonMapper.ToObject(jsonString);
            string strType = itemData[0]["License"].ToString();
            string strServerIp = itemData[0]["ServerIP"].ToString();
            string strPort = itemData[0]["Port"].ToString();

            ServerIp = strServerIp;
            ServerPort = strPort;
            type = (Type)Enum.Parse(typeof(Type), strType);
            Debug.Log($"{type}{ServerIp}");
            _manager.networkAddress = ServerIp;
            _kcp.port = ushort.Parse(ServerPort);

            return type;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return Type.Empty;
        }
    }

    private void Start()
    {
        type = LicenseType();
        //LicenseType에 따라 서버 또는 클라이언트 실행
        if (type.Equals(Type.Server))
        {
            StartServer();
        }
        else
        {
            StartClient();
        }
    }

    public void StartServer()
    {
        //웹은 서버 지원안함
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("WebGL cannot be Server");
        }
        else
        {
            _manager.StartServer();
            Debug.Log($"{_manager.networkAddress} StartServer...");
            NetworkServer.OnConnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"New Client Connect : {NetworkConnectionToClient.address}");
            };
            NetworkServer.OnDisconnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"Client DisConnect : {NetworkConnectionToClient.address}");
            };
        }
    }
    public void StartClient()
    {
        _manager.StartClient();
        Debug.Log($"{_manager.networkAddress} : StartClient...");
    }

    private void OnApplicationQuit()
    {
        //클라이언트라면 연결해제
        if(NetworkClient.isConnected)
        {
            _manager.StopClient();
        }
        //서버라면 서버 종료
        if(NetworkServer.active)
        {
            _manager.StopServer();
        }
    }
}
