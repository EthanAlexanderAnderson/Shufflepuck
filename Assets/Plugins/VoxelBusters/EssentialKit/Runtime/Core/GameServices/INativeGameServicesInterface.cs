using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary.NativePlugins;

namespace VoxelBusters.EssentialKit.GameServicesCore
{
    public interface INativeGameServicesInterface : INativeFeatureInterface
    {
        #region Methods

        void LoadLeaderboards(LeaderboardDefinition[] leaderboardDefinitions, LoadLeaderboardsInternalCallback callback);
        
        void ShowLeaderboard(string leaderboardId, string leaderboardPlatformId, LeaderboardTimeScope timeScope, ViewClosedInternalCallback callback);
        
        ILeaderboard CreateLeaderboard(string id, string platformId);

        void LoadAchievementDescriptions(LoadAchievementDescriptionsInternalCallback callback);

        void LoadAchievements(LoadAchievementsInternalCallback callback);
        
        void ShowAchievements(ViewClosedInternalCallback callback);

        void SetCanShowAchievementCompletionBanner(bool value);
        
        IAchievement CreateAchievement(string id, string platformId);
        
        void LoadPlayers(string[] playerIds, LoadPlayersInternalCallback callback);
        
        void SetAuthChangeCallback(AuthChangeInternalCallback callback);

        void Authenticate();

        void Signout();

        ILocalPlayer GetLocalPlayer();

        void LoadServerCredentials(LoadServerCredentialsInternalCallback callback);

        #endregion
    }
}