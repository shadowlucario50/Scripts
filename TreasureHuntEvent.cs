using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Server;
using Server.Network;
using Server.Events;
using Server.Maps;
using System.Linq;
using Server.Combat;
using Server.Players;
using Server.Stories;

namespace Script
{
    public class TreasureHuntEvent : AbstractEvent<TreasureHuntEvent.TreasureHuntData>
    {
        public class TreasureHuntData : AbstractEventData
        {
            public class TreasureData
            {
                public string MapID { get; set; }
                public int X { get; set; }
                public int Y { get; set; }
                public bool Claimed { get; set; }
            }

            public TreasureData[] EventItems = Array.Empty<TreasureData>();
        }

        public override string Identifier => "treasurehunt";

        public override string Name => "Treasure Hunt";

        public override string IntroductionMessage => "Treasure has been scattered throughout the overworld! Find it all!";

        public static readonly int TreasureItemID = 980;

        public override void ConfigurePlayer(Client client)
        {
            base.ConfigurePlayer(client);

            if (Data.Started)
            {
                if (Ranks.IsDisallowed(client, Enums.Rank.Scripter))
                {
                    client.Player.KillableAnywhere = true;
                }
                client.Player.BeginTempStatMode(100, true);
            }
        }

        public override void DeconfigurePlayer(Client client)
        {
            base.DeconfigurePlayer(client);

            client.Player.KillableAnywhere = false;
            client.Player.EndTempStatMode();

            PacketHitList packetHitList = null;
            PacketHitList.MethodStart(ref packetHitList);

            Main.RefreshCharacterSpeedLimit(client.Player.GetActiveRecruit(), client.Player.Map, packetHitList);

            PacketHitList.MethodEnded(ref packetHitList);
        }

        public override void OnActivateMap(IMap map)
        {
            base.OnActivateMap(map);

            foreach (var eventItem in Data.EventItems.Where(x => !x.Claimed && x.MapID == map.MapID))
            {
                var existingItem = map.ActiveItem.Enumerate().Where(x => x.Num == TreasureItemID && x.X == eventItem.X && x.Y == eventItem.Y).FirstOrDefault();

                if (existingItem == null)
                {
                    map.SpawnItem(TreasureItemID, 1, false, false, "", true, eventItem.X, eventItem.Y, null);
                }
            }
        }

        public override void OnPickupItem(ICharacter character, int itemSlot, InventoryItem invItem)
        {
            base.OnPickupItem(character, itemSlot, invItem);

            if (character.CharacterType == Enums.CharacterType.Recruit)
            {
                var player = ((Recruit)character).Owner.Player;

                if (invItem.Num == TreasureItemID)
                {
                    var eventItem = Data.EventItems.Where(x => x.MapID == character.MapID && x.X == character.X && x.Y == character.Y).FirstOrDefault();

                    if (eventItem != null)
                    {
                        eventItem.Claimed = true;

                        var claimedCount = Data.EventItems.Where(x => x.Claimed).Count();

                        Messenger.GlobalMsg($"{player.DisplayName} found some treasure! ({claimedCount}/{Data.EventItems.Length})", Text.BrightGreen);
                    }
                }
            }
        }

        public override void OnDeath(Client client)
        {
            base.OnDeath(client);

            if (Data.Started)
            {
                var itemCount = client.Player.HasItem(TreasureItemID);

                if (itemCount > 0)
                {
                    var quantityToLose = (int)System.Math.Max(1, itemCount * 0.3f);

                    client.Player.TakeItem(TreasureItemID, quantityToLose);

                    Messenger.GlobalMsg($"{client.Player.DisplayName} lost {quantityToLose} treasure(s)!", Text.BrightGreen);

                    var claimedTreasures = Data.EventItems.Where(x => x.Claimed).ToList();
                    for (var i = 0; i < quantityToLose; i++)
                    {
                        if (claimedTreasures.Count > 0)
                        {
                            var index = Server.Math.Rand(0, claimedTreasures.Count);

                            claimedTreasures[index].Claimed = false;

                            claimedTreasures.RemoveAt(index);
                        }
                    }

                    ActivateTreasures();
                }
            }
        }

        public override void Start()
        {
            base.Start();

            Data.EventItems = new TreasureHuntData.TreasureData[] {
                new TreasureHuntData.TreasureData() { MapID = "s152", X = 21, Y = 15, Claimed = false },
                new TreasureHuntData.TreasureData() { MapID = "s152", X = 20, Y = 15, Claimed = false },
                new TreasureHuntData.TreasureData() { MapID = "s152", X = 22, Y = 15, Claimed = false },
            };

            foreach (var client in EventManager.GetRegisteredClients())
            {
                CleanupTreasures(client);
            }

            ActivateTreasures();
        }

        public override void End()
        {
            base.End();

            foreach (var eventItem in Data.EventItems)
            {
                var map = MapManager.RetrieveActiveMap(eventItem.MapID);

                if (map != null)
                {
                    for (var i = 0; i < map.ActiveItem.Length; i++)
                    {
                        if (map.ActiveItem[i].Num == TreasureItemID && map.ActiveItem[i].X == eventItem.X && map.ActiveItem[i].Y == eventItem.Y)
                        {
                            map.SpawnItemSlot(i, -1, 0, false, false, "", map.IsZoneOrObjectSandboxed(), eventItem.X, eventItem.Y, null);
                        }
                    }
                }
            }
        }

        private void ActivateTreasures()
        {
            var activatedMaps = new HashSet<string>();
            foreach (var client in EventManager.GetRegisteredClients())
            {
                if (!activatedMaps.Contains(client.Player.MapID))
                {
                    OnActivateMap(client.Player.Map);

                    activatedMaps.Add(client.Player.MapID);
                }
            }
        }

        private void CleanupTreasures(Client client)
        {
            var treasureCount = client.Player.HasItem(TreasureItemID);

            if (treasureCount > 0)
            {
                client.Player.TakeItem(TreasureItemID, treasureCount);
            }
        }

        public override void CleanPlayer(Client client)
        {
            base.CleanPlayer(client);

            CleanupTreasures(client);
        }

        protected override List<EventRanking> DetermineRankings()
        {
            var rankings = new List<EventRanking>();
            foreach (var client in EventManager.GetRegisteredClients())
            {
                var amount = client.Player.HasItem(TreasureItemID);

                if (amount > 0)
                {
                    rankings.Add(new EventRanking(client, amount));
                }
            }

            return rankings;
        }
    }
}

