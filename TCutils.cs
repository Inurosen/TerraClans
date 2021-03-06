﻿using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;

namespace TerraClans
{
    class TCutils
    {
        public static void ClanMsg(string plUser, string msg, int type, bool ignoreInit)
        {
            var clr = Color.GreenYellow;
            string clanName = "";
            string prefix = "Clan";
            var DBQuery = TCdb.DB.QueryReader("SELECT clanname FROM Clans WHERE leaders LIKE '%" + plUser + "%' OR members LIKE '%" + plUser + "%' OR invites LIKE '%" + plUser + "%'");
            while (DBQuery.Read())
            {
                clanName = DBQuery.Get<string>("clanname");
            }
            DBQuery = TCdb.DB.QueryReader("SELECT leaders, members FROM Clans WHERE (leaders LIKE '%" + plUser + "%' OR members LIKE '%" + plUser + "%') AND (clanname = '" + clanName + "')");
            List<string> recievers = new List<string>();
            while (DBQuery.Read())
            {
                string[] inClan = DBQuery.Get<string>("leaders").Split(',');
                foreach (string plr in inClan)
                {
                    recievers.Add(plr);
                    if (plUser == plr)
                    {
                        prefix = "Leader";
                        clr = Color.SpringGreen;
                    }
                }
                inClan = DBQuery.Get<string>("members").Split(',');
                foreach (string plr in inClan)
                {
                    recievers.Add(plr);
                }
            }
            if (recievers.Count > 0)
            {
                for (int i = 0; i < recievers.Count; i++)
                {
                    foreach (Player player in TerraClans.Players)
                    {
                        if (player.TSPlayer.UserAccountName == recievers[i])
                        {
                            if (type == 0) // Chat Message
                            {
                                player.TSPlayer.SendMessage("[" + prefix + "] " + TCutils.GetPlayersByUserName(plUser)[0].TSPlayer.Name + ":" + msg, clr);
                            }
                            else if (type == 1) // Clan announcer
                            {
                                if (ignoreInit)
                                {
                                    if (player.TSPlayer.UserAccountName != plUser)
                                    {
                                        player.TSPlayer.SendMessage(msg, Color.Yellow);
                                    }
                                }
                                else
                                {
                                    player.TSPlayer.SendMessage(msg, Color.Yellow);
                                }
                                
                            }
                        }
                    }
                }
            }
            else
            {
                TCutils.GetPlayersByUserName(plUser)[0].TSPlayer.SendMessage("You are not in clan!", Color.Red);
            }
        }

        public static bool AlreadyInClan(string uName)
        {
            List<string> plInClan = new List<string>();
            bool yes = false;
            var DBQuery = TCdb.DB.QueryReader("SELECT leaders, members FROM Clans WHERE leaders LIKE '%" + uName + "%' OR members LIKE '%" + uName + "%'");
            while (DBQuery.Read())
            {
                string[] inClan = DBQuery.Get<string>("leaders").Split(',');
                foreach (string plr in inClan)
                {
                    plInClan.Add(plr);
                }
                inClan = DBQuery.Get<string>("members").Split(',');
                foreach (string plr in inClan)
                {
                    plInClan.Add(plr);
                }
            }
            foreach (string i in plInClan)
            {
                if (i.Equals(uName))
                {
                    yes = true;
                }
            }
            return yes;
        }

        public static List<string> GetInvites(string clanName)
        {
            List<string> invites = new List<string>();
            var DBQuery = TCdb.DB.QueryReader("SELECT invites FROM Clans WHERE clanname='" + clanName + "'");
            while (DBQuery.Read())
            {
                string[] row = DBQuery.Get<string>("invites").Split(',');
                if (row.Length > 0)
                {
                    for (int i = 0; i < row.Length; i++)
                    {
                        if (row[i] != "")
                        {
                            invites.Add(row[i]);
                        }
                    }
                }
            }
            return invites;
        }

        public static List<string> GetAllInvites()
        {
            List<string> invites = new List<string>();
            var DBQuery = TCdb.DB.QueryReader("SELECT invites FROM Clans");
            while (DBQuery.Read())
            {
                string[] row = DBQuery.Get<string>("invites").Split(',');
                if (row.Length > 0)
                {
                    for (int i = 0; i < row.Length; i++)
                    {
                        if (row[i] != "")
                        {
                            invites.Add(row[i]);
                        }
                    }
                }
            }
            return invites;
        }

        public static List<string> GetMembers(string clanName)
        {
            List<string> members = new List<string>();
            var DBQuery = TCdb.DB.QueryReader("SELECT members FROM Clans WHERE clanname='" + clanName + "'");
            while (DBQuery.Read())
            {
                string[] row = DBQuery.Get<string>("members").Split(',');
                if (row.Length > 0)
                {
                    for (int i = 0; i < row.Length; i++)
                    {
                        if (row[i] != "")
                        {
                            members.Add(row[i]);
                        }
                    }
                }
            }
            return members;
        }

        public static List<string> GetLeaders(string clanName)
        {
            List<string> leaders = new List<string>();
            var DBQuery = TCdb.DB.QueryReader("SELECT leaders FROM Clans WHERE clanname='" + clanName + "'");
            while (DBQuery.Read())
            {
                string[] row = DBQuery.Get<string>("leaders").Split(',');
                if (row.Length > 0)
                {
                    for (int i = 0; i < row.Length; i++)
                    {
                        if (row[i] != "")
                        {
                            leaders.Add(row[i]);
                        }
                    }
                }
            }
            return leaders;
        }

        public static bool IsLeader(string uName) {
            bool yes = false;
            List<string> clans = new List<string>();
            var DBQuery = TCdb.DB.QueryReader("SELECT clanname FROM Clans WHERE leaders LIKE '%" + uName + "%'");
            while (DBQuery.Read())
            {
                clans.Add(DBQuery.Get<string>("clanname"));
            }
            if (clans.Count > 0)
            {
                for (int c = 0; c < clans.Count; c++)
                {
                    List<string> leaders = GetLeaders(clans[c]);
                    foreach (string i in leaders)
                    {
                        if (i == uName)
                        {
                            yes = true;
                        }
                    }
                }
            }
            return yes;
        }

        public static List<Player> GetPlayersByName(string plrName)
        {
            List<Player> players = new List<Player>();
            foreach (Player plrs in TerraClans.Players)
            {
                if (plrs.TSPlayer.Name == plrName)
                {
                    players.Add(plrs);
                }
            }
            return players;
        }

        public static List<Player> GetPlayersByUserName(string uName)
        {
            List<Player> players = new List<Player>();
            foreach (Player plrs in TerraClans.Players)
            {
                if (plrs.TSPlayer.UserAccountName == uName)
                {
                    players.Add(plrs);
                }
            }
            return players;
        }

        public static List<string> InQueue(string clanName, Player founder, Player cofounder)
        {
            List<string> result = new List<string>();
            if (!founder.TSPlayer.IsLoggedIn)
            {
                result.Add("You are not logged in!");
            }
            if (!cofounder.TSPlayer.IsLoggedIn)
            {
                result.Clear();
                result.Add(cofounder + " is not logged in!");
            }
            var DBQuery = TCdb.DB.QueryReader("SELECT clanname FROM FoundQueue WHERE clanname='" + clanName + "'");
            while (DBQuery.Read())
            {
                if(clanName == DBQuery.Get<string>("clanname"))
                {
                    result.Clear();
                    result.Add("This clan name is already occupied!");
                }
            }
            DBQuery = TCdb.DB.QueryReader("SELECT founder FROM FoundQueue WHERE founder='" + founder.TSPlayer.UserAccountName + "'");
            while (DBQuery.Read())
            {
                if (founder.TSPlayer.UserAccountName == DBQuery.Get<string>("founder"))
                {
                    result.Clear();
                    result.Add("You have already invited someone!");
                    result.Add("/tcfound revoke - revokes invitation.");
                }
            }
            DBQuery = TCdb.DB.QueryReader("SELECT cofounder FROM FoundQueue WHERE cofounder='" + founder.TSPlayer.UserAccountName + "'");
            while (DBQuery.Read())
            {
                if (founder.TSPlayer.UserAccountName == DBQuery.Get<string>("cofounder"))
                {
                    result.Clear();
                    result.Add("You are already invited by someone!");
                    result.Add("/tcfound accept - accepts invitation");
                    result.Add("/tcfound decline - declines invitation");

                }
            }
            if (founder != cofounder)
            {
                DBQuery = TCdb.DB.QueryReader("SELECT cofounder FROM FoundQueue WHERE cofounder='" + cofounder.TSPlayer.UserAccountName + "'");
                while (DBQuery.Read())
                {
                    if (cofounder.TSPlayer.UserAccountName == DBQuery.Get<string>("cofounder"))
                    {
                        result.Clear();
                        result.Add(cofounder.TSPlayer.Name + " is already invited by someone!");
                    }
                }
                DBQuery = TCdb.DB.QueryReader("SELECT founder FROM FoundQueue WHERE founder='" + cofounder.TSPlayer.UserAccountName + "'");
                while (DBQuery.Read())
                {
                    if (cofounder.TSPlayer.UserAccountName == DBQuery.Get<string>("founder"))
                    {
                        result.Clear();
                        result.Add(founder.TSPlayer.Name + " has already invited someone!");
                    }
                }
            }

            return result;
        }

        public static int UnixTimestamp()
        {
            int unixTime = (int)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
            return unixTime;
        }

        public static void TCGreetPlayer(int plr)
        {
            string clanName = "";
            string motd = "";
            bool valid = false;
            string plName = TShock.Players[plr].UserAccountName;
            var DBQuery = TCdb.DB.QueryReader("SELECT clanname, motd FROM Clans WHERE leaders LIKE '%" + plName + "%' OR members LIKE '%" + plName + "%'");
            while (DBQuery.Read())
            {
                clanName = DBQuery.Get<string>("clanname");
                motd = DBQuery.Get<string>("motd");
            }
            if (clanName != "")
            {
                List<string> members = new List<string>();
                members = TCutils.GetMembers(clanName);
                foreach (string i in members)
                {
                    if (i == plName)
                    {
                        valid = true;
                    }
                }
                List<string> leaders = new List<string>();
                leaders = TCutils.GetLeaders(clanName);
                foreach (string i in leaders)
                {
                    if (i == plName)
                    {
                        valid = true;
                    }
                }
                if (valid)
                {
                    TShock.Players[plr].SendMessage("[" + clanName + " MotD]:", Color.GreenYellow);
                    if (motd != "")
                    {
                        TShock.Players[plr].SendMessage(motd, Color.Yellow);
                    }
                    else
                    {
                        TShock.Players[plr].SendMessage("No message.", Color.Yellow);
                    }
                }
            }

            List<string> invites = new List<string>();
            invites = TCutils.GetAllInvites();
            bool isInvited = false;
            {
                for (int i = 0; i < invites.Count; i++)
                {
                    if (plName == invites[i])
                    {
                        isInvited = true;
                        break;
                    }
                }
            }
            if (isInvited)
            {
                DBQuery = TCdb.DB.QueryReader("SELECT clanname FROM Clans WHERE invites LIKE '%" + plName + "%'");
                while (DBQuery.Read())
                {
                    clanName = DBQuery.Get<string>("clanname");
                }

                TShock.Players[plr].SendMessage("You have been invited to join the clan: " + clanName, Color.Orange);
                TShock.Players[plr].SendMessage("/tclan accept - to join", Color.Orange);
                TShock.Players[plr].SendMessage("/tclan decline - to decline invitation.", Color.Orange);
            }

            string clCoFounder = "";
            string clName = "";
            string clFounder = "";
            DBQuery = TCdb.DB.QueryReader("SELECT * FROM FoundQueue WHERE cofounder == '" + plName + "' OR founder='" + plName + "'");
            while (DBQuery.Read())
            {
                clName = DBQuery.Get<string>("clanname");
                clFounder = DBQuery.Get<string>("founder");
                clCoFounder = DBQuery.Get<string>("cofounder");
            }
            if (clName != "" && clFounder != "" && clCoFounder == plName)
            {
                TShock.Players[plr].SendMessage("You have been invited by to found the clan: " + clanName, Color.Orange);
                TShock.Players[plr].SendMessage("/tcfound accept - to join", Color.Orange);
                TShock.Players[plr].SendMessage("/tcfound decline - to decline invitation.", Color.Orange);
            }
            else if (clName != "" && clFounder == plName && clCoFounder != "")
            {
                TShock.Players[plr].SendMessage("You are gonna found the clan: " + clanName, Color.Orange);
                TShock.Players[plr].SendMessage("/tcfound revoke - revokes invitation.", Color.Orange);
            }
        }

        public static List<string> GetClanByLeader(string uName)
        {
            List<string> clName = new List<string>();
            var DBQuery = TCdb.DB.QueryReader("SELECT clanname, leaders FROM Clans WHERE leaders LIKE '%" + uName + "%'");
            while (DBQuery.Read())
            {
                string[] arr = DBQuery.Get<string>("leaders").Split(',');
                foreach (string i in arr)
                {
                    if (i == uName)
                    {
                        clName.Add(DBQuery.Get<string>("clanname"));
                        break;
                    }
                }

            }
            return clName;
        }

        public static List<string> GetClanByMember(string uName)
        {
            List<string> clName = new List<string>();
            var DBQuery = TCdb.DB.QueryReader("SELECT clanname, members FROM Clans WHERE members LIKE '%" + uName + "%'");
            while (DBQuery.Read())
            {
                string[] arr = DBQuery.Get<string>("members").Split(',');
                foreach (string i in arr)
                {
                    if (i == uName)
                    {
                        clName.Add(DBQuery.Get<string>("clanname"));
                        break;
                    }
                }

            }
            return clName;
        }

        public static bool IsValidMember(string mName, string lName)
        {
            bool yes = false;

            List<string> clName = GetClanByLeader(lName);
            if (clName.Count < 1 || clName.Count > 1)
            {
                return yes;
            }
            List<string> clMembers = GetMembers(clName[0]);
            foreach (string i in clMembers)
            {
                if (i == mName)
                {
                    yes = true;
                }
            }
            List<string> clLeaders = GetLeaders(clName[0]);
            foreach (string i in clLeaders)
            {
                if (i == mName)
                {
                    yes = true;
                }
            }
            return yes;
        }
    }
}
