using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using SFB;
using System.Xml;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;

public class Deact_GoToScene : MonoBehaviour
{
    [SerializeField]
    public Canvas mainMenu;
    [SerializeField]
    public Canvas settingsMenu;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        

        SceneManager.LoadScene("Arena");
    }

    public void ToggleLogEyeTracker(bool tog)
    {
        ArenaGame.LOG_EYE_TRACKER = tog;
    }

    public void ToggleColorScreensOnBoxes(bool tog)
    {
        
        ArenaGame.COLOR_SCREENS_ON_BOXES = tog;
    }
    public void Calibrate()
    {
       
        var error = ViveSR.anipal.Eye.SRanipal_Eye_Framework.Status;
        Debug.Log("SRanipal Engine Status: " + error.ToString());
        //Tomkil calib ok
        int result = ViveSR.anipal.Eye.SRanipal_Eye_API.LaunchEyeCalibration(System.IntPtr.Zero);
        if (result==0)
        {
            Debug.Log("SRanipal Engine Calib Successful.");

            ArenaGame.VR_HD_Calibrated = true;
        }
        else
        {
            Debug.Log("SRanipal Engine Calib Error: " + result.ToString());

        }
        
       
    }
       

    public void QuitGame()
    {
        //tomkil does not quit if run in editor
        Application.Quit();
    }

    public void ToSettings()
    {
        mainMenu.enabled = false;
        settingsMenu.enabled = true;
    }

    public void ToMain()
    {
        mainMenu.enabled = true;
        settingsMenu.enabled = false;
    }

    public void Save()
    {

        GameObject[] settingComponents,child;




        XmlDocument xmlDoc = new XmlDocument();

        XmlNode rootNode = xmlDoc.CreateElement("Settings");
        xmlDoc.AppendChild(rootNode);

        XmlNode settingNode;
        XmlAttribute attributeSetting;
        
        // ATTN All UI components have to be tagged "Setting" to be saved to settings file
        // Maybe PlayerPrefs is more appropriate?

        settingComponents = GameObject.FindGameObjectsWithTag("Setting");

        TMP_InputField inputField;
        foreach (GameObject comp in settingComponents)
        {
             
            
            inputField = comp.GetComponent<TMP_InputField>();

            if (inputField != null)
            {
                settingNode = xmlDoc.CreateElement(comp.name.Replace(" ", ""));
                settingNode.InnerText = comp.GetComponent<TMP_InputField>().text;
                rootNode.AppendChild(settingNode);
            }

        }

        UnityEngine.UI.Toggle toggle;
        foreach (GameObject comp in settingComponents)
        {

            toggle = comp.GetComponent<UnityEngine.UI.Toggle>();
            if (toggle != null)
            {
                settingNode = xmlDoc.CreateElement(comp.name.Replace(" ", ""));
                settingNode.InnerText = toggle.isOn ? "1" : "0";
                rootNode.AppendChild(settingNode);
            }
            
        }

        
        xmlDoc.Save(Application.dataPath+"\\Settings.xml");
        Debug.Log("Save settings\n");
        
    }

    public void Load()
    {

        GameObject[] settingComponents, child;


        var extensions = new[] {
                new ExtensionFilter("Extensible Markup Language ", "xml")
            };
        var path = StandaloneFileBrowser.OpenFilePanel("Open File Destination", "", extensions, false);
        
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path[0]);

        settingComponents = GameObject.FindGameObjectsWithTag("Setting");

        TMP_InputField field;
        foreach (GameObject comp in settingComponents)
        {

            field = comp.GetComponent<TMP_InputField>();
            if (field != null)
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)

                {

                    if (node.Name == comp.name.Replace(" ", "") && field != null)
                    {
                        field.text = node.InnerText;
                        if (node.Name != "DataAndConfigPath")
                        {
                            PlayerPrefs.SetFloat(comp.name, float.Parse(field.text));
                        }
                        else
                        {
                            PlayerPrefs.SetString(comp.name, node.InnerText);
                        }
                    }

                }
            }
            
        }

        UnityEngine.UI.Toggle toggle;
        foreach (GameObject comp in settingComponents)
        {

            toggle = comp.GetComponent<UnityEngine.UI.Toggle>();
            if (toggle != null)
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)

                {

                    if (node.Name == comp.name.Replace(" ", ""))
                    {
                        if (node.InnerText == "1")
                        {
                            toggle.isOn = true;
                            PlayerPrefs.SetString(comp.name, "1");
                        }
                        else
                        {
                            toggle.isOn = false;
                            PlayerPrefs.SetString(comp.name, "0");
                        }
                        
                    }

                }
            }

        }
    }
}

