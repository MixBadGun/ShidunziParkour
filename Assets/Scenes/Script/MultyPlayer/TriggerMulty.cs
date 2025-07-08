using System;
using System.IO;
using System.Linq;
using kcp2k;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class TriggerMulty : MonoBehaviour
{
    private MultyStart MultyObject;
    public InputField serverInput;
    public InputField portInput;
    public InputField playerIDInput;

    private void Start()
    {
        MultyObject = GameObject.Find("MultyScript").GetComponent<MultyStart>();
        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            // 启动服务器
            string serverip;
            ushort port;
            if (args.Contains("-S") || args.Contains("-server"))
            {
                if (args.Contains("-ip"))
                {
                    serverip = args[Array.IndexOf(args, "-ip") + 1];
                }
                else
                {
                    serverip = "localhost";
                }
                if (args.Contains("-port"))
                {
                    if (!ushort.TryParse(args[Array.IndexOf(args, "-port") + 1], out port))
                    {
                        throw new ArgumentException("Port is not valid!");
                    }
                }
                else
                {
                    port = 7892;
                }
                MultyObject.networkAddress = serverip;
                MultyObject.GetComponent<KcpTransport>().port = port;
                StartServer();
            }
            // 加载文件
            if (new[] { ".sdx", ".sdp" }.Contains(Path.GetExtension(args[1]).ToLower()))
            {
                FileBrowserSet.LoadFiles(new string[] { args[1] });
            }
        }
        playerIDInput.text = DataStorager.coninfo.playerID;
        serverInput.text = DataStorager.coninfo.ip;
        portInput.text = DataStorager.coninfo.port.ToString();
    }

    public void ForceStartClient()
    {
        ChangeConInfo();
        if (NetworkClient.isConnected)
        {
            NetworkClient.Disconnect();
        }
        MultyObject.StartClient();
    }

    void ChangeConInfo(){
        ushort port;
        if (!(serverInput.text.Length > 0))
        {
            return;
        }
        if (!ushort.TryParse(portInput.text, out port)){
            port = 7892;
        }
        DataStorager.coninfo.ip = serverInput.text;
        DataStorager.coninfo.port = port;
        DataStorager.coninfo.playerID = playerIDInput.text;
        MultyObject.networkAddress = DataStorager.coninfo.ip;
        MultyObject.GetComponent<KcpTransport>().port = DataStorager.coninfo.port;
        DataStorager.SaveConInfo();
    }

    public void StartServer()
    {
        MultyObject.StartServer();
    }
}
