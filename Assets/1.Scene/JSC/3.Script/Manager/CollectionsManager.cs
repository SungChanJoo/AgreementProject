using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using LitJson;
public class Crew
{
    public string CrewName { get; private set; }
    public string CrewDescript { get; private set; }
    public Crew(string crewName, string crewDescript)
    {
        CrewName = crewName;
        CrewDescript = crewDescript;
    }
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
    private ExpenditionCrew _collections;
    [SerializeField]  private List<GameObject> _crewList;
    private List<TMP_Text> _crewStatusText;
    private List<GameObject> _crewStatusBtn;

    [Header("Purchase")]
    [SerializeField] private GameObject _purchaseWindow;
    [SerializeField] private TMP_Text _purchaseText;
    private int _money = 200;
    [SerializeField] private TMP_Text _moneyText;

    [Header("ViewDetails")]
    [SerializeField] private GameObject _detailWindow;
    [SerializeField] private TextAsset _crewDataList;
    private List<Crew> _crewInfo;
    [SerializeField] private List<GameObject> _crewModel;
    [SerializeField] private TMP_Text _crewName;
    [SerializeField] private TMP_Text _crewDescript;
    [SerializeField] private GameObject _detailSelectBtn;

    private int _seletedDetailModel;
    private GameObject _ModelSpace;
    private bool _isDrag = false;
    [SerializeField] private float _rotateSpeedModifier = 1f;
    public Action OnCheckPurchasePossibility; //돈 갱신될때마다 호출

    private void Awake()
    {
        if (Instance == null)
            Instance = new CollectionsManager();
        else
            Destroy(gameObject);
        //todo 0220 DB에서 플레이어 돈 받아와서 할당해줘
        _moneyText.text = $"{_money}";
        SetCollections();
        DetailCrewData();
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

    public void OnViewPurchase(int crewNumber)
    {
        //영입창 켜질때
        if (!_purchaseWindow.activeSelf)
        {
            _purchaseText.text = $"[{_crewInfo[crewNumber].CrewName}]을 영입하시겠습니까?";
        }
        _purchaseWindow.SetActive(!_purchaseWindow.activeSelf);
    }


    #region 대원상세보기
    //대원상세보기버튼 이벤트
    public void OnViewDetails(int crewNumber)
    {
        Debug.Log($"crewNumber : {crewNumber}");
        //상세보기가 켜질 때
        if (!_detailWindow.activeSelf)
        {
            var btnText = _detailSelectBtn.transform.GetChild(0).GetComponent<TMP_Text>();
            btnText.text = _crewStatusText[crewNumber].text;
            SetupCrewSelectionBtn(crewNumber, _detailSelectBtn, btnText);
            if (_crewInfo[crewNumber] != null)
            {
                _crewModel[crewNumber].SetActive(true);
                _crewName.text = $"대원이름 : {_crewInfo[crewNumber].CrewName}";
                _crewDescript.text = $"대원설명 : {_crewInfo[crewNumber].CrewDescript}";
                _seletedDetailModel = crewNumber;

            }
            else
            {
                _crewName.text = $"대원이름 : 없음 {crewNumber}";
                _crewDescript.text = $"대원설명 : 없음 {crewNumber}";
            }
            //모델 회전 코루틴 시작
            if (currentRotateCrewModel_co == null)
            {
                currentRotateCrewModel_co = RotateCrewModel_co();
                StartCoroutine(currentRotateCrewModel_co);
            }
        }
        //상세보기가 꺼질 때
        else
        {
            if (_crewInfo[_seletedDetailModel] != null)
            {
                _crewModel[_seletedDetailModel].SetActive(false);
                _crewName.text = string.Empty;
                _crewDescript.text = string.Empty;
                _detailSelectBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            }
            //모델 회전 코루틴 종료
            if (currentRotateCrewModel_co != null)
            {
                currentRotateCrewModel_co = null;
            }
        }

        _detailWindow.SetActive(!_detailWindow.activeSelf);

    }
    IEnumerator currentRotateCrewModel_co = null;
    //Model이 있는 공간에서 드래그 할 시 모델을 좌우로 회전
    IEnumerator RotateCrewModel_co()
    {
        while (currentRotateCrewModel_co != null)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    //Model이 있는 공간판단
                    _ModelSpace = EventSystem.current.currentSelectedGameObject;
                    if(_ModelSpace != null && _ModelSpace.name.Equals("Crew_Model"))
                        _isDrag = true;
                }
                if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    _isDrag = false;
                }
                //드래그가 가능하다면 모델 드래그에 따라 회전
                if (_isDrag)
                {
                    float _modelRotationY = Input.GetTouch(0).deltaPosition.x * 1f;
                    _crewModel[_seletedDetailModel].transform.Rotate(Vector3.down, _modelRotationY);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //Model이 있는 공간판단
                    _ModelSpace = EventSystem.current.currentSelectedGameObject;
                    if(_ModelSpace != null && _ModelSpace.name.Equals( "Crew_Model"))
                        _isDrag = true;
                }
                if(Input.GetMouseButtonUp(0))
                {
                    _isDrag = false;
                }
                //드래그가 가능하다면 모델 드래그에 따라 회전
                if (_isDrag)
                {
                    float _modelRotationY = Input.GetAxis("Mouse X") * 2f;
                    _crewModel[_seletedDetailModel].transform.Rotate(Vector3.down, _modelRotationY);
                }
            }
            yield return null;
        }
    } 
    #endregion
    //전체 대원, 보유 대원 전환
    public void ToggleOwnedCrew()
    {
        for(int i =0; i < _crewList.Count; i++)
        {
            //선택된대원이 아니고, 보유중인 대원이 아닐경우 게임오브젝트 활성화 -> 비활성화, 비활성화 -> 활성화
            if (!_crewStatusText[i].text.Equals(_selectedCrew) && !_crewStatusText[i].text.Equals(_ownedCrew))
            {
                _crewList[i].SetActive(!_crewList[i].activeSelf);
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
        _collections = new ExpenditionCrew(selectedCrew, ownedCrew);

        //텍스트, 버튼 초기화
        _crewStatusBtn = new List<GameObject>();
        _crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < _crewList.Count; i++)
        {
            _crewStatusBtn.Add( _crewList[i].transform.Find("Button").gameObject);
            _crewStatusText.Add(_crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
        }

        for (int i=0; i< _crewList.Count; i++)
        {
            SetupCrewSelectionBtn(i);
        }
    }
    public void SetupCrewSelectionBtn(int i, GameObject btn = null, TMP_Text text = null)
    {
        Image buttonImg;
        Button button;
        TMP_Text buttonText;
        if (btn == null && text == null)
        {
            buttonImg = _crewStatusBtn[i].GetComponent<Image>();
            button = _crewStatusBtn[i].GetComponent<Button>();
            buttonText = _crewStatusText[i];
        }
        //상세보기 버튼 Setup
        else
        {
            buttonImg = btn.GetComponent<Image>();
            button = btn.GetComponent<Button>();
            buttonText = text;
        }
        int crewIndex = i; //클로저 방지
        //선택되어 있던 대원은 "출동!" 텍스트
        if (_collections.SelectedCrew == i)
        {
            if (btn == null && text == null) 
                button.onClick.AddListener(() => OnSelectPet(crewIndex));
            else
                button.onClick.AddListener(() => OnSelectPet(crewIndex, btn, text));
            buttonText.text = _selectedCrew;
            SetBtnColor(buttonImg, new Color32(_selectedR, _selectedG, _selectedB, 255));
        }
        //보유한 대원은 "출동 대기" 텍스트
        else if (_collections.OwnedCrew[i])
        {
            if (btn == null && text == null) 
                button.onClick.AddListener(() => OnSelectPet(crewIndex));
            else 
                button.onClick.AddListener(() => OnSelectPet(crewIndex, btn, text));
            buttonText.text = _ownedCrew;
            SetBtnColor(buttonImg, new Color32(_defaultR, _defaultG, _defaultB, 255));
        }
        //영입가능한 대원은 Cost 텍스트
        else
        {
            if (btn == null && text == null) 
                //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex));
                button.onClick.AddListener(() => OnViewPurchase(crewIndex));
            else 
                //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex, btn, text));
                button.onClick.AddListener(() => OnViewPurchase(crewIndex));
            //보유한 금액보다 탐험대원의 비용이 더 비싸면 deniedPurchase
            if (_money < Convert.ToInt32(_crewStatusText[i].text))
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
    //탐원대원 선택
    public void OnSelectPet(int selectIndex, GameObject btn = null, TMP_Text text = null)
    {
        Debug.Log("SelectPet");
        //대원이 "출동 대기" -> "출동!" 으로, 이전 "출동!" -> "출동 대기" 상태로 변경
        if (_crewStatusText[selectIndex].text.Equals(_ownedCrew))
        {
            //"출동!" 찾기
            for (int i = 0; i < _crewStatusText.Count; i++)
            {
                // 보유중이고, "출동!" 상태인 탐원대원
                if (_collections.OwnedCrew[i] && _crewStatusText[i].text.Equals(_selectedCrew))
                {
                    //"출동!" -> "출동 대기" 
                    _crewStatusText[i].text = _ownedCrew;
                    SetBtnColor(_crewStatusBtn[i].GetComponent<Image>(), new Color32(_defaultR, _defaultG, _defaultB, 255));
                    break;
                }
            }
            _crewStatusText[selectIndex].text = _selectedCrew;
            SetBtnColor(_crewStatusBtn[selectIndex].GetComponent<Image>(), new Color32(_selectedR, _selectedG, _selectedB, 255));
            //상세보기 버튼 변경
            if (btn != null && text != null)
            {
                text.text = _selectedCrew;
                SetBtnColor(btn.GetComponent<Image>(), new Color32(_selectedR, _selectedG, _selectedB, 255));
            }
            SelectCrewManager.Instance.SelectedCrewIndex = selectIndex;
            //선택된 대원 DB에 변경
            _collections.SelectedCrew = selectIndex;
        }
    }
    //대원 구매
    public void OnPurchaseCrew(int i, GameObject btn = null, TMP_Text text = null)
    {
        Debug.Log("PurchaseCrew :" + i);

        var crewCost = Convert.ToInt32(_crewStatusText[i].text);
        if (_money >= crewCost)
        {
            //돈 차감
            _money -= crewCost;
            _moneyText.text = $"{_money}";

            //DB에 반영
            _collections.OwnedCrew[i] = true;
            //텍스트 변경
            _crewStatusText[i].text = _ownedCrew;
            //상세보기 버튼 변경
            if (btn != null && text != null)
            {
                text.text = _ownedCrew;
            }
            OnCheckPurchasePossibility?.Invoke();
            //버튼 이벤트 추가
            var button = _crewStatusBtn[i].GetComponent<Button>();
            //이전 버튼 PurchaseCrew 이벤트 제거후 -> 버튼 SelectPet 이벤트 부여
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnSelectPet(i));
            if (btn != null && text != null)
            {
                button = btn.GetComponent<Button>();
                //이전 버튼 PurchaseCrew 이벤트 제거후 -> 버튼 SelectPet 이벤트 부여
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnSelectPet(i, btn, text));
            }
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
            if (_collections.OwnedCrew[i]) continue;
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
    //탐원대원 정보(이름, 설명) 불러오기
    public void DetailCrewData()
    {
        _crewInfo = new List<Crew>();
        JsonData CrewData = JsonMapper.ToObject(_crewDataList.text);
        for(int i =0; i < CrewData.Count; i++)
        {
            _crewInfo.Add(new Crew(CrewData[i]["name"].ToString(), 
                                  CrewData[i]["descript"].ToString()));
        }
    }
}
