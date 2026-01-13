using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;

namespace SketchBlossom.Model
{
   public class ModelManager
{

    public async Task<bool> serverReady()
    {
        using (UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1:8000/health"))
            {
                var op = www.SendWebRequest();
                while (!op.isDone){
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.Success){
                    return true;
                }
            }
        return false;
    }
    public async Task<string> SendImage(Texture2D tex, string item_key)
    {           
        Debug.Log("Connecting to Server");
        byte[] png = tex.EncodeToPNG();
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", png, "image.png", "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post($"http://127.0.0.1:8000/predict/{item_key}", form))
        {

            var op = www.SendWebRequest();
            while (!op.isDone)
            {
                await Task.Yield();
            }
            if (www.result != UnityWebRequest.Result.Success)
                return "error: " + www.error;
            else
                return www.downloadHandler.text;
        }
     }
    } 
}
