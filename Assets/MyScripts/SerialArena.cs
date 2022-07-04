
using System.IO.Ports;
using UnityEngine;
using UnityEngine.InputSystem;

public class SerialArena : MonoBehaviour
{
    public static SerialArena serialArena { get; private set; }
    public SerialPort sp;
    bool juice = true;
    string inString;
    string[] tokens;
    float juiceTime;
    public bool suctionBtn = false;
    bool inSuction = false;
    

    ulong tNow, tStart, validSuctionDuration;
    char vacSwitch='1';

    // Start is called before the first frame update
    void OnEnable()
    {

        if (serialArena == null)
            serialArena = this;
        else
            Object.Destroy(gameObject);


        if (PlayerPrefs.GetInt("ArenaMode") == 1)
        {

            juiceTime = PlayerPrefs.GetFloat("Max Juice Time");
            sp = new SerialPort("COM5", 1000000);
            sp.Open();
            sp.ReadTimeout = 1;

            validSuctionDuration = (ulong)PlayerPrefs.GetInt("SuctionDuration");
        }
    }
   
   

    // Update is called once per frame
    void FixedUpdate()
    {

        if (PlayerPrefs.GetInt("ArenaMode") == 1)
        {

            var keyboard = Keyboard.current;
            if (keyboard.spaceKey.isPressed && juice) GiveJuice();
        
            inString = sp.ReadLine();
            tokens = inString.Split(',');
        
            tNow = ulong.Parse(tokens[0]);
            vacSwitch = char.Parse(tokens[1]);

            if (vacSwitch == '0')
            {
                if (inSuction == false)
                {
                    tStart = tNow;
                    inSuction = true;
                }

            }
            else //'  vacSwitch == '1'
            {
                if (inSuction == true)
                {



                    if (tNow - tStart > validSuctionDuration)
                        suctionBtn = true; // Suction btn "is pressed" ATTN has to be reset caller!!!


                    inSuction = false;
                }
            }

        }


        

    }

    async public void GiveJuice()
    {
        if (PlayerPrefs.GetInt("ArenaMode") == 1)
        {

            //print(string.Format("juice time = {0}", juiceTime));
            juice = false;
            sp.Write(string.Format("{0}", juiceTime));
            await new WaitForSeconds(juiceTime / 1000.0f);
            juice = true;
        }
    }

    private void OnDisable()
    {
        if (PlayerPrefs.GetInt("ArenaMode") == 1)
        {

            if (sp.IsOpen) sp.Close();
        }
    }
}
