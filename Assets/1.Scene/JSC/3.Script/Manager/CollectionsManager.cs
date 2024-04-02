using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using LitJson;
//탐험대원 상세보기 정보 클래스
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
//버튼 텍스트 정보 클래스
class CrewButton
{
    public const string _selectedCrew = "출동!";
    public const string _ownedCrew = "대기";
    public readonly static string[] _crewCost = { "0", "0", "0", "0", "0", "10"
                                       , "90", "700", "900", "1500", "2900", "4300"
                                       , "5700", "7000", "8700", "9800", "10620", "11520"
                                       , "12340", "13180", "14040", "14930", "15750", "16650"
                                       , "17480", "18380", "19200", "20060", "20960", "21800"
                                       , "22680", "23530", "24390", "25290", "26190", "27777"
                                       , "55500"};
}

//탐험대원을 고르고 메타별에 입장을 관리 클래스
public class CollectionsManager : MonoBehaviour
{
    public static CollectionsManager Instance = null;

    [Header("Collections")]
    public ExpenditionCrew Collections;
    public List<GameObject> CrewList;
    private List<TMP_Text> crewStatusText;
    private List<GameObject> crewStatusBtn;
    [SerializeField] private GameObject alreadySeletedCrewUI;
    [SerializeField] private PlayModToggle pmToggle;
    public Sprite SelectedImg;
    public Sprite DefaultImg;
    public Sprite DeniedImg;

    [Header("Purchase")]
    [SerializeField] private GameObject purchaseWindow;
    [SerializeField] private TMP_Text purchaseText;
    [SerializeField] private GameObject purchaseBtn;
    [SerializeField] private GameObject deniedPurchaseUI;
    [SerializeField] private float deniedPurchaseDuration;
    private int money = 0;
    [SerializeField] private TMP_Text moneyText;

    [Header("ViewDetails")]
    [SerializeField] private GameObject detailWindow;
    [SerializeField] private TextAsset crewDataList;
    private List<Crew> crewInfo;
    [SerializeField] private GameObject crewModel;
    private List<GameObject> crewModelList = new List<GameObject>();
    [SerializeField] private TMP_Text crewName;
    [SerializeField] private TMP_Text crewDescript;
    [SerializeField] private GameObject detailSelectBtn;
    [SerializeField] private TMP_Text detailSelectText;
    private GameObject modelSpace;
    private int seletedDetailModel;
    private bool isDrag = false;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        SetCollections();
        InitDetailCrewData();
    }
    public void OnViewPurchase(int crewNumber)
    {
        Debug.Log("OnViewPurchase");
        if (crewNumber > -1)
        {
            var crewCost = Convert.ToInt32(crewStatusText[crewNumber].text);
            Debug.Log("crewCost : " + crewCost);
            Debug.Log("_money : " + money);
            if (money >= crewCost)
            {
                //영입창 켜질때
                if (!purchaseWindow.activeSelf)
                {
                    purchaseText.text = $"[{crewInfo[crewNumber].CrewName}]을 영입하시겠습니까?";
                    //상세보기가 켜져있다면
                    if (!detailWindow.activeSelf)
                        purchaseBtn.GetComponent<Button>().onClick.AddListener(() => OnPurchaseCrew(crewNumber));
                    else
                        purchaseBtn.GetComponent<Button>().onClick.AddListener(() => OnPurchaseCrew(crewNumber, detailSelectBtn, detailSelectText));
                }
                purchaseWindow.SetActive(!purchaseWindow.activeSelf);
            }
            else
            {
                StartCoroutine(ViewDeniedPurchase_co());
                Debug.Log("구매할 수 없음");
            }
        }
        //닫기버튼
        else
        {
            purchaseWindow.SetActive(!purchaseWindow.activeSelf);
        }

    }
    IEnumerator ViewDeniedPurchase_co()
    {
        deniedPurchaseUI.SetActive(true);
        yield return new WaitForSeconds(deniedPurchaseDuration);
        deniedPurchaseUI.SetActive(false);
    }
    IEnumerator ViewAlreadySeletedCrew_co()
    {
        alreadySeletedCrewUI.SetActive(true);
        yield return new WaitForSeconds(deniedPurchaseDuration);
        alreadySeletedCrewUI.SetActive(false);
    }
    #region 대원상세보기
    //대원상세보기버튼 이벤트
    public void OnViewDetails(int crewNumber)
    {
        Debug.Log($"crewNumber : {crewNumber}");
        //상세보기가 켜질 때
        if (!detailWindow.activeSelf)
        {
            detailSelectText.text = crewStatusText[crewNumber].text;
            SetupCrewSelectionBtn(crewNumber, detailSelectBtn, detailSelectText);
            if (crewInfo[crewNumber] != null)
            {
                crewModelList[crewNumber].SetActive(true);
                crewModelList[crewNumber].transform.rotation = Quaternion.Euler(0f, 157f, 0f);
                crewName.text = $"대원이름 : {crewInfo[crewNumber].CrewName}";
                crewDescript.text = $"대원설명 : {crewInfo[crewNumber].CrewDescript}";
                seletedDetailModel = crewNumber;

            }
            else
            {
                crewName.text = $"대원이름 : 없음 {crewNumber}";
                crewDescript.text = $"대원설명 : 없음 {crewNumber}";
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
            if (crewInfo[seletedDetailModel] != null)
            {
                crewModelList[seletedDetailModel].SetActive(false);
                crewName.text = string.Empty;
                crewDescript.text = string.Empty;
                detailSelectBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            }
            //모델 회전 코루틴 종료
            if (currentRotateCrewModel_co != null)
            {
                currentRotateCrewModel_co = null;
            }
        }

        detailWindow.SetActive(!detailWindow.activeSelf);

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
                    modelSpace = EventSystem.current.currentSelectedGameObject;
                    if(modelSpace != null && modelSpace.name.Equals("Crew_Model"))
                        isDrag = true;
                }
                if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    isDrag = false;
                }
                //드래그가 가능하다면 모델 드래그에 따라 회전
                if (isDrag)
                {
                    float _modelRotationY = Input.GetTouch(0).deltaPosition.x * 1f;
                    crewModelList[seletedDetailModel].transform.Rotate(Vector3.down, _modelRotationY);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //Model이 있는 공간판단
                    modelSpace = EventSystem.current.currentSelectedGameObject;
                    if(modelSpace != null && modelSpace.name.Equals( "Crew_Model"))
                        isDrag = true;
                }
                if(Input.GetMouseButtonUp(0))
                {
                    isDrag = false;
                }
                //드래그가 가능하다면 모델 드래그에 따라 회전
                if (isDrag)
                {
                    float _modelRotationY = Input.GetAxis("Mouse X") * 2f;
                    crewModelList[seletedDetailModel].transform.Rotate(Vector3.down, _modelRotationY);
                }
            }
            yield return null;
        }
    } 
    #endregion
    //전체 대원, 보유 대원 전환
    public void ToggleOwnedCrew()
    {
        for (int i =0; i < CrewList.Count; i++)
        {
            //전체 대원 활성화 -> 비활성화, 비활성화 -> 활성화
            if (Collections.OwnedCrew[i]) continue; // , 보유 대원은 계속 활성화 상태
            CrewList[i].SetActive(!CrewList[i].activeSelf);
        }
    }
    //DB에서 탐험대원 콜렉션 불러오기
    public void SetCollections()
    {
        if (Client.instance != null)
            money = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].StarCoin;
        moneyText.text = $"{money}";
        //DB에 데이터 받아서 선택된 대원과 보유한 대원 리스트 받기
        if (Client.instance != null)
        {
            Collections = DataBase.Instance.playerInfo.Collections;
            Debug.Log(DataBase.Instance.playerInfo.playerName);
        }
        //데이터를 불러오지 못하면 초기화
        else
        {
            int selectedCrew = 0;
            List<bool> ownedCrew = new List<bool>();
            for (int i = 0; i < CrewList.Count; i++)
            {
                if(i<5)
                {
                    ownedCrew.Add(true);
                }
                else
                {
                    ownedCrew.Add(false);
                }
            }
            Collections = new ExpenditionCrew(selectedCrew, ownedCrew);
        }


        //텍스트, 버튼 초기화
        crewStatusBtn = new List<GameObject>();
        crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < CrewList.Count; i++)
        {
            crewStatusBtn.Add( CrewList[i].transform.Find("Button").gameObject);
            crewStatusText.Add(crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
        }

        for (int i=0; i< CrewList.Count; i++)
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
            buttonImg = crewStatusBtn[i].GetComponent<Image>();
            button = crewStatusBtn[i].GetComponent<Button>();
            buttonText = crewStatusText[i];
        }
        //상세보기 버튼 Setup
        else
        {
            buttonImg = btn.GetComponent<Image>();
            button = btn.GetComponent<Button>();
            buttonText = text;
        }
        button.onClick.RemoveAllListeners();
        int crewNumber = i; //클로저 방지
        //선택되어 있던 대원은 "출동!" 텍스트
        if(Collections == null)
        {
            Debug.Log("Collections is null");
        }
        else
        {
            if (Collections.SelectedCrew == i)
            {
                if (btn == null && text == null)
                    button.onClick.AddListener(() => OnSelectPet(crewNumber));
                else
                    button.onClick.AddListener(() => OnSelectPet(crewNumber, btn, text));
                buttonText.text = CrewButton._selectedCrew;
                //SetBtnColor(buttonImg, SelectedBtnColor);
                buttonImg.sprite = SelectedImg;
            }
            //보유한 대원은 "출동" 텍스트
            else if (Collections.OwnedCrew[i])
            {
                if (btn == null && text == null)
                    button.onClick.AddListener(() => OnSelectPet(crewNumber));
                else
                    button.onClick.AddListener(() => OnSelectPet(crewNumber, btn, text));
                buttonText.text = CrewButton._ownedCrew;
                //SetBtnColor(buttonImg, DefaultBtnColor);
                buttonImg.sprite = DefaultImg;
            }
            //영입가능한 대원은 Cost 텍스트
            else
            {
                buttonText.text = CrewButton._crewCost[i];
                if (btn == null && text == null)
                    //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex));
                    button.onClick.AddListener(() => OnViewPurchase(crewNumber));
                else
                    //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex, btn, text));
                    button.onClick.AddListener(() => OnViewPurchase(crewNumber));
                //보유한 금액보다 탐험대원의 비용이 더 비싸면 deniedPurchase
                if (money < Convert.ToInt32(crewStatusText[i].text))
                {
                    //SetBtnColor(buttonImg, DeniedPurchaseBtnColor);
                    buttonImg.sprite = DeniedImg;
                }
                //아니면 영입가능
                else
                {
                    //SetBtnColor(buttonImg, DefaultBtnColor);
                    buttonImg.sprite = DefaultImg;
                }
            }
        }
        
    }
    //탐험대원 선택
    public void OnSelectPet(int selectIndex, GameObject btn = null, TMP_Text text = null)
    {
        //이미 선택된 대원입니다.
        if (Collections.SelectedCrew == selectIndex)
        {
            StartCoroutine(ViewAlreadySeletedCrew_co());
        }
        else
        {
            //대원이 "출동 대기" -> "출동!" 으로, 이전 "출동!" -> "출동 대기" 상태로 변경
            if (crewStatusText[selectIndex].text.Equals(CrewButton._ownedCrew))
            {
                //"출동!" 상태인 탐험대원 "출동!" -> "출동 대기"
                crewStatusText[Collections.SelectedCrew].text = CrewButton._ownedCrew;
                crewStatusBtn[Collections.SelectedCrew].GetComponent<Image>().sprite = DefaultImg;
                //"출동 대기" -> "출동!"
                crewStatusText[selectIndex].text = CrewButton._selectedCrew;
                crewStatusBtn[selectIndex].GetComponent<Image>().sprite = SelectedImg;
                //SetBtnColor(_crewStatusBtn[selectIndex].GetComponent<Image>(), SelectedBtnColor);

                //상세보기 버튼 변경
                if (btn != null && text != null)
                {
                    text.text = CrewButton._selectedCrew;
                    //SetBtnColor(btn.GetComponent<Image>(), SelectedBtnColor);
                    btn.GetComponent<Image>().sprite = SelectedImg;
                }
                if (CrewSelectManager.Instance != null)
                    CrewSelectManager.Instance.SelectedCrewIndex = selectIndex;
                //선택된 대원 도감에 반영
                Collections.SelectedCrew = selectIndex;
            }
        }
    }
    //대원 구매
    public void OnPurchaseCrew(int i, GameObject btn = null, TMP_Text text = null)
    {
        var crewCost = Convert.ToInt32(crewStatusText[i].text);
        if (money >= crewCost)
        {
            //DB에 반영
            Collections.OwnedCrew[i] = true;
            //텍스트 변경
            crewStatusText[i].text = CrewButton._ownedCrew;
            //상세보기 버튼 변경
            if (btn != null && text != null)
            {
                text.text = CrewButton._ownedCrew;
            }
            //버튼 이벤트 추가
            var button = crewStatusBtn[i].GetComponent<Button>();
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
            //구매후 창닫기
            purchaseWindow.SetActive(false);
            purchaseBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            //대원 구매 시 DB에 현재 탐험대원 도감 반영
            if (Client.instance != null)
            {
                Client.instance.AppExit_SaveExpenditionCrewDataToDB(Collections);
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
        for (int i = 0; i < crewStatusText.Count; i++)
        {
            if (Collections.OwnedCrew[i]) continue;
            //보유중이 아니고, 가격이 현재 가진 돈보다 비싸면 버튼 색 변경
            if (money < Convert.ToInt32(crewStatusText[i].text))
            {
                crewStatusBtn[i].GetComponent<Image>().sprite = DeniedImg;
            }
            else
            {
                crewStatusBtn[i].GetComponent<Image>().sprite = DefaultImg;
            }
        }
    }

    //탐험대원 상세보기 초기화
    public void InitDetailCrewData()
    {
        //상세보기 탐험대원 프리펩 초기화
        for(int i = 0; i< crewModel.transform.childCount; i++)
        {
            crewModelList.Add(crewModel.transform.GetChild(i).gameObject);
        }
        //탐험대원 정보 초기화
        crewInfo = new List<Crew>();
        JsonData CrewData = JsonMapper.ToObject(crewDataList.text);
        for(int i =0; i < CrewData.Count; i++)
        {
            crewInfo.Add(new Crew(CrewData[i]["name"].ToString(), 
                                  CrewData[i]["descript"].ToString()));
        }
    }
}
