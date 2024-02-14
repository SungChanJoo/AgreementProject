using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//펫을 고르고 메타별에 입장을 관리하는 매니져
public class SelectPetManager : MonoBehaviour
{
    public static SelectPetManager Instance = null;
    
    public List<GameObject> PetPrefebs = new List<GameObject>();
    public int SelectedPetIndex = 0;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            //다음 씬으로 선택한 펫을 넘겨주기 위해 
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

    public void SelectPet(int index)
    {
        //todo 0213 펫리스트 받아서 선택안되게 예외처리해줘
        SelectedPetIndex = index;
    }
}
