using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolyLine : MonoBehaviour
{
    public int sides;
    public float radius;
    public LineRenderer polyLineRenderer;
    float x, z, x_Box, z_Box;
    public float y;
    GameObject theBox;
    Transform transformBox;
    Renderer renderer;
    Vector3 theBoxCenter;
    public float angle;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Box Object
        theBox = this.gameObject;
        //renderer = GetComponentInParent<Renderer>();
        

        // Get it's transform
        transformBox = theBox.GetComponent<Transform>();

        //theBoxCenter = renderer.bounds.center;



        // Get the position needed to place the line
        x_Box = transformBox.position.x;
        z_Box = transformBox.position.z;
        Matrix4x4 mat = transformBox.localToWorldMatrix;
        
        angle = 270 - transformBox.eulerAngles.y ; // get euler angles of parent object tomkil TV1,TV2, Tv3 are 270,150,30 resp. 
       

        sides = 18;
        radius = 0.8f;
        y = 0.01f;
        polyLineRenderer.startWidth=0.05f;
        polyLineRenderer.endWidth=0.05f;
        
        //polyLineRenderer.SetColors()

        
    }

    // Update is called once per frame
    void Update()
    {
        DrawLoop();
    }
    void DrawLoop()
    {
        polyLineRenderer.positionCount = sides+1;
        float tau = Mathf.PI;
        float currentRadian;
        float phase = angle* Mathf.PI/180;


        for(int currentPoint=0; currentPoint < sides; currentPoint++)
        {
            currentRadian =  ((float)currentPoint / sides) * tau + phase;
            x = x_Box + radius * Mathf.Cos(currentRadian);
            z = z_Box + radius * Mathf.Sin(currentRadian);
            polyLineRenderer.SetPosition(currentPoint, new Vector3(x, y, z));

        }
        currentRadian =  tau + phase;
        x = x_Box + radius * Mathf.Cos(currentRadian);
        z = z_Box + radius * Mathf.Sin(currentRadian);
        polyLineRenderer.SetPosition(sides, new Vector3(x, y, z));

        //polyLineRenderer.loop = true;    
    }

    void DrawOverLapped()
    {

    }

}
