using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EAG_Animation : MonoBehaviour
{
    [SerializeField] private Material[] material;

    [SerializeField] private SkinnedMeshRenderer currentMesh;
    
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        //������ Ž������ Material ��������
        currentMesh.materials[0] = material[StepManager.Instance.SelectCharacter_num];
    }

    public void CreateProblem()
    {
        animator.SetTrigger("Attack1");        
    }


}
