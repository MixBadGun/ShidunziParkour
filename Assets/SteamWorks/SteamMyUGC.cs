using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using Unity.VisualScripting;
using UnityEngine;

public class SteamMyUGC : MonoBehaviour
{
    private static SteamMyUGC instance;
    protected CallResult<CreateItemResult_t> m_CreateItemResult;
    protected CallResult<RemoteStorageUnsubscribePublishedFileResult_t> m_UnsubscribeItemResult;
    protected CallResult<SubmitItemUpdateResult_t> m_SubmitItemUpdateResult;
    protected CallResult<SteamUGCQueryCompleted_t> m_SteamUGCQueryResult;
    protected Callback<DownloadItemResult_t> m_DownloadItemResult;
    ulong publishedFileId;
    bool created = false;

    private HashSet<ulong> InstalledItems = new();

    public struct UGCData
    {
        public string Title;
        public string Description;
        public string previewImagePath;
        public string[] beatmapPathNames;
        public string[] tags;
    }

    private UGCData uGCData;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        instance = this;
    }

    private void OnEnable()
    {
        if (!SteamManager.Initialized)
            return;
        m_CreateItemResult = CallResult<CreateItemResult_t>.Create(OnCreateItemResult);
        m_SubmitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemUpdateResult);
        m_SteamUGCQueryResult = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryResult);
        m_UnsubscribeItemResult = CallResult<RemoteStorageUnsubscribePublishedFileResult_t>.Create(OnUnsubscribeItemResult);
        m_DownloadItemResult = Callback<DownloadItemResult_t>.Create(OnDownloadItemResult);
    }

    private void OnDownloadItemResult(DownloadItemResult_t param)
    {
        Debug.Log($"Download item result: {param.m_nPublishedFileId}");
        if (param.m_eResult == EResult.k_EResultOK)
        {
            SteamUGC.GetItemInstallInfo(param.m_nPublishedFileId, out ulong punSizeOnDisk, out string pchFolder, 260, out uint punTimeStamp);
            FileBrowserSet.LoadMapPacks($"{pchFolder}/{param.m_nPublishedFileId}.sdp");
        }
        else
        {
            Debug.LogError($"Failed to download item {param.m_nPublishedFileId}. {param.m_eResult}");
        }
    }

    private void OnUnsubscribeItemResult(RemoteStorageUnsubscribePublishedFileResult_t param, bool bIOFailure)
    {
        if (param.m_eResult == EResult.k_EResultOK)
        {
            Debug.Log("Item unsubscribed successfully.");
        }
        else
        {
            Debug.LogError($"Failed to unsubscribe item. {param.m_eResult}");
        }
    }

    private void OnSteamUGCQueryResult(SteamUGCQueryCompleted_t param, bool bIOFailure)
    {
        if (param.m_eResult != EResult.k_EResultOK || bIOFailure)
        {
            Debug.LogError("Failed to retrieve UGC items.");
            return;
        }

        for (uint index = 0; index < param.m_unTotalMatchingResults; index++)
        {
            if (SteamUGC.GetQueryUGCResult(param.m_handle, index, out SteamUGCDetails_t details))
            {
                if (InstalledItems.Contains(details.m_nPublishedFileId.m_PublishedFileId))
                {
                    Debug.Log($"{details.m_nPublishedFileId.m_PublishedFileId} pass");
                    continue;
                }
                Debug.Log($"{details.m_nPublishedFileId.m_PublishedFileId} not pass");
                SteamUGC.DownloadItem(details.m_nPublishedFileId, false);
            }
            else
            {
                Debug.LogError($"Failed to get details for item at index {index}.");
            }
        }

        SteamUGC.ReleaseQueryUGCRequest(param.m_handle);
    }

    private void OnSubmitItemUpdateResult(SubmitItemUpdateResult_t param, bool bIOFailure)
    {
        if (param.m_eResult == EResult.k_EResultOK)
        {
            MessageSystem.SendInfo(LanguageManager.GetLocalizedString("uploadinfo_completed"));
            Debug.Log("Item update submitted successfully.");
            created = false;
        }
        else
        {
            MessageSystem.SendInfo(LanguageManager.GetLocalizedString("uploadinfo_failed"));
            Debug.LogError($"Failed to submit item update. {param.m_eResult}");
        }
    }

    private void UpdateItem()
    {
        // 全部添加身份文件
        FileBrowserSet.AppendSteamworkIdentityFileToBeatmapFolders(uGCData.beatmapPathNames, publishedFileId.ToString());
        // 打包文件
        string output_file = FileBrowserSet.PackFiles(uGCData.beatmapPathNames, publishedFileId.ToString());

        UGCUpdateHandle_t updateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), new PublishedFileId_t(publishedFileId));
        if (!SteamUGC.SetItemTitle(updateHandle, uGCData.Title))
        {
            Debug.LogError("Failed to set item title.");
        }
        if (!SteamUGC.SetItemDescription(updateHandle, uGCData.Description))
        {
            Debug.LogError("Failed to set item desc.");
        }
        if (!SteamUGC.SetItemPreview(updateHandle, ResizeImage()))
        {
            Debug.LogError("Failed to set item preview image.");
        }
        if (!SteamUGC.SetItemContent(updateHandle, output_file))
        {
            Debug.LogError("Failed to set item content.");
        }
        if (!SteamUGC.SetItemVisibility(updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic))
        {
            Debug.LogError("Failed to set item visibility.");
        }
        if (!SteamUGC.SetItemTags(updateHandle, uGCData.tags))
        {
            Debug.LogError("Failed to set item tags.");
        }


        SteamAPICall_t handle = SteamUGC.SubmitItemUpdate(updateHandle, "new beatmap");
        m_SubmitItemUpdateResult.Set(handle);
    }

    private string ResizeImage()
    {
        string dataFolder = $"{Application.persistentDataPath}/temp";
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }

        byte[] fileData = File.ReadAllBytes(uGCData.previewImagePath);
        Texture2D texture = new(2, 2);
        texture.LoadImage(fileData);
        Texture2D temptexture = new(512, 512);
        temptexture.SetPixels(ScaleTexture(texture, 512, 512));
        byte[] bytes = temptexture.EncodeToPNG();
        File.WriteAllBytes($"{dataFolder}/resized.png", bytes);
        return $"{dataFolder}/resized.png";
    }

    private static Color[] ScaleTexture(Texture2D source, int newWidth, int newHeight)
    {
        Color[] sourcePixels = source.GetPixels();
        Color[] newPixels = new Color[newWidth * newHeight];
        
        float ratioX = (float)source.width / newWidth;
        float ratioY = (float)source.height / newHeight;
        
        for (int y = 0; y < newHeight; y++)
        {
            int sourceY = (int)(y * ratioY);
            for (int x = 0; x < newWidth; x++)
            {
                int sourceX = (int)(x * ratioX);
                newPixels[y * newWidth + x] = sourcePixels[sourceY * source.width + sourceX];
            }
        }
        
        return newPixels;
    }

    private void OnCreateItemResult(CreateItemResult_t call_result, bool bIOFailure)
    {
        if (call_result.m_eResult == EResult.k_EResultOK)
        {
            created = true;
            publishedFileId = call_result.m_nPublishedFileId.m_PublishedFileId;
            if (call_result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
            {
                SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + publishedFileId);
            }
            UpdateItem();
        }
        else
        {
            Debug.Log("Failed to create item. Error: " + call_result.m_eResult);
        }
    }

    private void m_UploadItem(UGCData uGCData)
    {
        if (!SteamManager.Initialized)
            return;

        this.uGCData = uGCData;
        if (!created)
        {
            SteamAPICall_t handle = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeCommunity);
            m_CreateItemResult.Set(handle);
        }
        else
        {
            UpdateItem();
        }
    }

    public static void UploadItem(UGCData uGCData)
    {
        instance.m_UploadItem(uGCData);
    }

    private void m_QueryItemsAndDownload()
    {
        if (!SteamManager.Initialized)
            return;

        UGCQueryHandle_t queryHandle = SteamUGC.CreateQueryUserUGCRequest(SteamUser.GetSteamID().GetAccountID(),
            EUserUGCList.k_EUserUGCList_Subscribed,
            EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items,
            EUserUGCListSortOrder.k_EUserUGCListSortOrder_SubscriptionDateDesc,
            SteamUtils.GetAppID(),
            SteamUtils.GetAppID(),
            1);

        SteamAPICall_t apiCall = SteamUGC.SendQueryUGCRequest(queryHandle);
        m_SteamUGCQueryResult.Set(apiCall);
    }

    public static void QueryItemsAndDownload()
    {
        instance.m_QueryItemsAndDownload();
    }

    // private bool QueryItemBelongToUser()
    // {
    //     if (!SteamManager.Initialized)
    //         return false;

    //     Steam
    // }

    private void m_AddInstalledID(ulong id)
    {
        InstalledItems.Add(id);
    }

    public static void AddInstalledID(ulong id)
    {
        instance.m_AddInstalledID(id);
    }

    public static void ClearInstalledID()
    {
        instance.m_ClearInstalledID();
    }

    private void m_ClearInstalledID()
    {
        InstalledItems.Clear();
    }

    private void m_UnsubscribeItem(ulong steamwork_identity)
    {
        if (!SteamManager.Initialized)
            return;
        SteamAPICall_t handle = SteamUGC.UnsubscribeItem(new PublishedFileId_t(steamwork_identity));
        m_UnsubscribeItemResult.Set(handle);
    }

    public static void UnsubscribeItem(ulong steamwork_identity)
    {
        instance.m_UnsubscribeItem(steamwork_identity);
    }
}
