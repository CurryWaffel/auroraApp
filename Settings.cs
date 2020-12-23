using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    Command command;
    CommandList cmdlist;

    bool lastWasCommand;

    public GameObject commandSelect;
    public GameObject title;
    public GameObject lamps;
    public GameObject colorwheel;
    public GameObject colorwheel2;
    public GameObject wait_ms;
    public GameObject duration_ms;
    public GameObject iterations;
    public GameObject goback;
    public GameObject splitlist;
    public GameObject joinlist;
    public GameObject waitlist;
    public GameObject looplist;

    public GameObject lampsModify;

    // Start is called before the first frame update
    void Start()
    {
        /*
        commandSelect.transform.GetChild(0).GetComponent<Dropdown>().options = new List<Dropdown.OptionData>() {
            new Dropdown.OptionData("rainbow"),
            new Dropdown.OptionData("theaterChase"),
            new Dropdown.OptionData("rainbowStationary"),
            new Dropdown.OptionData("theaterChaseRainbow"),
            new Dropdown.OptionData("interpolate"),
            new Dropdown.OptionData("colorWipe"),
            new Dropdown.OptionData("setColor")
        };
        */
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void openSettings(Command cmd)
    {
        HideAll();

        command = cmd;
        lastWasCommand = true;
        Dictionary<Setting, object> sets = cmd.GetSettings();
        foreach (Setting s in cmd.settings)
        {
            switch (s) {
                case Setting.COLOR:
                    colorwheel.SetActive(true);
                    colorwheel.transform.GetChild(0).GetComponent<ColorWheelControl>().PickColor((Color) sets[Setting.COLOR]);
                    //Debug.Log((Color)sets[Setting.COLOR]);
                    break;

                case Setting.COLOR2:
                    colorwheel2.SetActive(true);
                    colorwheel2.transform.GetChild(0).GetComponent<ColorWheelControl>().PickColor((Color)sets[Setting.COLOR2]);
                    //Debug.Log((Color)sets[Setting.COLOR]);
                    break;

                case Setting.WAIT_MS:
                    wait_ms.SetActive(true);
                    wait_ms.transform.Find("input").GetComponent<InputField>().text = ((int)sets[Setting.WAIT_MS]).ToString();
                    break;

                case Setting.DURATION_MS:
                    duration_ms.SetActive(true);
                    duration_ms.transform.Find("input").GetComponent<InputField>().text = ((int)sets[Setting.DURATION_MS]).ToString();
                    break;

                case Setting.ITERATIONS:
                    iterations.SetActive(true);
                    iterations.transform.Find("input").GetComponent<InputField>().text = ((int)sets[Setting.ITERATIONS]).ToString();
                    break;
                case Setting.COMMAND:
                    commandSelect.SetActive(true);
                    int selval = 0;
                    Type cmdtype = cmd.GetType();
                    if (cmdtype.Equals(typeof(RainbowCommand)))
                    {
                        selval = 0;
                    }
                    else if (cmdtype.Equals(typeof(TheaterCommand)))
                    {
                        selval = 1;
                    }
                    else if (cmdtype.Equals(typeof(StatRainbowCommand)))
                    {
                        selval = 2;
                    }
                    else if (cmdtype.Equals(typeof(TheaterRainbowCommand)))
                    {
                        selval = 3;
                    }
                    else if (cmdtype.Equals(typeof(InterpolateCommand)))
                    {
                        selval = 4;
                    }
                    else if (cmdtype.Equals(typeof(ColorwipeCommand)))
                    {
                        selval = 5;
                    }
                    else if (cmdtype.Equals(typeof(SetColorCommand)))
                    {
                        selval = 6;
                    }
                    else if (cmdtype.Equals(typeof(WaitCommand)))
                    {
                        selval = 7;
                    }
                    else if (cmdtype.Equals(typeof(SplitCommand)))
                    {
                        selval = 8;
                    }
                    else if (cmdtype.Equals(typeof(JoinCommand)))
                    {
                        selval = 9;
                    }
                    commandSelect.transform.GetChild(0).GetComponent<Dropdown>().value = selval;
                    break;
                case Setting.TITLE:
                    title.SetActive(true);
                    title.transform.GetChild(0).GetComponent<InputField>().text = (string)sets[Setting.TITLE];
                    break;
                case Setting.LAMPS:
                    lamps.SetActive(true);
                    lamps.transform.Find("numbers").GetComponent<Text>().text = ((List<int>) sets[Setting.LAMPS]).Count > 0 ? string.Join(",", (List<int>)sets[Setting.LAMPS]) : "0,1,2,3,4,5,6";
                    break;
                case Setting.GOBACK:
                    goback.SetActive(true);
                    goback.transform.Find("Toggle").GetComponent<Toggle>().isOn = (bool)sets[Setting.GOBACK];
                    break;
                case Setting.SPLITLIST:
                    splitlist.SetActive(true);
                    splitlist.transform.Find("input").GetComponent<InputField>().text = ((int)sets[Setting.SPLITLIST]).ToString();
                    CorrectListNumIfWrong();
                    break;
                case Setting.JOINLIST:
                    joinlist.SetActive(true);
                    joinlist.transform.Find("input").GetComponent<InputField>().text = ((int)sets[Setting.JOINLIST]).ToString();
                    break;
                case Setting.WAITLIST:
                    waitlist.SetActive(true);
                    waitlist.transform.Find("Toggle").GetComponent<Toggle>().isOn = (bool)sets[Setting.WAITLIST];
                    break;
                case Setting.LOOPLIST:
                    looplist.SetActive(true);
                    looplist.transform.Find("Toggle").GetComponent<Toggle>().isOn = (bool)sets[Setting.LOOPLIST];
                    break;
            }
        }

        this.gameObject.SetActive(true);
    }
    public void openSettings(CommandList list)
    {
        HideAll();
        cmdlist = list;
        lastWasCommand = false;

        Dictionary<Setting, object> sets = list.GetSettings();

        foreach (Setting s in list.settings)
        {
            switch (s)
            {
                case Setting.TITLE:
                    title.SetActive(true);
                    title.transform.GetChild(0).GetComponent<InputField>().text = (string)sets[Setting.TITLE];
                    break;
            }
        }

        this.gameObject.SetActive(true);
    }
    public void closeSettings(bool saveChanges)
    {
        if (saveChanges)
        {
            //Debug.Log("FUCK");
            Dictionary<Setting, object> sets = new Dictionary<Setting, object>();

            bool changeCommand = false;

            foreach (Setting s in lastWasCommand ? command.settings : cmdlist.settings)
            {
                switch (s)
                {
                    case Setting.COLOR:
                        sets.Add(Setting.COLOR, colorwheel.transform.GetChild(0).GetComponent<ColorWheelControl>().Selection);
                        break;

                    case Setting.COLOR2:
                        sets.Add(Setting.COLOR2, colorwheel2.transform.GetChild(0).GetComponent<ColorWheelControl>().Selection);
                        break;

                    case Setting.WAIT_MS:
                        sets.Add(Setting.WAIT_MS, int.Parse(wait_ms.transform.Find("input").GetComponent<InputField>().text));
                        break;

                    case Setting.DURATION_MS:
                        sets.Add(Setting.DURATION_MS, int.Parse(duration_ms.transform.Find("input").GetComponent<InputField>().text));
                        break;

                    case Setting.ITERATIONS:
                        sets.Add(Setting.ITERATIONS, int.Parse(iterations.transform.Find("input").GetComponent<InputField>().text));
                        break;

                    case Setting.COMMAND:
                        if (commandSelect.transform.GetChild(0).GetComponent<Dropdown>().options[commandSelect.transform.GetChild(0).GetComponent<Dropdown>().value].text != command.commandTitle)
                            changeCommand = true;
                        break;
                    case Setting.TITLE:
                        sets.Add(Setting.TITLE, title.transform.GetChild(0).GetComponent<InputField>().text);
                        break;
                    case Setting.LAMPS:
                        List<int> newList = new List<int>();
                        newList.AddRange(Array.ConvertAll(lamps.transform.Find("numbers").GetComponent<Text>().text.Split(','), str => int.Parse(str)));
                        sets.Add(Setting.LAMPS, newList);
                        break;
                    case Setting.GOBACK:
                        sets.Add(Setting.GOBACK, goback.transform.Find("Toggle").GetComponent<Toggle>().isOn);
                        break;
                    case Setting.SPLITLIST:
                        int inputint = int.Parse(splitlist.transform.Find("input").GetComponent<InputField>().text);
                        int count = GameObject.Find("Main Camera").GetComponent<Main>().commands.list.Count;
                        sets.Add(Setting.SPLITLIST, int.Parse(splitlist.transform.Find("input").GetComponent<InputField>().text));
                        if (inputint >= count)
                            GameObject.Find("Main Camera").GetComponent<Main>().AddList();
                        break;
                    case Setting.JOINLIST:
                        sets.Add(Setting.JOINLIST, int.Parse(joinlist.transform.Find("input").GetComponent<InputField>().text));
                        break;
                    case Setting.WAITLIST:
                        sets.Add(Setting.WAITLIST, waitlist.transform.Find("Toggle").GetComponent<Toggle>().isOn);
                        break;
                    case Setting.LOOPLIST:
                        sets.Add(Setting.LOOPLIST, looplist.transform.Find("Toggle").GetComponent<Toggle>().isOn);
                        break;
                }
            }

            if (lastWasCommand)
            {
                command.SaveSettings(sets);
                GameObject.Find("Main Camera").GetComponent<Main>().Save();
                command.Render();
            }
            else
            {
                cmdlist.SaveSettings(sets);
                GameObject.Find("Main Camera").GetComponent<ListViewMain>().Save();
                cmdlist.IndivRender();
            }

            if (changeCommand)
                GameObject.Find("Main Camera").GetComponent<Main>().ChangeCommand(command,
                    commandSelect.transform.GetChild(0).GetComponent<Dropdown>().options
                    [commandSelect.transform.GetChild(0).GetComponent<Dropdown>().value]
                    .text);
        }

        this.gameObject.SetActive(false);
    }

    public void openLamps()
    {
        lampsModify.SetActive(true);

        Transform content = lampsModify.transform.Find("Scroll View/Viewport/Content");
        List<int> newList = new List<int>();
        newList.AddRange(Array.ConvertAll(lamps.transform.Find("numbers").GetComponent<Text>().text.Split(','), s => int.Parse(s)));


        if (newList.Count > 0)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                if (newList.Contains(i))
                {
                    content.GetChild(i).GetComponent<Toggle>().isOn = true;
                    content.GetChild(i).Find("Label").GetComponent<Text>().text = "Lamp " + i.ToString();
                } else
                {
                    content.GetChild(i).GetComponent<Toggle>().isOn = false;
                    content.GetChild(i).Find("Label").GetComponent<Text>().text = "Lamp " + i.ToString();
                }
            }
        } else
        {
            for (int i=0; i < content.childCount; i++)
            {
                content.GetChild(i).GetComponent<Toggle>().isOn = true;
                content.GetChild(i).Find("Label").GetComponent<Text>().text = "Lamp " + i.ToString();
            }
        }
    }
    public void closeLamps(bool saveChanges)
    {
        if (saveChanges)
        {
            Transform content = lampsModify.transform.Find("Scroll View/Viewport/Content");
            List<int> newList = new List<int>();
            for (int i = 0; i < content.childCount; i++)
            {
                if (content.GetChild(i).GetComponent<Toggle>().isOn)
                {
                    newList.Add(i);                    
                }
            }
            lamps.transform.Find("numbers").GetComponent<Text>().text = string.Join(",", newList);
        }


        lampsModify.SetActive(false);
    }

    public void HideAll()
    {
        colorwheel.SetActive(false);
        colorwheel2.SetActive(false);
        wait_ms.SetActive(false);
        duration_ms.SetActive(false);
        iterations.SetActive(false);
        commandSelect.SetActive(false);
        title.SetActive(false);
        lamps.SetActive(false);
        goback.SetActive(false);
        splitlist.SetActive(false);
        joinlist.SetActive(false);
        waitlist.SetActive(false);
        looplist.SetActive(false);
    }

    public void CorrectListNumIfWrong()
    {
        InputField input = splitlist.transform.Find("input").GetComponent<InputField>();
        int count = GameObject.Find("Main Camera").GetComponent<Main>().commands.list.Count;
        if (int.Parse(input.text) >= count)
        {
            input.text = count.ToString();
            splitlist.transform.Find("newlist").gameObject.SetActive(true);
        } else
        {
            splitlist.transform.Find("newlist").gameObject.SetActive(false);
        }
    }
}

public enum Setting
{
    // Command base class
    COMMAND,
    TITLE,
    LAMPS,
    // Colors
    COLOR,
    COLOR2,
    // Integers
    WAIT_MS,
    DURATION_MS,
    ITERATIONS,
    // Booleans
    GOBACK,
    // Command Specific
    SPLITLIST,
    LOOPLIST,
    JOINLIST,
    WAITLIST,
}
