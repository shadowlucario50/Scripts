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
        public static bool IsTestingEvent { get; set; }

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

        public static void SetEvent(Client client, string identifier, bool isTesting)
        {
            if (ActiveEvent != null)
            {
                Messenger.PlayerMsg(client, "An event has already been set.", Text.BrightRed);
                return;
            }

            Main.IsTestingEvent = isTesting;
                                
            var eventInstance = BuildEvent(identifier);

            if (eventInstance == null)
            {
                Messenger.PlayerMsg(client, $"Invalid event type: {identifier}", Text.BrightRed);
                return;
            }

            EventManager.ActiveEventIdentifier = eventInstance.Identifier;
            ActiveEvent = eventInstance;

            Messenger.PlayerMsg(client, $"The event has been set to {ActiveEvent.Name}!", Text.BrightGreen);
        }

        public static void StartEvent() 
        {
            if (ActiveEvent == null) 
            {
                return;
            }

            foreach (var registeredClient in EventManager.GetRegisteredClients())
            {
                Story story = new Story(Guid.NewGuid().ToString());
                StoryBuilderSegment segment = StoryBuilder.BuildStory();
                StoryBuilder.AppendSaySegment(segment, $"This event is... {ActiveEvent.Name}!", -1, 0, 0);
                StoryBuilder.AppendSaySegment(segment, ActiveEvent.IntroductionMessage, -1, 0, 0);

                if (ActiveEvent.Duration.HasValue) 
                {
                    StoryBuilder.AppendSaySegment(segment, $"The event will end in {ActiveEvent.Duration.Value.TotalMinutes} minutes.", -1, 0, 0);
                }
                if (Main.IsTestingEvent)
                {
                    StoryBuilder.AppendSaySegment(segment, $"This event is currently being tested and winners will not receive any prizes.", -1, 0, 0);
                } 
                else if (!string.IsNullOrEmpty(ActiveEvent.RewardMessage))
                {
                    StoryBuilder.AppendSaySegment(segment, ActiveEvent.RewardMessage, -1, 0, 0);
                }

                StoryBuilder.AppendSaySegment(segment, "The event has now begun!", -1, 0, 0);
                segment.AppendToStory(story);
                StoryManager.PlayStory(registeredClient, story);
            }

            ActiveEvent.Start();

            var eventStartMessage = new StringBuilder();
            if (Main.IsTestingEvent) 
            {
                eventStartMessage.Append("[Testing] ");
            }
            eventStartMessage.Append($"{ActiveEvent.Name} has started!");

            Task.Run(() => DiscordManager.Instance.SendAnnouncement(eventStartMessage.ToString()));
            Messenger.SendAnnouncement("Weekly Event", eventStartMessage.ToString());

            foreach (var registeredClient in EventManager.GetRegisteredClients())
            {
                ActiveEvent.ConfigurePlayer(registeredClient);
            }
        }

        public static void EndEvent()
        {
            ActiveEvent.End();
            Task.Run(() => DiscordManager.Instance.SendAnnouncement($"{ActiveEvent.Name} has finished!"));
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
