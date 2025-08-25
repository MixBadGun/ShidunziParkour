using UnityEngine;
using System.Collections;
using Steamworks;
using System;
using System.Collections.Generic;

class SteamStatsAndAchievements : MonoBehaviour {
	private static SteamStatsAndAchievements s_instance;

	public enum Achievement : int
	{
		NEW_ACHIEVEMENT_1_0,
		MUSIC_ACHIEVEMENT_NULL
	};

	// 成就字典表
	private Dictionary<Achievement, Achievement_t> Achievements = new Dictionary<Achievement, Achievement_t> {
		{ Achievement.NEW_ACHIEVEMENT_1_0, new Achievement_t(Achievement.NEW_ACHIEVEMENT_1_0, "打开石墩子", "打开你的石墩子。") },
		{ Achievement.MUSIC_ACHIEVEMENT_NULL, new Achievement_t(Achievement.MUSIC_ACHIEVEMENT_NULL, "裸露石墩爱好者", "在音乐模式下，避开了所有裸露的石墩子，但是却甘愿撞击被保护的石墩子。") }
	};

	// 统计数据字典表
	private Dictionary<string, int> Stats = new Dictionary<string, int> {
		{ "stat_mile", 0 },
		{ "stat_sss_times", 0 },
		{ "stat_sss_plus_times", 0 },
		{ "stat_clear_times", 0 }
	};

	// 是否已经获取成就到本地
	private bool m_storedAchievements;

	void Awake()
	{
		s_instance = this;
		DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
	{
		if (!SteamManager.Initialized)
			return;

		if (!m_storedAchievements)
		{
			UpdateLocalAchievements();
			UpdateLocalStats();
			m_storedAchievements = true;
		}
	}

	// 开始阶段可以解锁的成就
	void Start()
	{
		// 开始阶段可以解锁的成就
		UnlockAchievement(Achievement.NEW_ACHIEVEMENT_1_0);
	}

	float interval_time = 0;
	void Update()
    {
		// 定期上传数据
		interval_time += Time.deltaTime;
		if (interval_time >= 10f) // 每10秒上传一次
		{
			UploadLocalStats();
			interval_time = 0;
		}
    }

    //-----------------------------------------------------------------------------
    // 解锁成就，如果已经解锁则跳过
    //-----------------------------------------------------------------------------
    private void m_UnlockAchievement(Achievement achievement)
	{
		if (!SteamManager.Initialized)
			return;

		if (Achievements[achievement].m_bAchieved)
		{
			return;
		}
		Achievements[achievement].m_bAchieved = true;
		SteamUserStats.SetAchievement(achievement.ToString());
	}
	
	public static void UnlockAchievement(Achievement achievement) {
		s_instance.m_UnlockAchievement(achievement);
	}
	
	//-----------------------------------------------------------------------------
	// 更新本地成就
	//-----------------------------------------------------------------------------
	private void UpdateLocalAchievements()
	{
		if (!SteamManager.Initialized)
			return;

		foreach (Achievement_t ach in Achievements.Values)
		{
			bool ret = SteamUserStats.GetAchievement(ach.m_eAchievementID.ToString(), out ach.m_bAchieved);
			if (ret)
			{
				ach.m_strName = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "name");
				ach.m_strDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.m_eAchievementID.ToString(), "desc");
			}
			else
			{
				Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.m_eAchievementID + "\nIs it registered in the Steam Partner site?");
			}
		}
	}
	
	//-----------------------------------------------------------------------------
	// 更新本地统计数据
	//-----------------------------------------------------------------------------
	private void UpdateLocalStats()
	{
		if (!SteamManager.Initialized)
			return;

		var keys = new List<string>(Stats.Keys);
		foreach (var key in keys)
		{
            SteamUserStats.GetStat(key, out int value);
            Stats[key] = value;
		}
	}

	//-----------------------------------------------------------------------------
	// 单增本地统计数据，但是并不上传
	//-----------------------------------------------------------------------------
	private void m_AppendLocalStats(string target, int nums)
	{
		if (Stats.ContainsKey(target))
		{
			Stats[target] += nums;
		}
	}

	public static void AppendLocalStats(string target, int nums)
	{
		s_instance.m_AppendLocalStats(target, nums);
	}

	//-----------------------------------------------------------------------------
	// 上传本地统计数据
	//-----------------------------------------------------------------------------
	private void UploadLocalStats()
	{
		if (!SteamManager.Initialized)
			return;

		var keys = new List<string>(Stats.Keys);
		foreach (var key in keys)
		{
			SteamUserStats.SetStat(key, Stats[key]);
		}
		SteamUserStats.StoreStats();
	}

	private class Achievement_t
	{
		public Achievement m_eAchievementID;
		public string m_strName;
		public string m_strDescription;
		public bool m_bAchieved;

		/// <summary>
		/// Creates an Achievement. You must also mirror the data provided here in https://partner.steamgames.com/apps/achievements/yourappid
		/// </summary>
		/// <param name="achievement">The "API Name Progress Stat" used to uniquely identify the achievement.</param>
		/// <param name="name">The "Display Name" that will be shown to players in game and on the Steam Community.</param>
		/// <param name="desc">The "Description" that will be shown to players in game and on the Steam Community.</param>
		public Achievement_t(Achievement achievementID, string name, string desc)
		{
			m_eAchievementID = achievementID;
			m_strName = name;
			m_strDescription = desc;
			m_bAchieved = false;
		}
	}
}
