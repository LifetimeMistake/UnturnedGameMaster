﻿using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Models;
using UnturnedGameMaster.Services.Managers;

namespace UnturnedGameMaster.Commands.Teams
{
    public class AcceptInviteCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "accept";

        public string Help => "Akceptuje oczekujące zaproszenia do drużyny.";

        public string Syntax => "<team name/team id>";

        public List<string> Aliases => new List<string>() { "accept" };

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, "Musisz podać nazwę drużyny której zaproszenie chcesz przyjąć.");
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();

                if (gameManager.GetGameState() != Enums.GameState.InLobby)
                {
                    ChatHelper.Say(caller, "Nie można przyjmować zaproszeń do drużyn po rozpoczęciu gry!");
                    return;
                }

                PlayerData callerPlayerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                if (callerPlayerData == null)
                {
                    ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                    return;
                }

                if (callerPlayerData.TeamId.HasValue)
                {
                    ChatHelper.Say(caller, "Już należysz do drużyny!");
                    return;
                }

                string teamName = string.Join(" ", command);
                Team team = teamManager.ResolveTeam(teamName, false);
                if (team == null)
                {
                    ChatHelper.Say(caller, "Taka drużyna nie istnieje!");
                    return;
                }

                if (!team.GetInvitations().Any(x => x.TargetId == callerPlayerData.Id))
                {
                    ChatHelper.Say(caller, "Nie posiadasz oczekującego zaproszenia od tej drużyny.");
                    return;
                }

                if (!teamManager.AcceptInvitation(team, callerPlayerData))
                {
                    ChatHelper.Say(caller, "Nie udało się zaakceptować zaproszenia z powodu błedu systemu.");
                    return;
                }

                ChatHelper.Say(caller, "Zaakceptowano zaproszenie! Witaj na pokładzie!");
            }
            catch (Exception ex)
            {
                ExceptionHelper.Handle(ex, caller, $"Nie udało się zaakceptować zaproszenia z powodu błedu serwera: {ex.Message}");
            }
        }
    }
}