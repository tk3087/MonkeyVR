using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;
using SFB;
using System.Xml;

public class GoToScene : MonoBehaviour
{

    [SerializeField]
    public Canvas mainMenuArena;
    [SerializeField]
    public Canvas settingsMenuArena;
    [SerializeField]
    public Canvas mainMenu;
    [SerializeField]
    public Canvas settingsMenu;


    // Start is called before the first frame update
    void Start()
    {
        //UnityEngine.Debug.Log("Device Name: " + SystemInfo.deviceName + "\nDevice Model: " + SystemInfo.deviceModel + "\nCPU Name: " + SystemInfo.processorType + "\nCPU Frequency: " + SystemInfo.processorFrequency + "\nGPU Name: " + SystemInfo.graphicsDeviceName);

        PlayerPrefs.SetString("Switch Mode", "experiment");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        try
        {
            switch (PlayerPrefs.GetInt("Scene"))
            {
                case 0:
                    SceneManager.LoadScene("Monkey2D");
                    break;
                case 1:
                    SceneManager.LoadScene("Arena");
                    break;
                case 2:
                    // mouse2D
                    break;
                case 3:
                    // mouse arena
                    break;
                case 4:
                    // mouse corridor
                    break;
                case 9:
                    SceneManager.LoadScene("Monkey2D");
                    break;
                default:
                    throw new Exception("Invalid Scene Selected.\n");
            }
            //SceneManager.UnloadSceneAsync("MainMenu");
        }
        catch (Exception e)
        {
            Debug.Log(e, this);
        }
    }

    public void QuitGame()
    {
        //mainMenu.enabled = true;
        SceneManager.LoadScene("MainMenu");
        

            
        //Application.Quit();

    }

    public void ToggleLogEyeTracker(bool tog)
    {
        ArenaGame.LOG_EYE_TRACKER = tog;
    }

    public void ToggleColorScreensOnBoxes(bool tog)
    {

        ArenaGame.COLOR_SCREENS_ON_BOXES = tog;
    }

    public void ToArenaMenu() 
    {
        mainMenu.enabled = false;
        SceneManager.LoadScene("MainMenu_Arena");
    }

    public void StartTheArena()
    {
        SceneManager.LoadScene("Arena");
    }

    public void ToSettings()
    {
        //mainMenuArena.enabled = false;
        //settingsMenuArena.enabled = true;
    }

    public void ToMain()
    {
        //mainMenuArena.enabled = true;
        //settingsMenuArena.enabled = false;
    }

    public void Save()
    {

        GameObject[] settingComponents, child;




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


        xmlDoc.Save(Application.dataPath + "\\Settings.xml");
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
