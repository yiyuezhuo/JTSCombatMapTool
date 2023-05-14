using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using TMPro;

#if UNITY_EDITOR 
using UnityEditor;
#endif

public class LoadTextButton : MonoBehaviour
{
    public TMP_InputField InputField;
    public string title;
    public string extention;

    // Start is called before the first frame update
    void Start()
    {
        InputField.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPressed()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            // string path = EditorUtility.OpenFilePanel("Select the Oob File", "", "oob");
            string path = EditorUtility.OpenFilePanel(title, "", extention);
            // Debug.Log($"path={path}");
            var text = File.ReadAllText(path); // TODO: Determine Encoding
            // Debug.Log($"text={text}");
            InputField.text = text;
        }
        else
        {
#endif
            // TODO: Add File Browser Supporting for other plateforms other than WebGL
            // Consider: https://assetstore.unity.com/packages/tools/gui/runtime-file-browser-113006
            FileUploaderHelper.RequestFile((path) =>
            {
                if (string.IsNullOrWhiteSpace(path))
                    return;

                StartCoroutine(UploadText(path));
            }, ".oob");
#if UNITY_EDITOR
        }
#endif
    }

    IEnumerator UploadText(string path)
    {
        string text;
        using(UnityWebRequest textWeb = new UnityWebRequest(path, UnityWebRequest.kHttpVerbGET))
        {
            textWeb.downloadHandler = new DownloadHandlerTexture();

            yield return textWeb.SendWebRequest();

            text = textWeb.downloadHandler.text;
        }
        // Debug.Log($"text={text}");
        InputField.text = text;
    }
}
