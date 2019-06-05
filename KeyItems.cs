namespace Script
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Server;
    using Server.Maps;
    using Server.Players;
    using Server.RDungeons;
    using Server.Combat;
    using Server.Pokedex;
    using Server.Items;
    using Server.Moves;
    using Server.Npcs;
    using Server.Stories;
    using Server.Exp;
    using Server.Network;
    using PMDCP.Sockets;
    using Server.Players.Parties;
    using Server.Logging;
    using Server.Missions;
    using Server.Events.Player.TriggerEvents;
    using Server.WonderMails;
    using Server.Tournaments;
    using Server.Database;
    using Server.Events;
    using DataManager.Players;


    public partial class Main
    {
        public static void ScriptedKeyItem(Client client, KeyItem keyItem, int slot) 
        {
            switch (keyItem.ItemID)
            {
                case 1839: // Costume box
                    {
                        Messenger.SendAvailableCostumes(client);
                    }
                    break;
                case 1842: // Drif-Flute
                    {
                        Messenger.SendAvailableFlyPoints(client);
                    }
                    break;
            }
        }
    }
}