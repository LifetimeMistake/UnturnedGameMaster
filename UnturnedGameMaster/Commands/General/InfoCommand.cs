﻿using Rocket.API;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Helpers;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands.General
{
    public class InfoCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "info";

        public string Help => "Wyświetla informacje o graczach, drużynach i innych obiektach!";

        public string Syntax => "<player/team/game> [<name/id>]";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                ChatHelper.Say(caller, $"Musisz podać argument.");
                ShowSyntax(caller);
                return;
            }

            string[] verbArgs = command.Skip(1).ToArray();
            switch (command[0].ToLowerInvariant())
            {
                case "player":
                    VerbPlayerInfo(caller, verbArgs);
                    break;
                case "team":
                    VerbTeamInfo(caller, verbArgs);
                    break;
                case "game":
                    VerbGameInfo(caller);
                    break;
                default:
                    ChatHelper.Say(caller, $"Nieprawidłowy argument.");
                    ShowSyntax(caller);
                    break;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            ChatHelper.Say(caller, $"/{Name} {Syntax}");
        }

        private void VerbPlayerInfo(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                PlayerData playerData;

                if (command.Length == 0)
                {
                    playerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                    if (playerData == null)
                    {
                        ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                        return;
                    }
                }
                else
                {
                    string searchTerm = string.Join(" ", command);
                    playerData = playerDataManager.ResolvePlayer(searchTerm, false);
                    if (playerData == null)
                    {
                        ChatHelper.Say(caller, $"Nie znaleziono gracza \"{searchTerm}\"");
                        return;
                    }
                }

                StringBuilder sb = new StringBuilder();
                foreach (string line in playerDataManager.GetPlayerSummary(playerData).Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    sb.AppendLine(line);
                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się odnaleźć danych gracza z powodu błedu serwera: {ex.Message}");
                return;
            }
        }

        private void VerbTeamInfo(IRocketPlayer caller, string[] command)
        {
            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                TeamManager teamManager = ServiceLocator.Instance.LocateService<TeamManager>();
                Team team;

                if (command.Length == 0)
                {
                    PlayerData playerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                    if (playerData == null)
                    {
                        ChatHelper.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                        return;
                    }

                    if (!playerData.TeamId.HasValue)
                    {
                        ChatHelper.Say(caller, "Nie należysz do żadnej z drużyn.");
                        return;
                    }

                    team = teamManager.GetTeam(playerData.TeamId.Value);
                }
                else
                {
                    string searchTerm = string.Join(" ", command);
                    team = teamManager.ResolveTeam(searchTerm, false);
                }

                if (team == null)
                {
                    ChatHelper.Say(caller, $"Nie znaleziono drużyny");
                    return;
                }

                ChatHelper.Say(caller, teamManager.GetTeamSummary(team));
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się odczytać danych drużyny z powodu błędu serwera: {ex.Message}");
            }
        }

        private void VerbGameInfo(IRocketPlayer caller)
        {
            try
            {
                GameManager gameManager = ServiceLocator.Instance.LocateService<GameManager>();
                StringBuilder sb = new StringBuilder();
                foreach (string line in gameManager.GetGameSummary().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                    sb.AppendLine(line);
                ChatHelper.Say(caller, sb);
            }
            catch (Exception ex)
            {
                ChatHelper.Say(caller, $"Nie udało się odczytać stanu gry z powodu błędu serwera: {ex.Message}");
            }
        }
    }
}
