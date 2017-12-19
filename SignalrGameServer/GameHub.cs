using CommonDataItems;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;

namespace SignalrGameServer
{
    public class GameHub : Hub
    {
        // static is used to protect the data acros diffrent hub invocations...
        // a queue of RegisteredPlayers...
        public static Queue<PlayerData> RegisteredPlayers = new Queue<PlayerData>(new PlayerData[]
        {
            new PlayerData { GamerTag = "Jordan Davies", imageName = "", playerID = Guid.NewGuid().ToString(), XP = 50 },
            new PlayerData { GamerTag = "Jonny Reed", imageName = "", playerID = Guid.NewGuid().ToString(), XP = 40 },
            new PlayerData { GamerTag = "James Craven", imageName = "", playerID = Guid.NewGuid().ToString(), XP = 70 },
            new PlayerData { GamerTag = "Niall Nulty", imageName = "", playerID = Guid.NewGuid().ToString(), XP = 90 }
        });

        // List of Players...
        public static List<PlayerData> Players = new List<PlayerData>();

        //The Stack that can be popped, contains the 4 Players outlined above...
        public static Stack<string> characters = new Stack<string>(
                    new string[] { "Player 4", "Player 3", "Player 2", "Player 1" });

        public void Hello()
        {
            Clients.All.hello();
        }

        public PlayerData Join()
        {
            if (characters.Count > 0)
            {
                // The pop Name...
                string character = characters.Pop();
                // Check Characters...
                if (RegisteredPlayers.Count > 0)
                {
                    PlayerData newPlayer = RegisteredPlayers.Dequeue();
                    newPlayer.imageName = character;
                    newPlayer.playerPosition = new Position
                    {
                        X = new Random().Next(700),
                        Y = new Random().Next(500)
                    };
                    // Show other clients that the new player has joined...
                    Clients.Others.Joined(newPlayer);
                    // tell the new client about all the other clients...
                    Clients.Caller.CurrentPlayers(Players);
                    // add the newPlayer to the server...
                    Players.Add(newPlayer);
                    return newPlayer;
                }
            }
            return null;
        }
    }
}