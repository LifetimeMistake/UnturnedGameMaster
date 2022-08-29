﻿using Rocket.API;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Commands
{
    public class ManageTeamsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "teams";

        public string Help => "";

        public string Syntax => "<inspect/create/remove/getSpawn/setSpawn/setName/setDescription> <teamName/teamId> [<name/description/spawnpoint>]";
        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string> { "manage" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedChat.Say("augh");
        }
    }
}
