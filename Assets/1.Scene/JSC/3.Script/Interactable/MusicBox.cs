using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//�����ڽ� Ŭ����
public class MusicBox : ObjInteractable
{
    public AudioSource audioSource;
    //���� ����
    public override IEnumerator PlayInterctable()
    {
        if (Player != null)
        {
            Player.InteractablePlayMusicBox(audioSource, PlayTime); //����ð���ŭ ���
            yield return new WaitForSeconds(PlayTime + LimitTime); //���� ��� ��Ÿ��
            currentPlayInterctable = null;
        }
    }
}
