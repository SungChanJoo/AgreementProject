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
        //��ư�� ���������� ����
        if (InteractableButton.activeSelf)
            InteractableButton.SetActive(false);
    }
    //��ȣ�ۿ� ������Ʈ�ȿ� ������ ��
    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<ObjInteractor>();
        if(player != null)
            player.InteractableUI(InteractableButton, true);//��ư�� ���������� Ű��
    }
    //��ȣ�ۿ� ������Ʈ�� ������ ��
    private void OnTriggerExit(Collider other)
    {
        player = other.GetComponent<ObjInteractor>();
        if (player != null)
        {
            player.InteractableUI(InteractableButton, false);//��ư�� ���������� ����
            player = null; //�ʱ�ȭ
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
