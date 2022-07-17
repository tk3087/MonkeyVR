using UnityEngine;




public class CollisionHandler : MonoBehaviour
{

    public Transform player;
    bool hit_EW = false;
    bool hit_TW = false;
    GameObject theEW;
    GameObject theTW;
    Vector3 camNormal;
    public float dot1;
    public float dot2;
    public bool IN_PROX_ZONE = false;
    float startTimeCollisionOccured;
    // gracePeriod in seconds
    public float gracePeriodToBlockForwardMovement = 3.0f;

    private void OnCollisionEnter(Collision collision)
    {

        if (ArenaGame.theArenaGame.COLLISION_HANDLER_ENABLED == false)
            return;

        if (collision.gameObject.name == "ProxZone")
        {
            // 
            IN_PROX_ZONE = true;

        }


        //Debug.Log("Collision Enter: " + collision.gameObject.name);
        if (collision.gameObject.name == "Rod8T" && hit_TW==false)
        {
            this.hit_TW = true;
            theTW = collision.gameObject;
            startTimeCollisionOccured = Time.time;
            Debug.Log("Collision: Enter Rod8T");

        }
        else if (collision.gameObject.name == "Rod8W" && hit_EW==false)
        {
            this.hit_EW = true;
            theEW = collision.gameObject;
            startTimeCollisionOccured = Time.time;
            Debug.Log("Collision: Enter Rod8W");
        }

    }

    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log("Collision Stay: " + collision.gameObject.name );
        //if (collision.gameObject.name=="Rod8")
        //{
        //    sharedmovement.audioSource.clip = sharedmovement.loseSound;
        //    sharedmovement.audioSource.Play();
        //}
        if (ArenaGame.theArenaGame.COLLISION_HANDLER_ENABLED == false)
            return;


        if (IN_PROX_ZONE==true)
        {
            // IF in ProxZone

            ArenaGame.theArenaGame.FORWARD_MOVEMENT_ON_WALL_COLLISION_ENABLED = true;
            return;
        }

        if (Time.time-startTimeCollisionOccured < gracePeriodToBlockForwardMovement)
        {
            return;
        }

        camNormal = player.forward;

        if (collision.gameObject.name == "Rod8T")
        {
            dot1 = Vector3.Dot(camNormal, theTW.transform.right);
            dot2 = Vector3.Dot(camNormal, theEW.transform.right); // Debug purpose

            if (hit_EW == true)
            {
                //Debug.Log("Calc 2 Dot Products.");
                dot2=Vector3.Dot(camNormal, theEW.transform.right);
                
                
                if (dot1 >0.0f && dot2 > 0.0f)
                {
                    // Enable forward movement
                    ArenaGame.theArenaGame.FORWARD_MOVEMENT_ON_WALL_COLLISION_ENABLED = false;
                    Debug.Log("Col Stay2: Forward Movement DISABLED.");
                }
                else
                {
                    ArenaGame.theArenaGame.FORWARD_MOVEMENT_ON_WALL_COLLISION_ENABLED = true;
                    Debug.Log("Col Stay2: Forward Movement ENABLED.");

                }
                


            }
            else
            {
                // check single
                if (dot1 > 0.0f)
                {
                    // Enable forward movement
                    ArenaGame.theArenaGame.FORWARD_MOVEMENT_ON_WALL_COLLISION_ENABLED = false;
                    Debug.Log("Col Stay1: Forward Movement DISABLED.");
                }
                else
                {
                    ArenaGame.theArenaGame.FORWARD_MOVEMENT_ON_WALL_COLLISION_ENABLED = true;
                    Debug.Log("Col Stay1: Forward Movement ENABLED.");

                }
                
                //Debug.Log("Calc 1 Dot Product.");
            }
            return;
        }
        else if (collision.gameObject.name == "Rod8W")
        {
            dot2 = Vector3.Dot(camNormal, theEW.transform.right);
            dot1 = Vector3.Dot(camNormal, theTW.transform.right); //Debug purpose 
            if (hit_TW == true)
            {
                // check both
                Debug.Log("Calc 2 Dot Products.");

                dot1 = Vector3.Dot(camNormal, theTW.transform.right);

                if (dot1 > 0.0f && dot2 > 0.0f)
                {
                    // Enable forward movement
                    ArenaGame.theArenaGame.FORWARD_MOVEMENT_ON_WALL_COLLISION_ENABLED = false;
                    Debug.Log("Col Stay2: Forward Movement DISABLED.");
                }
                else
                {
                    ArenaGame.theArenaGame.FORWARD_MOVEMENT_ON_WALL_COLLISION_ENABLED = true;
                    Debug.Log("Col Stay2: Forward Movement ENABLED.");

                }
                
            }

            else
            {
                // check single
                if (dot2 > 0.0f)
                {
                    // Enable forward movement
                    ArenaGame.theArenaGame.FORWARD_MOVEMENT_ON_WALL_COLLISION_ENABLED = false;
                    Debug.Log("Col Stay1: Forward Movement DISABLED.");
                }
                else
                {
                    ArenaGame.theArenaGame.FORWARD_MOVEMENT_ON_WALL_COLLISION_ENABLED = true;
                    Debug.Log("Col Stay1: Forward Movement ENABLED.");

                }
                
            }
            return;

        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (ArenaGame.theArenaGame.COLLISION_HANDLER_ENABLED == false)
            return;

        Debug.Log("CollisionExit: " + collision.gameObject.name);

        if (collision.gameObject.name == "ProxZone")
        {
            // 
            IN_PROX_ZONE = false;
        }


        if (collision.gameObject.name == "Rod8T" && hit_TW == true)
        {
            this.hit_TW = false;
            
        }
        else if (collision.gameObject.name == "Rod8W" && hit_EW == true)
        {
            this.hit_EW = false;
            
        }

    }
}
