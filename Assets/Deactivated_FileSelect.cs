using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using SFB;

public class Deactivated_FileSelect : MonoBehaviour
{
    private TMP_InputField input;
    private GameObject obj;


    public void Select()
    {
        input = this.GetComponent<TMP_InputField>();
        obj = this.gameObject;
        string[] path = StandaloneFileBrowser.OpenFolderPanel("Set File Destination", Application.dataPath, false);
        input.text = path[0];
        PlayerPrefs.SetString(obj.name, path[0]);
    }
}
