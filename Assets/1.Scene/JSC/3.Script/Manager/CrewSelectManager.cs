using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using LitJson;
using System.IO;

//탐험대원을 선택하고 메타별에 입장을 관리하는 클래스
public class CrewSelectManager : MonoBehaviour
{
    public static CrewSelectManager Instance = null;
    [SerializeField] private GameObject crewContents;
    private List<GameObject> crewList = new List<GameObject>();
    [SerializeField] private GameObject crewSelectCanvas;
    [SerializeField] private GameObject enterMetaWorldCanvas;
    [SerializeField] private TMP_InputField metaWorldIpText;
    private List<TMP_Text> crewStatusText;
    private List<GameObject> crewStatusBtn;
    public int SelectedCrewIndex = 0;
    string path = string.Empty;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (path.Equals(string.Empty))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                path = Application.persistentDataPath + "/License";
            }
            else
                path = Application.dataPath + "/License";
        }
    }
    private void Start()
    {
        SetSeleteCrewUI();
    }
    public void LoadMetaWorld(string sceneName)
    {
        SettingIp(path);
        LoadingSceneController.LoadScene(sceneName);
        //메타별 입장시 노래 멈추기
        if(AudioManager.Instance != null)
           AudioManager.Instance.BGM_Stop();
        //다음 씬으로 선택한 대원을 넘겨주기 위해 
        DontDestroyOnLoad(gameObject);
        if(SettingManager.Instance != null)
            SettingManager.Instance.IsMetaWorld = true;
    }
    private void SettingIp(string path)
    {
        List<Item> item = new List<Item>();
        item.Add(new Item($"{Type.Client}", metaWorldIpText.text, "7777"));

        JsonData data = JsonMapper.ToJson(item);
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    public void OnViewSelectCrewUI()
    {
        crewSelectCanvas.SetActive(!crewSelectCanvas.activeSelf);
        SelectedCrewIndex = CollectionsManager.Instance.Collections.SelectedCrew;
        for (int i = 0; i < crewList.Count; i++)
        {
            //보유중인 대원이 아닐경우 게임오브젝트 활성화 -> 비활성화, 비활성화 -> 활성화
            if (CollectionsManager.Instance.Collections.OwnedCrew[i])
            {
                crewList[i].SetActive(true);
                //선택되어 있던 대원은 "출동!" 텍스트
                if(SelectedCrewIndex == i)
                {
                    crewStatusText[i].text = CrewButton._selectedCrew;
                    //_crewStatusBtn[i].GetComponent<Image>().color = CollectionsManager.Instance.SelectedBtnColor;
                    crewStatusBtn[i].GetComponent<Image>().sprite = CollectionsManager.Instance.SelectedImg;
                }
                //보유한 대원은 "출동 대기" 텍스트
                else
                {
                    crewStatusText[i].text = CrewButton._ownedCrew;
                    //_crewStatusBtn[i].GetComponent<Image>().color = CollectionsManager.Instance.DefaultBtnColor;
                    crewStatusBtn[i].GetComponent<Image>().sprite = CollectionsManager.Instance.DefaultImg;
                }
            }
            else
                crewList[i].SetActive(false);
        }
    }
    public void SetSeleteCrewUI()
    {
        //탐험대원 프리펩 초기화
        for (int i = 0; i < crewContents.transform.childCount; i++)
        {
            crewList.Add(crewContents.transform.GetChild(i).gameObject);
        }
        //텍스트, 버튼 초기화
        crewStatusBtn = new List<GameObject>();
        crewStatusText = new List<TMP_Text>();
        for (int i = 0; i < crewList.Count; i++)
        {
            crewStatusBtn.Add(crewList[i].transform.Find("Button").gameObject);
            crewStatusText.Add(crewStatusBtn[i].transform.Find("Text (TMP)").gameObject.GetComponent<TMP_Text>());
            
            var button = crewStatusBtn[i].GetComponent<Button>();
            int crewNumber = i; //클로저 방지
            button.onClick.AddListener(() => OnSelectPet(crewNumber));
        }
    }

    //탐험대원 선택
    public void OnSelectPet(int selectIndex)
    {
        //대원이 "출동 대기" -> "출동!" 으로, 이전 "출동!" -> "출동 대기" 상태로 변경
        if (crewStatusText[selectIndex].text.Equals(CrewButton._ownedCrew))
        {
            //"출동!" 상태인 탐험대원 "출동!" -> "출동 대기" 
            crewStatusBtn[SelectedCrewIndex].GetComponent<Image>().sprite = CollectionsManager.Instance.DefaultImg;
            crewStatusText[SelectedCrewIndex].text = CrewButton._ownedCrew;
            //"출동 대기" -> "출동!"
            crewStatusText[selectIndex].text = CrewButton._selectedCrew;
            crewStatusBtn[selectIndex].GetComponent<Image>().sprite = CollectionsManager.Instance.SelectedImg;

            //상세보기 버튼 변경
            SelectedCrewIndex = selectIndex;
            //선택된 대원 도감에 반영
            CollectionsManager.Instance.OnSelectPet(selectIndex);
        }
    }
    //메타월드 Ip입력창 보기
    public void OnViewEnterCanvas()
    {

        enterMetaWorldCanvas.SetActive(!enterMetaWorldCanvas.activeSelf);
    }
}
