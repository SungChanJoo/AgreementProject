using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//��ȣ�ۿ� ������Ʈ �θ� Ŭ����
public class ObjInteractable : NetworkBehaviour
{
    public GameObject InteractableButton; //��ȣ�ۿ� ��ư
    public ObjInteractor Player; //������ �÷��̾�
    public int PlayTime; // ����ð�
    public int LimitTime;
    public GameObject InteractableParticle;

    public virtual void Awake()
    {
        //��ư�� ���������� ����
        if (InteractableButton.activeSelf)
            InteractableButton.SetActive(false);
    }
    //��ȣ�ۿ� ������Ʈ�ȿ� ������ ��
    private void OnTriggerEnter(Collider other)
    {
        Player = other.GetComponent<ObjInteractor>();
        if(Player != null)
            Player.InteractableUI(InteractableButton, true);//��ư�� ���������� Ű��
    }
    //��ȣ�ۿ� ������Ʈ�� ������ ��
    private void OnTriggerExit(Collider other)
    {
        if(Player != null)
            Player.InteractableUI(InteractableButton, false);//��ư�� ���������� ����
        Player = null; //�ʱ�ȭ
    }

    public IEnumerator currentPlayInterctable = null;
    //��ȣ�ۿ� ��ư �̺�Ʈ
    public virtual void OnClickInteract()
    {
        CmdPlayInterctable(); 
    }
    //��ȣ�ۿ� ���� ����ȭ
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
