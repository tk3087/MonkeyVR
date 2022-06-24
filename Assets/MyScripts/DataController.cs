#undef USING_NAN 
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Monkey2D;
using static JoystickMonke;
using System.IO;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using static PlayerMovement;
using static ArenaGame;
#endif

namespace PupilLabs
{
    [DisallowMultipleComponent]
    public class DataController : MonoBehaviour
    {
        [Header("Scene References")]
        public Transform gazeOrigin;
        public GazeController gazeController;
        public GazeVisualizer gazeVisualizer;
        public CalibrationController calibrationController;
        public TimeSync timeSync;
        public static DataController dataController;

        [Header("Settings")]
        [Range(0f, 1f)]
        public float confidenceThreshold = 0.0f;
        public bool binocularOnly = false;

        GameObject firefly;
        GameObject player;
        public GazeData gazeDataNow;
        GazeData gazeNull;
        Dictionary<string, object> nullGaze = new Dictionary<string, object>();
        StringBuilder sb = new StringBuilder();
        object[] zero2d = new object[2] { 0.0, 0.0 };
        object[] zero3d = new object[3] { 0.0, 0.0, 0.0 };
        bool flagFFSceneLoaded = false;
        bool flagPlaying = false;
        bool flagMultiFF = false;
        bool flagRecording = false;
        double timeProgStart = 0.0f;
        [HideInInspector] public Vector3 gazeDirMod;
        [HideInInspector] public float xScale = 1f;
        [HideInInspector] public float yScale = 1f;
        [HideInInspector] public float xOffset = 0f;
        [HideInInspector] public float yOffset = 0f;

        [HideInInspector]
        public string sbPacket;

        [HideInInspector]
        byte rewardGiven_box1, rewardGiven_box2, rewardGiven_box3;
        //float timeRewardGiven_box1, timeRewardGiven_box2, timeRewardGiven_box3;
        //float timeToReward_box1, timeToReward_box2, timeToReward_box3;

        void OnEnable()
        {
            dataController = this;
            sb.Clear();
            if (PlayerPrefs.GetInt("ArenaMode") == 1)
            {

                sb.Append("time_stamp,pos_x,pos_y,pos_z,rot_x,rot_y,rot_z," +
                "b1_press,b1_rew,b1_time,b1_time_next_rew,b2_press,b2_rew,b2_time,b2_time_next_rew,b3_press,b3_rew,b3_time,b3_time_next_rew," +
                "Mapping_Context,Confidence,Target_Index, Mode, GazeX, GazeY, GazeZ, Gaze_Distance, CenterRX, CenterRY, CenterRZ, CenterLX, CenterLY, CenterLZ," +
                "NormRX, NormRY, NormRZ, NormLX, NormLY, NormLZ, Marker\n");
            
            }


            else
            {
                sb.Append("Trial,Status,Timestamp,Mapping Context,Confidence,Target Index,Mode,GazeX,GazeY,GazeZ,Gaze Distance,CenterRX,CenterRY,CenterRZ,CenterLX,CenterLY,CenterLZ,NormRX,NormRY,NormRZ,NormLX,NormLY,NormLZ,Marker," + PlayerPrefs.GetString("Name") + ", " + PlayerPrefs.GetString("Date") + ", " + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
            }
            
                nullGaze.Add("confidence", 0.0);
                nullGaze.Add("norm_pos", zero2d);
                nullGaze.Add("timestamp", 0.0);
                nullGaze.Add("gaze_normals_3d", new Dictionary<object, object>() { { "0", zero3d }, { "1", zero3d } });
                nullGaze.Add("eye_centers_3d", new Dictionary<object, object>() { { "0", zero3d }, { "1", zero3d } });
                nullGaze.Add("gaze_point_3d", zero3d);

                gazeNull = new GazeData("gaze.3d.01.", nullGaze);

                gazeDataNow = gazeNull;

                gazeController.OnReceive3dGaze += ReceiveGaze;

                DontDestroyOnLoad(this);


            SceneManager.activeSceneChanged += ChangedActiveScene;       

            
        }

        private void OnDisable()
        { 
            gazeController.OnReceive3dGaze -= ReceiveGaze;
        }

        async void FixedUpdate()
        {
            //print(sb.Length);
            //if (!calibrationController.subsCtrl.IsConnected)
            //{
            //    Debug.LogError("Pupil Disconnected. Check connection and try again.");
            //    SceneManager.LoadScene("MainMenu");
            //}
            if (PlayerPrefs.GetInt("ArenaMode") == 1)
            {
                if (Time.fixedTime < Time.fixedDeltaTime)
                {
                    await new WaitForSeconds(Time.fixedDeltaTime - Time.fixedTime);
                }

                // Do capture for arena
                if (theArenaGame!=null && flagPlaying && !calibrationController.IsCalibrating && calibrationController.subsCtrl.IsConnected)
                {
                    
                    if (theArenaGame.boxTVCntrl1.RewardLatch == TVcontrol.RewardState.Given)
                    {
                        rewardGiven_box1 = 1;
                        //            ResetLatch
                        theArenaGame.boxTVCntrl1.RewardLatch = TVcontrol.RewardState.NotGiven;
                    }
                    else
                    {
                        rewardGiven_box1 = 0;
                    }

                    if(theArenaGame.boxTVCntrl2.RewardLatch == TVcontrol.RewardState.Given)
                    {
                        rewardGiven_box2 = 1;
                        //            ResetLatch
                        theArenaGame.boxTVCntrl2.RewardLatch = TVcontrol.RewardState.NotGiven;
                    }
                    else
                    {
                        rewardGiven_box2 = 0;
                    }
                    if(theArenaGame.boxTVCntrl3.RewardLatch == TVcontrol.RewardState.Given)
                    {
                        rewardGiven_box3 = 1;
                        //            ResetLatch
                        theArenaGame.boxTVCntrl3.RewardLatch = TVcontrol.RewardState.NotGiven;
                    }
                    else
                    {
                        rewardGiven_box3 = 0;
                    }

                    sbPacket = string.Format("{0},{1},{2},{3},{4},{5},{6}," +
                                "{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}," +
                                "{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32}\n",
                                (double)Time.realtimeSinceStartup,
                                sharedmovement.transform.position.x,
                                sharedmovement.transform.position.y,
                                sharedmovement.transform.position.z,
                                sharedmovement.transform.localRotation.x,
                                sharedmovement.transform.rotation.eulerAngles.y > 180 ? sharedmovement.transform.rotation.eulerAngles.y - 360 : sharedmovement.transform.rotation.eulerAngles.y,
                                sharedmovement.transform.localRotation.z,
                                theArenaGame.boxTVCntrl1.timePointButtonPressed,
                                rewardGiven_box1,
                                theArenaGame.boxTVCntrl1.timePointButtonPressed,
                                theArenaGame.boxTVCntrl1.timeToReward,
                                theArenaGame.boxTVCntrl2.timePointButtonPressed,
                                rewardGiven_box2,
                                theArenaGame.boxTVCntrl2.timePointButtonPressed,
                                theArenaGame.boxTVCntrl2.timeToReward,
                                theArenaGame.boxTVCntrl3.timePointButtonPressed,
                                rewardGiven_box3,
                                theArenaGame.boxTVCntrl3.timePointButtonPressed,
                                theArenaGame.boxTVCntrl3.timeToReward,
                                gazeDataNow.MappingContext,
                                gazeDataNow.Confidence,
                                calibrationController.targetIdx,
                                calibrationController.mode,
                                gazeVisualizer.projectionMarker.position.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.GazeDistance,
                                gazeDataNow.EyeCenter0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.EyeCenter1.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.GazeNormal0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.GazeNormal1.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                calibrationController.LastMarker,
                                calibrationController.status);

                    sb.Append(sbPacket);
                }
            }
            else
            {
                if (Time.fixedTime < Time.fixedDeltaTime)
                {
                    await new WaitForSeconds(Time.fixedDeltaTime - Time.fixedTime);
                }

                if (flagPlaying && !calibrationController.IsCalibrating && calibrationController.subsCtrl.IsConnected)
                {
                    if (!flagFFSceneLoaded)
                    {
#if USING_NAN
                    if (gazeDataNow != gazeNull)
                    {
                        sb.Append(string.Format("{0},{1, 4:F9},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}\n",
                            calibrationController.status,
                            (double)Time.realtimeSinceStartup,
                            gazeDataNow.MappingContext,
                            gazeDataNow.Confidence,
                            calibrationController.targetIdx,
                            calibrationController.mode,
                            gazeDataNow.GazeDirection.ToString("F5").Trim('(', ')').Replace(" ", ""),
                            gazeDataNow.GazeDistance,
                            gazeDataNow.EyeCenter0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                            gazeDataNow.EyeCenter1.ToString("F5").Trim('(', ')').Replace(" ", ""),
                            gazeDataNow.GazeNormal0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                            gazeDataNow.GazeNormal1.ToString("F5").Trim('(', ')').Replace(" ", "")));
                    }
                    else
                    {
                        // "NaN"
                        sb.Append(string.Format("{0},{1, 4:F9},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}\n",
                            calibrationController.status,
                            (double)Time.realtimeSinceStartup,
                            "None",
                            "Nan",
                            calibrationController.targetIdx,
                            calibrationController.mode,
                            "NaN, NaN, NaN",
                            "Nan",
                            "NaN, NaN, NaN",
                            "NaN, NaN, NaN",
                            "NaN, NaN, NaN",
                            "NaN, NaN, NaN"));
                    }
#else
                        sbPacket = string.Format("{0},{1},{2, 4:F9},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}\n",
                                calibrationController.trialNum,
                                calibrationController.status,
                                (double)Time.realtimeSinceStartup,
                                gazeDataNow.MappingContext,
                                gazeDataNow.Confidence,
                                calibrationController.targetIdx,
                                calibrationController.mode,
                                gazeVisualizer.projectionMarker.position.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.GazeDistance,
                                gazeDataNow.EyeCenter0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.EyeCenter1.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.GazeNormal0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.GazeNormal1.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                calibrationController.LastMarker);
                        sb.Append(sbPacket);
#endif
                    }
                    else
                    {
                        if (timeProgStart == 0.0)
                        {
                            timeProgStart = (double)SharedMonkey.programT0;
                        }

                        var trial = SharedMonkey.trialNum;
                        var epoch = (int)SharedMonkey.currPhase;
                        var onoff = firefly.activeInHierarchy ? 1 : 0;
                        var position = player.transform.position.ToString("F5").Trim('(', ')').Replace(" ", "");
                        var rotation = player.transform.rotation.ToString("F5").Trim('(', ')').Replace(" ", "");
                        var linear = SharedJoystick.currentSpeed;
                        var angular = SharedJoystick.currentRot;
                        var FFlinear = SharedMonkey.velocity;
                        var FFposition = string.Empty;
                        var VKsi = SharedJoystick.velKsi;
                        var VEta = SharedJoystick.velEta;
                        var RKsi = SharedJoystick.rotKsi;
                        var REta = SharedJoystick.rotEta;
                        var PTBLV = SharedJoystick.currentSpeed;
                        var PTBRV = SharedJoystick.currentRot;
                        var RawX = SharedJoystick.rawX;
                        var RawY = SharedJoystick.rawY;
                        var CleanLV = SharedJoystick.cleanVel;
                        var CleanRV = SharedJoystick.cleanRot;
                        if (flagMultiFF)
                        {
                            foreach (Vector3 pos in SharedMonkey.ffPositions)
                            {
                                FFposition = string.Concat(FFposition, ",", pos.ToString("F5").Trim('(', ')').Replace(" ", "")).Substring(1);
                            }
                        }
                        else
                        {
                            FFposition = firefly.transform.position.ToString("F5").Trim('(', ')').Replace(" ", "");
                        }
#if USING_NAN
                    if (gazeDataNow != gazeNull)
                    {
                        sb.Append(string.Format("{0},{1, 4:F9},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}\n",
                            trial,
                            (double)Time.realtimeSinceStartup - timeProgStart,
                            epoch,
                            onoff,
                            position,
                            rotation,
                            linear,
                            angular,
                            FFposition,
                            FFlinear,
                            gazeDataNow.MappingContext,
                            gazeDataNow.Confidence,
                            gazeDataNow.GazeDirection.ToString("F5").Trim('(', ')').Replace(" ", ""),
                            gazeDataNow.GazeDistance,
                            gazeDataNow.EyeCenter0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                            gazeDataNow.EyeCenter1.ToString("F5").Trim('(', ')').Replace(" ", ""),
                            gazeDataNow.GazeNormal0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                            gazeDataNow.GazeNormal1.ToString("F5").Trim('(', ')').Replace(" ", "")));
                    }
                    else
                    {
                        //"NaN"
                        sb.Append(string.Format("{0},{1, 4:F9},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}\n",
                            trial,
                            (double)Time.realtimeSinceStartup - timeProgStart,
                            epoch,
                            onoff,
                            position,
                            rotation,
                            linear,
                            angular,
                            FFposition,
                            FFlinear,
                            "None",
                            "Nan",
                            "NaN, NaN, NaN",
                            "Nan",
                            "NaN, NaN, NaN",
                            "NaN, NaN, NaN",
                            "NaN, NaN, NaN",
                            "NaN, NaN, NaN"));
                    }
#else
                        if (SharedJoystick.ptb)
                        {
                            flagRecording = true;
                            sbPacket = string.Format("{0},{1, 4:F9},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25}\n",
                                trial,
                                (double)Time.realtimeSinceStartup - timeProgStart,
                                epoch,
                                onoff,
                                position,
                                rotation,
                                FFposition,
                                FFlinear,
                                gazeDataNow.MappingContext,
                                gazeDataNow.Confidence,
                                gazeVisualizer.projectionMarker.position.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.GazeDistance,
                                gazeDataNow.EyeCenter0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.EyeCenter1.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.GazeNormal0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                gazeDataNow.GazeNormal1.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                VKsi,
                                VEta,
                                RKsi,
                                REta,
                                PTBLV,
                                PTBRV,
                                CleanLV,
                                CleanRV,
                                RawX,
                                RawY);
                            sb.Append(sbPacket);
                        }
                        else
                        {
                            flagRecording = true;
                            sbPacket = string.Format("{0},{1, 4:F9},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}\n",
                                    trial,
                                    (double)Time.realtimeSinceStartup - timeProgStart,
                                    epoch,
                                    onoff,
                                    position,
                                    rotation,
                                    linear,
                                    angular,
                                    FFposition,
                                    FFlinear,
                                    gazeDataNow.MappingContext,
                                    gazeDataNow.Confidence,
                                    gazeVisualizer.projectionMarker.position.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                    gazeDataNow.GazeDistance,
                                    gazeDataNow.EyeCenter0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                    gazeDataNow.EyeCenter1.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                    gazeDataNow.GazeNormal0.ToString("F5").Trim('(', ')').Replace(" ", ""),
                                    gazeDataNow.GazeNormal1.ToString("F5").Trim('(', ')').Replace(" ", ""));

                            sb.Append(sbPacket);
                        }

#endif
                    }
#if USING_NAN
                gazeDataNow = gazeNull;
#endif
                }
            }
            
        }
        /// <summary>
        /// Receive Gaze Data from eye tracker and save it to a variable
        /// </summary>
        /// <param name="gazeData"></param>
        async void ReceiveGaze(GazeData gazeData)
        {
            if (binocularOnly && gazeData.MappingContext != GazeData.GazeMappingContext.Binocular) return;

            gazeDataNow = gazeData;

            /// if you need to apply offset and sclaing, just uncomment everything that starts with "//"

            //Vector3 tempGaze = gazeDataNow.GazeDirection;

            //gazeDirMod = new Vector3(tempGaze.x * xScale + xOffset, tempGaze.y * yScale + yOffset, tempGaze.z);

            //if (Physics.SphereCast(gazeOrigin.position, 0.05f, gazeDirMod, out RaycastHit hit, Mathf.Infinity))
            //{
            //    gazeDirMod = hit.point;
            //}

            /// If you need to correct the position of the Gaze Direction in all termsz (x,y,z) then use this:
            //gazeDataNow.GazeDirection = gazeDataNow.GazeDirection * correctionFactor;

            /// Else if its just one direction do this, make correctionFactorX,Y, and/or Z equal to 1 if it doesnt need to be corrected
            //gazeDataNow.GazeDirection = new Vector3(gazeDataNow.GazeDirection.x * correctionFactorX, gazeDataNow.GazeDirection.y * correctionFactorY, gazeDataNow.GazeDirection.z * correctionFactorZ) 

            await new WaitForSeconds(0.0f);
        }

        void ChangedActiveScene(Scene current, Scene next)
        {
            if (next.name == "Monkey2D")
            {
                var path = PlayerPrefs.GetString("Path") + "\\continuous_eye_data_" + PlayerPrefs.GetString("Name") + "_" + DateTime.Today.ToString("MMddyyyy") + "_" + PlayerPrefs.GetInt("Run Number").ToString("D3") + ".txt";
                File.AppendAllText(path, sb.ToString());
                sb.Clear();

                timeProgStart = 0.0f;

                flagPlaying = true;

                flagFFSceneLoaded = !flagFFSceneLoaded;
                gazeOrigin = Camera.main.transform;
                firefly = SharedMonkey.firefly;
                player = SharedMonkey.player;
                flagMultiFF = SharedMonkey.nFF > 1;

                if (flagMultiFF)
                {
                    var str = "";
                    for (int i = 0; i < SharedMonkey.nFF; i++)
                    {
                        str = string.Concat(str, string.Format("FFX{0},FFY{0},FFZ{0},", i));
                    }
                    sb.Append(string.Format("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,Linear Velocity,Angular Velocity,{0}FFV,MappingContext,Confidence,GazeX,GazeY,GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ,", str) + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
                }
                else
                {
                    if ((int)PlayerPrefs.GetFloat("PTBType") == 2)
                    {
                        sb.Append("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,Linear Velocity,Angular Velocity,FFX,FFY,FFZ,FFV,MappingContext,Confidence,GazeX,GazeY,GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ," + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
                    }
                    else
                    {
                        sb.Append("Trial,Time,Phase,FF On/Off,MonkeyX,MonkeyY,MonkeyZ,MonkeyRX,MonkeyRY,MonkeyRZ,MonkeyRW,FFX,FFY,FFZ,FFV,MappingContext,Confidence,GazeX,GazeY,GazeZ,GazeDistance,RCenterX,RCenterY,RCenterZ,LCenterX,LCenterY,LCenterZ,RNormalX,RNormalY,RNormalZ,LNormalX,LNormalY,LNormalZ,VKsi,Veta,RotKsi,RotEta,PTBLV,PTBRV,CleanLV,CleanRV,RawX,RawY," + PlayerPrefs.GetString("Name") + "," + PlayerPrefs.GetString("Date") + "," + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");
                    }
                }

                timeSync.UpdateTimeSync();
            }
            else if (next.name == "MainMenu" && flagRecording)
            {
                flagPlaying = false;

                print("saving");

                if (PlayerPrefs.GetInt("ArenaMode")==1)
                {

                    string path, pathname_cont, pathname_param, recordLine, subjName;


                    string Header, formatStr;

                    subjName = PlayerPrefs.GetString("SubjectName");

                    if (subjName.Length == 0)
                        subjName = "Subject0_";
                    else
                        subjName += "_";

                    path = PlayerPrefs.GetString("DataAndConfigPath");
                    //string almostUniqueFileName = $@"{DateTime.Now.Ticks}"; // maybe put a date om front plus ticks?
                    //pathname_cont = path + "\\" + subjName + "FMA_" + almostUniqueFileName+ "_cont.txt";
                    //pathname_param = path + "\\" + subjName + "FMA_" + almostUniqueFileName + "_param.txt";
                    pathname_cont = path + "\\" + subjName + "_cont.txt";
                    pathname_param = path + "\\" + subjName + "_param.txt";

                    File.AppendAllText(pathname_cont, sb.ToString());
                    sb.Clear();

                    //Save parameters
                    Header = "CorrectPush,WrongPush,RewardRadius,BoxMean1,BoxMean2,BoxMean3,BoxNoise1,BoxNoise2,BoxNoise3,TrialDuration,DataAndCofigPath,LogEyeTracker,ColoredBoxes,MovementMultiplier\n";

                    formatStr = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}\n";

                    File.AppendAllText(pathname_param, Header);
                    recordLine = string.Format(formatStr,
                        PlayerPrefs.GetFloat("CorrectPush").ToString(),
                        PlayerPrefs.GetFloat("WrongPush").ToString(),
                        PlayerPrefs.GetFloat("RewardRadius").ToString(),
                        PlayerPrefs.GetFloat("BoxMean1").ToString(),
                        PlayerPrefs.GetFloat("BoxMean2").ToString(),
                        PlayerPrefs.GetFloat("BoxMean3").ToString(),
                        PlayerPrefs.GetFloat("BoxNoise1").ToString(),
                        PlayerPrefs.GetFloat("BoxNoise2").ToString(),
                        PlayerPrefs.GetFloat("BoxNoise3").ToString(),
                        PlayerPrefs.GetFloat("TrialDuration").ToString(),
                        LOG_EYE_TRACKER == true ? "1" : "0",
                        COLOR_SCREENS_ON_BOXES == true ? "1" : "0",
                        PlayerPrefs.GetFloat("MovementMultiplier").ToString()

                         );
                    File.AppendAllText(pathname_param, recordLine);



                }
                else
                {
                    var path = PlayerPrefs.GetString("Path") + "\\continuous_data_" + PlayerPrefs.GetString("Name") + "_" + DateTime.Today.ToString("MMddyyyy") + "_" + PlayerPrefs.GetInt("Run Number").ToString("D3") + ".txt";
                    File.AppendAllText(path, sb.ToString());
                    sb.Clear();

                    var num = PlayerPrefs.GetInt("Run Number") + 1;
                    PlayerPrefs.SetInt("Run Number", num);
                }
                

                Destroy(this);

                flagRecording = false;

            }
            else if (next.name == "MonkeyGaze")
            {
                flagPlaying = true;

                gazeOrigin = Camera.main.transform;

                timeSync.UpdateTimeSync();
            }
            else if (next.name == "Arena")
            {
                flagPlaying = true;
                //flagRecording = true;
                gazeOrigin = Camera.main.transform;
                timeSync.UpdateTimeSync();
            }
        }

        private void OnApplicationQuit()
        {
            // TODO: save data on quit
        }
    }
}
