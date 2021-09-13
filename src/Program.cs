using System;
using System.Text.RegularExpressions;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using ChessBot.Pgn;
using ChessBot.Pgn.Model;

namespace TwitchBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();
            Console.ReadLine();
        }
    }

    class Bot
    {
        TwitchClient client;

        String bot = "ninjamonkersbot";
        String channel = "ninjamonkers";

        Regex ChessMove = new Regex(@"(^|\s)(.{0,2}[a-hA-H][1-8].?)($|\s|,)");

        public Bot()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(bot, System.IO.File.ReadAllText(@"Password.txt"));
	        var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, channel);

            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnWhisperReceived += Client_OnWhisperReceived;
            client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnConnected += Client_OnConnected;

            client.Connect();
        }
  
        private void Client_OnLog(object sender, OnLogArgs e)
        {
            Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }
  
        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }
  
        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine($"Successfully joined {channel}");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            // Check if a user suggested a chess move in chat
            if (ChessMove.IsMatch(e.ChatMessage.Message))
            {
                // Grab current game
                var game = Lichess.GetCurrGameAsync("ninjamonkers").GetAwaiter().GetResult();

                // If I am in a rated game, delete the message to avoid cheating
                if(game != null && game.GetTagOrNull("Termination")?.Value == "Unterminated" && game.GetTagOrNull("Event")?.Value.Split(' ')[0] == "Rated")
                {
                    client.SendMessage(channel, $".delete {e.ChatMessage.Id}");
                    client.SendMessage(channel, "Please refrain from giving move suggestions while I am playing rated!");
                }
            }
        }
        
        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            client.SendWhisper(e.WhisperMessage.Username, "Hey! My whispers are still not implemented. :(");
        }
        
        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName}! Thanks for the Twitch Prime!");
            else
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName}! Thanks for the subscription!");
        }
    }
}
