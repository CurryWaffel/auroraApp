using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

/**
 * <summary>
 * Class for the upper order of List, housing every sublist and thus
 * representing a full command flow that can be executed at once
 * </summary>
 */
public class CommandList
{
    #region Static Fields
    public static int Id_Counter;
    public static GameObject networkObject;
    #endregion

    #region Fields
    int id;
    public List<PartCommandList> lists;
    public string name;
    public string title;
    public List<Setting> settings;

    public GameObject listObject;
    #endregion
    
    public CommandList()
    {
        this.id = Command.Id_Counter + 1;
        Command.Id_Counter++;

        lists = new List<PartCommandList>();
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
    public CommandList(GameObject obj) : this()
    {
        this.listObject = obj;
        Init();
    }

    /**
     * <summary>
     * Initializes all basic functionality for usage in the main list viewing area
     * </summary>
     */
    public void Init()
    {
        listObject.transform.Find("play").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(PostPlay));
        listObject.transform.Find("open").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { PlayerPrefs.SetString("list", name); SceneManager.LoadScene("CommandView"); }));

        listObject.transform.Find("remove").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<ListViewMain>().PotRemoveList(this); }));
        listObject.transform.Find("remove").gameObject.SetActive(false);

        listObject.transform.Find("duplicate").gameObject.SetActive(false);

        listObject.transform.Find("settings").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<ListViewMain>().settingsObject.GetComponent<Settings>().openSettings(this); }));
        //listObject.transform.Find("settings").gameObject.SetActive(false);

        listObject.transform.Find("moveup").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<ListViewMain>().MoveUp(this); }));
        listObject.transform.Find("moveup").gameObject.SetActive(false);

        listObject.transform.Find("movedown").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<ListViewMain>().MoveDown(this); }));
        listObject.transform.Find("movedown").gameObject.SetActive(false);

        IndivRender();
    }
    /**
     * <summary>
     * Initialize all visuals, currently only title
     * </summary>
     */
    public virtual void IndivRender()
    {
        listObject.transform.Find("title").gameObject.GetComponent<Text>().text = title;
    }

    /**
     * <summary>
     * Moves a certain <see cref="Command"/> up one spot in its respective <see cref="PartCommandList"/>, if possible
     * </summary>
     */
    public void MoveUp(Command cmd)
    {
        foreach (PartCommandList part in lists)
        {
            if (part.list.Contains(cmd))
            {
                part.MoveUp(cmd);
                break;
            }                
        }
    }
    /**
     * <summary>
     * Moves a certain <see cref="Command"/> down one spot in its respective <see cref="PartCommandList"/>, if possible
     * </summary>
     */
    public void MoveDown(Command cmd)
    {
        foreach (PartCommandList part in lists)
        {
            if (part.list.Contains(cmd))
            {
                part.MoveDown(cmd);
                break;
            }
        }
    }

    /**
     * <summary>
     * Play all content of this list over post on the lamp
     * </summary>
     */
    public void PostPlay()
    {
        WWWForm www = new WWWForm();
        List<List<Dictionary<string, object>>> commandlistlist = new List<List<Dictionary<string, object>>>();
        foreach (PartCommandList cmd in lists)
        {
            commandlistlist = cmd.GetRequestList(commandlistlist);
        }
        www.AddField("cmd", JsonConvert.SerializeObject(commandlistlist));

        networkObject.GetComponent<Network>().Request(www);
    }

    /**
     * <summary>
     * Retrieving all settings from this <see cref="CommandList"/>
     * </summary>
     */
    public Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets =  new Dictionary<Setting, object>();
        sets.Add(Setting.TITLE, title);
        return sets;
    }
    /**
     * <summary>
     * Saving all settings to this <see cref="CommandList"/>
     * </summary>
     */
    public void SaveSettings(Dictionary<Setting, object> settings)
    {
        if (settings.TryGetValue(Setting.TITLE, out object value))
            title = (string)value;
    }

    /**
     * <summary>
     * Get a dictionary containing all values that need to be saved
     * </summary>
     */
    public Dictionary<string, object> GetSaveDics()
    {
        Dictionary<string, object> output = new Dictionary<string, object>();
        output.Add("title", title);
        output.Add("name", name);

        List<List<Dictionary<string, object>>> parts = new List<List<Dictionary<string, object>>>();
        foreach (PartCommandList cmdlist in lists)
            parts.Add(cmdlist.GetSaveDics());
        output.Add("parts", parts);

        return output;
    }
}

/**
 * <summary>
 * A sublist directly containing <see cref="Command"/> objects. Represents one flow of commands,
 * use multiple in a higher order list to get concurrency
 * </summary>
 */
public class PartCommandList
{
    public List<Command> list; // list to contain all commands housed by this object
    public GameObject listObject; // Object representing the list onscreen

    public PartCommandList()
    {
        list = new List<Command>();
    }

    /**
     * <summary>
     * Add a <see cref="Command"/> to this list
     * </summary>
     */
    public void Add(Command cmd) { list.Add(cmd); this.Render(); }
    /**
     * <summary>
     * Add a <see cref="Command"/> to this list without rendering it to the screen
     * </summary>
     */
    public void AddHidden(Command cmd) { list.Add(cmd); }
    /**
     * <summary>
     * Add multiple <see cref="Command"/> to this list
     * </summary>
     */
    public void AddAll(List<Command> commands) { list.AddRange(commands); this.Render(); }
    /**
     * <summary>
     * Add multiple <see cref="Command"/> to this list without rendering it to the screen
     * </summary>
     */
    public void AddAllHidden(List<Command> commands) { list.AddRange(commands); }

    /**
     * <summary>
     * Removes the given <see cref="Command"/> from this list and returns its previous index
     * </summary>
     */
    public int Remove(Command cmd) { int idx = list.IndexOf(cmd); list.Remove(cmd); this.Render(); return idx; }
    /**
     * <summary>
     * Removes the <see cref="Command"/> from this list at the given index
     * </summary>
     */
    public void Remove(int index) { list.RemoveAt(index); this.Render(); }

    /**
     * <summary>
     * Moves a certain <see cref="Command"/> up one spot, if possible
     * </summary>
     */
    public void MoveUp(Command cmd)
    {
        int idx = list.IndexOf(cmd);
 
        if (idx > 0)
        {
            Command cmd2 = list[idx - 1];
            list[idx - 1] = cmd;
            list[idx] = cmd2;
        }

        this.Render();
    }
    /**
     * <summary>
     * Moves a certain <see cref="Command"/> down one spot, if possible
     * </summary>
     */
    public void MoveDown(Command cmd)
    {
        int idx = list.IndexOf(cmd);

        if (idx < list.Count - 1)
        {
            Command cmd2 = list[idx + 1];
            list[idx + 1] = cmd;
            list[idx] = cmd2;
        }

        this.Render();
    }

    /**
     * <summary>
     * Puts all viable information about this list and its <see cref="Command"/> objects in a format to be saved to file
     * </summary>
     */
    public List<Dictionary<string, object>> GetSaveDics()
    {
        List<Dictionary<string, object>> output = new List<Dictionary<string, object>>();
        foreach (Command cmd in list)
            output.Add(cmd.GetSaveDic());
        return output;
    }
    /**
     * <summary>
     * Puts the information from every <see cref="Command"/> in a List and appends it to the input list
     * </summary>
     */
    public List<List<Dictionary<string, object>>> GetRequestList(List<List<Dictionary<string, object>>> commandListList)
    {
        List<Dictionary<string, object>> dicList = new List<Dictionary<string, object>>();

        // Add Information from every Command
        foreach (Command cmd in list)
            dicList = cmd.GetRequestList(dicList);

        // Append
        commandListList.Add(dicList);
        return commandListList;
    }

    /**
     * <summary>
     * Orders all command objects on screen in this list according to the order of the <see cref="Command"/> objects in the attribute <see cref="list"/>
     * </summary>
     */
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

