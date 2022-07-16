using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement;

public class CollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collision Enter: " + collision.gameObject.name);
        
    }

    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log("Collision Stay: " + collision.gameObject.name );
        //if (collision.gameObject.name=="Rod8")
        //{
        //    sharedmovement.audioSource.clip = sharedmovement.loseSound;
        //    sharedmovement.audioSource.Play();
        //}
        if (collision.gameObject.name == "Rod8")
        {
            
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log("CollisionExit: " + collision.gameObject.name);
        if (collision.gameObject.name == "Rod8")
        {

        }

    }
}
