using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class CommandList
{
    public static int Id_Counter;
    public static GameObject networkObject;

    int id;
    public List<PartCommandList> list;
    public string name;
    public string title;
    public List<Setting> settings;

    public GameObject listObject;

    public CommandList()
    {
        this.id = Command.Id_Counter + 1;
        Command.Id_Counter++;

        list = new List<PartCommandList>();
        name = "list_" + this.id;
        title = "Some List";

        settings = new List<Setting>() { Setting.TITLE };
    }
    public CommandList(string name) : this()
    {
        this.name = name;
    }
    public CommandList(string name, string title) : this(name)
    {
        this.title = title;
    }
    public CommandList(List<PartCommandList> list) : this()
    {
        this.list = list;
    }

    public CommandList(GameObject obj) : this()
    {
        this.listObject = obj;
        Init();
    }
    public CommandList(GameObject obj, string name) : this(obj)
    {
        this.name = name;
    }
    public CommandList(GameObject obj, List<PartCommandList> list) : this (obj)
    {
        this.list = list;
    }

    public void Init()
    {
        listObject.transform.Find("play").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(PostPlay));
        listObject.transform.Find("open").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { PlayerPrefs.SetString("list", name); SceneManager.LoadScene("CommandView"); }));

        listObject.transform.Find("remove").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<ListViewMain>().PotRemoveList(this); }));
        listObject.transform.Find("remove").gameObject.SetActive(false);

        //listObject.transform.Find("duplicate").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<ListViewMain>().RemoveCommand(this); }));
        listObject.transform.Find("duplicate").gameObject.SetActive(false);

        listObject.transform.Find("settings").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<ListViewMain>().settingsObject.GetComponent<Settings>().openSettings(this); }));
        //listObject.transform.Find("settings").gameObject.SetActive(false);

        listObject.transform.Find("moveup").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<ListViewMain>().MoveUp(this); }));
        listObject.transform.Find("moveup").gameObject.SetActive(false);

        listObject.transform.Find("movedown").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<ListViewMain>().MoveDown(this); }));
        listObject.transform.Find("movedown").gameObject.SetActive(false);

        IndivRender();
    }
    public virtual void IndivRender()
    {
        listObject.transform.Find("title").gameObject.GetComponent<Text>().text = title;
    }

    public void MoveUp(Command cmd)
    {
        foreach (PartCommandList part in list)
        {
            if (part.list.Contains(cmd))
                part.MoveUp(cmd);
        }
    }
    public void MoveDown(Command cmd)
    {
        foreach (PartCommandList part in list)
        {
            if (part.list.Contains(cmd))
                part.MoveDown(cmd);
        }
    }

    public void PostPlay()
    {
        WWWForm www = new WWWForm();
        List<List<Dictionary<string, object>>> commandlistlist = new List<List<Dictionary<string, object>>>();
        foreach (PartCommandList cmd in list)
        {
            commandlistlist = cmd.GetRequestList(commandlistlist);
        }
        www.AddField("cmd", JsonConvert.SerializeObject(commandlistlist));

        networkObject.GetComponent<Network>().Request(www);
    }

    public Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets =  new Dictionary<Setting, object>();
        sets.Add(Setting.TITLE, title);
        return sets;
    }
    public void SaveSettings(Dictionary<Setting, object> settings)
    {
        if (settings.TryGetValue(Setting.TITLE, out object value))
            title = (string)value;
    }   

    public List<string> GetSaveStrings()
    {
        List<string> output = new List<string>();
        foreach (PartCommandList cmdlist in list)
            output.Add(string.Join("*", cmdlist.GetSaveStrings()));
        return output;
    }
    public Dictionary<string, object> GetSaveDics()
    {
        Dictionary<string, object> output = new Dictionary<string, object>();
        output.Add("title", title);
        output.Add("name", name);

        List<List<Dictionary<string, object>>> parts = new List<List<Dictionary<string, object>>>();
        foreach (PartCommandList cmdlist in list)
            parts.Add(cmdlist.GetSaveDics());
        output.Add("parts", parts);

        return output;
    }
}

public class PartCommandList
{
    public List<Command> list;
    public GameObject listObject;

    public PartCommandList()
    {
        list = new List<Command>();
    }

    public void Add(Command cmd)
    {
        list.Add(cmd);
        this.Render();
    }
    public void AddHidden(Command cmd) { list.Add(cmd); }
    public void AddAll(List<Command> commands) { list.AddRange(commands); this.Render(); }
    public void AddAllHidden(List<Command> commands) { list.AddRange(commands); }

    public int Remove(Command cmd) { int idx = list.IndexOf(cmd); list.Remove(cmd); this.Render(); return idx; }
    public void Remove(int index) { list.RemoveAt(index); this.Render(); }

    public void MoveUp(Command cmd)
    {
        int idx = list.IndexOf(cmd);
        //Debug.Log(idx);

        if (idx > 0)
        {
            Command cmd2 = list[idx - 1];
            list[idx - 1] = cmd;
            list[idx] = cmd2;
        }
        //Debug.Log(string.Join(", ", list));

        this.Render();
    }
    public void MoveDown(Command cmd)
    {
        int idx = list.IndexOf(cmd);
        //Debug.Log(idx);

        if (idx < list.Count - 1)
        {
            Command cmd2 = list[idx + 1];
            list[idx + 1] = cmd;
            list[idx] = cmd2;
        }
        //Debug.Log(string.Join(", ", list));

        this.Render();
    }

    public List<string> GetSaveStrings()
    {
        List<string> output = new List<string>();
        foreach (Command cmd in list)
            output.Add(cmd.GetSaveString());
        return output;
    }
    public List<Dictionary<string, object>> GetSaveDics()
    {
        List<Dictionary<string, object>> output = new List<Dictionary<string, object>>();
        foreach (Command cmd in list)
            output.Add(cmd.GetSaveDic());
        return output;
    }

    public List<List<Dictionary<string, object>>> GetRequestList(List<List<Dictionary<string, object>>> commandlistlist)
    {
        List<Dictionary<string, object>> diclist = new List<Dictionary<string, object>>();
        foreach (Command cmd in list)
        {
            diclist = cmd.GetRequestList(diclist);
        }
        commandlistlist.Add(diclist);
        return commandlistlist;
    }

    public void Render()
    {
        int idx = 0;
        foreach (Command cmd in list)
        {
            cmd.commandObject.transform.SetSiblingIndex(idx);
            idx++;
        }
    }
}

