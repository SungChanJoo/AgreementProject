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
//IP�� �ٲ�� �ֱ� ������ Json���� ����
public class Item
{
    //�������� Ű������ Json ���� ����
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

//�������� Ŭ���̾�Ʈ���� �Ǻ����ִ� Ŭ����
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
        if(!File.Exists(_path))//���� �˻�
        {
            Directory.CreateDirectory(_path);
        }
        if(!File.Exists(_path + "/License.json"))// ���� �˻�
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

    //License.json�� �о�� �������� Ŭ���̾�Ʈ���� �Ǻ�
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
        //LicenseType�� ���� ���� �Ǵ� Ŭ���̾�Ʈ ����
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
        //���� ���� ��������
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
        //Ŭ���̾�Ʈ��� ��������
        if(NetworkClient.isConnected)
        {
            _manager.StopClient();
        }
        //������� ���� ����
        if(NetworkServer.active)
        {
            _manager.StopServer();
        }
    }
}
