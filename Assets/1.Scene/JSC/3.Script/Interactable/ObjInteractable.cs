using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ObjInteractable : NetworkBehaviour
{
    public GameObject InteractableButton;
    public ObjInteractor player;
    public int PlayTime;
    public int LimitTime;
    public virtual void Awake()
    {
        //버튼이 켜져있으면 끄기
        if (InteractableButton.activeSelf)
            InteractableButton.SetActive(false);
    }
    //상호작용 오브젝트안에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<ObjInteractor>();
        if(player != null)
            player.InteractableUI(InteractableButton, true);//버튼이 꺼져있으면 키기
    }
    //상호작용 오브젝트를 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        player = other.GetComponent<ObjInteractor>();
        if (player != null)
        {
            player.InteractableUI(InteractableButton, false);//버튼이 켜져있으면 끄기
            player = null; //초기화
        }
    }

    public IEnumerator currentPlayInterctable = null;
    public virtual void OnClickInteract()
    {
        CmdPlayInterctable();
    }

    [Command(requiresAuthority = false)]
    public virtual void CmdPlayInterctable()
    {
        RpcPlayEffect();
    }
    [ClientRpc]
    public virtual void RpcPlayEffect()
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
