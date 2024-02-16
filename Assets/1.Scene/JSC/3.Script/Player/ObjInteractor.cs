using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//���ο��Ը� ��ȣ�ۿ� �����ϰ� ����� ���� ũ����
public class ObjInteractor : NetworkBehaviour
{
    //��ȣ�ۿ� ������ UI show or hide 
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
