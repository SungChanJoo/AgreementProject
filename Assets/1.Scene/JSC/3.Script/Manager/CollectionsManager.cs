using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;
using LitJson;
//Ž���� �󼼺��� ���� Ŭ����
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
//��ư �ؽ�Ʈ ���� Ŭ����
class CrewButton
{
    public const string _selectedCrew = "�⵿!";
    public const string _ownedCrew = "���";
    public readonly static string[] _crewCost = { "0", "0", "0", "0", "0", "10"
                                       , "90", "700", "900", "1500", "2900", "4300"
                                       , "5700", "7000", "8700", "9800", "10620", "11520"
                                       , "12340", "13180", "14040", "14930", "15750", "16650"
                                       , "17480", "18380", "19200", "20060", "20960", "21800"
                                       , "22680", "23530", "24390", "25290", "26190", "27777"
                                       , "55500"};
}

//Ž������ ���� ��Ÿ���� ������ ���� Ŭ����
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
                //����â ������
                if (!purchaseWindow.activeSelf)
                {
                    purchaseText.text = $"[{crewInfo[crewNumber].CrewName}]�� �����Ͻðڽ��ϱ�?";
                    //�󼼺��Ⱑ �����ִٸ�
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
                Debug.Log("������ �� ����");
            }
        }
        //�ݱ��ư
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
    #region ����󼼺���
    //����󼼺����ư �̺�Ʈ
    public void OnViewDetails(int crewNumber)
    {
        Debug.Log($"crewNumber : {crewNumber}");
        //�󼼺��Ⱑ ���� ��
        if (!detailWindow.activeSelf)
        {
            detailSelectText.text = crewStatusText[crewNumber].text;
            SetupCrewSelectionBtn(crewNumber, detailSelectBtn, detailSelectText);
            if (crewInfo[crewNumber] != null)
            {
                crewModelList[crewNumber].SetActive(true);
                crewModelList[crewNumber].transform.rotation = Quaternion.Euler(0f, 157f, 0f);
                crewName.text = $"����̸� : {crewInfo[crewNumber].CrewName}";
                crewDescript.text = $"������� : {crewInfo[crewNumber].CrewDescript}";
                seletedDetailModel = crewNumber;

            }
            else
            {
                crewName.text = $"����̸� : ���� {crewNumber}";
                crewDescript.text = $"������� : ���� {crewNumber}";
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
            if (crewInfo[seletedDetailModel] != null)
            {
                crewModelList[seletedDetailModel].SetActive(false);
                crewName.text = string.Empty;
                crewDescript.text = string.Empty;
                detailSelectBtn.GetComponent<Button>().onClick.RemoveAllListeners();
            }
            //�� ȸ�� �ڷ�ƾ ����
            if (currentRotateCrewModel_co != null)
            {
                currentRotateCrewModel_co = null;
            }
        }

        detailWindow.SetActive(!detailWindow.activeSelf);

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
                    modelSpace = EventSystem.current.currentSelectedGameObject;
                    if(modelSpace != null && modelSpace.name.Equals("Crew_Model"))
                        isDrag = true;
                }
                if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    isDrag = false;
                }
                //�巡�װ� �����ϴٸ� �� �巡�׿� ���� ȸ��
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
                    //Model�� �ִ� �����Ǵ�
                    modelSpace = EventSystem.current.currentSelectedGameObject;
                    if(modelSpace != null && modelSpace.name.Equals( "Crew_Model"))
                        isDrag = true;
                }
                if(Input.GetMouseButtonUp(0))
                {
                    isDrag = false;
                }
                //�巡�װ� �����ϴٸ� �� �巡�׿� ���� ȸ��
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
        if (Client.instance != null)
            money = DataBase.Instance.PlayerCharacter[DataBase.Instance.CharacterIndex].StarCoin;
        moneyText.text = $"{money}";
        //DB�� ������ �޾Ƽ� ���õ� ����� ������ ��� ����Ʈ �ޱ�
        if (Client.instance != null)
        {
            Collections = DataBase.Instance.playerInfo.Collections;
            Debug.Log(DataBase.Instance.playerInfo.playerName);
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
        //�󼼺��� ��ư Setup
        else
        {
            buttonImg = btn.GetComponent<Image>();
            button = btn.GetComponent<Button>();
            buttonText = text;
        }
        button.onClick.RemoveAllListeners();
        int crewNumber = i; //Ŭ���� ����
        //���õǾ� �ִ� ����� "�⵿!" �ؽ�Ʈ
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
                buttonText.text = CrewButton._crewCost[i];
                if (btn == null && text == null)
                    //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex));
                    button.onClick.AddListener(() => OnViewPurchase(crewNumber));
                else
                    //button.onClick.AddListener(() => OnPurchaseCrew(crewIndex, btn, text));
                    button.onClick.AddListener(() => OnViewPurchase(crewNumber));
                //������ �ݾ׺��� Ž������ ����� �� ��θ� deniedPurchase
                if (money < Convert.ToInt32(crewStatusText[i].text))
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
            if (crewStatusText[selectIndex].text.Equals(CrewButton._ownedCrew))
            {
                //"�⵿!" ������ Ž���� "�⵿!" -> "�⵿ ���"
                crewStatusText[Collections.SelectedCrew].text = CrewButton._ownedCrew;
                crewStatusBtn[Collections.SelectedCrew].GetComponent<Image>().sprite = DefaultImg;
                //"�⵿ ���" -> "�⵿!"
                crewStatusText[selectIndex].text = CrewButton._selectedCrew;
                crewStatusBtn[selectIndex].GetComponent<Image>().sprite = SelectedImg;
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
        var crewCost = Convert.ToInt32(crewStatusText[i].text);
        if (money >= crewCost)
        {
            //DB�� �ݿ�
            Collections.OwnedCrew[i] = true;
            //�ؽ�Ʈ ����
            crewStatusText[i].text = CrewButton._ownedCrew;
            //�󼼺��� ��ư ����
            if (btn != null && text != null)
            {
                text.text = CrewButton._ownedCrew;
            }
            //��ư �̺�Ʈ �߰�
            var button = crewStatusBtn[i].GetComponent<Button>();
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
            purchaseWindow.SetActive(false);
            purchaseBtn.GetComponent<Button>().onClick.RemoveAllListeners();
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
        for (int i = 0; i < crewStatusText.Count; i++)
        {
            if (Collections.OwnedCrew[i]) continue;
            //�������� �ƴϰ�, ������ ���� ���� ������ ��θ� ��ư �� ����
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

    //Ž���� �󼼺��� �ʱ�ȭ
    public void InitDetailCrewData()
    {
        //�󼼺��� Ž���� ������ �ʱ�ȭ
        for(int i = 0; i< crewModel.transform.childCount; i++)
        {
            crewModelList.Add(crewModel.transform.GetChild(i).gameObject);
        }
        //Ž���� ���� �ʱ�ȭ
        crewInfo = new List<Crew>();
        JsonData CrewData = JsonMapper.ToObject(crewDataList.text);
        for(int i =0; i < CrewData.Count; i++)
        {
            crewInfo.Add(new Crew(CrewData[i]["name"].ToString(), 
                                  CrewData[i]["descript"].ToString()));
        }
    }
}
