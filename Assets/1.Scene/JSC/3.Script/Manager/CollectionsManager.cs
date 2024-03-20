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

class CrewButton
{
    public const string _selectedCrew = "�⵿!";
    public const string _ownedCrew = "���";
}

//���� ���� ��Ÿ���� ������ �����ϴ� �Ŵ���
public class CollectionsManager : MonoBehaviour
{


    public static CollectionsManager Instance = null;

    [Header("Collections")]
    public ExpenditionCrew Collections;
    public List<GameObject> CrewList;
    private List<TMP_Text> _crewStatusText;
    private List<GameObject> _crewStatusBtn;
    [SerializeField] private GameObject _alreadySeletedCrewUI;
    [SerializeField] private PlayerMod_Toggle PMToggle;
    public Sprite SelectedImg;
    public Sprite DefaultImg;
    public Sprite DeniedImg;

    [Header("Purchase")]
    [SerializeField] private GameObject _purchaseWindow;
    [SerializeField] private TMP_Text _purchaseText;
    [SerializeField] private GameObject _purchaseBtn;
    [SerializeField] private GameObject _deniedPurchaseUI;
    [SerializeField] private float _deniedPurchaseDuration;
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
    [SerializeField] private TMP_Text _detailSelectText;
    private GameObject _ModelSpace;
    private int _seletedDetailModel;
    private bool _isDrag = false;

    public Action OnCheckPurchasePossibility; //�� ���ŵɶ����� ȣ��

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        if(Client.instance != null)
            _money= DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].StarCoin;
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
        //���� ��Ȱ��ȭ �� DB�� ���� Ž���� ���� �ݿ�
        if (Client.instance != null)
        {
            Client.instance.AppExit_SaveExpenditionCrewDataToDB(Collections);
        }
    }
    public void LoadMetaWorld(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnViewPurchase(int crewNumber)
    {
        Debug.Log("OnViewPurchase");
        if (crewNumber > -1)
        {
            var crewCost = Convert.ToInt32(_crewStatusText[crewNumber].text);
            Debug.Log("crewCost : " + crewCost);
            Debug.Log("_money : " + _money);
            if (_money >= crewCost)
            {
                //����â ������
                if (!_purchaseWindow.activeSelf)
                {
                    _purchaseText.text = $"[{_crewInfo[crewNumber].CrewName}]�� �����Ͻðڽ��ϱ�?";
                    //�󼼺��Ⱑ �����ִٸ�
                    if (!_detailWindow.activeSelf)
                        _purchaseBtn.GetComponent<Button>().onClick.AddListener(() => OnPurchaseCrew(crewNumber));
                    else
                        _purchaseBtn.GetComponent<Button>().onClick.AddListener(() => OnPurchaseCrew(crewNumber, _detailSelectBtn, _detailSelectText));
                }
                _purchaseWindow.SetActive(!_purchaseWindow.activeSelf);
            }
            else
            {
                StartCoroutine(ViewDeniedPurchase_co());
                Debug.Log("������ �� ����");
            }
        }
        //�ݱ��ư
        else
        {
            _purchaseWindow.SetActive(!_purchaseWindow.activeSelf);
        }

    }
    IEnumerator ViewDeniedPurchase_co()
    {
        _deniedPurchaseUI.SetActive(true);
        yield return new WaitForSeconds(_deniedPurchaseDuration);
        _deniedPurchaseUI.SetActive(false);
    }
    IEnumerator ViewAlreadySeletedCrew_co()
    {
        _alreadySeletedCrewUI.SetActive(true);
        yield return new WaitForSeconds(_deniedPurchaseDuration);
        _alreadySeletedCrewUI.SetActive(false);
    }
    #region ����󼼺���
    //����󼼺����ư �̺�Ʈ
    public void OnViewDetails(int crewNumber)
    {
        Debug.Log($"crewNumber : {crewNumber}");
        //�󼼺��Ⱑ ���� ��
        if (!_detailWindow.activeSelf)
        {
            _detailSelectText.text = _crewStatusText[crewNumber].text;
            SetupCrewSelectionBtn(crewNumber, _detailSelectBtn, _detailSelectText);
            if (_crewInfo[crewNumber] != null)
            {
                _crewModel[crewNumber].SetActive(true);
                _crewModel[crewNumber].transform.rotation = Quaternion.Euler(0f, 157f, 0f);
                _crewName.text = $"����̸� : {_crewInfo[crewNumber].CrewName}";
                _crewDescript.text = $"������� : {_crewInfo[crewNumber].CrewDescript}";
                _seletedDetailModel = crewNumber;

            }
            else
            {
                _crewName.text = $"����̸� : ���� {crewNumber}";
                _crewDescript.text = $"������� : ���� {crewNumber}";
            }
            //�� ȸ�� �ڷ�ƾ ����
            if (currentRotateCrewModel_co == null)
            {
                currentRotateCrewModel_co = RotateCrewModel_co();
                StartCoroutine(currentRotateCrewModel_co);
            }
        }
        //�󼼺��Ⱑ ���� ��
        else
        {
            if (_crewInfo[_seletedDetailModel] != null)
            {
                _crewModel[_seletedDetailModel].SetActive(false);
                _crewName.text = string.Empty;
                _crewDescript.text = string.Empty;
                _detailSelectBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            }
            //�� ȸ�� �ڷ�ƾ ����
            if (currentRotateCrewModel_co != null)
            {
                currentRotateCrewModel_co = null;
            }
        }

        _detailWindow.SetActive(!_detailWindow.activeSelf);

    }
    IEnumerator currentRotateCrewModel_co = null;
    //Model�� �ִ� �������� �巡�� �� �� ���� �¿�� ȸ��
    IEnumerator RotateCrewModel_co()
    {
        while (currentRotateCrewModel_co != null)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    //Model�� �ִ� �����Ǵ�
                    _ModelSpace = EventSystem.current.currentSelectedGameObject;
                    if(_ModelSpace != null && _ModelSpace.name.Equals("Crew_Model"))
                        _isDrag = true;
                }
                if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    _isDrag = false;
                }
                //�巡�װ� �����ϴٸ� �� �巡�׿� ���� ȸ��
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
                    //Model�� �ִ� �����Ǵ�
                    _ModelSpace = EventSystem.current.currentSelectedGameObject;
                    if(_ModelSpace != null && _ModelSpace.name.Equals( "Crew_Model"))
                        _isDrag = true;
                }
                if(Input.GetMouseButtonUp(0))
                {
                    _isDrag = false;
                }
                //�巡�װ� �����ϴٸ� �� �巡�׿� ���� ȸ��
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
    //��ü ���, ���� ��� ��ȯ
    public void ToggleOwnedCrew()
    {
        for (int i =0; i < CrewList.Count; i++)
        {
            //��ü ��� Ȱ��ȭ -> ��Ȱ��ȭ, ��Ȱ��ȭ -> Ȱ��ȭ
            if (Collections.OwnedCrew[i]) continue; // , ���� ����� ��� Ȱ��ȭ ����
            CrewList[i].SetActive(!CrewList[i].activeSelf);
        }
    }
    //DB���� Ž���� �ݷ��� �ҷ�����
    public void SetCollections()
    {
        //DB�� ������ �޾Ƽ� ���õ� ����� ������ ��� ����Ʈ �ޱ�
        if(Client.instance != null)
        {
            Collections = DataBase.Instance.playerInfo.Collections;
        }
        //�����͸� �ҷ����� ���ϸ� �ʱ�ȭ
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


        //�ؽ�Ʈ, ��ư �ʱ�ȭ
        _crewStatusBtn = new List<GameObject>();
        _crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < CrewList.Count; i++)
        {
            _crewStatusBtn.Add( CrewList[i].transform.Find("Button").gameObject);
            _crewStatusText.Add(_crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
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
            buttonImg = _crewStatusBtn[i].GetComponent<Image>();
            button = _crewStatusBtn[i].GetComponent<Button>();
            buttonText = _crewStatusText[i];
        }
        //�󼼺��� ��ư Setup
        else
        {
            buttonImg = btn.GetComponent<Image>();
            button = btn.GetComponent<Button>();
            buttonText = text;
        }
        int crewNumber = i; //Ŭ���� ����
        //���õǾ� �ִ� ����� "�⵿!" �ؽ�Ʈ
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
        //������ ����� "�⵿" �ؽ�Ʈ
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
        //���԰����� ����� Cost �ؽ�Ʈ
        else
        {
            if (btn == null && text == null) 
                //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex));
                button.onClick.AddListener(() => OnViewPurchase(crewNumber));
            else 
                //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex, btn, text));
                button.onClick.AddListener(() => OnViewPurchase(crewNumber));
            //������ �ݾ׺��� Ž������ ����� �� ��θ� deniedPurchase
            if (_money < Convert.ToInt32(_crewStatusText[i].text))
            {
                //SetBtnColor(buttonImg, DeniedPurchaseBtnColor);
                buttonImg.sprite = DeniedImg;
            }
            //�ƴϸ� ���԰���
            else
            {
                //SetBtnColor(buttonImg, DefaultBtnColor);
                buttonImg.sprite = DefaultImg;
            }
        }
    }
    //Ž���� ����
    public void OnSelectPet(int selectIndex, GameObject btn = null, TMP_Text text = null)
    {
        //�̹� ���õ� ����Դϴ�.
        if (Collections.SelectedCrew == selectIndex)
        {
            StartCoroutine(ViewAlreadySeletedCrew_co());
        }
        else
        {
            //����� "�⵿ ���" -> "�⵿!" ����, ���� "�⵿!" -> "�⵿ ���" ���·� ����
            if (_crewStatusText[selectIndex].text.Equals(CrewButton._ownedCrew))
            {
                //"�⵿!" ������ Ž���� "�⵿!" -> "�⵿ ���"
                _crewStatusText[Collections.SelectedCrew].text = CrewButton._ownedCrew;
                _crewStatusBtn[Collections.SelectedCrew].GetComponent<Image>().sprite = DefaultImg;
                //"�⵿ ���" -> "�⵿!"
                _crewStatusText[selectIndex].text = CrewButton._selectedCrew;
                _crewStatusBtn[selectIndex].GetComponent<Image>().sprite = SelectedImg;
                //SetBtnColor(_crewStatusBtn[selectIndex].GetComponent<Image>(), SelectedBtnColor);

                //�󼼺��� ��ư ����
                if (btn != null && text != null)
                {
                    text.text = CrewButton._selectedCrew;
                    //SetBtnColor(btn.GetComponent<Image>(), SelectedBtnColor);
                    btn.GetComponent<Image>().sprite = SelectedImg;
                }
                if (CrewSelectManager.Instance != null)
                    CrewSelectManager.Instance.SelectedCrewIndex = selectIndex;
                //���õ� ��� ������ �ݿ�
                Collections.SelectedCrew = selectIndex;
            }
        }
    }
    //��� ����
    public void OnPurchaseCrew(int i, GameObject btn = null, TMP_Text text = null)
    {
        var crewCost = Convert.ToInt32(_crewStatusText[i].text);
        if (_money >= crewCost)
        {
            //�� ����
            _money -= crewCost;
            _moneyText.text = $"{_money}";

            //DB�� �ݿ�
            Collections.OwnedCrew[i] = true;
            //�ؽ�Ʈ ����
            _crewStatusText[i].text = CrewButton._ownedCrew;
            //�󼼺��� ��ư ����
            if (btn != null && text != null)
            {
                text.text = CrewButton._ownedCrew;
            }
            OnCheckPurchasePossibility?.Invoke();
            //��ư �̺�Ʈ �߰�
            var button = _crewStatusBtn[i].GetComponent<Button>();
            //���� ��ư PurchaseCrew �̺�Ʈ ������ -> ��ư SelectPet �̺�Ʈ �ο�
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnSelectPet(i));
            if (btn != null && text != null)
            {
                button = btn.GetComponent<Button>();
                //���� ��ư PurchaseCrew �̺�Ʈ ������ -> ��ư SelectPet �̺�Ʈ �ο�
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnSelectPet(i, btn, text));
            }
            //������ â�ݱ�
            _purchaseWindow.SetActive(false);
            _purchaseBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            //��� ���� �� DB�� ���� Ž���� ���� �ݿ�
            if (Client.instance != null)
            {
                Client.instance.AppExit_SaveExpenditionCrewDataToDB(Collections);
            }
        }
        else
        {
            Debug.Log("������ �� ����");
        }
    }
    //���Ű������� Ȯ��
    public void CheckPurchasePossibility()
    {
        for (int i = 0; i < _crewStatusText.Count; i++)
        {
            if (Collections.OwnedCrew[i]) continue;
            //�������� �ƴϰ�, ������ ���� ���� ������ ��θ� ��ư �� ����
            if (_money < Convert.ToInt32(_crewStatusText[i].text))
            {
                _crewStatusBtn[i].GetComponent<Image>().sprite = DeniedImg;
            }
            else
            {
                _crewStatusBtn[i].GetComponent<Image>().sprite = DefaultImg;
            }
        }
    }

    //Ž���� ����(�̸�, ����) �ҷ�����
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
