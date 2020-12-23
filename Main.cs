using System.Collections;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Main : MonoBehaviour
{
    GameObject settings;

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

    void Start()
    {
        // Load Settings from Storage
        save = SettingSave.LoadSave();
        
        // Load Commands from Storage
        commands = Parser.Parse(PlayerPrefs.GetString("list", "base"));
        foreach (PartCommandList cmd in commands.list)
        {
            cmd.listObject = Instantiate(partCommandPrefab, commandsObject.transform);
            foreach(Command cmdd in cmd.list)
            {
                cmdd.commandObject = Instantiate(commandPrefab, cmd.listObject.transform);
                cmdd.Init();
            }
        }

        // Init important Objects status
        Command.networkObject = this.gameObject;
        CommandList.networkObject = this.gameObject;
        settingsObject.SetActive(false);
        CheckLists();
    }

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
    public void AddCommand(int listnum)
    {
        PartCommandList part;
        if (listnum >= commands.list.Count)
            part = AddList();
        else
            part = commands.list[listnum];

        AddCommand(part);
    }
    public void DuplicateCommand(Command cmd)
    {
        foreach (PartCommandList list in commands.list)
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
    public void RemoveCommand(Command cmd)
    {
        Destroy(cmd.commandObject);
        List<PartCommandList> liststoRemove = new List<PartCommandList>();
        foreach (PartCommandList list in commands.list)
        {
            if (list.list.Contains(cmd))
                list.list.Remove(cmd);
            /**
            if (list.list.Count == 0)
                liststoRemove.Add(list);
            */
        }
        /**
        foreach (PartCommandList list in liststoRemove)
        {
            commands.list.Remove(list);
            Destroy(list.listObject);
        }
        */


        CheckLists();

        if (save.autosave)
            Save();
    }
    public void ChangeCommand(Command cmd, string newSetting)
    {
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

        newCommand.SaveSettings(cmd.GetSettings());
        int idx = 0;
        int i = 0;
        foreach (PartCommandList list in commands.list)
        {
            if (list.list.Contains(cmd))
            {
                idx = list.Remove(cmd);
                list.list.Insert(idx, newCommand);

                i = commands.list.IndexOf(list);
            }
        }
        GameObject obj = Instantiate(commandPrefab, commands.list[i].listObject.transform);
        newCommand.commandObject = obj;
        newCommand.Init();

        obj.transform.SetSiblingIndex(idx);

        GameObject.Destroy(cmd.commandObject);
    }
    public void ToggleModify(bool state)
    {
        if (state)
        {
            foreach (PartCommandList cmdlist in commands.list)
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
        } else
        {
            foreach (PartCommandList cmdlist in commands.list)
            {
                foreach (Command cmd in cmdlist.list)
                {
                    cmd.commandObject.transform.Find("remove").gameObject.SetActive(false);
                    cmd.commandObject.transform.Find("duplicate").gameObject.SetActive(false);

                    if (!cmd.GetType().Equals(typeof(SplitCommand)) &&
                        !cmd.GetType().Equals(typeof(JoinCommand)))
                        cmd.commandObject.transform.Find("play").gameObject.SetActive(true);
                    cmd.commandObject.transform.Find("settings").gameObject.SetActive(true);
                    cmd.commandObject.transform.Find("moveup").gameObject.SetActive(false);
                    cmd.commandObject.transform.Find("movedown").gameObject.SetActive(false);
                }
            }
        }
    }

    public void MoveUp(Command cmd)
    {
        commands.MoveUp(cmd);

        if (save.autosave)
            Save();
    }
    public void MoveDown(Command cmd)
    {
        commands.MoveDown(cmd);

        if (save.autosave)
            Save();
    }

    public PartCommandList AddList()
    {
        PartCommandList part = new PartCommandList();
        part.listObject = Instantiate(partCommandPrefab, commandsObject.transform);
        commands.list.Add(part);

        CheckLists();

        return part;
    }

    public void PlayAll()
    {
        commands.PostPlay();
    }
    public void StopAll()
    {
        new SetColorCommand(new Color(0f, 0f, 0f)).PostPlay();
    }
    public void PotStop()
    {
        stopDialog.SetActive(true);
        stopDialog.transform.Find("yes").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        stopDialog.transform.Find("yes").gameObject.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { Stop(); BrightnessStop(); stopDialog.SetActive(false); }));
    }
    public void Stop()
    {
        new StopCommand().PostPlay();
    }

    public void Save()
    {
        Parser.Encode(commands);
    }

    public void CheckLists()
    {
        List<PartCommandList> liststoRemove = new List<PartCommandList>();
        for (int i = 1; i < commands.list.Count; i++)
        {
            bool hasverweis = false;
            foreach(PartCommandList list in commands.list)
            {
                foreach (Command cmd in list.list)
                {
                    //Debug.Log("BRAH");
                    if (cmd.GetType().Equals(typeof(SplitCommand)))
                    {
                        //Debug.Log("BREH");
                        if (((SplitCommand)cmd).newList == i)
                        {
                            //Debug.Log("BRUH");
                            hasverweis = true;
                        }
                    }
                }
                if (!hasverweis)
                    liststoRemove.Add(list);
            }
            i++;
        }
        foreach (PartCommandList list in liststoRemove)
        {
            int listint = 0;
            for (int i = 0; i < commands.list.Count; i++)
                if (commands.list[i] == list)
                    listint = i;
            Debug.Log("Removing list #: " + listint.ToString());
            commands.list.Remove(list);
            Destroy(list.listObject);
        }

        if (commands.list.Count == 0)
            nothing.SetActive(true);
        else
            nothing.SetActive(false);
    }

    public void Brightnesstest(Slider slider)
    {
        WWWForm www = new WWWForm();

        www.AddField("brightness[]", "brightness;" + slider.value.ToString());
        Debug.Log(slider.value.ToString());
        this.gameObject.GetComponent<Network>().Request(www);
    }
    public void BrightnessStop()
    {
        WWWForm www = new WWWForm();

        www.AddField("brightness[]", "terminate");

        this.gameObject.GetComponent<Network>().Request(www);
    }
}

public static class Parser
{
    public static CommandList Parse(string name = "base")
    {
        Dictionary<string, object> inputt;
        
        string dataPath = Application.persistentDataPath + "/commandlists";
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);

        //Debug.Log(name);
        if (File.Exists(dataPath + "/" + name + ".json"))
        {
            using (StreamReader sr = new StreamReader(dataPath + "/" + name + ".json"))
            {
                inputt = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadLine());
            }
            
            //Debug.Log("Yes, this file exists");
        }
        else if (File.Exists(dataPath + "/base.json"))
        {
            using (StreamReader sr = new StreamReader(dataPath + "/base.json"))
            {
                inputt = JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadLine());
            }
            PlayerPrefs.SetString("list", "base");
            //Debug.Log("Yes, this file exists");
        } else
        {
            //Debug.Log("no, fuck yu");
            inputt = new Dictionary<string, object>();
        }

        
        
        CommandList commandss;
        if (inputt.Count > 0)
        {
            if (inputt.TryGetValue("name", out object nam))
                commandss = new CommandList(name, (string)nam);
            else
                commandss = new CommandList(name, "Some List");

            if (inputt.TryGetValue("parts", out object partlistobj))
            {
                List<List<Dictionary<string, object>>> partlist = ((Newtonsoft.Json.Linq.JArray)partlistobj).ToObject<List<List<Dictionary<string, object>>>>();
                foreach (List<Dictionary<string, object>> part in partlist)
                {
                    PartCommandList parrr = new PartCommandList();
                    foreach (Dictionary<string, object> cmd in part)
                    {
                        if (cmd.TryGetValue("commandTitle", out object commandTitle))
                        {
                            Command newCmd = null;
                            switch (commandTitle)
                            {
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
                                    if (cmd.TryGetValue("goback", out object goback))
                                        Debug.Log(goback.GetType());
                                        ((InterpolateCommand)newCmd).goback = (bool)goback;
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
                            if (newCmd != null)
                            {
                                if (cmd.TryGetValue("title", out object title)) newCmd.title = (string)title;
                                if (cmd.TryGetValue("lamps", out object lamps)) newCmd.lamps = ((Newtonsoft.Json.Linq.JArray)lamps).ToObject<List<int>>();
                                parrr.AddHidden(newCmd);
                            }
                            
                        }
                    }
                    commandss.list.Add(parrr);
                }
            }

            return commandss;
        }

        //Debug.Log(string.Join("      *      ", parts));
        //Debug.Log(string.Join(" ; ", partss[0]));
        //Debug.Log(string.Join(" ; ", partss[1]));
        //Debug.Log(string.Join(";", commands.list));

        return null;
    }
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

    public static List<string> FetchLists()
    {
        List<string> output = new List<string>();

#if UNITY_ANDROID
        if (!Directory.Exists(Application.persistentDataPath + "/commandlists"))
            Directory.CreateDirectory(Application.persistentDataPath + "/commandlists");

        string[] files = Directory.GetFiles(Application.persistentDataPath + "/commandlists");

        foreach (string s in files)
        {
            if (Path.GetFileName(s).EndsWith(".json"))
                output.Add(Path.GetFileNameWithoutExtension(s));
        }
#endif

        //Debug.Log(string.Join(", ", output));

        return output;
    }

    public static void Encode(CommandList commands)
    {
#if UNITY_ANDROID
        //Debug.Log("Datapath: " + Application.persistentDataPath);
        if (!Directory.Exists(Application.persistentDataPath + "/commandlists"))
            Directory.CreateDirectory(Application.persistentDataPath + "/commandlists");

        /*
        using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/commandlists/" + commands.name + ".txt"))
        {
            sw.Write(string.Join("#", commands.GetSaveStrings()));
        }
        using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/commandlists/" + commands.name + "_settings.txt"))
        {
            sw.Write(string.Join("#", commands.title));
        }
        */

        using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/commandlists/" + commands.name + ".json"))
        {
            sw.Write(JsonConvert.SerializeObject(commands.GetSaveDics()));
        }
#endif
    }
    public static void Encode(List<CommandList> lists)
    {
        foreach (CommandList cmd in lists)
        {
            Encode(cmd);
        }
    }
}
