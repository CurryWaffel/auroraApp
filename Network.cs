using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Network : MonoBehaviour
{
    public void Request(string request)
    {
        //Debug.Log("lol got called too!!");
        Debug.Log(request);
        StartCoroutine(GetRequest(string.Format("http://192.168.178.77/led-server/requestHandler.php?{0}", request)));
    }

    public void Request(WWWForm form)
    {
        StartCoroutine(PostRequest(form));
    }

    IEnumerator GetRequest(string uri)
    {
        //Debug.Log("lol me too!!");

        UnityWebRequest uwr = UnityWebRequest.Get(uri);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            //Debug.Log("Response Code: " + uwr.responseCode);

        }
    }

    IEnumerator PostRequest(WWWForm form)
    {
        UnityWebRequest uwr = UnityWebRequest.Post("http://192.168.178.77/led-server/requestHandler.php", form);

        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            //Debug.Log("Response Code: " + uwr.responseCode);
        }
    }
}
