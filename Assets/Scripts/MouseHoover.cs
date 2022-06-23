using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHoover : MonoBehaviour
{

    // to hide the depracated base class renderer tomkil
    static new Renderer renderer;
        
    // Start is called before the first frame update
    void Start()
    {
        Renderer theRenderer = GetComponent<Renderer>();
        renderer = theRenderer;
        renderer.material.color = Color.black;
    }


    private void OnMouseEnter()
    {
        renderer.material.color = Color.red;
    }

    private void OnMouseExit()
    {
        renderer.material.color = Color.black;
    }

}
