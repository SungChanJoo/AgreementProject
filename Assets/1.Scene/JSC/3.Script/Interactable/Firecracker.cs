using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//���� Ŭ����
public class Firecracker : ObjInteractable
{
    public List<GameObject> ParticleList;
    //����Ʈ ����
    public override IEnumerator PlayInterctable()
    {
        var rand = Random.Range(0, ParticleList.Count); //ParticleList���� ������ ���� ����Ʈ ���
        InteractableParticle.SetActive(false);
        ParticleList[rand].SetActive(true);
        yield return new WaitForSeconds(PlayTime); //���� ����Ʈ ����ð�
        ParticleList[rand].SetActive(false);
        yield return new WaitForSeconds(LimitTime);//���� ����Ʈ ��Ÿ��
        if (Player != null)
            InteractableParticle.SetActive(true); // ��ȣ�ۿ� ������ ���� �˸��� ���� ����Ʈ
        currentPlayInterctable = null;
    }
}
