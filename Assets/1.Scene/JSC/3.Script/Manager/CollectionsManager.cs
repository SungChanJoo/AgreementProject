using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//펫을 고르고 메타별에 입장을 관리하는 매니져
public class CollectionsManager : MonoBehaviour
{
    public ExpenditionCrew collections;
    [SerializeField] GameObject Content;
    public GameObject UI;


    private void Awake()
    {
        SetCollections();
    }
    public void LoadMetaWorld(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SelectPet(int index)
    {
        //todo 0213 펫리스트 받아서 선택안되게 예외처리해줘
        SelectCrewManager.Instance.SelectedCrewIndex = index;
        //todo 0219 선택된 대원 DB에 변경시켜줘
        collections.SelectedCrew = index;
    }

    public void ViewDetails()
    {

    }

    //DB에서 탐원대원 콜렉션 불러오기
    public void SetCollections()
    {

        //todo 0219 DB에 데이터 받아서 선택된대원과 보유한대원 리스트 받아와줘
        int selectedCrew = 0;
        List<bool> ownedCrew = new List<bool>();
        ownedCrew.Add(true);
        ownedCrew.Add(false);
        collections = new ExpenditionCrew(selectedCrew, ownedCrew);
    }
}
