using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

//메타월드 나갈때 사용
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
        NetworkClient.Disconnect(); // 클라이언트 연결해제
        Destroy(FindObjectOfType<PetSwitchNetworkManager>().gameObject);
        Destroy(FindObjectOfType<CrewSelectManager>().gameObject);
        if (AudioManager.Instance != null)
            AudioManager.Instance.BGM_Play(3);
        if (SettingManager.Instance != null)
            SettingManager.Instance.IsMetaWorld = false;
        SceneManager.LoadScene(1); // 메인메뉴씬으로 이동
        
    }
}
