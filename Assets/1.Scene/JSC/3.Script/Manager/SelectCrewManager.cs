using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//펫을 고르고 메타별에 입장을 관리하는 매니져
public class SelectCrewManager : MonoBehaviour
{
    public static SelectCrewManager Instance = null;

    public int SelectedCrewIndex = 0;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            //다음 씬으로 선택한 대원을 넘겨주기 위해 
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void LoadMetaWorld(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
