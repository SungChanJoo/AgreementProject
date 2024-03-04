using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class ExitMetaWorld : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        NetworkClient.Disconnect();
        Destroy(FindObjectOfType<PetSwitchNetworkManager>().gameObject);
        Destroy(FindObjectOfType<CrewSelectManager>().gameObject);
        if (AudioManager.Instance != null)
            AudioManager.Instance.BGM_Play(0);
        SceneManager.LoadScene(sceneName);
        
    }
}
