using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Script.Events;
using Server;
using Server.Discord;
using Server.Events;
using Server.Network;
using Server.Stories;

namespace Script
{
    public partial class Main
    {
        public static readonly int EventHubMap = 152;
        public static readonly int EventHubMapX = 15;
        public static readonly int EventHubMapY = 16;

        public static IEvent ActiveEvent { get; set; }

        public static IEvent BuildEvent(string identifier)
        {
            switch (identifier)
            {
                case "treasurehunt":
                    return new TreasureHuntEvent();
                case "shinyspectacular":
                    return new ShinySpectacular();
                case "paintball":
                    return new Paintball();
                case "werewolf":
                    return new WerewolfEvent();
                default:
                    return null;
            }
        }

        public static void EndEvent()
        {
            ActiveEvent.End();
            Task.Run(() => DiscordManager.Instance.SendAnnouncement($"{ActiveEvent.Name} has started!"));
            Messenger.GlobalMsg($"{ActiveEvent.Name} has finished!", Text.BrightGreen);

            foreach (var registeredClient in EventManager.GetRegisteredClients())
            {
                ActiveEvent.DeconfigurePlayer(registeredClient);

                Story story = new Story(Guid.NewGuid().ToString());
                StoryBuilderSegment segment = StoryBuilder.BuildStory();
                StoryBuilder.AppendSaySegment(segment, $"The event is now finished!", -1, 0, 0);
                StoryBuilder.AppendSaySegment(segment, $"Please wait as a winner is announced...", -1, 0, 0);
                segment.AppendToStory(story);
                StoryManager.PlayStory(registeredClient, story);
            }
        }
    }
}
