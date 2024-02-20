using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using kcp2k;

public class Crew
{
    public GameObject DetailCrew;
    public Text CrewName;
    public Text CrewDescript;
}

//펫을 고르고 메타별에 입장을 관리하는 매니져
public class CollectionsManager : MonoBehaviour
{
    const string _selectedCrew = "출동!";
    const string _ownedCrew = "출동 대기";
    const byte _selectedR = 83;
    const byte _selectedG = 88;
    const byte _selectedB = 101;
    const byte _defaultR = 126;
    const byte _defaultG = 163;
    const byte _defaultB = 255;
    const byte _deniedPurchaseR = 150;
    const byte _deniedPurchaseG = 150;
    const byte _deniedPurchaseB = 150;

    public static CollectionsManager Instance = null;

    [Header("Collections")]
    public ExpenditionCrew collections;
    public List<GameObject> CrewList;
    private List<TMP_Text> _crewStatusText;
    private List<GameObject> _crewStatusBtn;

    [Header("돈")]
    private int _money = 200;
    [SerializeField] private TMP_Text moneyText;

    [Header("ViewDetails")]
    public GameObject DetailWindow;
    public List<Crew> CrewInfo;


    public Action OnCheckPurchasePossibility; //돈 갱신될때마다 호출

    private void Awake()
    {
        if (Instance == null)
            Instance = new CollectionsManager();
        else
            Destroy(gameObject);
        //todo 0220 DB에서 플레이어 돈 받아와서 할당해줘
        moneyText.text = $"{_money}";
        SetCollections();
    }
    private void OnEnable()
    {
        OnCheckPurchasePossibility += CheckPurchasePossibility;
    }
    private void OnDisable()
    {
        OnCheckPurchasePossibility -= CheckPurchasePossibility;
    }
    public void LoadMetaWorld(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    //탐원대원 선택
    public void SelectPet(int selectIndex)
    {
        //대원이 "출동 대기" 상태라면 "출동!" 으로, 이전 "출동!"은 "출동 대기" 상태로 변경
        if (_crewStatusText[selectIndex].text.Equals(_ownedCrew))
        {
            //"출동!" 찾기
            for (int i = 0; i < _crewStatusText.Count; i++)
            {
                //보유중이고, "출동!" 상태인 탐원대원
                if(collections.OwnedCrew[i] && _crewStatusText[i].text.Equals(_selectedCrew))
                {

                    _crewStatusText[i].text = _ownedCrew;
                    SetBtnColor(_crewStatusBtn[i].GetComponent<Image>(), new Color32(_defaultR, _defaultG, _defaultB, 255));
                    break;
                }
            }
            _crewStatusText[selectIndex].text = _selectedCrew;
            SelectCrewManager.Instance.SelectedCrewIndex = selectIndex;
            SetBtnColor(_crewStatusBtn[selectIndex].GetComponent<Image>(), new Color32(_selectedR, _selectedG, _selectedB, 255));
            //선택된 대원 DB에 변경
            collections.SelectedCrew = selectIndex;
        }
    }

    public void ViewDetails()
    {

    }
    //전체 대원, 보유 대원 전환
    public void ToggleOwnedCrew()
    {
        for(int i =0; i < CrewList.Count; i++)
        {
            if (!_crewStatusText[i].text.Equals(_selectedCrew) && !_crewStatusText[i].text.Equals(_ownedCrew))
            {
                CrewList[i].SetActive(!CrewList[i].activeSelf);
            }

        }
    }
    //DB에서 탐험대원 콜렉션 불러오기
    public void SetCollections()
    {
        //todo 0219 DB에 데이터 받아서 선택된 대원과 보유한 대원 리스트 받아와줘
        int selectedCrew = 0;
        List<bool> ownedCrew = new List<bool>();
        ownedCrew.Add(true);
        ownedCrew.Add(false);
        ownedCrew.Add(false);
        ownedCrew.Add(false);
        ownedCrew.Add(false);
        ownedCrew.Add(false);
        ownedCrew.Add(false);

        collections = new ExpenditionCrew(selectedCrew, ownedCrew);

        //텍스트, 버튼 초기화
        _crewStatusBtn = new List<GameObject>();
        _crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < CrewList.Count; i++)
        {
            _crewStatusBtn.Add( CrewList[i].transform.Find("Button").gameObject);
            _crewStatusText.Add(_crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
        }

        for (int i=0; i< CrewList.Count; i++)
        {
            var buttonImg = _crewStatusBtn[i].GetComponent<Image>();
            var button = _crewStatusBtn[i].GetComponent<Button>();
            int crewIndex = i; //클로저 방지

            //선택되어 있던 대원은 "출동" 텍스트
            if (collections.SelectedCrew == i)
            {
                button.onClick.AddListener(() => SelectPet(crewIndex));
                _crewStatusText[i].text = _selectedCrew;
                SetBtnColor(buttonImg, new Color32(_selectedR, _selectedG, _selectedB, 255));
            }
            //보유한 대원은 "출동 대기" 텍스트
            else if (collections.OwnedCrew[i])
            {
                button.onClick.AddListener(() => SelectPet(crewIndex));
                _crewStatusText[i].text = _ownedCrew;
                SetBtnColor(buttonImg, new Color32(_defaultR, _defaultG, _defaultB, 255));
            }
            //영입가능한 대원은 Cost 텍스트
            else
            {
                button.onClick.AddListener(() => PurchaseCrew(crewIndex));
                //보유한 금액보다 탐험대원의 비용이 더 비싸면 deniedPurchase
                if (_money< Convert.ToInt32(_crewStatusText[i].text))
                {
                    SetBtnColor(buttonImg, new Color32(_deniedPurchaseR, _deniedPurchaseG, _deniedPurchaseB, 255));
                }
                //아니면 영입가능
                else
                {
                    SetBtnColor(buttonImg, new Color32(_defaultR, _defaultG, _defaultB, 255));
                }
            }
        }
    }


    //대원 구매
    public void PurchaseCrew(int index)
    {
        var crewCost = Convert.ToInt32(_crewStatusText[index].text);
        if (_money >= crewCost)
        {
            //돈 차감
            _money -= crewCost;
            moneyText.text = $"{_money}";

            //DB에 반영
            collections.OwnedCrew[index] = true;
            //텍스트 변경
            _crewStatusText[index].text = _ownedCrew;
            OnCheckPurchasePossibility?.Invoke();
            //버튼 이벤트 추가
            var button = _crewStatusBtn[index].GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectPet(index));
        }
        else
        {
            Debug.Log("구매할 수 없음");
        }
    }
    //구매가능한지 확인
    public void CheckPurchasePossibility()
    {
        for (int i = 0; i < _crewStatusText.Count; i++)
        {
            if (collections.OwnedCrew[i]) continue;
            //보유중이 아니고, 가격이 현재 가진 돈보다 비싸면 버튼 색 변경
            if (_money < Convert.ToInt32(_crewStatusText[i].text))
            {
                SetBtnColor(_crewStatusBtn[i].GetComponent<Image>(), new Color32(_deniedPurchaseR, _deniedPurchaseG, _deniedPurchaseB, 255));
            }
            else
            {
                SetBtnColor(_crewStatusBtn[i].GetComponent<Image>(), new Color32(_defaultR, _defaultG, _defaultB, 255));
            }
        }
    }
    //버튼 색 변경
    public void SetBtnColor(Image button, Color color)
    {
        button.color = color;
    }
}
