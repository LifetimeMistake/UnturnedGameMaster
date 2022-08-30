﻿using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnturnedGameMaster.Autofac;
using UnturnedGameMaster.Managers;
using UnturnedGameMaster.Models;

namespace UnturnedGameMaster.Commands.Admin
{
    public class GiveLoadoutCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "giveloadout";

        public string Help => "";

        public string Syntax => "<loadoutname/loadoutid> [<playername/playerid>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>();

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, $"Musisz podać argument.");
                ShowSyntax(caller);
                return;
            }

            try
            {
                PlayerDataManager playerDataManager = ServiceLocator.Instance.LocateService<PlayerDataManager>();
                LoadoutManager loadoutManager = ServiceLocator.Instance.LocateService<LoadoutManager>();
                PlayerData playerData;

                Loadout loadout = loadoutManager.ResolveLoadout(command[0], false);
                if (loadout == null)
                {
                    UnturnedChat.Say(caller, "Nie znaleziono zestawu");
                    return;
                }

                if (command.Length == 1)
                {
                    playerData = playerDataManager.GetPlayer((ulong)((UnturnedPlayer)caller).CSteamID);
                    if (playerData == null)
                    {
                        UnturnedChat.Say(caller, "Wystąpił błąd (nie można odnaleźć profilu gracza??)");
                        return;
                    }
                }
                else
                {
                    playerData = playerDataManager.ResolvePlayer(command[1], false);
                    if (playerData == null)
                    {
                        UnturnedChat.Say(caller, $"Nie znaleziono gracza \"{command[1]}\"");
                        return;
                    }
                }


                loadoutManager.GiveLoadout(UnturnedPlayer.FromCSteamID((CSteamID)playerData.Id), loadout);
                UnturnedChat.Say(caller, "Nadano zestaw wyposażenia");
            }
            catch (Exception ex)
            {
                UnturnedChat.Say(caller, $"Nie udało się nadać zestawu z powodu błedu serwera: {ex.Message}");
                return;
            }
        }

        private void ShowSyntax(IRocketPlayer caller)
        {
            UnturnedChat.Say(caller, $"/{Name} {Syntax}");
        }
    }
}
