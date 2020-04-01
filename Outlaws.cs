namespace Script
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    using Server;
    using Server.Maps;
    using Server.Players;
    using Server.RDungeons;
    using Server.Dungeons;
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
    using Server.Events;
    using Server.Trading;
    using Server.SecretBases;

    using DataManager.Players;
    using Server.Database;
    using Script.Models;
    using Server.Events.World;
    using Server.Legendaries;

    public partial class Main
    {
        public static void HandleOutlawGameOver(Client client, ref PacketHitList hitList)
        {
            PacketHitList.MethodStart(ref hitList);

            client.Player.OutlawRole = Enums.OutlawRole.None;
            client.Player.PlayerData.PendingOutlawPoints = 0;
            client.Player.KillableAnywhere = false;

            PacketBuilder.AppendPlayerData(client, hitList);

            PacketHitList.MethodEnded(ref hitList);
        }
    }
}