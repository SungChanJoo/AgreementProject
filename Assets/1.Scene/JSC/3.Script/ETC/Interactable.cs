using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Interactable : NetworkBehaviour
{
    public GameObject InteractableButton;
    public GameObject Particle;
    private void Awake()
    {
        //��ư�� ���������� ����
        if (InteractableButton.activeSelf)
            InteractableButton.SetActive(false);
        if (Particle.activeSelf)
            Particle.SetActive(false);
    }
    //��ȣ�ۿ� ������Ʈ�ȿ� ������ ��
    private void OnTriggerEnter(Collider other)
    {
        //��ư�� ���������� Ű��
        other.GetComponent<PlayerMovement>().InteractableUI(InteractableButton, true);
    }
    //��ȣ�ۿ� ������Ʈ�� ������ ��
    private void OnTriggerExit(Collider other)
    {
        //��ư�� ���������� ����
        other.GetComponent<PlayerMovement>().InteractableUI(InteractableButton, false);
    }

    public void OnClickInteract()
    {
        CmdPlayerEffect();
    }
    #region PlayEffect
    [Command(requiresAuthority = false)]
    public void CmdPlayerEffect()
    {
        //��ƼŬ�� Ȱ��ȭ�� ���°� �ƴ϶��
        if (!Particle.activeSelf)
            RpcPlayEffect();
    }
    [ClientRpc]
    private void RpcPlayEffect()
    {
        StartCoroutine(PlayEffect());
    }
    //����Ʈ ����
    IEnumerator PlayEffect()
    {
        Particle.SetActive(true);
        while (Particle.GetComponent<ParticleSystem>().isPlaying)
        {
            yield return null;
        }
        Particle.SetActive(false);
    } 
    #endregion

}
