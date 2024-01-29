using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capsulation : MonoBehaviour
{
    private float speed = 20f;
    public float Speed
    {
        get
        {
            return speed;   
        }
        set
        {
            speed = value;
            
            if (speed > 10)
            {
                speed = 10f;
            }
            
        }

        
    }
    public Capsulation(float speed)
    {
        Speed = speed;
    }
}

//public float Speed
//{
//    get { return speed; }
//    set { speed = value; }
//}





//public float Speed => _Speed;
//
//public Capsulation(float speed)
//{
//    _Speed = speed;
//}