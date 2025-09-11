using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public GameObject MilesDisplay;
    public GameObject SpeedDisplay;
    public GameObject CoinDisplay;
    public GameObject LifeDisplay;
    public GameObject StatusDisplay;

    public GameObject dunzi;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private string miles_text;
    private string speed_text;
    private string coin_text;
    private string life_text;
    private string status_text;

    public Dictionary<string, string> STATUS_DICT = new()
    {
        {"invincible", LanguageManager.GetLocalizedString("invincible")}
    };
    void Start()
    {
        miles_text = MilesDisplay.GetComponent<Text>().text;
        speed_text = SpeedDisplay.GetComponent<Text>().text;
        coin_text = CoinDisplay.GetComponent<Text>().text;
        life_text = LifeDisplay.GetComponent<Text>().text;
        status_text = StatusDisplay.GetComponent<Text>().text;
    }

    // Update is called once per frame
    void Update()
    {
        MilesDisplay.GetComponent<Text>().text = miles_text + dunzi.GetComponent<Move>().GetMiles().ToString("0.00") + " m";
        SpeedDisplay.GetComponent<Text>().text = speed_text + dunzi.GetComponent<Move>().GetDisplaySpeed().ToString("0.00") + " m/s";
        CoinDisplay.GetComponent<Text>().text = coin_text + DataStorager.coin.count;
        LifeDisplay.GetComponent<Text>().text = life_text + dunzi.GetComponent<Move>().GetLife() + " / " + DataStorager.settings.CustomMaxLife;
        var buffTags = dunzi.GetComponent<Move>().buffTags;
        var buffTimes = dunzi.GetComponent<Move>().buffTimes;
        var display_tags = "";
        for(int i = 0; i < buffTags.Count;i ++){
            display_tags += STATUS_DICT[buffTags[i]] + " " + buffTimes[i].ToString("0.00") + "s\n";
        }
        StatusDisplay.GetComponent<Text>().text = status_text + display_tags;
    }
}
