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
        //버튼이 켜져있으면 끄기
        if (InteractableButton.activeSelf)
            InteractableButton.SetActive(false);
        if (Particle.activeSelf)
            Particle.SetActive(false);
    }
    //상호작용 오브젝트안에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        //버튼이 꺼져있으면 키기
        other.GetComponent<PlayerMovement>().InteractableUI(InteractableButton, true);
    }
    //상호작용 오브젝트를 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        //버튼이 켜져있으면 끄기
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
        //파티클이 활성화된 상태가 아니라면
        if (!Particle.activeSelf)
            RpcPlayEffect();
    }
    [ClientRpc]
    private void RpcPlayEffect()
    {
        StartCoroutine(PlayEffect());
    }
    //이펙트 실행
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
