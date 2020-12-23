using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Newtonsoft.Json;

/**
 * <summary>
 * Base Class for all Commands.
 * Contains all essential methods to interface with a command
 * </summary>
 */
public abstract class Command
{
    #region Static Fields
    public static int Id_Counter;
    public static GameObject networkObject;
    #endregion

    #region Fields
    public int id;
    public string title;
    public string commandTitle;
    public List<int> lamps;
    public List<Setting> settings;

    public Color backgroundColor;

    public GameObject commandObject;
    #endregion

    /**
     * <summary>
     * Base Constructor which initializes basic values
     * </summary>
     */
    protected Command()
    {
        this.id = Command.Id_Counter + 1;
        Command.Id_Counter++;
        this.title = "ERROR";
        this.lamps = new List<int>() { 0,1,2,3,4,5,6 };

        this.settings = new List<Setting>() { Setting.COMMAND, Setting.TITLE, Setting.LAMPS };
    }

    /**
     * <summary>
     * Get the WWWForm that is required in order to play this specific command
     * </summary>
     */
    public virtual WWWForm GetRequestForm()
    {
        WWWForm www = new WWWForm();
        www.AddField("cmd", JsonConvert.SerializeObject(new List<List<Dictionary<string, object>>>() { GetRequestList() }));
        return www;
    }
    /**
     * <summary>
     * Get a command list containing only this command
     * </summary>
     */
    public virtual List<Dictionary<string, object>> GetRequestList()
    {
        return GetRequestList(new List<Dictionary<string, object>>());
    }
    /**
     * <summary>
     * Get a command list where this command is appended to all previous
     * </summary>
     */
    public abstract List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics);
    /**
     * <summary>
     * Get a dictionary containing all values that need to be saved
     * </summary>
     */
    public virtual Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = new Dictionary<string, object>();

        output.Add("commandTitle", commandTitle);
        output.Add("title", title);
        output.Add("lamps", lamps);

        return output;
    }

    /**
     * <summary>
     * Initialize the commands object with all buttons and display it
     * </summary>
     */
    public virtual void Init()
    {
        // Add functionality to play button
        commandObject.transform.Find("play").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(PostPlay));

        // Add functionality to command remove button and hide
        commandObject.transform.Find("remove").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().RemoveCommand(this); }));
        commandObject.transform.Find("remove").gameObject.SetActive(false);

        // Add functionality to command duplicate button and hide
        commandObject.transform.Find("duplicate").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().DuplicateCommand(this); }));
        commandObject.transform.Find("duplicate").gameObject.SetActive(false);

        // Add functionality to command settings button
        commandObject.transform.Find("settings").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().settingsObject.GetComponent<Settings>().openSettings(this); }));
        //commandObject.transform.Find("settings").gameObject.SetActive(false);

        // Add functionality to command moveup button and hide
        commandObject.transform.Find("moveup").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().MoveUp(this); }));
        commandObject.transform.Find("moveup").gameObject.SetActive(false);

        // Add functionality to command movedown button and hide
        commandObject.transform.Find("movedown").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().MoveDown(this); }));
        commandObject.transform.Find("movedown").gameObject.SetActive(false);
        
        Render();
    }
    /**
     * <summary>
     * Display all changes that arent buttons, such like titles or background specifics.
     * Overwrite in child classes to further realize command specific changes
     * </summary>
     */
    public virtual void Render()
    {
        commandObject.transform.Find("title").gameObject.GetComponent<Text>().text = title;
    }

    /**
     * <summary>
     * Play the command associated with this object via posting all neccesary values to the server
     * </summary>
     */
    public void PostPlay()
    {
        networkObject.GetComponent<Network>().Request(this.GetRequestForm());
    }

    /**
     * <summary>
     * Method base for retrieving all settings from this <see cref="Command"/>
     * </summary>
     */
    public virtual Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = new Dictionary<Setting, object>();
        sets.Add(Setting.COMMAND, commandTitle);
        sets.Add(Setting.TITLE, title);
        sets.Add(Setting.LAMPS, lamps);
        return sets;
    }
    /**
     * <summary>
     * Method base for saving all settings to this <see cref="Command"/>
     * </summary>
     */
    public virtual void SaveSettings(Dictionary<Setting, object> settings)
    {
        if (settings.TryGetValue(Setting.TITLE, out object value))
            this.title = (string)value;
        if (settings.TryGetValue(Setting.LAMPS, out value))
            this.lamps = (List<int>) value;
    }
}

#region Basic Command Subclasses
/**
 * <summary>
 * Class representing the rainbow command
 * which is a moving rainbow over all configured lamps
 * </summary>
 */
public class RainbowCommand : Command
{
    public int wait_ms;

    public RainbowCommand()
    {
        this.commandTitle = "rainbow";
        this.title = "rbw_" + this.id.ToString();
        this.backgroundColor = new Color(164 / 255, 164 / 255, 164 / 255);
        this.wait_ms = 50;

        this.settings.Add(Setting.WAIT_MS);
    }
    public RainbowCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    
    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();

        output.Add("wait_ms", wait_ms);

        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();
        
        args.Add("wait_ms", wait_ms);
        args.Add("lamps", lamps);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.WAIT_MS, wait_ms);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.WAIT_MS, out object value))
            this.wait_ms = (int)value;
    }
}
/**
 * <summary>
 * Class representing the stationary rainbow command
 * which is a rainbow that fades over all configured lamps simultaniously.
 * </summary>
 * Difference to the normal rainbow command is here all lamps have the same color at a given time
 */
public class StatRainbowCommand : Command
{
    public int wait_ms;

    public StatRainbowCommand()
    {
        this.commandTitle = "rainbowStationary";
        this.title = "rbw-stat_" + this.id.ToString();
        this.backgroundColor = new Color(164 / 255, 164 / 255, 164 / 255);
        this.wait_ms = 50;

        this.settings.AddRange(new Setting[] { Setting.WAIT_MS });
    }
    public StatRainbowCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    
    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();

        output.Add("wait_ms", wait_ms);

        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();
        
        args.Add("wait_ms", wait_ms);
        args.Add("lamps", lamps);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.WAIT_MS, wait_ms);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.WAIT_MS, out object value))
            this.wait_ms = (int)value;
    }
}
/**
 * <summary>
 * Class representing the theater command,
 * which is a theater chase over all configured lamps
 * </summary>
 */
public class TheaterCommand : Command
{
    public Color color;
    public int wait_ms;
    public int iterations;

    public TheaterCommand()
    {
        this.commandTitle = "theaterChase";
        this.title = "tht_" + this.id.ToString();
        this.color = new Color(1f, 1f, 1f);
        this.wait_ms = 50;
        this.iterations = 1;

        this.settings.AddRange(new Setting[] { Setting.COLOR, Setting.WAIT_MS, Setting.ITERATIONS });
    }
    public TheaterCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    
    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();

        output.Add("color", Helper.To32Bit(color));
        output.Add("wait_ms", wait_ms);
        output.Add("iterations", iterations);

        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();

        args.Add("color", Helper.To32Bit(color));
        args.Add("wait_ms", wait_ms);
        args.Add("iterations", iterations);
        args.Add("lamps", lamps);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override void Render()
    {
        base.Render();
        commandObject.transform.Find("background").gameObject.GetComponent<RawImage>().color = this.color;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.COLOR, color);
        sets.Add(Setting.WAIT_MS, wait_ms);
        sets.Add(Setting.ITERATIONS, iterations);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.COLOR, out object value))
            this.color = (Color)value;
        if (settings.TryGetValue(Setting.WAIT_MS, out value))
            this.wait_ms = (int)value;
        if (settings.TryGetValue(Setting.ITERATIONS, out value))
            this.iterations = (int)value;
    }
}
/**
 * <summary>
 * Class representing the theater rainbow command,
 * which is a moving theater chase rainbow over all configured lamps
 * </summary>
 */
public class TheaterRainbowCommand : Command
{
    public int wait_ms;

    public TheaterRainbowCommand()
    {
        this.commandTitle = "theaterChaseRainbow";
        this.title = "tht-rbw_" + this.id.ToString();
        this.wait_ms = 50;

        this.settings.AddRange(new Setting[] { Setting.WAIT_MS });
    }
    public TheaterRainbowCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    
    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();
        output.Add("wait_ms", wait_ms);
        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();
        
        args.Add("wait_ms", wait_ms);
        args.Add("lamps", lamps);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.WAIT_MS, wait_ms);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.WAIT_MS, out object value))
            this.wait_ms = (int)value;
    }
}
/**
 * <summary>
 * Class representing the colorwipe command,
 * which is a wipe of a certain color over all configured lamps
 * </summary>
 */
public class ColorwipeCommand : Command
{
    public Color color;
    public int wait_ms;

    public ColorwipeCommand()
    {
        this.color = new Color(1f, 1f, 1f);
        this.wait_ms = 50;
        this.title = "cwip_" + this.id.ToString();
        this.commandTitle = "colorWipe";

        this.settings.AddRange(new Setting[] { Setting.COLOR, Setting.WAIT_MS });
    }
    public ColorwipeCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    
    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();

        output.Add("color", Helper.To32Bit(color));
        output.Add("wait_ms", wait_ms);

        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();

        args.Add("color", Helper.To32Bit(color));
        args.Add("wait_ms", wait_ms);
        args.Add("lamps", lamps);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override void Render()
    {
        base.Render();
        commandObject.transform.Find("background").gameObject.GetComponent<RawImage>().color = this.color;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.COLOR, color);
        sets.Add(Setting.WAIT_MS, wait_ms);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.COLOR, out object value))
            this.color = (Color)value;
        if (settings.TryGetValue(Setting.WAIT_MS, out value))
            this.wait_ms = (int)value;
    }
}
/**
 * <summary>
 * Class representing the set color command,
 * which sets a certain color all configured lamps
 * </summary>
 */
public class SetColorCommand : Command
{
    public Color color;

    public SetColorCommand()
    {
        this.title = "set_" + this.id;
        this.commandTitle = "setColor";
        this.color = new Color(1f, 1f, 1f);

        this.settings.Add(Setting.COLOR);
    }
    public SetColorCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        Init();
    }

    public override void Render()
    {
        base.Render();
        commandObject.transform.Find("background").gameObject.GetComponent<RawImage>().color = this.color;
    }
    
    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();
        output.Add("color", Helper.To32Bit(color));
        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();

        args.Add("color", Helper.To32Bit(color));
        args.Add("lamps", lamps);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.COLOR, color);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.COLOR, out object value))
            this.color = (Color)value;
    }
}
/**
 * <summary>
 * Class representing the interpolate command,
 * which interpolates between two certain colors on all configured lamps
 * </summary>
 */
public class InterpolateCommand : Command
{
    public Color color;
    public Color color2;
    public int duration_ms;
    public bool goback;

    public InterpolateCommand()
    {
        this.commandTitle = "interpolate";
        this.color = new Color(0f, 0f, 0f);
        this.color2 = new Color(1f, 1f, 1f);
        this.duration_ms = 5000;
        this.goback = false;

        this.title = "inter_" + this.id.ToString();
        this.settings.AddRange(new Setting[]{ Setting.COLOR, Setting.COLOR_2, Setting.DURATION_MS, Setting.GOBACK });
    }
    public InterpolateCommand(GameObject obj) : this()
    {
        this.commandObject = obj;
        this.Init();
    }
    
    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();

        output.Add("color", Helper.To32Bit(color));
        output.Add("color2", Helper.To32Bit(color2));
        output.Add("duration_ms", duration_ms);
        output.Add("goback", goback);

        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();

        args.Add("color", Helper.To32Bit(color));
        args.Add("color2", Helper.To32Bit(color2));
        args.Add("duration_ms", duration_ms);
        args.Add("goback", goback);
        args.Add("lamps", lamps);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override void Init()
    {
        base.Init();
        this.commandObject.transform.Find("background/twopart").gameObject.SetActive(true);
    }
    public override void Render()
    {
        base.Render();
        Transform twopart = this.commandObject.transform.Find("background/twopart");
        twopart.Find("one").gameObject.GetComponent<RawImage>().color = color;
        twopart.Find("two").gameObject.GetComponent<RawImage>().color = color2;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.COLOR, color);
        sets.Add(Setting.COLOR_2, color2);
        sets.Add(Setting.DURATION_MS, duration_ms);
        sets.Add(Setting.GOBACK, goback);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.COLOR, out object value))
            this.color = (Color)value;
        if (settings.TryGetValue(Setting.COLOR_2, out value))
            this.color2 = (Color)value;
        if (settings.TryGetValue(Setting.DURATION_MS, out value))
            this.duration_ms = (int)value;
        if (settings.TryGetValue(Setting.GOBACK, out value))
            this.goback = (bool)value;
    }
}
#endregion

#region Structure Command Subclasses
/**
 * <summary>
 * Class representing the split command,
 * which splits the thread flow into the current list and a new list to concurrently be executed
 * </summary>
 */
public class SplitCommand : Command
{
    public int newList;
    public bool looplist;

    public SplitCommand()
    {
        this.commandTitle = "split";
        this.newList = 1;
        this.looplist = true;

        this.title = "Go To " + this.newList.ToString();
        this.settings.AddRange(new Setting[] { Setting.SPLITLIST, Setting.LOOPLIST });
    }
    public SplitCommand(GameObject obj) : this()
    {
        this.commandObject = obj;
        this.Init();
    }

    public override void Init()
    {
        base.Init();
        commandObject.transform.Find("play").gameObject.SetActive(false);
    }

    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();

        output.Add("newList", newList);
        output.Add("looplist", looplist);

        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();

        args.Add("newList", newList);
        args.Add("looplist", looplist);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.SPLITLIST, newList);
        sets.Add(Setting.LOOPLIST, looplist);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.SPLITLIST, out object value))
            this.newList = (int)value;
        if (settings.TryGetValue(Setting.LOOPLIST, out value))
            this.looplist = (bool)value;
    }
}
/**
 * <summary>
 * Class representing the join command,
 * which joins back a certain thread in the thread flow to resynchronize it in respect to the current thread
 * </summary>
 */
public class JoinCommand : Command
{
    public int listtojoin;
    public bool waitlist;

    public JoinCommand()
    {
        this.commandTitle = "join";
        this.listtojoin = 1;
        this.waitlist = true;

        this.title = "Go To " + this.listtojoin.ToString();
        this.settings.AddRange(new Setting[] { Setting.JOINLIST, Setting.WAITLIST });
    }
    public JoinCommand(GameObject obj) : this()
    {
        this.commandObject = obj;
        this.Init();
    }

    public override void Init()
    {
        base.Init();
        commandObject.transform.Find("play").gameObject.SetActive(false);
    }
    
    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();

        output.Add("listtojoin", listtojoin);
        output.Add("waitlist", waitlist);

        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();

        args.Add("joinList", listtojoin);
        args.Add("waitlist", waitlist);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.JOINLIST, listtojoin);
        sets.Add(Setting.WAITLIST, waitlist);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.JOINLIST, out object value))
            this.listtojoin = (int)value;
        if (settings.TryGetValue(Setting.WAITLIST, out value))
            this.waitlist = (bool)value;
    }
}

/**
 * <summary>
 * Class representing the stop command,
 * which terminates all command and program execution.
 * </summary>
 * This command is intended for debugging and remote shutdown purposes
 */
public class StopCommand : Command
{
    public StopCommand()
    {
        title = "Terminate";
        commandTitle = "terminate";
    }

    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();
        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override Dictionary<Setting, object> GetSettings() { return base.GetSettings(); }
    public override void SaveSettings(Dictionary<Setting, object> settings) { base.SaveSettings(settings); }
}
/**
 * <summary>
 * Class representing the wait command,
 * which waits a certain amount of time before the next command is executed
 * </summary>
 */
public class WaitCommand : Command
{
    public int duration_ms;

    public WaitCommand()
    {
        this.title = "Wait";
        this.commandTitle = "wait";
        this.duration_ms = 1000;
        this.settings.Add(Setting.DURATION_MS);
    }
    public WaitCommand(GameObject obj) : this()
    {
        this.commandObject = obj;
        Init();
    }

    public override void Render()
    {
        base.Render();
        commandObject.transform.Find("title").gameObject.GetComponent<Text>().text = "Wait " + duration_ms + " ms";
    }
    
    public override Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = base.GetSaveDic();

        output.Add("duration_ms", duration_ms);

        return output;
    }
    public override List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics)
    {
        Dictionary<string, object> command = new Dictionary<string, object>();
        Dictionary<string, object> args = new Dictionary<string, object>();
        args.Add("duration_ms", duration_ms);

        command.Add("command", commandTitle);
        command.Add("args", args);
        dics.Add(command);
        return dics;
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.DURATION_MS, duration_ms);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.DURATION_MS, out object value))
            this.duration_ms = (int)value;
    }
}
#endregion

/**
 * <summary>
 * Static Helper class to use in Command Base or Subclass,
 * currently only containing color bit format conversion
 * </summary>
 */
public static class Helper
{
    /**
     * <summary>
     * Converting a color from the unity color system to an integer
     * </summary>
     */
    public static int To32Bit(Color color)
    {
        return Convert.ToInt32(
            Convert.ToString(Mathf.RoundToInt(color.g * 255f), 2).PadLeft(8, '0')
            + Convert.ToString(Mathf.RoundToInt(color.r * 255f), 2).PadLeft(8, '0')
            + Convert.ToString(Mathf.RoundToInt(color.b * 255f), 2).PadLeft(8, '0')
            , 2);
    }
    /**
     * <summary>
     * Converting a color from an integer to a unity color value
     * </summary>
     */
    public static Color ToColor(int color)
    {
        string binary = Convert.ToString(color, 2).PadLeft(24, '0');
        return new Color(
            Convert.ToInt32(binary.Substring(8, 8), 2) / 255f,
            Convert.ToInt32(binary.Substring(0, 8), 2) / 255f,
            Convert.ToInt32(binary.Substring(16, 8), 2) / 255f);
    }
}
