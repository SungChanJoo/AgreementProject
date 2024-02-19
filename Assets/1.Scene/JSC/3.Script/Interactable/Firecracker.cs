using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Firecracker : ObjInteractable
{
    public List<GameObject> ParticleList;


    //����Ʈ ����
    public override IEnumerator PlayInterctable()
    {
        //��ƼŬ ����Ʈ ���� ���� ��ƼŬ ���� todo 02.16
        var rand = Random.Range(0, ParticleList.Count);
        ParticleList[rand].SetActive(true);
        yield return new WaitForSeconds(PlayTime);
        ParticleList[rand].SetActive(false);
        yield return new WaitForSeconds(LimitTime);
        currentPlayInterctable = null;
    }
}
