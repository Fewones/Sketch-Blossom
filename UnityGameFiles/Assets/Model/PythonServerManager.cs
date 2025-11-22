using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
public class PythonServerManager : MonoBehaviour
{
    public IEnumerator SendImage(byte[] imageBytes, System.Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageBytes, "image.png", "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/predict", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                callback(null);
            }
            else
            {
                callback(www.downloadHandler.text);
            }
        }
    }
}