using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//개인에게만 상호작용 가능하게 만들기 위한 크랠스
public class ObjInteractor : NetworkBehaviour
{
    //상호작용 가능한 UI show or hide 
    public void InteractableUI(GameObject UI, bool value)
    {
        if (!isLocalPlayer) return;

        UI.SetActive(value);
    }
    public void InteractablePlayMusicBox(AudioSource audioSource, int playTime)
    {
        if (!isLocalPlayer) return;
        Debug.Log("MusicBoxPlay");
        StartCoroutine(PlayMusic_co(audioSource, playTime));
    }
    IEnumerator PlayMusic_co(AudioSource audioSource, int playTime)
    {
        audioSource.Play();
        yield return new WaitForSeconds(playTime);
        audioSource.Stop();
        Debug.Log("MusicBoxStop");

    }
}
