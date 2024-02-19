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
        Destroy(FindObjectOfType<SelectCrewManager>().gameObject);
        SceneManager.LoadScene(sceneName);
    }
}
