using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;

public class SettingsMain : MonoBehaviour
{
    public SettingSave save;

    public Slider brightnessSlider;
    public InputField led_countInput;
    public Toggle autosaveToggle;
    public Toggle brightnessliveupdatesToggle;

    // Start is called before the first frame update
    void Start()
    {
        save = new SettingSave();
        LoadSave();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetBrightness(Slider slider)
    {
        WWWForm www = new WWWForm();
        
        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic.Add("brightness", slider.value.ToString());

        www.AddField("setting", JsonConvert.SerializeObject(dic));
        this.gameObject.GetComponent<Network>().Request(www);
        
        save.brightness = Mathf.RoundToInt(slider.value);
        Save();
    }
    public void SetLedCount(InputField input)
    {
        WWWForm www = new WWWForm();

        www.AddField("brightness[]", "led_count;" + input.text);
        this.gameObject.GetComponent<Network>().Request(www);

        save.led_count = int.Parse(input.text);
        Save();
    }

    public void SetAutoSave(Toggle toggle)
    {
        save.autosave = toggle.isOn;
        Save();
    }
    public void SetBrightnessLiveUpdates(Toggle toggle)
    {
        save.brightnessliveupdates = toggle.isOn;
        Save();

        if (save.brightnessliveupdates)
        {
            brightnessSlider.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>((fl) => { SetBrightness(brightnessSlider); }));
        } else
        {
            brightnessSlider.onValueChanged.RemoveAllListeners();
        }
    }

    public void Save()
    {
        SettingSave.Save(save);
    }

    public void LoadSave()
    {
        save = SettingSave.LoadSave();

        brightnessSlider.value = save.brightness;
        led_countInput.text = save.led_count.ToString();
        autosaveToggle.isOn = save.autosave;
        brightnessliveupdatesToggle.isOn = save.brightnessliveupdates;
    }





    public void Test()
    {
        WWWForm www = new WWWForm();

        List<Dictionary<string, object>> dictionaries = new List<Dictionary<string, object>>();
        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic.Add("command", "rainbow_new");

        Dictionary<string, object> argsdic = new Dictionary<string, object>();
        argsdic.Add("wait_ms", 0);
        dic.Add("args", argsdic);
        dictionaries.Add(dic);

        Debug.Log(JsonConvert.SerializeObject(dictionaries));
        www.AddField("cmd", JsonConvert.SerializeObject(dictionaries));

        this.gameObject.GetComponent<Network>().Request(www);
    }
}


public class SettingSave
{
    public int brightness;
    public int led_count;
    public bool autosave;
    public bool brightnessliveupdates;

    public SettingSave()
    {
        brightness = 255;
        led_count = 281;
        autosave = true;
        brightnessliveupdates = false;
    }
    public SettingSave(int brightness, int led_count, bool autosave, bool brightnessliveupdates) : this()
    {
        this.brightness = brightness;
        this.led_count = led_count;
        this.autosave = autosave;
        this.brightnessliveupdates = brightnessliveupdates;
    }

    public static void Save(SettingSave save)
    {
#if UNITY_ANDROID
        //Debug.Log("POG Datapath: " + Application.persistentDataPath);
        if (!Directory.Exists(Application.persistentDataPath + "/settings"))
            Directory.CreateDirectory(Application.persistentDataPath + "/settings");

        XmlSerializer serializer = new XmlSerializer(typeof(SettingSave));
        TextWriter writer = new StreamWriter(Application.persistentDataPath + "/settings/settings.xml");

        serializer.Serialize(writer, save);
        writer.Close();
#endif
    }
    public static SettingSave LoadSave()
    {
        SettingSave save;

#if UNITY_ANDROID
        //Debug.Log("Datapath: " + Application.persistentDataPath);
        if (!Directory.Exists(Application.persistentDataPath + "/settings"))
            Directory.CreateDirectory(Application.persistentDataPath + "/settings");

        if (File.Exists(Application.persistentDataPath + "/settings/settings.xml"))
        {
            using (FileStream fs = new FileStream(Application.persistentDataPath + "/settings/settings.xml", FileMode.Open))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SettingSave));
                save = (SettingSave)serializer.Deserialize(fs);
            }
        }
        else
        {
            save = new SettingSave();
        }
#endif
        return save;
    }
}
