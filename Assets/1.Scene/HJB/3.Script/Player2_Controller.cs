using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2_Controller : PlayerController
{


    private void Start()
    {

        //capsulation = new Capsulation(speed: MoveSpeed);
        capsulation = new Capsulation(speed: MoveSpeed);
        capsulation.Speed = MoveSpeed;

    }
    private void Update()
    {
        ControllKey();
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            capsulation.Speed = 20;
        }
    }
    public override void ControllKey()
    {
        if (!Input.anyKey)
        {
            hori = 0f;
            verti = 0f;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            hori = 1f;
        }        
        if (Input.GetKeyDown(KeyCode.F))
        {
            hori = -1f;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            verti = 1f;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            verti = -1f;
        }
        rigidbody.velocity = new Vector3(hori * capsulation.Speed, verti * capsulation.Speed);

        
    }
}
