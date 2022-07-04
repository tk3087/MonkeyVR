using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Cylinder : MonoBehaviour
{

    private void Awake()
    {
        Debug.Log("Cylinder is awake");
        DelayIt(300, 12000);

    }

    // Start is called before the first frame update

    void Start()
    {
        DelayIt(301, 6000);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void DelayIt(int id, int time)
    {

        var t = Task.Run(async delegate
        {
            await Task.Delay(time);
            return id;
        });
        t.Wait();
        Debug.Log("DELAY>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Status:" + t.Status.ToString() + " Result: " + t.Result.ToString());
    }
}