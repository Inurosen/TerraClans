using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Terraria;
using TShockAPI;

namespace TerraClans
{
	[APIVersion(1, 11)]
	public class TerraClans : TerrariaPlugin
	{
        public static string SavePath = "tshock";
        public static string terraPath = Path.Combine(SavePath, "terraclans");
        public static List<Player> Players = new List<Player>();

        public TerraClans(Main game)
            : base(game)
		{
		}
		public override void Initialize()
		{
            if (!Directory.Exists(terraPath))
                Directory.CreateDirectory(terraPath);
            TCcommand.Load();
            TCdb.InitTerraDB();
            Hooks.ServerHooks.Chat += OnChat;
            Hooks.NetHooks.GreetPlayer += OnGreetPlayer;
            Hooks.ServerHooks.Leave += OnLeave;
		}
		public override Version Version
		{
			get { return new Version("1.1.0.0202"); }
		}
		public override string Name
		{
			get { return "TerraClans"; }
		}
		public override string Author
		{
			get { return "Rosen"; }
		}
		public override string Description
		{
			get { return "In-game clans management system."; }
		}

        public void OnChat(messageBuffer msg, int sender, string text, HandledEventArgs e)
        {
            if (text.StartsWith("/"))
            {
                string cmd = text.Split(' ')[0];
                if (cmd == "/login")
                {
                    if (TShock.Players[sender].IsLoggedIn)
                    {
                        TCutils.TCGreetPlayer(sender);
                    }

                }
            }
            else
            {

            }
        }

        public void OnGreetPlayer(int who, HandledEventArgs e) 
        {
            lock (Players)
                Players.Add(new Player(who));
        }

        public void OnLeave(int whoami)
        {
            lock (Players)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].Index == whoami)
                    {
                        Players.RemoveAt(i);
                        break; //Found the player, break.
                    }
                }
            } 
        }
	}

	public class TCcommand
	{
		public static void Load()
		{
            TShockAPI.Commands.ChatCommands.Add(new Command(TClan.CMDtclan, "tclan"));
            TShockAPI.Commands.ChatCommands.Add(new Command(TCsay.CMDtcsay, "tcsay", "c"));
            TShockAPI.Commands.ChatCommands.Add(new Command("manageclans", TCman.CMDtcman, "tcman"));
            TShockAPI.Commands.ChatCommands.Add(new Command("foundclans", TCfound.CMDtcfound, "tcfound"));
		}

	}

    public class Player
    {
        public int Index { get; set; }
        public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }

        public Player(int index)
        {
            Index = index;
        }
    }
    
}