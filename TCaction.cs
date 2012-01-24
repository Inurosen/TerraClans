using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;



namespace TerraClans
{
    public class TCaction
    {
        public static void CMDtcsay(CommandArgs args)
        {
            if (TCutils.AlreadyInClan(args.Player.UserAccountName))
            {
                //tcsay <msg>
                string msg = "";
                if (args.Parameters.Count > 0)
                {              
                    for (int i = 0; i < args.Parameters.Count; i++)
                    {
                        msg = msg + " " + args.Parameters[i];
                    }
                    TCutils.ClanMsg(args.Player, msg, 0);   
                }
                else
                {
                    args.Player.SendMessage("TerraClans.", Color.Red);
                    args.Player.SendMessage("/tcsay <message>; /c <message> - sends message to your clan chat.", Color.Red);
                }
            }
            else
            {
                args.Player.SendMessage("You are not in clan!", Color.Red);
            }
        }

        public static void CMDtcman(CommandArgs args)
        {
            //tcman add "clan name" "leader" "group"

            if (args.Parameters.Count == 4)
            {
                if (args.Player.Group.HasPermission("manageclans"))
                {
                    if (args.Parameters[0] == "add")
                    {
                        TCAdd(args);
                    }
                    else
                    {
                        args.Player.SendMessage("TerraClans:", Color.Red);
                        args.Player.SendMessage("/tcman add <clanname> <leader> <group> - adds clan.", Color.Red);
                    }
                }
            }
            else
            {
                args.Player.SendMessage("TerraClans:", Color.Red);
                args.Player.SendMessage("/tcman add <clanname> <leader> <group> - adds clan.", Color.Red);
            }
        }

        public static void CMDtclan(CommandArgs args)
        {
            //tclan invite player
            if (args.Parameters.Count == 2)
            {
                if (TCutils.AlreadyInClan(args.Player.UserAccountName))
                {
                    List<Player> players = new List<Player>();
                    players = TCutils.GetPlayersByName(args.Parameters[1]);
                    if (players.Count == 0)
                    {
                        args.Player.SendMessage("Player " + args.Parameters[1] + " doesn't exist!", Color.Red);
                    }
                    else if (players.Count > 1)
                    {
                        args.Player.SendMessage("There are at least 2 players match to " + args.Parameters[1] + "!", Color.Red);
                    }
                    else
                    {
                        var reciever = players[0].TSPlayer.IsLoggedIn;
                        if (!reciever)
                        {
                            args.Player.SendMessage("Player " + args.Parameters[1] + " is not logged in!", Color.Red);
                        }
                        else if (TCutils.AlreadyInClan(players[0].TSPlayer.UserAccountName))
                        {
                            args.Player.SendMessage("One of " + args.Parameters[1] + "'s characters is alredy in clan!", Color.Red);
                        }
                        else
                        {
                            var DBQuery = TCdb.DB.QueryReader("SELECT clanname FROM Clans WHERE leaders LIKE '%" + args.Player.UserAccountName + "%' OR members LIKE '%" + args.Player.UserAccountName + "%'");
                            List<string> Clan = new List<string>();
                            while (DBQuery.Read())
                            {
                                Clan.Add(DBQuery.Get<string>("clanname"));
                            }
                            TCInvite(players[0].TSPlayer.Name, Clan[0], args.Player.Name);
                        }
                    }
                }
                else
                {
                    args.Player.SendMessage("You are not in clan!", Color.Red);
                }
            }
            else if (args.Parameters.Count == 1)
            {
                //tclan accept
                if (args.Parameters[0] == "accept" && !TCutils.AlreadyInClan(args.Player.UserAccountName))
                {
                    TCAccept(args);
                }
                //tclan decline
                else if (args.Parameters[0] == "decline" && !TCutils.AlreadyInClan(args.Player.UserAccountName))
                {
                    TCDecline(args);
                }
                else if (TCutils.AlreadyInClan(args.Player.UserAccountName))
                {
                    args.Player.SendMessage("You are already in clan!", Color.Red);
                }
                else
                {
                    args.Player.SendMessage("TerraClans:", Color.Red);
                    args.Player.SendMessage("/tclan accept/decline - accepts/declines invitation.", Color.Red);
                }
            }
            else
            {
                if (TCutils.AlreadyInClan(args.Player.UserAccountName))
                {
                    args.Player.SendMessage("TerraClans:", Color.Red);
                    args.Player.SendMessage("/tcsay <message>; /c <message> - sends message to your clan chat", Color.Red);
                    args.Player.SendMessage("/tclan invite <player> - invites player to your clan chat", Color.Red);
                }
                else
                {
                    args.Player.SendMessage("/tclan accept/decline - accepts/declines invitation.", Color.Red);
                }
                if (args.Player.Group.HasPermission("manageclans"))
                {
                    args.Player.SendMessage("/tcman add <clanname> <leader> <clangroup> - adds clan.", Color.Red);
                }

            }
        }

        public static void CMDtcfound(CommandArgs args) {
            if (!TCutils.AlreadyInClan(args.Player.UserAccountName))
            {
                if (args.Parameters.Count == 2)
                {
                    //tcfound
                    TCFound(args);

                }
                else if (args.Parameters.Count == 1)
                {
                    if (args.Parameters[0] == "accept")
                    {
                        //tcfound accept
                        TCFoundAccept(args);
                    }
                    else if (args.Parameters[0] == "decline")
                    {
                        //tcfound decline
                        TCFoundDecline(args);
                    }
                    else if (args.Parameters[0] == "revoke")
                    {
                        TCFoundRevoke(args);
                    }
                    else
                    {
                        args.Player.SendMessage("TerraClans:", Color.Orange);
                        args.Player.SendMessage("/tcfound accept - accepts founder's invitation.", Color.Orange);
                        args.Player.SendMessage("/tcfound decline - declines founder's invitation.", Color.Orange);
                        args.Player.SendMessage("/tcfound \"Clan Name\" \"co-founder\" - invite co-founder to found new clan with specified name.", Color.Orange);
                    }
                }
                else
                {
                    args.Player.SendMessage("TerraClans:", Color.Orange);
                    args.Player.SendMessage("/tcfound accept - accepts founder's invitation.", Color.Orange);
                    args.Player.SendMessage("/tcfound decline - declines founder's invitation.", Color.Orange);
                    args.Player.SendMessage("/tcfound \"Clan Name\" \"co-founder\" - invite co-founder to found new clan with specified name.", Color.Orange);
                }
            }
            else
            {
                args.Player.SendMessage("You are already in clan!", Color.Red);
            }
        }

        public static void TCAdd(CommandArgs incArgs)
        {
            bool error = false;

            var DBQuery = TCdb.DB.QueryReader("SELECT clangroup FROM Clans WHERE clangroup='" + incArgs.Parameters[3] + "'");
            List<string> existedGroup = new List<string>();
            while (DBQuery.Read())
            {
                existedGroup.Add(DBQuery.Get<string>("clangroup"));
            }

            DBQuery = TCdb.DB.QueryReader("SELECT clanname FROM Clans WHERE clanname='" + incArgs.Parameters[1] + "'");
            List<string> existedClan = new List<string>();
            while (DBQuery.Read())
            {
                existedClan.Add(DBQuery.Get<string>("clanname"));
            }

            List<Player> players = new List<Player>();
            players = TCutils.GetPlayersByName(incArgs.Parameters[2]);
            if (players.Count == 0)
            {
                incArgs.Player.SendMessage("Player " + incArgs.Parameters[2] + " doesn't exist!", Color.Red);
                error = true;
            }
            else
            {
                var leader = players[0].TSPlayer.IsLoggedIn;
                if (!leader)
                {
                    incArgs.Player.SendMessage("Player " + incArgs.Parameters[2] + " is not logged in!", Color.Red);
                    error = true;
                }
                else if (TCutils.AlreadyInClan(players[0].TSPlayer.UserAccountName))
                {
                    incArgs.Player.SendMessage("One of " + incArgs.Parameters[2] + "'s characters is alredy in clan!", Color.Red);
                    error = true;
                }
                List<string> invites = new List<string>();
                invites = TCutils.GetAllInvites();
                foreach (string pl in invites)
                {
                    if (pl == TCutils.GetPlayersByName(incArgs.Parameters[2])[0].TSPlayer.UserAccountName)
                    {
                        incArgs.Player.SendMessage(incArgs.Parameters[2] + " is already invited to another clan!", Color.Red);
                        error = true;
                    }
                }
            }

            var group = TShock.Groups.GroupExists(incArgs.Parameters[3]);
            if (existedGroup.Count > 0)
            {
                incArgs.Player.SendMessage("Group " + incArgs.Parameters[3] + " is already bound to another clan!", Color.Red);
                error = true;
            }
            if (existedClan.Count > 0)
            {
                incArgs.Player.SendMessage("Clan " + incArgs.Parameters[1] + " is already exist!", Color.Red);
                error = true;
            }
            List<string> strQueue = TCutils.InQueue(incArgs.Parameters[1], TCutils.GetPlayersByName(incArgs.Parameters[2])[0], TCutils.GetPlayersByName(incArgs.Parameters[2])[0]);
            if (strQueue.Count > 0)
            {
                incArgs.Player.SendMessage("Clan name is occupied or player is gonna found his own clan.", Color.Red);
                error = true;
            }

            if (!error)
            {
                if (!group)
                {
                    TShock.Groups.AddGroup(incArgs.Parameters[3], "default", "");
                    incArgs.Player.SendMessage("Creating new group: " + incArgs.Parameters[3], Color.LawnGreen);

                }
                else
                {
                    List<string> perms = new List<string>();
                    perms = TShock.Utils.GetGroup(incArgs.Parameters[3]).permissions;
                    //perms.Add("clanchat");
                    incArgs.Player.SendMessage("Binding clan to existing group: " + incArgs.Parameters[3], Color.LawnGreen);
                    TShock.Groups.AddPermissions(incArgs.Parameters[3], perms);
                }
                TCdb.DB.QueryReader("INSERT INTO Clans (clanname, clangroup, leaders, members, motd, invites) VALUES ('" + incArgs.Parameters[1] + "', '" + incArgs.Parameters[3] + "', '" + players[0].TSPlayer.UserAccountName + "', '', 'Welcome to " + incArgs.Parameters[1] + "!', '')");
                incArgs.Player.SendMessage("Added clan: " + incArgs.Parameters[1], Color.LawnGreen);
                var user = new User();
                user.Name = players[0].TSPlayer.UserAccountName;
                if (!players[0].TSPlayer.Group.HasPermission("terraclans"))
                {
                    TShock.Users.SetUserGroup(user, incArgs.Parameters[3]);
                }
                players[0].TSPlayer.SendMessage(incArgs.Player.Name + " created clan \"" + incArgs.Parameters[1] + "\" with you as leader!", Color.YellowGreen);
                players[0].TSPlayer.SendMessage("Please, relogin to apply new settings for your account.", Color.Orange);

            }
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

        public static void TCInvite(string receiver, string clanName, string inviter)
        {
            List<Player> plr = new List<Player>();
            plr = TCutils.GetPlayersByName(receiver);
            List<string> invites = new List<string>();
            List<string> strQueue = TCutils.InQueue(clanName, TCutils.GetPlayersByName(receiver)[0], TCutils.GetPlayersByName(receiver)[0]);
            if (strQueue.Count > 0)
            {
                TCutils.GetPlayersByName(inviter)[0].TSPlayer.SendMessage(receiver + "is gonna found his own clan!", Color.Red);
                return;
            }
            invites = TCutils.GetAllInvites();
            bool isInvited = false;
            if (invites.Count > 0)
            {
                for (int i = 0; i < invites.Count; i++ )
                {
                    if (plr[0].TSPlayer.UserAccountName == invites[i])
                    {
                        isInvited = true;
                    }
                }
            }
            if (isInvited)
            {
                TCutils.GetPlayersByName(inviter)[0].TSPlayer.SendMessage("Player " + plr[0].TSPlayer.Name + " is already invited to another clan!", Color.Red);
            }
            else
            {
                invites = TCutils.GetInvites(clanName);
                invites.Add(plr[0].TSPlayer.UserAccountName);
                string str = string.Join(",", invites.ToArray());
                TCdb.DB.QueryReader("UPDATE Clans SET invites='" + str +"' WHERE clanname='" + clanName + "'");
                plr[0].TSPlayer.SendMessage(inviter + " invites you to join the clan: " + clanName, Color.Orange);
                plr[0].TSPlayer.SendMessage("/tclan accept - to join", Color.Orange);
                plr[0].TSPlayer.SendMessage("/tclan decline - to decline invitation.", Color.Orange);
                TShock.Utils.FindPlayer(inviter)[0].SendMessage("You invited " + plr[0].TSPlayer.Name + " to join your clan!", Color.Yellow);
            }

        }

        public static void TCAccept(CommandArgs incArgs)
        {
            var plr = incArgs.Player;
            string clanName = "";
            string clanGroup = "";
            bool isInvited = false;
            var DBQuery = TCdb.DB.QueryReader("SELECT clanname, clangroup, invites FROM Clans WHERE invites LIKE '%" + plr.UserAccountName + "%'");
            while (DBQuery.Read())
            {
                string[] arr = DBQuery.Get<string>("invites").ToString().Split(',');
                foreach (string i in arr)
                {
                    if (i == plr.UserAccountName)
                    {
                        clanName = DBQuery.Get<string>("clanname").ToString();
                        clanGroup = DBQuery.Get<string>("clangroup").ToString();
                        isInvited = true;
                    }
                }
            }
            if (isInvited)
            {
                string strInv = "";
                string strMem = "";
                List<string> invites = new List<string>();
                invites = TCutils.GetInvites(clanName);
                invites.Remove(plr.UserAccountName);
                if (invites.Count > 0)
                {
                    strInv = string.Join(",", invites.ToArray());
                }
                List<string> members = new List<string>();
                members = TCutils.GetMembers(clanName);
                members.Add(plr.UserAccountName);
                if (members.Count > 0)
                {
                    strMem = string.Join(",", members.ToArray());
                }
                TCdb.DB.QueryReader("UPDATE Clans SET invites='" + strInv + "', members='" + strMem + "' WHERE clanname='" + clanName + "'");
                var user = new User();
                user.Name = plr.UserAccountName;
                if (!plr.Group.HasPermission("terraclans"))
                {
                    TShock.Users.SetUserGroup(user, clanGroup);
                }
                plr.SendMessage("You have joined to " + clanName, Color.Orange);
                plr.SendMessage("Please, relogin to apply new settings for your account.", Color.Orange);
                string msg = incArgs.Player.Name + " has joined the clan.";
                TCutils.ClanMsg(incArgs.Player, msg, 1);
            }
            else
            {
                plr.SendMessage("You are not invited to any clan!", Color.Red);
            }
        }

        public static void TCDecline(CommandArgs incArgs)
        {
            var plr = incArgs.Player;
            string clanName = "";
            string clanGroup = "";
            bool isInvited = false;
            var DBQuery = TCdb.DB.QueryReader("SELECT clanname, clangroup, invites FROM Clans WHERE invites LIKE '%" + plr.UserAccountName + "%'");
            while (DBQuery.Read())
            {
                string[] arr = DBQuery.Get<string>("invites").ToString().Split(',');
                foreach (string i in arr)
                {
                    if (i == plr.UserAccountName)
                    {
                        clanName = DBQuery.Get<string>("clanname").ToString();
                        clanGroup = DBQuery.Get<string>("clangroup").ToString();
                        isInvited = true;
                    }
                }
            }
            if (isInvited)
            {
                string strInv = "";
                List<string> invites = new List<string>();
                invites = TCutils.GetInvites(clanName);
                invites.Remove(plr.UserAccountName);
                if (invites.Count > 0)
                {
                    strInv = string.Join(",", invites.ToArray());
                }
                string msg = incArgs.Player.Name + " has declined invitation.";
                TCutils.ClanMsg(incArgs.Player, msg, 1); TCdb.DB.QueryReader("UPDATE Clans SET invites='" + strInv + "' WHERE clanname='" + clanName + "'");
                plr.SendMessage("You have declined the invitation.", Color.Orange);

            }
            else
            {
                plr.SendMessage("You are not invited to any clan!", Color.Red);
            }
        }

        public static void TCFound(CommandArgs incArgs)
        {
            if (!incArgs.Player.IsLoggedIn)
            {
                incArgs.Player.SendMessage("You are not logged in!", Color.Red);
                return;
            }
            if (TCutils.AlreadyInClan(incArgs.Player.UserAccountName))
            {
                incArgs.Player.SendMessage("You are already in clan!", Color.Red);
                return;
            }
            if (incArgs.Player.Name == incArgs.Parameters[1])
            {
                incArgs.Player.SendMessage("You cannot invite yourself!", Color.Red);
                return;
            }
            List<Player> coFounder = new List<Player>();
            coFounder = TCutils.GetPlayersByName(incArgs.Parameters[1]);
            if (coFounder.Count == 0)
            {
                incArgs.Player.SendMessage("Player " + incArgs.Parameters[1] + " doesn't exist!", Color.Red);
                return;
            }
            if (!coFounder[0].TSPlayer.IsLoggedIn)
            {
                incArgs.Player.SendMessage("Player " + coFounder[0].TSPlayer.Name + " is not logged in!", Color.Red);
                return;
            }
            if (TCutils.AlreadyInClan(coFounder[0].TSPlayer.UserAccountName))
            {
                incArgs.Player.SendMessage("Player " + coFounder[0].TSPlayer.Name + " is already in clan!", Color.Red);
                return;
            }
            List<string> invites = new List<string>();
            invites = TCutils.GetAllInvites();
            foreach (string i in invites)
            {
                if (i == incArgs.Player.UserAccountName)
                {
                    incArgs.Player.SendMessage("You are invited to another clan! Decline the invitation first!", Color.Red);
                    incArgs.Player.SendMessage("Type /tclan decline", Color.Red);
                    return;
                }
                if (i == coFounder[0].TSPlayer.UserAccountName)
                {
                    incArgs.Player.SendMessage("Player " + coFounder[0].TSPlayer.Name + " is invited to another clan!", Color.Red);
                    return;
                }
            }
            List<string> strQueue = TCutils.InQueue(incArgs.Parameters[0], TCutils.GetPlayersByName(incArgs.Player.Name)[0], coFounder[0]);
            if (strQueue.Count > 0)
            {
                foreach (string i in strQueue)
                {
                    incArgs.Player.SendMessage(i, Color.Red);
                }
                return;
            }

            TCdb.DB.QueryReader("INSERT INTO FoundQueue (clanname, founder, cofounder) VALUES ('" + incArgs.Parameters[0] + "', '" + incArgs.Player.UserAccountName + "', '" + coFounder[0].TSPlayer.UserAccountName + "')");

            coFounder[0].TSPlayer.SendMessage(incArgs.Player.Name + " invites you to found new clan: " + incArgs.Parameters[0], Color.Orange);
            incArgs.Player.SendMessage("You have invited " + coFounder[0].TSPlayer.Name + " to found new clan.", Color.Orange);
        }

        public static void TCFoundAccept(CommandArgs incArgs)
        {
            string clName = "";
            string clFounder = "";
            string clCoFounder = "";
            var DBQuery = TCdb.DB.QueryReader("SELECT * FROM FoundQueue WHERE cofounder='" + incArgs.Player.UserAccountName + "'");
            while (DBQuery.Read())
            {
                clName = DBQuery.Get<string>("clanname");
                clFounder = DBQuery.Get<string>("founder");
                clCoFounder = DBQuery.Get<string>("cofounder");
            }

            if (!incArgs.Player.IsLoggedIn)
            {
                incArgs.Player.SendMessage("You are not logged in!", Color.Red);
                return;
            }
            if (TCutils.AlreadyInClan(incArgs.Player.UserAccountName))
            {
                incArgs.Player.SendMessage("You are already in clan!", Color.Red);
                return;
            }
            List<string> strQueue = TCutils.InQueue(incArgs.Parameters[0], TCutils.GetPlayersByName(incArgs.Player.Name)[0], TCutils.GetPlayersByName(incArgs.Player.Name)[0]);
            if (strQueue.Count == 0)
            {
                incArgs.Player.SendMessage("You are not invited to any clan!", Color.Red);
                return;
            }
            if (TCutils.GetPlayersByUserName(clFounder).Count == 0)
            {
                incArgs.Player.SendMessage("Your inviter should be online!", Color.Red);
                return;
            }
            if (clName != "" && clFounder != "" && clCoFounder != "")
            {
                TCdb.DB.QueryReader("DELETE FROM FoundQueue WHERE cofounder='" + incArgs.Player.UserAccountName + "'");
                int timestamp = TCutils.UnixTimestamp();
                string clGroup = "clan" + timestamp.ToString();
                string clMotd = "Welcome to " + clName + "!";
                TCdb.DB.QueryReader("INSERT INTO Clans (clanname, clangroup, leaders, members, motd, invites) VALUES ('" + clName + "', '" + clGroup + "', '" + clFounder + "," + incArgs.Player.UserAccountName + "', '', '" + clMotd + "', '')");
                incArgs.Player.SendMessage("Congratulations! You have founded clan: " + clName, Color.LawnGreen);
                incArgs.Player.SendMessage("Please, relogin to apply new settings for your account." , Color.Orange);
                TCutils.GetPlayersByUserName(clFounder)[0].TSPlayer.SendMessage("Congratulations! You have founded clan: " + clName, Color.LawnGreen);
                TCutils.GetPlayersByUserName(clFounder)[0].TSPlayer.SendMessage("Please, relogin to apply new settings for your account.", Color.Orange);
                TShock.Groups.AddGroup(clGroup, "default", "");
                var user = new User();
                user.Name = clFounder;
                if (!TCutils.GetPlayersByUserName(clFounder)[0].TSPlayer.Group.HasPermission("terraclans"))
                {
                    TShock.Users.SetUserGroup(user, clGroup);
                }
                var user2 = new User();
                user2.Name = incArgs.Player.UserAccountName;
                if (!incArgs.Player.Group.HasPermission("terraclans"))
                {
                    TShock.Users.SetUserGroup(user2, clGroup);
                }
            }
            else
            {
                incArgs.Player.SendMessage("You are not invited to any clan!", Color.Red);
            }
        }

        public static void TCFoundDecline(CommandArgs incArgs)
        {
            string clName = "";
            string clFounder = "";
            string clCoFounder = "";
            var DBQuery = TCdb.DB.QueryReader("SELECT * FROM FoundQueue WHERE cofounder='" + incArgs.Player.UserAccountName + "'");
            while (DBQuery.Read())
            {
                clName = DBQuery.Get<string>("clanname");
                clFounder = DBQuery.Get<string>("founder");
                clCoFounder = DBQuery.Get<string>("cofounder");
            }

            if (!incArgs.Player.IsLoggedIn)
            {
                incArgs.Player.SendMessage("You are not logged in!", Color.Red);
                return;
            }
            if (TCutils.AlreadyInClan(incArgs.Player.UserAccountName))
            {
                incArgs.Player.SendMessage("You are already in clan!", Color.Red);
                return;
            }
            List<string> strQueue = TCutils.InQueue(clName, TCutils.GetPlayersByName(incArgs.Player.Name)[0], TCutils.GetPlayersByName(incArgs.Player.Name)[0]);
            if (strQueue.Count == 0)
            {
                incArgs.Player.SendMessage("You are not invited to any clan!", Color.Red);
                return;
            }

            if (clName != "" && clFounder != "" && clCoFounder != "")
            {
                TCdb.DB.QueryReader("DELETE FROM FoundQueue WHERE cofounder='" + incArgs.Player.UserAccountName + "'");
                incArgs.Player.SendMessage("You have declined invitation!", Color.Orange);
                if (TCutils.GetPlayersByUserName(clFounder).Count == 1)
                {
                    TCutils.GetPlayersByUserName(clFounder)[0].TSPlayer.SendMessage(incArgs.Player.Name + " has declined invitation!", Color.Orange);
                    return;
                }
            }
            else
            {
                incArgs.Player.SendMessage("You are not invited to any clan!", Color.Red);
            }
        }

        public static void TCFoundRevoke(CommandArgs incArgs)
        {
            string clName = "";
            string clFounder = "";
            string clCoFounder = "";
            var DBQuery = TCdb.DB.QueryReader("SELECT * FROM FoundQueue WHERE founder='" + incArgs.Player.UserAccountName + "'");
            while (DBQuery.Read())
            {
                clName = DBQuery.Get<string>("clanname");
                clFounder = DBQuery.Get<string>("founder");
                clCoFounder = DBQuery.Get<string>("cofounder");
            }

            if (!incArgs.Player.IsLoggedIn)
            {
                incArgs.Player.SendMessage("You are not logged in!", Color.Red);
                return;
            }
            if (TCutils.AlreadyInClan(incArgs.Player.UserAccountName))
            {
                incArgs.Player.SendMessage("You are already in clan!", Color.Red);
                return;
            }
            List<string> strQueue = TCutils.InQueue(clName, TCutils.GetPlayersByName(incArgs.Player.Name)[0], TCutils.GetPlayersByName(incArgs.Player.Name)[0]);
            if (strQueue.Count == 0)
            {
                incArgs.Player.SendMessage("You didn't invite anyone!", Color.Red);
                return;
            }

            if (clName != "" && clFounder != "" && clCoFounder != "")
            {
                TCdb.DB.QueryReader("DELETE FROM FoundQueue WHERE founder='" + incArgs.Player.UserAccountName + "'");
                incArgs.Player.SendMessage("You have revoked invitation!", Color.Orange);
                if (TCutils.GetPlayersByUserName(clCoFounder).Count == 1)
                {
                    TCutils.GetPlayersByUserName(clCoFounder)[0].TSPlayer.SendMessage(incArgs.Player.Name + " has revoked invitation!", Color.Orange);
                    return;
                }
            }
            else
            {
                incArgs.Player.SendMessage("You didn't invite anyone!", Color.Red);
            }
        }
    }
}
