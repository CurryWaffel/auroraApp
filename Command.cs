using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Newtonsoft.Json;
using System.Collections;

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

    protected Command()
    {
        this.id = Command.Id_Counter + 1;
        Command.Id_Counter++;
        //Debug.Log("New Command id is: " + this.id);
        this.title = "error uwu";
        this.lamps = new List<int>() { 0,1,2,3,4,5,6 };

        this.settings = new List<Setting>() { Setting.COMMAND, Setting.TITLE, Setting.LAMPS };
    }
    
    public virtual WWWForm GetRequestForm()
    {
        WWWForm www = new WWWForm();
        //Debug.Log(JsonConvert.SerializeObject(new List<List<Dictionary<string, object>>>() { GetRequestList() }));
        www.AddField("cmd", JsonConvert.SerializeObject(new List<List<Dictionary<string, object>>>() { GetRequestList() }));
        return www;
    }
    public virtual List<Dictionary<string, object>> GetRequestList()
    {
        return GetRequestList(new List<Dictionary<string, object>>());
    }
    public abstract List<Dictionary<string, object>> GetRequestList(List<Dictionary<string, object>> dics);
    public virtual string GetSaveString()
    {
        return string.Format("{0};{1};{2}", commandTitle, title, string.Join(",", lamps));
    }
    public virtual Dictionary<string, object> GetSaveDic()
    {
        Dictionary<string, object> output = new Dictionary<string, object>();

        output.Add("commandTitle", commandTitle);
        output.Add("title", title);
        output.Add("lamps", lamps);

        return output;
    }
    public virtual void Init()
    {
        commandObject.transform.Find("play").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(PostPlay));

        commandObject.transform.Find("remove").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().RemoveCommand(this); }));
        commandObject.transform.Find("remove").gameObject.SetActive(false);

        commandObject.transform.Find("duplicate").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().DuplicateCommand(this); }));
        commandObject.transform.Find("duplicate").gameObject.SetActive(false);

        commandObject.transform.Find("settings").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().settingsObject.GetComponent<Settings>().openSettings(this); }));
        //commandObject.transform.Find("settings").gameObject.SetActive(false);

        commandObject.transform.Find("moveup").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().MoveUp(this); }));
        commandObject.transform.Find("moveup").gameObject.SetActive(false);

        commandObject.transform.Find("movedown").gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { GameObject.Find("Main Camera").GetComponent<Main>().MoveDown(this); }));
        commandObject.transform.Find("movedown").gameObject.SetActive(false);

        Render();
    }
    public virtual void Render()
    {
        commandObject.transform.Find("title").gameObject.GetComponent<Text>().text = title;
    }

    public void PostPlay()
    {
        networkObject.GetComponent<Network>().Request(this.GetRequestForm());
    }

    public virtual Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = new Dictionary<Setting, object>();
        sets.Add(Setting.COMMAND, commandTitle);
        sets.Add(Setting.TITLE, title);
        sets.Add(Setting.LAMPS, lamps);
        return sets;
    }
    public virtual void SaveSettings(Dictionary<Setting, object> settings)
    {
        if (settings.TryGetValue(Setting.TITLE, out object value))
            this.title = (string)value;
        if (settings.TryGetValue(Setting.LAMPS, out value))
            this.lamps = (List<int>) value;
    }
}

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
    public RainbowCommand(Command cmd) : this(cmd.title, ((RainbowCommand)cmd).wait_ms)
    {

    }
    public RainbowCommand(Command cmd, GameObject obj) : this(obj, cmd.title, ((RainbowCommand)cmd).wait_ms)
    {

    }

    public RainbowCommand(string title) : this()
    {
        this.title = title;
    }
    public RainbowCommand(int wait_ms) : this()
    {
        this.wait_ms = wait_ms;
    }
    public RainbowCommand(string title, int wait_ms) : this(title)
    {
        this.wait_ms = wait_ms;
    }

    public RainbowCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    public RainbowCommand(GameObject obj, string title) : this(obj)
    {
        this.title = title;
        this.Render();
    }
    public RainbowCommand(GameObject obj, string title, int wait_ms) : this(obj, title)
    {
        this.wait_ms = wait_ms;
    }
    
    public override string GetSaveString()
    {
        return string.Format("{0};{1}", base.GetSaveString(), wait_ms);
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
        object value;
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.WAIT_MS, out value))
            this.wait_ms = (int)value;
    }
}
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
    public StatRainbowCommand(string title) : this()
    {
        this.title = title;
    }
    public StatRainbowCommand(int wait_ms) : this()
    {
        this.wait_ms = wait_ms;
    }
    public StatRainbowCommand(string title, int wait_ms) : this(title)
    {
        this.wait_ms = wait_ms;
    }

    public StatRainbowCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    public StatRainbowCommand(GameObject obj, string title) : this(obj)
    {
        this.title = title;
        this.Render();
    }
    public StatRainbowCommand(GameObject obj, string title, int wait_ms) : this(obj, title)
    {
        this.wait_ms = wait_ms;
    }
    
    public override string GetSaveString()
    {
        return string.Format("{0};{1}", base.GetSaveString(), wait_ms);
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

    public override void Render()
    {
        base.Render();
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.WAIT_MS, wait_ms);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        object value;
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.WAIT_MS, out value))
            this.wait_ms = (int)value;
    }
}
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
    public TheaterCommand(string title) : this()
    {
        this.title = title;
    }
    public TheaterCommand(Color color) : this()
    {
        this.color = color;
    }
    public TheaterCommand(Color color, int wait_ms, int iterations) : this(color)
    {
        this.wait_ms = wait_ms;
        this.iterations = iterations;
    }
    public TheaterCommand(string title, Color color) : this(title)
    {
        this.color = color;
    }
    public TheaterCommand(string title, Color color, int wait_ms, int iterations) : this(color, wait_ms, iterations)
    {
        this.title = title;
    }

    public TheaterCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    public TheaterCommand(GameObject obj, string title) : this(obj)
    {
        this.title = title;

        this.Render();
    }
    public TheaterCommand(GameObject obj, Color color) : this(obj)
    {
        this.color = color;

        this.Render();
    }
    public TheaterCommand(GameObject obj, string title, Color color) : this(obj, title)
    {
        this.color = color;

        this.Render();
    }
    
    public override string GetSaveString()
    {
        return string.Format("{0};{1},{2},{3};{4};{5}", base.GetSaveString(), Mathf.Round(this.color.r * 255), Mathf.Round(this.color.g * 255), Mathf.Round(this.color.b * 255), this.wait_ms, this.iterations);
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
        object value;
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.COLOR, out value))
            this.color = (Color)value;
        if (settings.TryGetValue(Setting.WAIT_MS, out value))
            this.wait_ms = (int)value;
        if (settings.TryGetValue(Setting.ITERATIONS, out value))
            this.iterations = (int)value;
    }
}
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
    public TheaterRainbowCommand(string title) : this()
    {
        this.title = title;
    }
    public TheaterRainbowCommand(int wait_ms) : this()
    {
        this.wait_ms = wait_ms;
    }
    public TheaterRainbowCommand(string title, int wait_ms) : this(wait_ms)
    {
        this.title = title;
    }

    public TheaterRainbowCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    public TheaterRainbowCommand(GameObject obj, string title) : this(obj)
    {
        this.title = title;

        this.Render();
    }
    public TheaterRainbowCommand(GameObject obj, int wait_ms) : this(obj)
    {
        this.wait_ms = wait_ms;
    }
    
    public override string GetSaveString()
    {
        return string.Format("{0};{1}", base.GetSaveString(), this.wait_ms);
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

    public override void Render()
    {
        base.Render();
    }

    public override Dictionary<Setting, object> GetSettings()
    {
        Dictionary<Setting, object> sets = base.GetSettings();
        sets.Add(Setting.WAIT_MS, wait_ms);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        object value;
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.WAIT_MS, out value))
            this.wait_ms = (int)value;
    }
}
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
    public ColorwipeCommand(Color color) : this()
    {
        this.color = color;
    }
    public ColorwipeCommand(int wait_ms) : this()
    {
        this.wait_ms = wait_ms;
    }
    public ColorwipeCommand(Color color, int wait_ms) : this()
{
        this.color = color;
        this.wait_ms = wait_ms;
    }
    public ColorwipeCommand(string title, Color color, int wait_ms) : this(color, wait_ms)
    {
        this.title = title;
    }

    public ColorwipeCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        this.Init();
    }
    public ColorwipeCommand(GameObject obj, Color color) : this(obj)
    {
        this.color = color;

        this.Render();
    }
    public ColorwipeCommand(GameObject obj, int wait_ms) : this(obj)
    {
        this.wait_ms = wait_ms;
    }
    
    public override string GetSaveString()
    {
        return string.Format("{0};{1},{2},{3};{4}", base.GetSaveString(), Mathf.Round(this.color.r * 255), Mathf.Round(this.color.g * 255), Mathf.Round(this.color.b * 255), this.wait_ms);
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
        object value;
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.COLOR, out value))
            this.color = (Color)value;
        if (settings.TryGetValue(Setting.WAIT_MS, out value))
            this.wait_ms = (int)value;
    }
}
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
    public SetColorCommand(Color color) : this()
    {
        this.color = color;
    }
    public SetColorCommand(string title, Color color) : this(color)
    {
        this.title = title;
    }
    public SetColorCommand(GameObject obj) : this()
    {
        this.commandObject = obj;

        Init();
    }
    public SetColorCommand(GameObject obj, Color color) : this(obj)
    {
        this.color = color;
        this.Render();
    }
    public SetColorCommand(GameObject obj, string title, Color color) : this(obj, color)
    {
        this.title = title;
        this.Render();
    }

    public override void Render()
    {
        base.Render();
        commandObject.transform.Find("background").gameObject.GetComponent<RawImage>().color = this.color;
    }
    
    public override string GetSaveString()
    {
        return string.Format("{0};{1},{2},{3}", base.GetSaveString(), Mathf.Round(this.color.r * 255), Mathf.Round(this.color.g * 255), Mathf.Round(this.color.b * 255));
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
        object value;
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.COLOR, out value))
            this.color = (Color)value;
    }
}
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
        this.settings.AddRange(new Setting[]{ Setting.COLOR, Setting.COLOR2, Setting.DURATION_MS, Setting.GOBACK });
    }
    public InterpolateCommand(Color color, Color color2, int duration_ms, bool goback) : this()
    {
        this.color = color;
        this.color2 = color2;
        this.duration_ms = duration_ms;
        this.goback = goback;
    }
    public InterpolateCommand(string title, Color color, Color color2, int duration_ms, bool goback) : this(color, color2, duration_ms, goback)
    {
        this.title = title;
    }
    public InterpolateCommand(GameObject obj) : this()
    {
        this.commandObject = obj;
        this.Init();
    }
    
    public override string GetSaveString()
    {
        return string.Format("{0};" +
            "{1},{2},{3};" +
            "{4},{5},{6};" +
            "{7};{8}",
            base.GetSaveString(),
            Mathf.Round(this.color.r * 255), Mathf.Round(this.color.g * 255), Mathf.Round(this.color.b * 255),
            Mathf.Round(this.color2.r * 255), Mathf.Round(this.color2.g * 255), Mathf.Round(this.color2.b * 255),
            this.duration_ms, this.goback);
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
        sets.Add(Setting.COLOR2, color2);
        sets.Add(Setting.DURATION_MS, duration_ms);
        sets.Add(Setting.GOBACK, goback);
        return sets;
    }
    public override void SaveSettings(Dictionary<Setting, object> settings)
    {
        object value;
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.COLOR, out value))
            this.color = (Color)value;
        if (settings.TryGetValue(Setting.COLOR2, out value))
            this.color2 = (Color)value;
        if (settings.TryGetValue(Setting.DURATION_MS, out value))
            this.duration_ms = (int)value;
        if (settings.TryGetValue(Setting.GOBACK, out value))
            this.goback = (bool)value;
    }
}

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
    public SplitCommand(int newList) : this()
    {
        this.newList = newList;
    }
    public SplitCommand(GameObject obj) : this()
    {
        this.commandObject = obj;
        this.Init();
    }
    public SplitCommand(GameObject obj, int newList) : this(obj)
    {
        this.newList = newList;
    }

    public override void Init()
    {
        base.Init();
        commandObject.transform.Find("play").gameObject.SetActive(false);
    }

    public override string GetSaveString()
    {
        return string.Format("{0};" + "{1}",
            base.GetSaveString(), newList);
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
        object value;
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.SPLITLIST, out value))
            this.newList = (int)value;
        if (settings.TryGetValue(Setting.LOOPLIST, out value))
            this.looplist = (bool)value;
    }
}
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
    public JoinCommand(int listtojoin) : this()
    {
        this.listtojoin = listtojoin;
    }
    public JoinCommand(GameObject obj) : this()
    {
        this.commandObject = obj;
        this.Init();
    }
    public JoinCommand(GameObject obj, int listtojoin) : this(obj)
    {
        this.listtojoin = listtojoin;
    }

    public override void Init()
    {
        base.Init();
        commandObject.transform.Find("play").gameObject.SetActive(false);
    }

    public override string GetSaveString()
    {
        return string.Format("{0};" + "{1}",
            base.GetSaveString(), listtojoin);
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
        object value;
        base.SaveSettings(settings);
        if (settings.TryGetValue(Setting.JOINLIST, out value))
            this.listtojoin = (int)value;
        if (settings.TryGetValue(Setting.WAITLIST, out value))
            this.waitlist = (bool)value;
    }
}

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

    public override Dictionary<Setting, object> GetSettings() { return new Dictionary<Setting, object>(); }
    public override void SaveSettings(Dictionary<Setting, object> settings) { }
}
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
    public WaitCommand(int duration_ms) : this()
    {
        this.duration_ms = duration_ms;
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

    public override string GetSaveString()
    {
        return string.Format("{0};{1}", base.GetSaveString(), duration_ms);
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

public static class Helper
{
    public static int To32Bit(Color color)
    {
        return Convert.ToInt32(
            Convert.ToString(Mathf.RoundToInt(color.g * 255f), 2).PadLeft(8, '0')
            + Convert.ToString(Mathf.RoundToInt(color.r * 255f), 2).PadLeft(8, '0')
            + Convert.ToString(Mathf.RoundToInt(color.b * 255f), 2).PadLeft(8, '0')
            , 2);
    }
    public static Color ToColor(int color)
    {
        string binary = Convert.ToString(color, 2).PadLeft(24, '0');
        return new Color(
            Convert.ToInt32(binary.Substring(8, 8), 2) / 255f,
            Convert.ToInt32(binary.Substring(0, 8), 2) / 255f,
            Convert.ToInt32(binary.Substring(16, 8), 2) / 255f);
    }
}
