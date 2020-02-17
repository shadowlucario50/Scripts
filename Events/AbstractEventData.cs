using System;
using System.Collections.Generic;
using System.Text;
using Server.Network;

namespace Script.Events
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
        public List<string> RegisteredCharacters { get; set; }

        public DateTime? CompletionTime { get; set; }


        public AbstractEventData()
        {
            this.PlayerData = new Dictionary<string, TPlayerData>();
            this.RegisteredCharacters = new List<string>();
        }

        public TPlayerData ExtendPlayer(Client client)
        {
            return ExtendPlayer(client.Player.CharID);
        }

        public TPlayerData ExtendPlayer(string charID)
        {
            if (!PlayerData.TryGetValue(charID, out var playerData))
            {
                playerData = new TPlayerData();

                PlayerData.Add(charID, playerData);
            }

            return playerData;
        }
    }
}
