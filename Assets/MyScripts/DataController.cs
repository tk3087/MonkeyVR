﻿#undef USING_NAN 
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
        void OnEnable()
        {
            dataController = this;
            sb.Clear();
            sb.Append("Trial,Status,Timestamp,Mapping Context,Confidence,Target Index,Mode,GazeX,GazeY,GazeZ,Gaze Distance,CenterRX,CenterRY,CenterRZ,CenterLX,CenterLY,CenterLZ,NormRX,NormRY,NormRZ,NormLX,NormLY,NormLZ,Marker," + PlayerPrefs.GetString("Name") + ", " + PlayerPrefs.GetString("Date") + ", " + PlayerPrefs.GetInt("Run Number").ToString("D3") + "\n");

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

#if UNITY_EDITOR
            EditorSceneManager.activeSceneChanged += ChangedActiveScene;
#else
            SceneManager.activeSceneChanged += ChangedActiveScene;       
#endif
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
                    if (SharedJoystick.CtrlDynamicsFlag)
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

                var path = PlayerPrefs.GetString("Path") + "\\continuous_data_" + PlayerPrefs.GetString("Name") + "_" + DateTime.Today.ToString("MMddyyyy") + "_" + PlayerPrefs.GetInt("Run Number").ToString("D3") + ".txt";
                File.AppendAllText(path, sb.ToString());
                sb.Clear();

                var num = PlayerPrefs.GetInt("Run Number") + 1;
                PlayerPrefs.SetInt("Run Number", num);

                Destroy(this);

                flagRecording = false;
            }
            else if (next.name == "MonkeyGaze")
            {
                flagPlaying = true;

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
