using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FileBrowserSet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FileBrowser.SetFilters( true, new FileBrowser.Filter( "Shidunzi Beatmap File", ".sdx"), new FileBrowser.Filter( "Shidunzi Beatmap Files Pack", ".sdp") );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openFileDialog(){
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
	{
		yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load" );

		if( FileBrowser.Success ){
			OnFilesSelected( FileBrowser.Result );
		}
	}

	void LoadSingleMap(string file_dir)
	{
		string dataFolder = $"{Application.persistentDataPath}/music";
		string tempPath = $"{dataFolder}/temp";
		if (Directory.Exists(tempPath))
		{
			FileBrowserHelpers.DeleteDirectory(tempPath);
		}
		Directory.CreateDirectory(tempPath);
		string post_dir = $"{dataFolder}/temp.sdx";
		FileBrowserHelpers.CopyFile(file_dir,post_dir);
		ZipFile.ExtractToDirectory(post_dir, tempPath);
		FileBrowserHelpers.DeleteFile(post_dir);
		string path = $"{dataFolder}/temp/data.sdz";
		if(!File.Exists(path)){
			FileBrowserHelpers.DeleteDirectory(tempPath);
			return;
		}
		string title = Random.Range(10000,99999).ToString();
		float level = 0;
		foreach( string line in File.ReadAllText(path).Split("\n")){
			string[] data = line.Split("=");
			if(data[0].Trim() == "title"){
				title = data[1].Trim();
			}
			if(data[0].Trim() == "level"){
				level = float.Parse(data[1].Trim());
			}
		}
		title += "_" + level.ToString();
		char[] invalidChars = Path.GetInvalidPathChars();
		foreach (char c in invalidChars)
		{
			title = title.Replace(c.ToString(), "");
		}
		string dest_path = $"{dataFolder}/{title}";
		if (Directory.Exists(dest_path))
		{
			FileBrowserHelpers.DeleteDirectory(dest_path);
		}
		Directory.Move(tempPath, dest_path);
	}

	void LoadMapPacks(string file_dir)
	{
		string dataFolder = $"{Application.persistentDataPath}/music";
		string tempPath = $"{dataFolder}/packtemp";
		if (Directory.Exists(tempPath))
		{
			FileBrowserHelpers.DeleteDirectory(tempPath);
		}
		Directory.CreateDirectory(tempPath);
		string post_dir = $"{dataFolder}/temp.sdp";
		FileBrowserHelpers.CopyFile(file_dir, post_dir);
		ZipFile.ExtractToDirectory(post_dir, tempPath);
		FileBrowserHelpers.DeleteFile(post_dir);
		DirectoryInfo dirInfo = new DirectoryInfo(tempPath);
		foreach (FileInfo file in dirInfo.GetFiles())
		{
			if (file.Name.Split(".").Last() == "sdx")
			{
				LoadSingleMap(Path.Join(file.DirectoryName,file.Name));
			}
		}
		FileBrowserHelpers.DeleteDirectory(tempPath);
	}

	void OnFilesSelected( string[] filePaths )
	{
		string dataFolder = $"{Application.persistentDataPath}/music";
		if(!Directory.Exists(dataFolder)){
            Directory.CreateDirectory(dataFolder);
        }

		for (int i = 0; i < filePaths.Length; i++)
		{
			string file_dir = filePaths[i];
			switch (file_dir.Split(".").Last())
			{
				case "sdx": LoadSingleMap(file_dir); break;
				case "sdp": LoadMapPacks(file_dir); break;
			}
		}
		SceneManager.LoadScene("MusicLobby");
	}
}
