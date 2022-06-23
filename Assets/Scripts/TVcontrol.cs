//#define T_DEBUG

//Script to handle screen reward
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using FFTW.NET;
using Foraging3Screens;

//using UE = UnityEngine;


public class TVcontrol : MonoBehaviour
{

    //AudioSource audioSource;
    //int audioTimeIndex = 0;
    //public float rewardFrequency = 440f;
    //public float noreawardFrequency = 220f;
    //float Frequency;
    
    //float sampleRate = AudioSettings.outputSampleRate;

    //public float waveLenInSecs = 1.5f;


    public float press;
    public Transform player;
    public float RewardRadius = 1;
    //public float BoxMean1 = 10;
    //public float BoxMean2 = 10;
    //public float BoxMean3 = 10;

    public float rewardInSec;
    public float timeToReward = 0;
    //public float timeButtonPressed = 0;

    public enum RewardState
    { Given,NotGiven}
    
    public RewardState RewardLatch = RewardState.NotGiven;
    public float timePointButtonPressed;

    public enum ButtonState
    { Pressed,Released}


    public ButtonState BoxButton = ButtonState.Released;
    
    [ShowOnly]
    public float CountDown;
    
    
    System.Timers.Timer LeTimer;

    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();


    const int E_STIM_WIDTH_PIX = 64;
    const int FFT_AMP_SCALE = E_STIM_WIDTH_PIX * E_STIM_WIDTH_PIX;
    const double DOUBLE_PI = Math.PI * 2;
    

    // Start is called before the first frame update

    //tomkil
    private Texture2D tex2D;
    private byte[] mem = new byte[E_STIM_WIDTH_PIX * E_STIM_WIDTH_PIX * 3];
    private System.Random rndGen = new System.Random(Guid.NewGuid().GetHashCode());

    private System.Random rndGenD = new System.Random(Guid.NewGuid().GetHashCode());


    // tomkil test

    private string numStr;


    public int screenID;

    
    float lastClick=0;

    int posRewardPoints;
    int negRewardPoints;

    // Distance to Plane - Screen
    public float dist;

    UnityEngine.Vector3 planesNormal;
    UnityEngine.Vector3 camNormal;
    float dotProd;
    MeshRenderer meshRenderer;
    Color nColor;
    public Color actColor;
    


    //---------------------------------
    //FFTW 



    AlignedArrayComplex input = new AlignedArrayComplex(16, E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX);
    //AlignedArrayComplex output = new AlignedArrayComplex(16, input.GetSize());
    AlignedArrayComplex output = new AlignedArrayComplex(16, E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX);
    AlignedArrayComplex temp = new AlignedArrayComplex(16, E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX);

    //FftwPlanC2C fft = FftwPlanC2C.Create(timeDomain, frequencyDomain, DftDirection.Forwards);
    FftwPlanC2C ifft;
    //---------------------------------
    //  DEBUG to remove
    
    Complex[,] SH_input = new Complex[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];
    Complex[,] SH_output = new Complex[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];
    Complex[,] SH_temp = new Complex[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];


    //------
    double[,] spect_u = new double[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];
    double[,] spect_v = new double[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];
    double[,] S_f = new double[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];
    //tomkil SS_f is the sqrt of S_f
    double[,] SS_f = new double[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];

    double[,] k = new double[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];
    double[,] invk = new double[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];

    //tomkil invk_S is the sqrt(1 - invk * invk)
    double[,] invk_S = new double[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];


    public double colorVal;
    double[,] Xgray = new double[E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX];


    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip neutralSound;
    public AudioClip loseSound;

    
    
    void Awake()
    {

        //audioSource = gameObject.GetComponent<AudioSource>();
        
        //audioSource.spatialBlend = 0; // 2D sound
        //audioSource.Stop(); // Silent Audio Source
        
        
        //Set the plan for FFTW
        ifft = FftwPlanC2C.Create(input, output, DftDirection.Backwards);

        
        //Gen initial spectrum
        int HalfWidth;
        int i, j;
        double beta = -2;

        HalfWidth = E_STIM_WIDTH_PIX / 2;

        for (j = 0; j < E_STIM_WIDTH_PIX/2; j++)
        {
            //ATTN spect_u 1st col calc
            spect_u[j, 0] = (double)j / E_STIM_WIDTH_PIX;

            // ATTN spect_v 1st line calc
            spect_v[0, j] = (double)j / E_STIM_WIDTH_PIX;

        }
        
        spect_u[HalfWidth, 0] = (double)j / E_STIM_WIDTH_PIX;
        spect_v[0, HalfWidth] = (double)j / E_STIM_WIDTH_PIX;

        for (j = 1; j <HalfWidth; j++)
        {
            //ATTN spect_u 1st col calc
            spect_u[HalfWidth+j, 0] = (double)(-HalfWidth + j) / E_STIM_WIDTH_PIX;

            
            // ATTN spect_v 1st line calc
            
            spect_v[0, HalfWidth + j] = (double)(-HalfWidth + j) / E_STIM_WIDTH_PIX;

        }


        for (j = 1; j < E_STIM_WIDTH_PIX; j++)
            for (i = 0; i < E_STIM_WIDTH_PIX; i++)
            {
                spect_u[i, j] = spect_u[i, 0]; // repmat
                spect_v[j, i] = spect_v[0, i]; // repmat
            }

        for (i = 0; i < E_STIM_WIDTH_PIX; i++)
            for (j = 0; j < E_STIM_WIDTH_PIX; j++)
            {

                S_f[i, j] = Math.Pow((spect_u[i, j] * spect_u[i, j] + spect_v[i, j] * spect_v[i, j]), beta/2);

                if (Double.IsInfinity(S_f[i, j])) S_f[i, j] = 0;
                SS_f[i, j] = Math.Sqrt(S_f[i, j]);
            }

        // Generate initial texture
        for (i = 0; i < E_STIM_WIDTH_PIX; i++)
            for (j = 0; j < E_STIM_WIDTH_PIX; j++)
            {
                Complex z = new Complex (Std_Norm(rndGen)*Math.Sqrt(S_f[i,j]), Std_Norm(rndGen) * Math.Sqrt(S_f[i, j]) );
                input[i, j] = z;
            }

        // time freq weighting

        for (i = 0; i < E_STIM_WIDTH_PIX; i++)
            for (j = 0; j < E_STIM_WIDTH_PIX; j++)
            {

                k[i,j] = Math.Sqrt(spect_u[i,j] * spect_u[i,j] + spect_v[i,j] * spect_v[i,j]);
                invk[i,j] = Math.Exp(-k[i,j] * ArenaGame.fg3Sc.stimKappa );
                invk_S[i,j] = Math.Sqrt(1 - invk[i, j] * invk[i, j]);
                if (Double.IsInfinity(invk[i,j])) invk[i,j] = 0;

            }



        actColor.r = 187;
        actColor.g = 239;
        actColor.b = 0;
        actColor.a = 255;

    }







    void Start()
    {




        
        Renderer aRend = GetComponent<Renderer>();
        tex2D = new Texture2D(E_STIM_WIDTH_PIX, E_STIM_WIDTH_PIX, TextureFormat.RGB24, false);
        //tex2D = new Texture2D(16, 16, TextureFormat.RGB24, false);


        //tomkil 
        //RewardRadius = PlayerPrefs.GetFloat("Reward Radius");
        
        
        GameObject screen = this.gameObject;
        var screenRenderer = screen.GetComponent<Renderer>();
        // tomkil screenRenderer.material.SetColor("_Color", Color.blue);
        //tomkil
        //BoxMean1 = PlayerPrefs.GetFloat("BoxMean1");
        //BoxMean2 = PlayerPrefs.GetFloat("BoxMean2");
        //BoxMean3 = PlayerPrefs.GetFloat("BoxMean3");
        CountDown = 0;

        string varNameInPrefs;
        


        //Get myID




        foreach (var c in screen.name)
        {

            if (c >= '0' && c <= '9')
                numStr = string.Concat(numStr, c.ToString());

        }
        screenID = Int32.Parse(numStr)-1; // Use for 0 indexed arrays

        GameObject boxTV;

        boxTV = GameObject.Find("BoxTV" + (screenID+1).ToString() );
        
        
        //Weird way to get to the renderer tomkil...
        meshRenderer = (MeshRenderer) boxTV.transform.Find("Cube").gameObject.GetComponent<Renderer>();
        nColor = meshRenderer.material.GetColor("_Color");
        
        //tomkil
        if (aRend != null)
            Debug.Log("Renderer name is: " + aRend.name);
        else
            Debug.Log("Renderer is NULL!");

        tex2D.LoadRawTextureData(mem);
        //tex2D.LoadRawTextureData(pvrtcBytes1);
        tex2D.Apply();

        aRend.material.mainTexture = tex2D;
        //screenRenderer.material.mainTexture = tex2D;



        // Update Settings
        RewardRadius = PlayerPrefs.GetFloat("Reward Radius");

        varNameInPrefs = "BoxMean" + (screenID + 1).ToString();
        ArenaGame.fg3Sc.tauList[screenID,0] = PlayerPrefs.GetFloat(varNameInPrefs);

        varNameInPrefs = "BoxNoise" + (screenID + 1).ToString();
        ArenaGame.fg3Sc.concentrationList[screenID, 0] = PlayerPrefs.GetFloat(varNameInPrefs);


        posRewardPoints = (int)PlayerPrefs.GetFloat("Correct Push");
        negRewardPoints = (int)PlayerPrefs.GetFloat("Wrong Push");

        // generate interval to activate reward         

        double pp;
        
        
        pp = 1-rndGen.NextDouble();

        //rewardInSec =  (float) (-1 / (ArenaGame.fg3Sc.tauList[screenID, 0]) * Math.Log(pp));
        rewardInSec = (float)(-1*(ArenaGame.fg3Sc.tauList[screenID, 0]) * Math.Log(pp));
        timeToReward = Time.time + rewardInSec;

        var filter = GetComponent<MeshFilter>();
        //UnityEngine.Vector3 normal;
        if (filter && filter.mesh.normals.Length > 0)
            planesNormal = filter.transform.TransformDirection(filter.mesh.normals[0]);
       
        planesNormal *= -1;

    }

    // Update is called once per frame
    void Update()
    {
        GameObject screen = this.gameObject;
        var screenRenderer = screen.GetComponent<Renderer>();
        //tomkil screenRenderer.material.SetColor("_Color", Color.blue);
        //planesNormal = thePlane.normal;

        //camNormal = Camera.current.transform.forward; << causes NullRef Exception
        camNormal = player.forward;
        dotProd = UnityEngine.Vector3.Dot(planesNormal, camNormal); 




        CTIJoystick joystick = CTIJoystick.current;
        dist = UnityEngine.Vector3.Distance(transform.position, player.position);
        
        press = joystick.leftButton.ReadValue();
        

        if (dist < RewardRadius)
        {
            //meshRenderer.material.SetColor("_Color", Color.cyan);
            meshRenderer.material.color = actColor;
        }
        else
        {
            meshRenderer.material.color = nColor;
        }

        
       if (press==1 && dist < RewardRadius && BoxButton==ButtonState.Released && dotProd > 0.707)
        {
            double pp;

            //Button pressed 
            BoxButton = ButtonState.Pressed;
            lastClick = Time.time;
            Debug.Log("[" + lastClick.ToString("F3") + "]  Click at screen" + screenID.ToString());
            //lastClick = Time.time;
            
            if (lastClick>= timeToReward)
            
            {
                

                Debug.Log("[" + lastClick.ToString("F3") + "]  Reward given at screen" + screenID.ToString());
                //Frequency = rewardFrequency;
                //audioSource.Play();
                RewardLatch = RewardState.Given;

                // Add to score
                ArenaGame.theScore += posRewardPoints;

                //tomkil wrong reset of reward interval.
                //pp = 1 - rndGen.NextDouble();

                //rewardInSec = (float)(-1 * (ArenaGame.fg3Sc.tauList[screenID, 0]) * Math.Log(pp));
                //timeToReward = lastClick+ rewardInSec;
                audioSource.clip = winSound;
                audioSource.Play();


            }
            else
            {
                Debug.Log("[" + lastClick.ToString("F3") + "]  NO Reward given at screen" + screenID.ToString());
                //Frequency = noreawardFrequency;

                ArenaGame.theScore += negRewardPoints;

                audioSource.clip = loseSound;
                audioSource.Play();
                
                // ATTN RewardLatch will be cleared by logger once state is read 

            }

            // BTN pressed reset reward interval
            pp = 1 - rndGen.NextDouble();

            rewardInSec = (float)(-1 * (ArenaGame.fg3Sc.tauList[screenID, 0]) * Math.Log(pp));
            timeToReward = lastClick+ rewardInSec;


            timePointButtonPressed = lastClick;

        }
        else if (press==0)
        {
            // Buton Released
            BoxButton = ButtonState.Released;
            //audioSource.Stop();
        }


        //if (CountDown == 0)
        //{
        //    double rnddbl = UnityEngine.Random.value;
        //    float lambda = 1 / BoxMean1;
        //    CountDown = (float)Math.Log(1 - rnddbl) / (-lambda);
        //}
        //else
        //{
        //    CountDown -= Time.deltaTime;
        //    if (CountDown <= 0)
        //    {
        //        //tomkil screenRenderer.material.SetColor("_Color", Color.red);
        //        //tex2D.LoadRawTextureData(pvrtcBytes1);

        //        ;
        //        if (press == 1 && dist < RewardRadius)
        //        {
        //            //tomkil screenRenderer.material.SetColor("_Color", Color.blue);
        //            //tex2D.LoadRawTextureData(pvrtcBytes2);
        //            CountDown = 0;
        //        }
        //    }
        //    tex2D.Apply();
        //}

        //print(CountDown);
        //GenerateTexture();
        float timeNow = Time.time;

        var myTask = Task.Run(() => GeneratetexturedFrame(timeNow));
        //myTask.Wait();
        //if (Time.frameCount % 4 == 0)
        //{
        //    GeneratetexturedFrame();
        //}
        
        //StartCoroutine(GeneratetexturedFrame());
        
    }

    void FixedUpdate()
    {
        
        tex2D.LoadRawTextureData(mem);
        tex2D.Apply();
    }

    void OnDestroy()
    {
        // Dispose resources
        input.Dispose();
        output.Dispose();
        temp.Dispose();

        ifft.Dispose();
    }

    void GenerateTexture()
    {
        
        rndGen.NextBytes(mem);
               


    }

    void GeneratetexturedFrame(float timeNow)
    {
        int i, j;
        double x1, x2, y1, y2, yy,two_pi;
        
        two_pi = 2.0 * Math.PI;

       
        colorVal = 1 - Math.Exp(-(timeNow - lastClick) / ArenaGame.fg3Sc.tauList[screenID, 0]);
        // Compute New Spectrum
        //double a, b;
        //Complex zzz = new Complex(Std_Norm(rndGen),  Std_Norm(rndGen) );

#if T_DEBUG
        stopWatch.Start();
#endif       
        for (i = 0; i < E_STIM_WIDTH_PIX; i++)
            for (j = 0; j < E_STIM_WIDTH_PIX; j++)
            {

                x1 = 1 - rndGen.NextDouble();
                x2 = 1 - rndGen.NextDouble();
                yy = Math.Sqrt(-2.0 * Math.Log(x1));
                y1 = yy* Math.Cos( two_pi* x2);
                y2 = yy* Math.Sin( two_pi* x2);
                //Complex z = new Complex (y1 * SS_f[i, j], y2 * SS_f[i, j]);
                //a = Std_Norm(rndGen) * SS_f[i, j];
                //b = Std_Norm(rndGen) * SS_f[i, j];

                temp[i,j] = new Complex(y1 * SS_f[i, j], y2 * SS_f[i, j]);

                //temp[i, j] = z;
                //SH_temp[i,j] = z;
            }
#if T_DEBUG
        stopWatch.Stop();
        Debug.Log("[" + screenID.ToString() + "] StdNorm " + stopWatch.ElapsedMilliseconds.ToString() + "\n");
        stopWatch.Reset();

        stopWatch.Start();
#endif


        for (i = 0; i < E_STIM_WIDTH_PIX; i++)
            for (j = 0; j < E_STIM_WIDTH_PIX; j++)
            {
                
                input[i, j] = invk[i, j] * input[i, j] +invk_S[i, j] * temp[i, j];
                //SH_input[i, j] = input[i, j];

            }

#if T_DEBUG
        stopWatch.Stop();
        Debug.Log("[" + screenID.ToString() + "] AddCmplx " + stopWatch.ElapsedMilliseconds.ToString() + "\n");
        stopWatch.Reset();
#endif
        //stopWatch.Start();

        ifft.Execute();

        //stopWatch.Stop();
        //UE.Debug.Log("[" + screenID.ToString() + "] IFFT " + stopWatch.ElapsedMilliseconds.ToString() + "\n");
        //stopWatch.Reset();

        // Map Colors
        //ATTN Xgray is outout complex array!!!
#if T_DEBUG
        stopWatch.Start();
#endif

        for (i = 0; i < E_STIM_WIDTH_PIX; i++)
            for (j = 0; j < E_STIM_WIDTH_PIX; j++)
            {
                double XXgray;
                
                
                Complex z1 = new Complex(
                    ArenaGame.fg3Sc.concentrationList[screenID, 0] * Math.Cos(Math.PI * (colorVal-1) / 2),
                    ArenaGame.fg3Sc.concentrationList[screenID, 0] * Math.Sin(Math.PI * (colorVal-1) / 2)
                    );
               

                //Complex z1 = new Complex(1, 0);

                //SH_output[i, j] = output[i, j];

                //output[i, j] /= FFT_AMP_SCALE;

                output[i, j] = output[i, j]/FFT_AMP_SCALE + z1;
                
                

                //SH_output[i, j] = output[i, j];


                XXgray = output[i, j].Phase / DOUBLE_PI;

                XXgray= (XXgray <= 0) ? 1 + XXgray : XXgray;
               
                Xgray[i, j] = XXgray;
                
                //int ii,jj;
                //ii = 0;
                //jj = 0;
                for (int k = 0; k < 3; k++)
                {
                    //ii = 3 * i * E_STIM_WIDTH_PIX + 3 * j + k;
                    //jj = (int)Math.Ceiling((double)XXgray * ArenaGame.fg3Sc.clutpoints);
                    mem[3 * i * E_STIM_WIDTH_PIX + 3 * j + k] =
                        ArenaGame.fg3Sc.Xrgb[(int)Math.Ceiling((double)XXgray * ArenaGame.fg3Sc.clutpointsZIA), k];
                }

            }
#if T_DEBUG  
        stopWatch.Stop();
        Debug.Log("[" + screenID.ToString() + "] ColorMap " + stopWatch.ElapsedMilliseconds.ToString() + "\n");
        stopWatch.Reset();
#endif
        //tex2D.LoadRawTextureData(mem);
        //tex2D.Apply();
    }

    double Std_Norm(System.Random random)
    {
        // The method requires sampling from a uniform random of (0,1]
        // but Random.NextDouble() returns a sample of [0,1).
        double x1 = 1 - random.NextDouble();
        double x2 = 1 - random.NextDouble();

        double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
        // return y1 * stdev + mean;
        return y1; // stdev =1 mean =0
    }

    //void OnAudioFilterRead(float[] data, int channels)
    //{
    //    for (int i = 0; i < data.Length; i += channels)
    //    {
    //        data[i] = CreateSine(audioTimeIndex, Frequency, sampleRate);

    //        if (channels == 2)
    //            data[i + 1] = CreateSine(audioTimeIndex, Frequency, sampleRate);

    //        audioTimeIndex++;

    //        //if timeIndex gets too big, reset it to 0
    //        if (audioTimeIndex >= (sampleRate * waveLenInSecs))
    //        {
    //            audioTimeIndex = 0;
    //        }
    //    }
    //}

    ////Creates a sinewave
    //public float CreateSine(int timeIndex, float frequency, float sampleRate)
    //{
    //    return Mathf.Sin(2 * Mathf.PI * timeIndex * frequency / sampleRate);
    //}



}
