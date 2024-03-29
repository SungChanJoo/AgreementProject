using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//상호작용 오브젝트 부모 클래스
public class ObjInteractable : NetworkBehaviour
{
    public GameObject InteractableButton; //상호작용 버튼
    public ObjInteractor Player; //실행할 플레이어
    public int PlayTime; // 실행시간
    public int LimitTime;
    public GameObject InteractableParticle;

    public virtual void Awake()
    {
        //버튼이 켜져있으면 끄기
        if (InteractableButton.activeSelf)
            InteractableButton.SetActive(false);
    }
    //상호작용 오브젝트안에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        Player = other.GetComponent<ObjInteractor>();
        if(Player != null)
            Player.InteractableUI(InteractableButton, true);//버튼이 꺼져있으면 키기
    }
    //상호작용 오브젝트를 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        if(Player != null)
            Player.InteractableUI(InteractableButton, false);//버튼이 켜져있으면 끄기
        Player = null; //초기화
    }

    public IEnumerator currentPlayInterctable = null;
    //상호작용 버튼 이벤트
    public virtual void OnClickInteract()
    {
        CmdPlayInterctable(); 
    }
    //상호작용 서버 동기화
    [Command(requiresAuthority = false)]
    public virtual void CmdPlayInterctable()
    {
        RpcPlayInterctable();
    }
    [ClientRpc]
    public virtual void RpcPlayInterctable()
    {
        if (currentPlayInterctable == null)
        {
            currentPlayInterctable = PlayInterctable();
            StartCoroutine(currentPlayInterctable);
        }
    }
    public virtual IEnumerator PlayInterctable()
    {
        yield return null;

    }
}
