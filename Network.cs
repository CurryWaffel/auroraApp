using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/**
 * <summary>
 * Class for managing requests to the server, may be altered if there is more communication required
 * </summary>
 */
public class Network : MonoBehaviour
{
    /**
     * <summary>
     * Starts a seperate thread to asynchronously send the form to the server
     * </summary>
     */
    public void Request(WWWForm form)
    {
        StartCoroutine(PostRequest(form));
    }

    /**
     * <summary>
     * Sends the form to the server, displays any errors encountered
     * </summary>
     */
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
