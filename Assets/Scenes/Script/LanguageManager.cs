using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class LanguageManager : MonoBehaviour
{
    private static LanguageManager instance;
    private StringTable table;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        instance = this;
        table = LocalizationSettings.StringDatabase.GetTable("LocalizationTable");
        LocalizationSettings.SelectedLocaleChanged += (locale) =>
        {
            table = LocalizationSettings.StringDatabase.GetTable("LocalizationTable");
        };
    }

    private string m_GetLocalizedString(string key)
    {

        var entry = table.GetEntry(key);
        if(entry == null)
        {
            return key;
        }
        return entry.GetLocalizedString();
    }

    public static string GetLocalizedString(string key)
    {
        return instance.m_GetLocalizedString(key);
    }
    
    public static void LoadLanguage(string locale)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(locale);
    }
}
