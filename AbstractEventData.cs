using System;
using System.Collections.Generic;
using System.Text;
using Server.Network;

namespace Script
{
    public abstract class AbstractEventData : AbstractEventData<AbstractEventData.NullPlayerData>
    {
        public class NullPlayerData
        {
        }
    }

    public abstract class AbstractEventData<TPlayerData> where TPlayerData : new()
    {
        public bool Started { get; set; }
        public Dictionary<string, TPlayerData> PlayerData { get; set; }

        public AbstractEventData()
        {
            this.PlayerData = new Dictionary<string, TPlayerData>();
        }

        public TPlayerData ExtendPlayer(Client client)
        {
            if (!PlayerData.TryGetValue(client.Player.CharID, out var playerData))
            {
                playerData = new TPlayerData();

                PlayerData.Add(client.Player.CharID, playerData);
            }

            return playerData;
        }
    }
}
