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
        // Init important attributes
        CommandList.networkObject = this.gameObject;
        Command.networkObject = this.gameObject;
        settingsObject.SetActive(false);

        // Parse all available files to lists
        lists = Parser.ParseAll();

        // Display all lists represented by an object each
        foreach (CommandList cmd in lists)
        {
            GameObject obj = Instantiate(listPrefab, listObject.transform);
            cmd.listObject = obj;
            cmd.Init();
        }
    }

    /**
     * <summary>
     * Adds a <see cref="CommandList"/> visible for the user and creates a file for it
     * </summary>
     */
    public void AddList()
    {
        GameObject obj = Instantiate(listPrefab, listObject.transform);
        lists.Add(new CommandList(obj));

        Parser.Encode(lists);
    }

    /**
     * <summary>
     * Displays the confirm dialog for removing a <see cref="CommandList"/>
     * </summary>
     */
    public void PotRemoveList(CommandList list)
    {
        deleteDialogObject.SetActive(true);
        deleteDialogObject.transform.Find("yes").gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        deleteDialogObject.transform.Find("yes").gameObject.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { RemoveList(list); deleteDialogObject.SetActive(false); }));
    }
    /**
     * <summary>
     * Removes a <see cref="CommandList"/> and its <see cref="GameObject"/>
     * </summary>
     */
    public void RemoveList(CommandList list)
    {
        Destroy(list.listObject);
        lists.Remove(list);

        File.Delete(Application.persistentDataPath + "/commandlists/" + list.name + ".json");
    }

    /**
     * <summary>
     * Save all lists to their respective files
     * </summary>
     */
    public void Save()
    {
        Parser.Encode(lists);
    }

    /**
     * <summary>
     * Toggles all modify buttons on the <see cref="CommandList"/> objects on or off
     * </summary>
     */
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

    /**
     * <summary>
     * Moves a <see cref="CommandList"/> object up one spot, if possible
     * </summary>
     */
    public void MoveUp(CommandList list)
    {
        int idx = lists.IndexOf(list);

        if (idx > 0)
        {
            CommandList cmd2 = lists[idx - 1];
            lists[idx - 1] = list;
            lists[idx] = cmd2;
        }
        Render();
    }
    /**
     * <summary>
     * Moves a <see cref="CommandList"/> object down one spot, if possible
     * </summary>
     */
    public void MoveDown(CommandList list)
    {
        int idx = lists.IndexOf(list);

        if (idx < lists.Count - 1)
        {
            CommandList cmd2 = lists[idx + 1];
            lists[idx + 1] = list;
            lists[idx] = cmd2;
        }
        Render();
    }

    /**
     * <summary>
     * Displays all changes in object order onscreen
     * </summary>
     */
    void Render()
    {
        int idx = 0;
        foreach (CommandList cmd in lists)
        {
            cmd.listObject.transform.SetSiblingIndex(idx);
            idx++;
        }
    }

    /**
     * <summary>
     * Effectively turns the lamp off by setting the color of all lamps to black
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
     * Terminates the lamp program and process execution
     * </summary>
     */
    public void Stop()
    {
        new StopCommand().PostPlay();
    }
}
