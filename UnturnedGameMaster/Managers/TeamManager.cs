﻿using JetBrains.Annotations;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Models.EventArgs;
using UnturnedGameMaster.Providers;

namespace UnturnedGameMaster.Managers
{
    public class TeamManager : IManager
    {
        private DataManager dataManager;
        private PlayerDataManager playerDataManager;
        private LoadoutManager loadoutManager;
        private TeamIdProvider teamIdProvider;

        public event EventHandler<TeamMembershipEventArgs> OnPlayerJoinedTeam;
        public event EventHandler<TeamMembershipEventArgs> OnPlayerLeftTeam;
        public event EventHandler<TeamEventArgs> OnTeamCreated;
        public event EventHandler<TeamEventArgs> OnTeamRemoved;

        public TeamManager(DataManager dataManager, PlayerDataManager playerDataManager, LoadoutManager loadoutManager, TeamIdProvider teamIdProvider)
        {
            this.dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            this.playerDataManager = playerDataManager ?? throw new ArgumentNullException(nameof(playerDataManager));
            this.loadoutManager = loadoutManager ?? throw new ArgumentNullException(nameof(loadoutManager));
            this.teamIdProvider = teamIdProvider ?? throw new ArgumentNullException(nameof(teamIdProvider));
        }

        public void Init()
        { }

        public Team CreateTeam(string name, string description = "", Loadout defaultLoadout = null)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (teams.Values.Any(x => x.Name == name))
                return null;

            int teamId = teamIdProvider.GenerateId();
            int? loadoutId = null;

            if (defaultLoadout != null)
                loadoutId = defaultLoadout.Id;

            Team team = new Team(teamId, name, description, loadoutId, null, null);
            teams.Add(teamId, team);
            OnTeamCreated?.Invoke(this, new TeamEventArgs(team));

            return team;
        }

        public bool DeleteTeam(int id)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (!teams.ContainsKey(id))
                return false;

            // Make sure all players leave the team prior to deletion.
            foreach(PlayerData data in playerDataManager.GetPlayers())
            {
                if (data.TeamId == id)
                    LeaveTeam(UnturnedPlayer.FromCSteamID((CSteamID)data.Id));
            }

            Team team = teams[id];
            teams.Remove(id);
            OnTeamRemoved?.Invoke(this, new TeamEventArgs(team));
            return true;
        }

        public Team GetTeam(int id)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (!teams.ContainsKey(id))
                return null;

            return teams[id];
        }

        public Team GetTeamByName(string name)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            return teams.Values.FirstOrDefault(x => x.Name == name);
        }

        public int GetTeamPlayerCount(int id)
        {
            Dictionary<int, Team> teams = dataManager.GameData.Teams;
            if (!teams.ContainsKey(id))
                return 0;

            int playerCount = 0;
            foreach(PlayerData playerData in playerDataManager.GetPlayers())
            {
                if(playerData.TeamId == id)
                    playerCount++;
            }

            return playerCount;
        }

        public bool JoinTeam(UnturnedPlayer player, Team team)
        {
            PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
            if (playerData == null || playerData.TeamId != null) // player doesn't exist or is already in a team
                return false;

            playerData.TeamId = team.Id;
            OnPlayerJoinedTeam?.Invoke(this, new TeamMembershipEventArgs(player, team));
            return true;
        }

        public bool LeaveTeam(UnturnedPlayer player)
        {
            PlayerData playerData = playerDataManager.GetPlayer((ulong)player.CSteamID);
            if (playerData == null || playerData.TeamId == null) // player doesn't exist or does not belong to a team
                return false;

            Team team = GetTeam(playerData.TeamId.Value);
            playerData.TeamId = null;

            OnPlayerLeftTeam?.Invoke(this, new TeamMembershipEventArgs(player, team));
            return true;
        }

        public Team[] GetTeams()
        {
            return dataManager.GameData.Teams.Values.ToArray();
        }
    }
}
