﻿using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Managers.EventMessageManagers
{
    public class ArenaEventMessageManager : IDisposableManager
    {
        [InjectDependency]
        private ArenaManager arenaManager { get; set; }
        [InjectDependency]
        private TeamManager teamManager { get; set; }

        public void Init()
        {
            arenaManager.OnBossFightCompleted += ArenaManager_OnBossFightCompleted;
            arenaManager.OnBossFightFailed += ArenaManager_OnBossFightFailed;
            arenaManager.OnBossFightCreated += ArenaManager_OnBossFightCreated;
            arenaManager.OnBossFightDominantTeamChanged += ArenaManager_OnBossFightDominantTeamChanged;
        }

        public void Dispose()
        {
            arenaManager.OnBossFightCompleted -= ArenaManager_OnBossFightCompleted;
            arenaManager.OnBossFightFailed -= ArenaManager_OnBossFightFailed;
            arenaManager.OnBossFightCreated -= ArenaManager_OnBossFightCreated;
            arenaManager.OnBossFightDominantTeamChanged -= ArenaManager_OnBossFightDominantTeamChanged;
        }

        private void ArenaManager_OnBossFightDominantTeamChanged(object sender, Models.EventArgs.BossFightDominationEventArgs e)
        {
            Team dominantTeam = e.NewAttackers;
            Team oldTeam = e.OldAttackers;

            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(dominantTeam))
            {
                ChatHelper.Say(player, $"Twoja drużyna uzyskała status dominującej w arenie");
            }

            foreach (PlayerData player in teamManager.GetOnlineTeamMembers(oldTeam))
            {
                ChatHelper.Say(player, $"Twoja drużyna straciła status dominującej w arenie");
            }
        }

        private void ArenaManager_OnBossFightCreated(object sender, Models.EventArgs.BossFightEventArgs e)
        {
            Team team = e.BossFight.DominantTeam;
            BossArena arena = e.BossFight.Arena;

            ChatHelper.Say($"Drużyna \"{team.Name}\" rozpoczęła walkę z boss'em \"{arena.BossModel.Name}\"");
        }

        private void ArenaManager_OnBossFightFailed(object sender, Models.EventArgs.BossFightEventArgs e)
        {
            Team team = e.BossFight.DominantTeam;
            BossArena arena = e.BossFight.Arena;

            ChatHelper.Say($"Drużyna \"{team.Name}\" nie zdołała pokonać boss'a \"{arena.BossModel.Name}\"");
        }

        private void ArenaManager_OnBossFightCompleted(object sender, Models.EventArgs.BossFightEventArgs e)
        {
            Team team = e.BossFight.DominantTeam;
            BossArena arena = e.BossFight.Arena;

            Color red = UnturnedChat.GetColorFromRGB(255, 0, 0);

            // used UnturnedChat to give the message a color, because kil boss cool B)
            UnturnedChat.Say($"Drużyna \"{team.Name}\" pokonała boss'a \"{arena.BossModel.Name}\" i otrzymała jego klucz!", red);
        }
    }
}
