using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.InputSystem;
using static DataManager;

public class DataStorager : MonoBehaviour
{
  // public GameObject dunzi;
  public static Item coin;
  public static ConnectionInfo coninfo;

  public static Item maxLife;

  public static DunziSettings settings;
  public static KeySettings keysettings;
  public static HistoryRecord historyRecord;

  private void Awake()
  {
    DontDestroyOnLoad(gameObject);
    historyRecord = InitHistory();
    settings = InitSettings();
    keysettings = InitKeySettings();
    coin = InitItem("coin", 0);
    maxLife = InitItem("life", 1);
    if (IsDataed("lastcon"))
    {
      coninfo = Load<ConnectionInfo>("lastcon");
    }
    else
    {
      coninfo = new()
      {
        ip = "",
        port = 7892,
        playerID = ""
      };
    }

    // 新手大礼包
    if (!historyRecord.isOldPlayer)
    {
      // TODO
    }
    if (!historyRecord.isHaveNewSkin)
    {
      GiveNewSkin();
      historyRecord.isHaveNewSkin = true;
    }
    SaveHistory();
  }

  private void GiveNewSkin(){
    if(!Directory.Exists($"{Application.persistentDataPath}/skin")){
      Directory.CreateDirectory($"{Application.persistentDataPath}/skin");
    }
    TextAsset skinObject = Resources.Load<TextAsset>("Skin/default");
    byte[] skinData = skinObject.bytes;
    string tempZipPath = Path.Combine(Application.temporaryCachePath, "temp.zip");
    File.WriteAllBytes(tempZipPath, skinData);
    string outputPath = $"{Application.persistentDataPath}/skin/";
    ZipFile.ExtractToDirectory(tempZipPath, outputPath);
    if(File.Exists(tempZipPath)) {
      File.Delete(tempZipPath);
    }
  }

  private Item InitItem(string item_name,int default_count = 0){
    if(IsDataed(item_name)){
      return Load<Item>(item_name);
    } else {
      return new (){
        name = item_name,
        count = default_count,
      };
    }
  }

  private DunziSettings InitSettings(){
    if(IsDataed("settings")){
      return Load<DunziSettings>("settings");
    } else {
      return new (){
        SoundVolume = 1f,
        MusicVolume = 1f,
        hasMotionBlur = true,
        CustomMaxLife = maxLife.count,
      };
    }
  }

  private KeySettings InitKeySettings(){
    if(IsDataed("keysettings")){
      return Load<KeySettings>("keysettings");
    } else {
      return GetDefaultKeySettings();
    }
  }

  private HistoryRecord InitHistory(){
    if(IsDataed("historyrecord")){
      return Load<HistoryRecord>("historyrecord");
    } else {
      return new();
    }
  }

  public static KeySettings GetDefaultKeySettings(){
    return new (){
        esc = new KeyCode[]{ KeyCode.Escape },
        left = new KeyCode[]{ KeyCode.A, KeyCode.LeftArrow },
        right = new KeyCode[]{ KeyCode.D, KeyCode.RightArrow },
        up = new KeyCode[]{ KeyCode.Space, KeyCode.W, KeyCode.UpArrow},
        down = new KeyCode[]{ KeyCode.DownArrow, KeyCode.S },
        pad1 = new KeyCode[]{KeyCode.Z,KeyCode.Keypad1,KeyCode.Alpha1},
        pad2 = new KeyCode[]{KeyCode.X,KeyCode.Keypad2,KeyCode.Alpha2},
        pad3 = new KeyCode[]{KeyCode.C,KeyCode.Keypad3,KeyCode.Alpha3}
    };
  }
  // private void Update(){
  //   if(!dunzi.GetComponent<Move>().isAlive()){

  //   }
  // }

  public static void SaveStatus(){
    Save("coin", coin);
  }

  public static void SaveConInfo(){
    Save("lastcon", coninfo);
  }

  public static void SaveMaxLife(){
    Save("life", maxLife);
  }

  public static void SaveSettings(){
    Save("settings", settings);
  }

  public static void SaveKeySettings(){
    Save("keysettings", keysettings);
  }

  public static void SaveHistory(){
    Save("historyrecord", historyRecord);
  }
}
