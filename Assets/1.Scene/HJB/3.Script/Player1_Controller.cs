using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player1_Controller : PlayerController
{
        
    private void Start()
    {
        capsulation = new Capsulation(speed: MoveSpeed);
        capsulation.Speed = MoveSpeed;

    }
    private void Update()
    {
        ControllKey();
        Debug.Log("1111 �Դϴ�. :::  " + capsulation.Speed);
    }

    public override void ControllKey()
    {
        hori = Input.GetAxis("Horizontal");
        verti = Input.GetAxis("Vertical");
        
        rigidbody.velocity = new Vector3(hori * capsulation.Speed, verti * capsulation.Speed);
        
    }
}
