using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Server;
using Server.Combat;
using Server.Events;
using Server.Maps;
using Server.Network;
using Server.Players;

namespace Script.Events
{
    public class BossRushEvent : AbstractEvent<BossRushEvent.BossRushData, BossRushEvent.PlayerData>
    {
        private readonly int MaxRoomCount = 200;

        private readonly int MinionCount = 1;

        private readonly int StartTileX = 12;
        private readonly int StartTileY = 15;
        private readonly int CompleteTileX = 12;
        private readonly int CompleteTileY = 0;

        public override string Identifier => "bossrush";
        public override string Name => "Boss Rush";
        public override string IntroductionMessage => "Defeat the bosses and reach the end!";
        public override TimeSpan? Duration => new TimeSpan(0, 10, 0);

        private readonly List<int> minionNpcs = new List<int>() 
        {
            3941
        };

        private readonly Dictionary<Enums.PokemonType, int> typeMappings = new Dictionary<Enums.PokemonType, int>() 
        {
            { Enums.PokemonType.Normal, 1301 },
            { Enums.PokemonType.Grass, 1302 },
            { Enums.PokemonType.Water, 1303 },
            { Enums.PokemonType.Fire, 1304 },
            { Enums.PokemonType.Bug, 1305 },
            { Enums.PokemonType.Flying, 1306 },
            { Enums.PokemonType.Fighting, 1307 },
            { Enums.PokemonType.Psychic, 1308 },
            { Enums.PokemonType.Dark, 1309 },
            { Enums.PokemonType.Electric, 1310 },
            { Enums.PokemonType.Ghost, 1311 },
            { Enums.PokemonType.Rock, 1312 }
        };

        public class BossRushData : AbstractEventData<PlayerData>
        {
            public List<Enums.PokemonType> SelectedTypes { get; set; }

            public BossRushData() {
                this.SelectedTypes = new List<Enums.PokemonType>();
            }
        }

        public class PlayerData
        {
            public DateTime? StartTime { get; set; }

            public int CurrentRoom { get; set; }
            public int RoomsCleared { get; set; }
        }

        protected override void PrepareData()
        {
            var availableTypes = typeMappings.Keys.ToArray();

            for (var i = 0; i < MaxRoomCount; i++) {
                var selectedTypeIndex = Server.Math.Rand(0, availableTypes.Length);

                Data.SelectedTypes.Add(availableTypes[selectedTypeIndex]);
            }
        }

        public override void ConfigurePlayer(Client client)
        {
            base.ConfigurePlayer(client);

            if (Data.Started)
            {
                var playerData = Data.ExtendPlayer(client);

            }
        }

        public override void DeconfigurePlayer(Client client)
        {
            base.DeconfigurePlayer(client);

            var playerData = Data.ExtendPlayer(client);
        }

        protected override void OnboardNewPlayer(Client client) 
        {
            var playerData = Data.ExtendPlayer(client);

            playerData.CurrentRoom = 0;
            playerData.RoomsCleared = 0;
            playerData.StartTime = DateTime.UtcNow;

            WarpToCurrentRoom(client);
        }

        public override void OnActivateMap(IMap map)
        {
            for (var i = 0; i < MinionCount; i++)
            {
                var minionSlot = Server.Math.Rand(0, minionNpcs.Count);
                var minion = minionNpcs[minionSlot];

                var npc = new MapNpcPreset();
                npc.SpawnX = 9;
                npc.SpawnY = 9;
                npc.NpcNum = minion;
                npc.MaxLevel = 1;
                npc.MinLevel = 1;

                map.SpawnNpc(npc);
            }
        }

        public override void OnNpcDeath(PacketHitList hitlist, ICharacter attacker, MapNpc npc)
        {
            SetCompletionTile(MapManager.RetrieveActiveMap(attacker.MapID));
        }

        private void SetCompletionTile(IMap map)
        {
            Tile tile = new Tile(new DataManager.Maps.Tile());
            MapCloner.CloneTile(map, CompleteTileX, CompleteTileY, tile);
            tile.Data1 = 85;
            tile.Type = Enums.TileType.Scripted;

            var hitlist = new PacketHitList();
            PacketHitList.MethodStart(ref hitlist);

            foreach (var client in map.GetClients())
            {
                Messenger.SendTemporaryTileTo(hitlist, client, CompleteTileX, CompleteTileY, tile);
            }

            PacketHitList.MethodEnded(ref hitlist);
        }

        private void WarpToCurrentRoom(Client client) 
        {
            var playerData = Data.ExtendPlayer(client);

            var currentRoomMapId = typeMappings[Data.SelectedTypes[playerData.CurrentRoom]];

            Messenger.PlayerWarp(client, currentRoomMapId, StartTileX, StartTileY);
            Messenger.PlayerMsg(client, $"Welcome to room {playerData.CurrentRoom + 1}!", Text.BrightGreen);
        }

        public void CompleteRoom(Client client)
        {
            var playerData = Data.ExtendPlayer(client);

            playerData.CurrentRoom++;
            playerData.RoomsCleared++;

            WarpToCurrentRoom(client);
        }

        protected override List<EventRanking> DetermineRankings()
        {
            var rankings = new List<EventRanking>();

            foreach (var client in EventManager.GetRegisteredClients())
            {
                var playerData = Data.ExtendPlayer(client);

                rankings.Add(new EventRanking(client, playerData.RoomsCleared));
            }

            return rankings;
        }

        public override string HandoutReward(EventRanking eventRanking, int position, bool isTesting)
        {
            base.HandoutReward(eventRanking, position, isTesting);

            switch (position)
            {
                case 1:
                    {
                        if (!isTesting)
                        {
                            eventRanking.Client.Player.GiveItem(133, 10);
                        }
                        return "10 Arcade Tokens";
                    }
                case 2:
                    {
                        if (!isTesting)
                        {
                            eventRanking.Client.Player.GiveItem(133, 5);
                        }
                        return "5 Arcade Tokens";
                    }
                case 3:
                    {
                        if (!isTesting)
                        {
                            eventRanking.Client.Player.GiveItem(133, 3);
                        }
                        return "3 Arcade Tokens";
                    }
            }

            return "";
        }
    }
}
