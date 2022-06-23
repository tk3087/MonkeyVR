using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    public GameObject top;
    public GameObject piston1;
    public GameObject cylinder1;
    public GameObject base1;
    public GameObject piston2;
    public GameObject cylinder2;
    public GameObject base2;
    public GameObject piston3;
    public GameObject cylinder3;
    public GameObject base3;
    public GameObject piston4;
    public GameObject cylinder4;
    public GameObject base4;
    public GameObject piston5;
    public GameObject cylinder5;
    public GameObject base5;
    public GameObject piston6;
    public GameObject cylinder6;
    public GameObject base6;

    public GameObject shaft1;
    public GameObject shaft2;
    public GameObject shaft3;
    public GameObject shaft4;
    public GameObject shaft5;
    public GameObject shaft6;

    private List<GameObject> pistons = new List<GameObject>();
    private List<GameObject> cylinders = new List<GameObject>();
    private List<GameObject> bases = new List<GameObject>();
    private List<GameObject> shafts = new List<GameObject>();

    private float distance;

    private int step = 0;
    private int limit = 150;
    private string key = "w";
    private int iter = 1;

    // Start is called before the first frame update
    void Start()
    {
        pistons.Add(piston1);
        pistons.Add(piston2);
        pistons.Add(piston3);
        pistons.Add(piston4);
        pistons.Add(piston5);
        pistons.Add(piston6);

        cylinders.Add(cylinder1);
        cylinders.Add(cylinder2);
        cylinders.Add(cylinder3);
        cylinders.Add(cylinder4);
        cylinders.Add(cylinder5);
        cylinders.Add(cylinder6);

        bases.Add(base1);
        bases.Add(base2);
        bases.Add(base3);
        bases.Add(base4);
        bases.Add(base5);
        bases.Add(base6);

        shafts.Add(shaft1);
        shafts.Add(shaft2);
        shafts.Add(shaft3);
        shafts.Add(shaft4);
        shafts.Add(shaft5);
        shafts.Add(shaft6);

        for (int i = 0; i < pistons.Count; i++)
        {
            cylinders[i].transform.LookAt(pistons[i].transform, pistons[i].transform.up);
            cylinders[i].transform.Rotate(90.0f, 0.0f, 0.0f);
            cylinders[i].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
        }

        distance = Vector3.Distance(shaft1.transform.position, piston1.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        switch (key)
        {
            case "w":
                top.transform.position += Vector3.forward * 0.001f;
                for (int i = 0; i < pistons.Count; i++)
                {
                    pistons[i].transform.position += Vector3.forward * 0.001f;
                    cylinders[i].transform.LookAt(pistons[i].transform, pistons[i].transform.up);
                    cylinders[i].transform.Rotate(90.0f, 0.0f, 0.0f);
                    cylinders[i].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                }
                break;
            case "s":
                top.transform.position -= Vector3.forward * 0.001f;
                for (int i = 0; i < pistons.Count; i++)
                {
                    pistons[i].transform.position -= Vector3.forward * 0.001f;
                    cylinders[i].transform.LookAt(pistons[i].transform, pistons[i].transform.up);
                    cylinders[i].transform.Rotate(90.0f, 0.0f, 0.0f);
                    cylinders[i].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                }
                break;
            case "a":
                top.transform.position -= Vector3.right * 0.001f;
                for (int i = 0; i < pistons.Count; i++)
                {
                    pistons[i].transform.position -= Vector3.right * 0.001f;
                    cylinders[i].transform.LookAt(pistons[i].transform, pistons[i].transform.up);
                    cylinders[i].transform.Rotate(90.0f, 0.0f, 0.0f);
                    cylinders[i].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                }
                break;
            case "d":
                top.transform.position += Vector3.right * 0.001f;
                for (int i = 0; i < pistons.Count; i++)
                {
                    pistons[i].transform.position += Vector3.right * 0.001f;
                    cylinders[i].transform.LookAt(pistons[i].transform, pistons[i].transform.up);
                    cylinders[i].transform.Rotate(90.0f, 0.0f, 0.0f);
                    cylinders[i].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                }
                break;
        }
        step++;
        if (iter >= 3)
        {
            limit = 300;
        }
        if (step == limit)
        {
            switch (key)
            {
                case "w":
                    key = "d";
                    break;
                case "d":
                    key = "s";
                    break;
                case "s":
                    key = "a";
                    break;
                case "a":
                    key = "w";
                    break;
            }
            iter++;
            step = 0;
        }
        //if (Input.GetKey(KeyCode.W))
        //{
        //    top.transform.position += Vector3.forward * 0.005f;
        //    for (int i = 0; i < pistons.Count; i++)
        //    {
        //        pistons[i].transform.position += Vector3.forward * 0.005f;
        //        cylinders[i].transform.LookAt(pistons[i].transform, pistons[i].transform.up);
        //        cylinders[i].transform.Rotate(90.0f, 0.0f, 0.0f);
        //        cylinders[i].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
        //    }
        //}
        //else if (Input.GetKey(KeyCode.S))
        //{
        //    top.transform.position -= Vector3.forward * 0.005f;
        //    for (int i = 0; i < pistons.Count; i++)
        //    {
        //        pistons[i].transform.position -= Vector3.forward * 0.005f;
        //        cylinders[i].transform.LookAt(pistons[i].transform, pistons[i].transform.up);
        //        cylinders[i].transform.Rotate(90.0f, 0.0f, 0.0f);
        //        cylinders[i].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
        //    }
        //}
        //else if (Input.GetKey(KeyCode.A))
        //{
        //    top.transform.position -= Vector3.right * 0.005f;
        //    for (int i = 0; i < pistons.Count; i++)
        //    {
        //        pistons[i].transform.position -= Vector3.right * 0.005f;
        //        cylinders[i].transform.LookAt(pistons[i].transform, pistons[i].transform.up);
        //        cylinders[i].transform.Rotate(90.0f, 0.0f, 0.0f);
        //        cylinders[i].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
        //    }
        //}
        //else if (Input.GetKey(KeyCode.D))
        //{
        //    top.transform.position += Vector3.right * 0.005f;
        //    for (int i = 0; i < pistons.Count; i++)
        //    {
        //        pistons[i].transform.position += Vector3.right * 0.005f;
        //        cylinders[i].transform.LookAt(pistons[i].transform, pistons[i].transform.up);
        //        cylinders[i].transform.Rotate(90.0f, 0.0f, 0.0f);
        //        cylinders[i].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
        //    }
        //}
        for (int i = 0; i < shafts.Count; i++)
        {
            //shafts[i].transform.position += Vector3.up * 0.4540501f; 0.4893375
            shafts[i].transform.position = (shafts[i].transform.position - pistons[i].transform.position).normalized * distance + pistons[i].transform.position;
        }
    }
}
