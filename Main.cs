using System.IO;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Main : MonoBehaviour
{
    private GameObject settings;

    public CommandList commands = new CommandList();
    public GameObject commandsObject;
    public List<GameObject> partCommandsObjects;
    public GameObject commandPrefab;
    public GameObject partCommandPrefab;
    public GameObject twoPartPrefab;

    public GameObject settingsObject;

    public GameObject nothing;
    public GameObject stopDialog;
    
    SettingSave save;

    // Start is called once before the first frame
    void Start()
    {
        // Load Settings from Storage
        save = SettingSave.LoadSave();
        
        // Load Commands from Storage
        commands = Parser.Parse(PlayerPrefs.GetString("list", "base"));
        foreach (PartCommandList cmd in commands.lists)
        {
            cmd.listObject = Instantiate(partCommandPrefab, commandsObject.transform);
            foreach(Command cmdd in cmd.list)
            {
                cmdd.commandObject = Instantiate(commandPrefab, cmd.listObject.transform);
                cmdd.Init();
            }
        }

        // Init important Object attributes
        Command.networkObject = this.gameObject;
        CommandList.networkObject = this.gameObject;
        settingsObject.SetActive(false);
        CheckLists();
    }

    /**
     * <summary>
     * Adds a <see cref="Command"/> to the <see cref="PartCommandList"/>, if no command name is specified it uses <see cref="RainbowCommand"/>
     * </summary>
     */
    public void AddCommand(PartCommandList part, string name="rainbow")
    {
        if (name == "rainbow")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new RainbowCommand(obj));
        }
        else if (name == "rainbowStationary")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new StatRainbowCommand(obj));
        }
        else if (name == "theaterChaseRainbow")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new TheaterRainbowCommand(obj));
        }
        else if (name == "theaterChase")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new TheaterCommand(obj));
        }
        else if (name == "colorWipe")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new ColorwipeCommand(obj));
        }
        else if (name == "setColor")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new SetColorCommand(obj));
        }
        else if (name == "interpolate")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new InterpolateCommand(obj));
        }
        else if (name == "wait")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new WaitCommand(obj));
        }
        else if (name == "split")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new SplitCommand(obj));
        }
        else if (name == "join")
        {
            GameObject obj = Instantiate(commandPrefab, part.listObject.transform);
            part.Add(new JoinCommand(obj));
        }

        CheckLists();

        if (save.autosave)
            Save();
    }
    /**
     * <summary>
     * Adds a standard <see cref="RainbowCommand"/> to the <see cref="PartCommandList"/> specified by the parameter
     * </summary>
     */
    public void AddCommand(int listnum)
    {
        PartCommandList part;
        if (listnum >= commands.lists.Count)
            part = AddList();
        else
            part = commands.lists[listnum];

        AddCommand(part);
    }
    /**
     * <summary>
     * Duplicates a given <see cref="Command"/> and appends it to the <see cref="PartCommandList"/> it is in
     * </summary>
     */
    public void DuplicateCommand(Command cmd)
    {
        foreach (PartCommandList list in commands.lists)
        {
            if (list.list.Contains(cmd))
            {
                AddCommand(list, cmd.commandTitle);
                Command newCommand = list.list[list.list.Count - 1];
                newCommand.SaveSettings(cmd.GetSettings());
                newCommand.Render();
            }
        }

        CheckLists();

        if (save.autosave)
            Save();
    }
    /**
     * <summary>
     * Removes a given <see cref="Command"/> from its <see cref="PartCommandList"/>
     * </summary>
     */
    public void RemoveCommand(Command cmd)
    {
        Destroy(cmd.commandObject);
        List<PartCommandList> liststoRemove = new List<PartCommandList>();
        foreach (PartCommandList list in commands.lists)
        {
            if (list.list.Contains(cmd))
                list.list.Remove(cmd);
        }

        CheckLists();

        if (save.autosave)
            Save();
    }
    /**
     * <summary>
     * Changes the <see cref="Command"/> object to the new dynamic type specified by newSetting
     * </summary>
     */ 
    public void ChangeCommand(Command cmd, string newSetting)
    {
        // Initializing the subtype
        Command newCommand;
        if (newSetting == "rainbow")
        {
            newCommand = new RainbowCommand();
        }
        else if (newSetting == "rainbowStationary")
        {
            newCommand = new StatRainbowCommand();
        }
        else if (newSetting == "theaterChaseRainbow")
        {
            newCommand = new TheaterRainbowCommand();
        }
        else if (newSetting == "theaterChase")
        {
            newCommand = new TheaterCommand();
        }
        else if (newSetting == "colorWipe")
        {
            newCommand = new ColorwipeCommand();
        }
        else if (newSetting == "interpolate")
        {
            newCommand = new InterpolateCommand();
        }
        else if (newSetting == "setColor")
        {
            newCommand = new SetColorCommand();
        }
        else if (newSetting == "wait")
        {
            newCommand = new WaitCommand();
        }
        else if (newSetting == "split")
        {
            newCommand = new SplitCommand();
        }
        else if (newSetting == "join")
        {
            newCommand = new JoinCommand();
        }
        else
        {
            newCommand = new RainbowCommand();
        }

        // Saves all settings from the old command that the new Command has
        newCommand.SaveSettings(cmd.GetSettings());

        // Replaces the old Gameobject with a fresh one and initializes it
        int idx = 0;
        int i = 0;
        foreach (PartCommandList list in commands.lists)
        {
            if (list.list.Contains(cmd))
            {
                idx = list.Remove(cmd);
                list.list.Insert(idx, newCommand);

                i = commands.lists.IndexOf(list);
            }
        }
        GameObject obj = Instantiate(commandPrefab, commands.lists[i].listObject.transform);
        newCommand.commandObject = obj;
        newCommand.Init();

        obj.transform.SetSiblingIndex(idx);

        // Deletes the old Gameobject to be picked up by garbage collector
        GameObject.Destroy(cmd.commandObject);
    }

    /**
     * <summary>
     * Toggle for the modify buttons on all current <see cref="Command"/> GameObjects
     * </summary>
     */
    public void ToggleModify(bool modifyState)
    {
        // Toggle on
        if (modifyState)
        {
            foreach (PartCommandList cmdlist in commands.lists)
            {
                foreach (Command cmd in cmdlist.list)
                {
                    cmd.commandObject.transform.Find("remove").gameObject.SetActive(true);
                    cmd.commandObject.transform.Find("duplicate").gameObject.SetActive(true);

                    cmd.commandObject.transform.Find("play").gameObject.SetActive(false);
                    cmd.commandObject.transform.Find("settings").gameObject.SetActive(false);
                    cmd.commandObject.transform.Find("moveup").gameObject.SetActive(true);
                    cmd.commandObject.transform.Find("movedown").gameObject.SetActive(true);
                }
            }
        } else // Toggle off
        {
            foreach (PartCommandList cmdlist in commands.lists)
            {
                foreach (Command cmd in cmdlist.list)
                {
                    cmd.commandObject.transform.Find("remove").gameObject.SetActive(false);
                    cmd.commandObject.transform.Find("duplicate").gameObject.SetActive(false);

                    // Only activate the play button if the command is not of type split or join
                    if (!cmd.GetType().Equals(typeof(SplitCommand)) && !cmd.GetType().Equals(typeof(JoinCommand)))
                        cmd.commandObject.transform.Find("play").gameObject.SetActive(true);

                    cmd.commandObject.transform.Find("settings").gameObject.SetActive(true);
                    cmd.commandObject.transform.Find("moveup").gameObject.SetActive(false);
                    cmd.commandObject.transform.Find("movedown").gameObject.SetActive(false);
                }
            }
        }
    }

    /**
     * <summary>
     * Moves the supplied <see cref="Command"/> in its <see cref="PartCommandList"/> up one spot, if possible
     * </summary>
     */
    public void MoveUp(Command cmd)
    {
        commands.MoveUp(cmd);

        if (save.autosave)
            Save();
    }
    /**
     * <summary>
     * Moves the supplied <see cref="Command"/> in its <see cref="PartCommandList"/> down one spot, if possible
     * </summary>
     */
    public void MoveDown(Command cmd)
    {
        commands.MoveDown(cmd);

        if (save.autosave)
            Save();
    }

    /**
     * <summary>
     * Adds a new <see cref="PartCommandList"/> to the <see cref="CommandList"/> and the screen
     * </summary>
     */
    public PartCommandList AddList()
    {
        PartCommandList part = new PartCommandList();
        part.listObject = Instantiate(partCommandPrefab, commandsObject.transform);
        commands.lists.Add(part);

        CheckLists();

        return part;
    }

    /**
     * <summary>
     * Plays all Commands in the current <see cref="CommandList"/>
     * </summary>
     */
    public void PlayAll()
    {
        commands.PostPlay();
    }
    /**
     * <summary>
     * Turns the lamp off by setting black as the color
     * </summary>
     */
    public void Off()
    {
        SetColorCommand cmd = new SetColorCommand();
        cmd.color = new Color(0f, 0f, 0f);
        cmd.PostPlay();
    }
    /**
     * <summary>
     * Displays the dialog to confirm a full process and programm termination
     * </summary>
     */
    public void PotentialStop()
    {
        stopDialog.SetActive(true);
        stopDialog.transform.Find("yes").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        stopDialog.transform.Find("yes").gameObject.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { Stop(); stopDialog.SetActive(false); }));
    }
    /**
     * <summary>
     * Terminates the full process on the server
     * </summary>
     */
    public void Stop()
    {
        new StopCommand().PostPlay();
    }

    /**
     * <summary>
     * Saves the current information to a save file
     * </summary>
     */
    public void Save()
    {
        Parser.Encode(commands);
    }

    /**
     * <summary>
     * Checks if any <see cref="PartCommandList"/> in the current <see cref="CommandList"/> aren't referenced anymore and thus to be removed.
     * </summary>
     */
    public void CheckLists()
    {
        // Searches for non-referenced lists
        List<PartCommandList> listsToRemove = new List<PartCommandList>();
        for (int i = 0; i < commands.lists.Count; i++)
        {
            bool hasReference = false;
            foreach(PartCommandList list in commands.lists)
            {
                foreach (Command cmd in list.list)
                    if (cmd.GetType().Equals(typeof(SplitCommand)) && ((SplitCommand)cmd).newList == i)
                        hasReference = true;

                if (!hasReference)
                    listsToRemove.Add(list);
            }
            i++;
        }

        // Removes those lists and destroys their objects
        foreach (PartCommandList list in listsToRemove)
        {
            int listint = 0;
            for (int i = 0; i < commands.lists.Count; i++)
                if (commands.lists[i] == list)
                    listint = i;

            commands.lists.Remove(list);
            Destroy(list.listObject);
        }

        // Displays the "nothing here" message if there are no lists left
        if (commands.lists.Count == 0)
            nothing.SetActive(true);
        else
            nothing.SetActive(false);
    }
}

/**
 * <summary>
 * Helper class for prsing and enconding information about <see cref="CommandList"/> objects
 * </summary>
 */
public static class Parser
{
    /**
     * <summary>
     * Parses the <see cref="CommandList"/> with the specified file name to an object
     * </summary>
     */
    public static CommandList Parse(string name = "base")
    {
        Dictionary<string, object> jsonData;
        
        // Check if the files Directory exists, if not, create it
        string dataPath = Application.persistentDataPath + "/commandlists";
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);
        
        // Read from the file, handle cases the specified file, or even the standard file doesnt exist
        if (File.Exists(dataPath + "/" + name + ".json"))
        {
            using (StreamReader sr = new StreamReader(dataPath + "/" + name + ".json"))
            {
                jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadLine());
            }
            
        }
        else if (File.Exists(dataPath + "/base.json"))
        {
            using (StreamReader sr = new StreamReader(dataPath + "/base.json"))
            {
                jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadLine());
            }
            PlayerPrefs.SetString("list", "base");
        }
        else
        {
            jsonData = new Dictionary<string, object>();
        }

        // Parse the string to an object
        CommandList newCommandList;
        if (jsonData.Count > 0)
        { 
            // Try get the name of the list
            if (jsonData.TryGetValue("name", out object dataName))
                newCommandList = new CommandList(name, (string) dataName);
            else
                newCommandList = new CommandList(name, "Some List");

            // Try get the content of the list
            if (jsonData.TryGetValue("parts", out object partlistobj))
            {
                // Iterate over each PartCommandList in the JSON data
                foreach (List<Dictionary<string, object>> part in ((Newtonsoft.Json.Linq.JArray)partlistobj).ToObject<List<List<Dictionary<string, object>>>>())
                {
                    PartCommandList newPart = new PartCommandList();
                    // Iterate over each Command in that list in the JSON data
                    foreach (Dictionary<string, object> cmd in part)
                    {
                        if (cmd.TryGetValue("commandTitle", out object commandTitle))
                        {
                            // Incase there is a commandTitle specified in JSON, try and get meaning from it
                            Command newCmd = null;
                            switch (commandTitle)
                            {
                                // Incase the title means something, try and get the values for that specific Command
                                case "rainbow":
                                    newCmd = new RainbowCommand();
                                    if (cmd.TryGetValue("wait_ms", out object wait_ms)) ((RainbowCommand)newCmd).wait_ms = Convert.ToInt32(wait_ms);
                                    break;
                                case "rainbowStationary":
                                    newCmd = new StatRainbowCommand();
                                    if (cmd.TryGetValue("wait_ms", out wait_ms)) ((StatRainbowCommand)newCmd).wait_ms = Convert.ToInt32(wait_ms);
                                    break;
                                case "colorWipe":
                                    newCmd = new ColorwipeCommand();
                                    if (cmd.TryGetValue("color", out object color)) ((ColorwipeCommand)newCmd).color = Helper.ToColor(Convert.ToInt32(color));
                                    if (cmd.TryGetValue("wait_ms", out wait_ms)) ((ColorwipeCommand)newCmd).wait_ms = Convert.ToInt32(wait_ms);
                                    break;
                                case "setColor":
                                    newCmd = new SetColorCommand();
                                    if (cmd.TryGetValue("color", out color)) ((SetColorCommand)newCmd).color = Helper.ToColor(Convert.ToInt32(color));
                                    break;
                                case "theaterChase":
                                    newCmd = new TheaterCommand();
                                    if (cmd.TryGetValue("color", out color)) ((TheaterCommand)newCmd).color = Helper.ToColor(Convert.ToInt32(color));
                                    if (cmd.TryGetValue("wait_ms", out wait_ms)) ((TheaterCommand)newCmd).wait_ms = Convert.ToInt32(wait_ms);
                                    if (cmd.TryGetValue("iterations", out object iterations)) ((TheaterCommand)newCmd).iterations = Convert.ToInt32(iterations);
                                    break;
                                case "theaterChaseRainbow":
                                    newCmd = new TheaterRainbowCommand();
                                    if (cmd.TryGetValue("wait_ms", out wait_ms)) ((TheaterRainbowCommand)newCmd).wait_ms = Convert.ToInt32(wait_ms);
                                    break;
                                case "interpolate":
                                    newCmd = new InterpolateCommand();
                                    if (cmd.TryGetValue("color", out color)) ((InterpolateCommand)newCmd).color = Helper.ToColor(Convert.ToInt32(color));
                                    if (cmd.TryGetValue("color2", out object color2)) ((InterpolateCommand)newCmd).color2 = Helper.ToColor(Convert.ToInt32(color2));
                                    if (cmd.TryGetValue("duration_ms", out object duration_ms)) ((InterpolateCommand)newCmd).duration_ms = Convert.ToInt32(duration_ms);
                                    if (cmd.TryGetValue("goback", out object goback)) ((InterpolateCommand)newCmd).goback = (bool)goback;
                                    break;
                                case "wait":
                                    newCmd = new WaitCommand();
                                    if (cmd.TryGetValue("duration_ms", out duration_ms)) ((WaitCommand)newCmd).duration_ms = Convert.ToInt32(duration_ms);
                                    break;
                                case "split":
                                    newCmd = new SplitCommand();
                                    if (cmd.TryGetValue("newList", out object newList))
                                        ((SplitCommand)newCmd).newList = Convert.ToInt32(newList);
                                    break;
                                case "join":
                                    newCmd = new JoinCommand();
                                    if (cmd.TryGetValue("listtojoin", out object listtojoin)) ((JoinCommand)newCmd).listtojoin = Convert.ToInt32(listtojoin);
                                    if (cmd.TryGetValue("waitlist", out object waitlist)) ((JoinCommand)newCmd).waitlist = (bool)waitlist;
                                    break;
                            }
                            // Only add to list if the title meant sth
                            if (newCmd != null)
                            {
                                if (cmd.TryGetValue("title", out object title)) newCmd.title = (string)title;
                                if (cmd.TryGetValue("lamps", out object lamps)) newCmd.lamps = ((Newtonsoft.Json.Linq.JArray)lamps).ToObject<List<int>>();
                                newPart.AddHidden(newCmd);
                            }
                            
                        }
                    }
                    newCommandList.lists.Add(newPart);
                }
            }
            return newCommandList;
        }
        return new CommandList();
    }
    /**
     * <summary>
     * Parses all available files in the commandslists directory to <see cref="CommandList"/> objects
     * </summary>
     */
    public static List<CommandList> ParseAll()
    {
        List<string> files = Parser.FetchLists();
        List<CommandList> output = new List<CommandList>();

        foreach (string s in files)
        {
            output.Add(Parse(s));
        }

        return output;
    }
    /**
     * <summary>
     * Fetches all available file names for parsing from the standard directory
     * </summary>
     */
    public static List<string> FetchLists()
    {
        List<string> output = new List<string>();
        
        if (!Directory.Exists(Application.persistentDataPath + "/commandlists"))
            Directory.CreateDirectory(Application.persistentDataPath + "/commandlists");

        string[] files = Directory.GetFiles(Application.persistentDataPath + "/commandlists");

        foreach (string s in files)
        {
            if (Path.GetFileName(s).EndsWith(".json"))
                output.Add(Path.GetFileNameWithoutExtension(s));
        }

        return output;
    }

    /**
     * <summary>
     * Encodes the supplied <see cref="CommandList"/> to a file named appropriatly
     * </summary>
     */
    public static void Encode(CommandList commands)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/commandlists"))
            Directory.CreateDirectory(Application.persistentDataPath + "/commandlists");

        using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/commandlists/" + commands.name + ".json"))
        {
            sw.Write(JsonConvert.SerializeObject(commands.GetSaveDics()));
        }
    }
    /**
     * <summary>
     * Encodes all supplied <see cref="CommandList"/> to files named appropriatly
     * </summary>
     */
    public static void Encode(List<CommandList> lists)
    {
        foreach (CommandList cmd in lists)
        {
            Encode(cmd);
        }
    }
}
