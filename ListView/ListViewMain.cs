using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ListViewMain : MonoBehaviour
{
    List<CommandList> lists = new List<CommandList>();
    public GameObject listObject;
    public GameObject listPrefab;
    public GameObject deleteDialogObject;

    public GameObject settingsObject;
    

    // Start is called before the first frame update
    void Start()
    {
        CommandList.networkObject = this.gameObject;
        Command.networkObject = this.gameObject;
        settingsObject.SetActive(false);

        lists = Parser.ParseAll();

        foreach (CommandList cmd in lists)
        {
            GameObject obj = Instantiate(listPrefab, listObject.transform);
            cmd.listObject = obj;
            cmd.Init();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddList()
    {
        GameObject obj = Instantiate(listPrefab, listObject.transform);
        lists.Add(new CommandList(obj));

        Parser.Encode(lists);
    }

    public void PotRemoveList(CommandList list)
    {
        deleteDialogObject.SetActive(true);
        deleteDialogObject.transform.Find("yes").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        deleteDialogObject.transform.Find("yes").gameObject.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { RemoveList(list); deleteDialogObject.SetActive(false); }));
    }
    public void RemoveList(CommandList list)
    {
        Destroy(list.listObject);
        lists.Remove(list);

        File.Delete(Application.persistentDataPath + "/commandlists/" + list.name + ".json");
    }

    public void Save()
    {
        Parser.Encode(lists);
    }

    public void ToggleModify(bool state)
    {
        if (state)
        {
            foreach (CommandList cmd in lists)
            {
                Transform t = cmd.listObject.transform;
                t.Find("remove").gameObject.SetActive(true);
                t.Find("duplicate").gameObject.SetActive(true);

                t.Find("play").gameObject.SetActive(false);
                t.Find("settings").gameObject.SetActive(false);
                t.Find("open").gameObject.SetActive(false);
                t.Find("moveup").gameObject.SetActive(true);
                t.Find("movedown").gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (CommandList cmd in lists)
            {
                Transform t = cmd.listObject.transform;
                t.Find("remove").gameObject.SetActive(false);
                t.Find("duplicate").gameObject.SetActive(false);

                t.Find("play").gameObject.SetActive(true);
                t.Find("settings").gameObject.SetActive(true);
                t.Find("open").gameObject.SetActive(true);
                t.Find("moveup").gameObject.SetActive(false);
                t.Find("movedown").gameObject.SetActive(false);
            }
        }
    }

    public void MoveUp(CommandList list)
    {
        int idx = lists.IndexOf(list);
        //Debug.Log(idx);

        if (idx > 0)
        {
            CommandList cmd2 = lists[idx - 1];
            lists[idx - 1] = list;
            lists[idx] = cmd2;
        }
        //Debug.Log(string.Join(", ", list));
        Render();
    }
    public void MoveDown(CommandList list)
    {
        int idx = lists.IndexOf(list);
        //Debug.Log(idx);

        if (idx < lists.Count - 1)
        {
            CommandList cmd2 = lists[idx + 1];
            lists[idx + 1] = list;
            lists[idx] = cmd2;
        }
        //Debug.Log(string.Join(", ", list));
        Render();
    }

    void Render()
    {
        int offset = -60;
        foreach (CommandList cmd in lists)
        {
            Vector3 pos = cmd.listObject.transform.localPosition;
            cmd.listObject.transform.localPosition = new Vector3(pos.x, offset, pos.z);
            offset -= 130;
        }
    }

    public void Off()
    {
        new SetColorCommand(new Color(0f, 0f, 0f)).PostPlay();
    }
    public void Stop()
    {
        new StopCommand().PostPlay();
        BrightnessStop();
    }
    public void BrightnessStop()
    {
        WWWForm www = new WWWForm();

        www.AddField("brightness[]", "terminate");

        this.gameObject.GetComponent<Network>().Request(www);
    }
}
