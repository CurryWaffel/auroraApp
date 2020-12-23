using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        // Load the setting save if there is any
        save = new SettingSave();
        LoadSave();
    }

    /**
     * <summary>
     * Sets the brightness of the lamp for both save and lamp
     * </summary>
     */
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
    /**
     * <summary>
     * Sets the led count of the lamp for both save and lamp
     * </summary>
     */
    public void SetLedCount(InputField input)
    {
        WWWForm www = new WWWForm();

        Dictionary<string, object> dic = new Dictionary<string, object>();
        dic.Add("led_count", int.Parse(input.text));

        www.AddField("setting", JsonConvert.SerializeObject(dic));
        this.gameObject.GetComponent<Network>().Request(www);

        save.led_count = int.Parse(input.text);
        Save();
    }

    /**
     * <summary>
     * Sets the autosave feature on or off, depending on toggle state
     * </summary>
     */
    public void SetAutoSave(Toggle toggle)
    {
        save.autosave = toggle.isOn;
        Save();
    }
    /**
     * <summary>
     * Sets the live updates for brightness changes, depending on toggle state
     * </summary>
     */
    public void SetBrightnessLiveUpdates(Toggle toggle)
    {
        save.brightnessLiveUpdates = toggle.isOn;
        Save();

        if (save.brightnessLiveUpdates)
        {
            brightnessSlider.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>((fl) => { SetBrightness(brightnessSlider); }));
        } else
        {
            brightnessSlider.onValueChanged.RemoveAllListeners();
        }
    }

    /**
     * <summary>
     * Save the current save to a file
     * </summary>
     */
    public void Save()
    {
        SettingSave.Save(save);
    }
    /**
     * <summary>
     * Load a save from the file
     * </summary>
     */
    public void LoadSave()
    {
        save = SettingSave.LoadSave();

        brightnessSlider.value = save.brightness;
        led_countInput.text = save.led_count.ToString();
        autosaveToggle.isOn = save.autosave;
        brightnessliveupdatesToggle.isOn = save.brightnessLiveUpdates;
    }
}

/**
 * <summary>
 * Class representing a save for all values that can be changed in the non <see cref="Command"/> Specific settings
 * </summary>
 */
public class SettingSave
{
    public int brightness;
    public int led_count;
    public bool autosave;
    public bool brightnessLiveUpdates;

    public SettingSave()
    {
        brightness = 255;
        led_count = 281;
        autosave = true;
        brightnessLiveUpdates = false;
    }

    /**
     * <summary>
     * Saves the supplied save to a file via XML encoding
     * </summary>
     */
    public static void Save(SettingSave save) // TODO Change to JSON
    {
        if (!Directory.Exists(Application.persistentDataPath + "/settings"))
            Directory.CreateDirectory(Application.persistentDataPath + "/settings");

        XmlSerializer serializer = new XmlSerializer(typeof(SettingSave));
        TextWriter writer = new StreamWriter(Application.persistentDataPath + "/settings/settings.xml");

        serializer.Serialize(writer, save);
        writer.Close();
    }
    /**
     * <summary>
     * Loads the save from a file to a <see cref="SettingSave"/> object via XML decoding
     * </summary>
     */
    public static SettingSave LoadSave() // TODO Change to JSON
    {
        SettingSave save;
        
        // If the directory doesnt exist, create it
        if (!Directory.Exists(Application.persistentDataPath + "/settings"))
            Directory.CreateDirectory(Application.persistentDataPath + "/settings");

        // Read from file, handle nonexistent file cases
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

        return save;
    }
}
