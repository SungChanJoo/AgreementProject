using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

//펫을 고르고 메타별에 입장을 관리하는 매니져
public class CrewSelectManager : MonoBehaviour
{
    public static CrewSelectManager Instance = null;
    public List<GameObject> _crewList;
    public GameObject CrewSelectCanvas;
    private List<TMP_Text> _crewStatusText;
    private List<GameObject> _crewStatusBtn;
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
    private void Start()
    {
        SetSeleteCrewUI();
    }
    public void LoadMetaWorld(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnViewSelectCrewUI()
    {
        CrewSelectCanvas.SetActive(!CrewSelectCanvas.activeSelf);
        SelectedCrewIndex = CollectionsManager.Instance.Collections.SelectedCrew;
        for (int i = 0; i < _crewList.Count; i++)
        {
            //보유중인 대원이 아닐경우 게임오브젝트 활성화 -> 비활성화, 비활성화 -> 활성화
            if (CollectionsManager.Instance.Collections.OwnedCrew[i])
            {
                _crewList[i].SetActive(true);
                //선택되어 있던 대원은 "출동!" 텍스트
                if(SelectedCrewIndex == i)
                {
                    _crewStatusText[i].text = CrewButton._selectedCrew;
                    //_crewStatusBtn[i].GetComponent<Image>().color = CollectionsManager.Instance.SelectedBtnColor;
                    _crewStatusBtn[i].GetComponent<Image>().sprite = CollectionsManager.Instance.SelectedImg;
                }
                //보유한 대원은 "출동 대기" 텍스트
                else
                {
                    _crewStatusText[i].text = CrewButton._ownedCrew;
                    //_crewStatusBtn[i].GetComponent<Image>().color = CollectionsManager.Instance.DefaultBtnColor;
                    _crewStatusBtn[i].GetComponent<Image>().sprite = CollectionsManager.Instance.DefaultImg;
                }
            }
            else
                _crewList[i].SetActive(false);
        }
    }
    public void SetSeleteCrewUI()
    {
        //텍스트, 버튼 초기화
        _crewStatusBtn = new List<GameObject>();
        _crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < _crewList.Count; i++)
        {
            _crewStatusBtn.Add(_crewList[i].transform.Find("Button").gameObject);
            _crewStatusText.Add(_crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
            
            var button = _crewStatusBtn[i].GetComponent<Button>();
            int crewNumber = i; //클로저 방지
            button.onClick.AddListener(() => OnSelectPet(crewNumber));
        }
    }

    //탐험대원 선택
    public void OnSelectPet(int selectIndex)
    {
        Debug.Log("SelectPet");
        //이미 선택된 대원입니다.
/*        if (CollectionsManager.Instance.Collections.SelectedCrew == selectIndex)
        {
            StartCoroutine(ViewAlreadySeletedCrew_co());
        }*/
        //else
        {
            //대원이 "출동 대기" -> "출동!" 으로, 이전 "출동!" -> "출동 대기" 상태로 변경
            if (_crewStatusText[selectIndex].text.Equals(CrewButton._ownedCrew))
            {

                //"출동!" 상태인 탐험대원 "출동!" -> "출동 대기" 
                _crewStatusBtn[SelectedCrewIndex].GetComponent<Image>().sprite = CollectionsManager.Instance.DefaultImg;
                _crewStatusText[SelectedCrewIndex].text = CrewButton._ownedCrew;
                //"출동 대기" -> "출동!"
                _crewStatusText[selectIndex].text = CrewButton._selectedCrew;
                _crewStatusBtn[selectIndex].GetComponent<Image>().sprite = CollectionsManager.Instance.SelectedImg;
                //_crewStatusBtn[selectIndex].GetComponent<Image>().color = CollectionsManager.Instance.SelectedBtnColor;

                //상세보기 버튼 변경
                SelectedCrewIndex = selectIndex;
                //선택된 대원 도감에 반영
                CollectionsManager.Instance.OnSelectPet(selectIndex);
            }
        }
    }
}
