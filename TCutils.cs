using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;

namespace TerraClans
{
    class TCutils
    {
        public static void ClanMsg(TSPlayer plObj, string msg, int type)
        {
            var clr = Color.GreenYellow;
            string clanName = "";
            string prefix = "Clan";
            var DBQuery = TCdb.DB.QueryReader("SELECT clanname FROM Clans WHERE leaders LIKE '%" + plObj.UserAccountName + "%' OR members LIKE '%" + plObj.UserAccountName + "%' OR invites LIKE '%" + plObj.UserAccountName + "%'");
            while (DBQuery.Read())
            {
                clanName = DBQuery.Get<string>("clanname");
            }
            DBQuery = TCdb.DB.QueryReader("SELECT leaders, members FROM Clans WHERE (leaders LIKE '%" + plObj.UserAccountName + "%' OR members LIKE '%" + plObj.UserAccountName + "%') AND (clanname = '" + clanName + "')");
            List<string> recievers = new List<string>();
            while (DBQuery.Read())
            {
                string[] inClan = DBQuery.Get<string>("leaders").Split(',');
                foreach (string plr in inClan)
                {
                    recievers.Add(plr);
                    if (plObj.UserAccountName == plr)
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
                                player.TSPlayer.SendMessage("[" + prefix + "] " + plObj.Name + ":" + msg, clr);
                            }
                            else if (type == 1) // Clan announcer
                            {
                                player.TSPlayer.SendMessage(msg, Color.Yellow);
                            }
                        }
                    }
                }
            }
            else
            {
                plObj.SendMessage("You are not in clan!", Color.Red);
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

    }
}
