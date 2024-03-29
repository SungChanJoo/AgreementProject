using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

//��Ÿ���� ������ ���
public class ExitMetaWorld : MonoBehaviour
{
    public static ExitMetaWorld Instanse = null;
    private void Awake()
    {
        if(Instanse == null)
        {
            Instanse = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ExitWorld()
    {
        NetworkClient.Disconnect(); // Ŭ���̾�Ʈ ��������
        Destroy(FindObjectOfType<PetSwitchNetworkManager>().gameObject);
        Destroy(FindObjectOfType<CrewSelectManager>().gameObject);
        if (AudioManager.Instance != null)
            AudioManager.Instance.BGM_Play(3);
        if (SettingManager.Instance != null)
            SettingManager.Instance.IsMetaWorld = false;
        SceneManager.LoadScene(1); // ���θ޴������� �̵�
        
    }
}
