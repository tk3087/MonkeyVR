using UnityEngine;
using TMPro;
using SFB;
using System;
using System.Xml;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using static ViveSR.anipal.Eye.SRanipal_Eye_Framework;

public class DeAct_GoToSettings : MonoBehaviour
{
    public GameObject obj;
    public GameObject settingMenu;
    private TMP_InputField input;
    public static float timeStarted;


    void Awake()
    {
        input = obj.GetComponent<TMP_InputField>();
        input.text = PlayerPrefs.GetFloat(obj.name).ToString();


    }

    // Start is called before the first frame update
    void Start()
    {
        input = obj.GetComponent<TMP_InputField>();
        timeStarted = Time.time;

    }
    
    /// <summary>
    /// Instead of hard-coding every single setting, just use the name of the
    /// object that this function is currently acting upon as the key for its
    /// value. There is no avoiding hard-coding setting the respective varibles
    /// to the correct value, however; you need to remember what the names of
    /// the objects and what variable they are associated with.
    /// 
    /// For example:
    /// If I have an object whose name is "Distance" and, in the game, I set it
    /// to "90", as in the TMP_InputField.text = "90", that value gets stored in
    /// PlayerPrefs associated to the key "Distance", but there is no way to 
    /// store the keys in a separate class and use them later. Anyway, trying to
    /// get keys from somewhere is harder, so just hard-code it when retrieving
    /// the values.
    /// </summary>
    public void saveSetting()
    {
        print(obj.name);

        
        try
        {
            if (obj.name == "Moving ON")
            {
                PlayerPrefs.SetInt(obj.name, obj.GetComponent<UnityEngine.UI.Toggle>().isOn ? 1 : 0);
            }
            else if (obj.name == "Eye Mode")
            {
                PlayerPrefs.SetInt(obj.name, obj.GetComponent<TMP_Dropdown>().value);
            }
            else if (obj.name == "Path")
            {
                string temp = input.text;
                PlayerPrefs.SetString(obj.name, input.text);
                try
                {
                    File.WriteAllText(temp, "test");
                }
                catch (Exception e)
                {
                    Debug.LogException(e, this);
                }
            }
            else if (obj.name == "SubjectName")
            {
                string temp = input.text;
                PlayerPrefs.SetString(obj.name, input.text);
            }
            else
            {
                PlayerPrefs.SetFloat(obj.name, float.Parse(input.text));
                if (input.text == null)
                {
                    throw new Exception("Invalid or missing TMP_InputField text.");
                }
            }
            if (obj.name == null)
            {
                throw new Exception("Invalid or missing object name.");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }
    
    public void SetXML()
    {
        try
        {
            string path = PlayerPrefs.GetString("Config Path");

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            if (settingMenu.activeInHierarchy)
            {
                foreach (Transform child in settingMenu.transform)
                {
                    foreach (Transform children in child)
                    {
                        if (children.gameObject.CompareTag("Setting"))
                        {
                            if (children.name == "Drop Down")
                            {
                                TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            drop.value = int.Parse(setting.InnerText);
                                            PlayerPrefs.SetInt(children.name, drop.value);
                                        }
                                    }
                                }
                            }
                            else if (children.name == "ON OFF")
                            {
                                UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            toggle.isOn = int.Parse(setting.InnerText) == 1;
                                            PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                        }
                                    }
                                }
                            }
                            else if (children.name == "Path")
                            {
                                TMP_InputField field = children.GetComponent<TMP_InputField>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            field.text = setting.InnerText;
                                            PlayerPrefs.SetString(children.name, field.text);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TMP_InputField field = children.GetComponent<TMP_InputField>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            field.text = setting.InnerText;
                                            PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    public void LoadXML()
    {
        try
        {
            var extensions = new[] {
                new ExtensionFilter("Extensible Markup Language ", "xml")
            };
            var path = StandaloneFileBrowser.OpenFilePanel("Open File Destination", "", extensions, false);

            XmlDocument doc = new XmlDocument();
            doc.Load(path[0]);

            if (settingMenu.activeInHierarchy)
            {
                foreach (Transform child in settingMenu.transform)
                {
                    foreach (Transform children in child)
                    {
                        if (children.gameObject.CompareTag("Setting"))
                        {
                            if (children.name == "DropDown")
                            {
                                TMP_Dropdown drop = children.GetComponent<TMP_Dropdown>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            drop.value = int.Parse(setting.InnerText);
                                            PlayerPrefs.SetInt(children.name, drop.value);
                                        }
                                    }
                                }
                            }
                            else if (children.name == "Toggle")
                            {
                                UnityEngine.UI.Toggle toggle = children.GetComponent<UnityEngine.UI.Toggle>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            toggle.isOn = int.Parse(setting.InnerText) == 1;
                                            PlayerPrefs.SetInt(children.name, toggle.isOn ? 1 : 0);
                                        }
                                    }
                                }
                            }
                            else if (children.name == "Path")
                            {
                                TMP_InputField field = children.GetComponent<TMP_InputField>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            field.text = setting.InnerText;
                                            PlayerPrefs.SetString(children.name, field.text);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TMP_InputField field = children.GetComponent<TMP_InputField>();
                                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                                {
                                    foreach (XmlNode setting in node.ChildNodes)
                                    {
                                        if (setting.Name == children.name.Replace(" ", ""))
                                        {
                                            field.text = setting.InnerText;
                                            PlayerPrefs.SetFloat(children.name, float.Parse(field.text));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }
}
