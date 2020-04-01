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
        public static readonly int OutlawPointInterval = 2000;
        public static readonly int OutlawPointRewardTimeInterval = 1000;

        private static int lastOutlawPointsTick = 0;

        public static void OutlawDefeated(Client attacker, Client defender)
        {
            if (attacker.Player.OutlawRole != Enums.OutlawRole.Hunter)
            {
                Messenger.PlayerMsg(attacker, $"You defeated an outlaw! However, as you were not a hunter, you did not earn any points.", Text.BrightGreen);
            }
        }

        public static void HandoutOutlawPoints(TickCount tickCount)
        {
            if (tickCount.Elapsed(lastOutlawPointsTick, OutlawPointRewardTimeInterval) || lastOutlawPointsTick == 0)
            {
                lastOutlawPointsTick = tickCount.Tick;

                foreach (var client in ClientManager.GetClients())
                {
                    if (client.Player.OutlawRole == Enums.OutlawRole.Outlaw && !client.Player.Dead) 
                    {
                        if (client.Player.Map.MapType == Enums.MapType.Standard && !client.Player.Map.IsZoneOrObjectSandboxed())
                        {
                            client.Player.PlayerData.PendingOutlawPoints += 1;
                        }
                    }
                }
            }
        }

        public static void HandleOutlawGameOver(Client client, ref PacketHitList hitList)
        {
            PacketHitList.MethodStart(ref hitList);

            if (client.Player.OutlawRole == Enums.OutlawRole.Outlaw)
            {
                var lostPoints = client.Player.PlayerData.PendingOutlawPoints % OutlawPointInterval;
                var gainedPoints = client.Player.PlayerData.PendingOutlawPoints - lostPoints;

                client.Player.PlayerData.LockedOutlawPoints += gainedPoints;

                Messenger.PlayerMsg(client, $"You have been defeated! You gained {gainedPoints} from this round!", Text.BrightGreen);
            }

            client.Player.OutlawRole = Enums.OutlawRole.None;
            client.Player.PlayerData.PendingOutlawPoints = 0;
            client.Player.KillableAnywhere = false;

            PacketBuilder.AppendPlayerData(client, hitList);

            PacketHitList.MethodEnded(ref hitList);
        }
    }
}