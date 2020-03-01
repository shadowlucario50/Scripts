using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Script.Events;
using Server;
using Server.Discord;
using Server.Events;
using Server.Leaderboards;
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

        public static Story BuildEventIntroStory()
        {
            Story story = new Story(Guid.NewGuid().ToString());
            StoryBuilderSegment segment = StoryBuilder.BuildStory();
            StoryBuilder.AppendCreateFNPCAction(segment, "0", "s152", 15, 11, 169, name: "Eventful", direction: Enums.Direction.Down, isShiny: true);
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Greetings!");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Welcome to our event for the week!");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Today, we have a special surprise for you.");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "\"We\", you wonder?");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Well, allow me to introduce our guest...");
            StoryBuilder.AppendMoveFNPCAction(segment, "0", 14, 11, Enums.Speed.Walking, true);
            StoryBuilder.AppendChangeFNPCDirAction(segment, "0", Enums.Direction.Right);
            StoryBuilder.AppendCreateFNPCAction(segment, "1", "s152", 15, 11, 571, name: "Zoro", direction: Enums.Direction.Down);
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Zoro!");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Thanks for coming, Zoro.");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 1, "Owooooo!!!!");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Yes, well, that's worrying.");
            StoryBuilder.AppendChangeFNPCDirAction(segment, "0", Enums.Direction.Down);
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Now, lets get this event started!");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "We'll be checking the leaderboards and handing out prizes.");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "First place in each category will receive 3 Arcade Tokens.");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Second will receive 2 Arcade Tokens.");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Third will receive 1 Arcade Token.");

            foreach (var leaderboard in LeaderBoardManager.ListLeaderboards()) 
            {
                var leaderboardItems = LeaderBoardManager.LoadLeaderboard(leaderboard.Counter).OrderByDescending(x => x.Value).ToList();

                StoryBuilder.AppendSpeechBubbleSegment(segment, 0, $"In the {leaderboard.Name} category...");

                if (leaderboardItems.Count > 0)
                {
                    StoryBuilder.AppendSpeechBubbleSegment(segment, 0, $"First place goes to {leaderboardItems[0].Name}!");
                }
                if (leaderboardItems.Count > 1)
                {
                    StoryBuilder.AppendSpeechBubbleSegment(segment, 0, $"Second place goes to {leaderboardItems[1].Name}!");
                }
                if (leaderboardItems.Count > 2)
                {
                    StoryBuilder.AppendSpeechBubbleSegment(segment, 0, $"Third place goes to {leaderboardItems[2].Name}!");
                }
            }

            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "That's every category!");
            StoryBuilder.AppendSpeechBubbleSegment(segment, 0, "Now let the event begin!");

            segment.AppendToStory(story);

            return story;
        }

        public static void RunEventIntro() 
        {
            foreach (var registeredClient in EventManager.GetRegisteredClients())
            {
                var story = BuildEventIntroStory();
                
                StoryManager.PlayStory(registeredClient, story);
            }
        }
    }
}
