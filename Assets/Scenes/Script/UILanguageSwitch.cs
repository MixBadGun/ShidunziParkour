using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class UILanguageSwitch : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown languageDropdown;

    public void Start()
    {
        //清空下拉框选项
        languageDropdown.ClearOptions();
        //获取LocalizationSettings中AvailableLocales的本地化语言列表
        List<UnityEngine.Localization.Locale> locales = LocalizationSettings.AvailableLocales.Locales;
        List<string> options = new List<string>();
        int currentLocaleIndex = 0;
        for (int i = 0; i < locales.Count; i++)
        {
            //将本地化语言的名称添加到下拉框选项列表中
            options.Add(locales[i].Identifier.CultureInfo.NativeName);
            //获取当前使用的本地化语言在列表中的下标
            if (locales[i] == LocalizationSettings.SelectedLocale)
            {
                currentLocaleIndex = i;
            }
        }
        //将选项列表添加到下拉框中
        languageDropdown.AddOptions(options);
        //设置下拉框当前选中选项为当前使用的本地化语言
        languageDropdown.value = currentLocaleIndex;
        languageDropdown.RefreshShownValue();
    }

    public void SelectLanguage()
    {
        //将下拉框当前选中选项的下标作为参数设置到LocalizationSettings的SelectedLocale达到实现语言切换的效果
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageDropdown.value];
    }
}
