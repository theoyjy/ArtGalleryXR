using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class NfpExample : MonoBehaviour
{
    private string pdfFileType;

    public Image img;
    void Start()
    {
        RequestPermissionAsynchronously(false);
    }

    void Update()
    {

    }

    private async void RequestPermissionAsynchronously(bool readPermissionOnly = false)
    {
        NativeFilePicker.Permission permission = await NativeFilePicker.RequestPermissionAsync(readPermissionOnly);
        Debug.Log("Permission result: " + permission);
    }

    public void OpenImageFile()
    {
#if UNITY_ANDROID
		// Use MIMEs on Android
		string[] fileTypes = new string[] { "image/*" };
		
#else
        // Use UTIs on iOS
        string[] fileTypes = new string[] { "public.image" };

#endif

        // Pick an image file
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((path) =>
        {
            if (path == null)
            {
                Debug.Log("Operation cancelled");
            }
            else
            {
                Debug.Log("Picked file: " + path);

                byte[] bytes = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(1, 1);
                texture.filterMode = FilterMode.Trilinear;
                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.0f), 1.0f);
                img.sprite = sprite;
            }
        }, fileTypes);

        Debug.Log("Permission result: " + permission);
    }



}