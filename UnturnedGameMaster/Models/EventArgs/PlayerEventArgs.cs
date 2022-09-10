﻿using System;

namespace UnturnedGameMaster.Models.EventArgs
{
    public class PlayerEventArgs : System.EventArgs
    {
        public PlayerData Player;

        public PlayerEventArgs(PlayerData player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }
    }
}
