using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;

namespace TerraClans
{
    public class TClan
    {

        public static void CMDtclan(CommandArgs args)
        {
            //tclan invite player
            if (args.Parameters.Count == 2)
            {
                if (args.Parameters[0] == "invite")
                {
                    if (TCutils.AlreadyInClan(args.Player.UserAccountName))
                    {
                        TCInvite(args);
                    }
                    else
                    {
                        args.Player.SendMessage("You are not in clan!", Color.Red);
                    }
                }
                else if (args.Parameters[0] == "kick")
                {
                    //tclan kick player
                    if (TCutils.AlreadyInClan(args.Player.UserAccountName))
                    {
                        TCkick(args);
                    }
                    else
                    {
                        args.Player.SendMessage("You are not in clan!", Color.Red);
                    }
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
                //tclan leave/quit
                else if (args.Parameters[0] == "leave" || args.Parameters[0] == "quit")
                {
                    TCQuit(args);
                }
                else
                {
                    if (!TCutils.AlreadyInClan(args.Player.UserAccountName))
                    {
                        args.Player.SendMessage("TerraClans:", Color.Red);
                        args.Player.SendMessage("/tclan accept/decline - accepts/declines invitation.", Color.Red);
                    }
                }
            }
            else
            {
                if (TCutils.AlreadyInClan(args.Player.UserAccountName))
                {
                    args.Player.SendMessage("TerraClans:", Color.Red);
                    args.Player.SendMessage("/tcsay <message>; /c <message> - sends message to your clan chat", Color.Red);
                    args.Player.SendMessage("/tclan invite <player> - invites player to your clan chat", Color.Red);
                    if (TCutils.IsLeader(args.Player.UserAccountName))
                    {
                        args.Player.SendMessage("/tclan kick <player> - kicks player from your clan.", Color.Red);
                    }
                    else
                    {
                        args.Player.SendMessage("/tclan leave/quit - to leave the clan.", Color.Red);
                    }
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

        public static void TCInvite(CommandArgs incArgs)
        {

            List<Player> players = new List<Player>();
            players = TCutils.GetPlayersByName(incArgs.Parameters[1]);
            if (players.Count == 0)
            {
                incArgs.Player.SendMessage("Player " + incArgs.Parameters[1] + " doesn't exist!", Color.Red);
            }
            else if (players.Count > 1)
            {
                incArgs.Player.SendMessage("There are at least 2 players match to " + incArgs.Parameters[1] + "!", Color.Red);
            }
            else
            {
                var reciever = players[0].TSPlayer.IsLoggedIn;
                if (!reciever)
                {
                    incArgs.Player.SendMessage("Player " + players[0].TSPlayer.Name + " is not logged in!", Color.Red);
                }
                else if (TCutils.AlreadyInClan(players[0].TSPlayer.UserAccountName))
                {
                    incArgs.Player.SendMessage("One of " + players[0].TSPlayer.Name + "'s characters is alredy in clan!", Color.Red);
                }
                else
                {
                    var DBQuery = TCdb.DB.QueryReader("SELECT clanname FROM Clans WHERE leaders LIKE '%" + incArgs.Player.UserAccountName + "%' OR members LIKE '%" + incArgs.Player.UserAccountName + "%'");
                    List<string> Clan = new List<string>();
                    while (DBQuery.Read())
                    {
                        Clan.Add(DBQuery.Get<string>("clanname"));
                    }
                    string clanName = Clan[0];
                    List<string> invites = new List<string>();
                    List<string> strQueue = TCutils.InQueue(clanName, players[0], players[0]);
                    if (strQueue.Count > 0)
                    {
                        incArgs.Player.SendMessage(incArgs.Parameters[1] + "is gonna found his own clan!", Color.Red);
                        return;
                    }
                    invites = TCutils.GetAllInvites();
                    bool isInvited = false;
                    if (invites.Count > 0)
                    {
                        for (int i = 0; i < invites.Count; i++)
                        {
                            if (players[0].TSPlayer.UserAccountName == invites[i])
                            {
                                isInvited = true;
                            }
                        }
                    }
                    if (isInvited)
                    {
                        incArgs.Player.SendMessage("Player " + players[0].TSPlayer.Name + " is already invited to another clan!", Color.Red);
                    }
                    else
                    {
                        invites = TCutils.GetInvites(clanName);
                        invites.Add(players[0].TSPlayer.UserAccountName);
                        string str = string.Join(",", invites.ToArray());
                        TCdb.DB.QueryReader("UPDATE Clans SET invites='" + str + "' WHERE clanname='" + clanName + "'");
                        players[0].TSPlayer.SendMessage(incArgs.Player.Name + " invites you to join the clan: " + clanName, Color.Orange);
                        players[0].TSPlayer.SendMessage("/tclan accept - to join", Color.Orange);
                        players[0].TSPlayer.SendMessage("/tclan decline - to decline invitation.", Color.Orange);
                        incArgs.Player.SendMessage("You invited " + players[0].TSPlayer.Name + " to join your clan!", Color.Yellow);
                    }
                }
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
                TCutils.ClanMsg(incArgs.Player.UserAccountName, msg, 1, true);
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
                TCutils.ClanMsg(incArgs.Player.UserAccountName, msg, 1, false);
                TCdb.DB.QueryReader("UPDATE Clans SET invites='" + strInv + "' WHERE clanname='" + clanName + "'");
                plr.SendMessage("You have declined the invitation.", Color.Orange);

            }
            else
            {
                plr.SendMessage("You are not invited to any clan!", Color.Red);
            }
        }

        public static void TCkick(CommandArgs incArgs)
        {
            if (!incArgs.Player.IsLoggedIn)
            {
                incArgs.Player.SendMessage("You are not not logged in!", Color.Red);
                return;
            }
            if (!TCutils.AlreadyInClan(incArgs.Player.UserAccountName))
            {
                incArgs.Player.SendMessage("You are not in clan!", Color.Red);
                return;
            }
            if (!TCutils.IsLeader(incArgs.Player.UserAccountName))
            {
                incArgs.Player.SendMessage("You are not a leader!", Color.Red);
                return;
            }
            List<Player> victims = TCutils.GetPlayersByName(incArgs.Parameters[1]);
            if (victims.Count == 0)
            {
                incArgs.Player.SendMessage("Player does not exist!", Color.Red);
                return;
            }
            if (incArgs.Player.UserAccountName == TCutils.GetPlayersByName(incArgs.Parameters[1])[0].TSPlayer.UserAccountName)
            {
                incArgs.Player.SendMessage("You can't kick yourself!", Color.Red);
                return;
            }
            if (!victims[0].TSPlayer.IsLoggedIn)
            {
                incArgs.Player.SendMessage("Player is not logged in!", Color.Red);
                return;
            }
            if (!TCutils.IsValidMember(victims[0].TSPlayer.UserAccountName, incArgs.Player.UserAccountName))
            {
                incArgs.Player.SendMessage("Player is not in your clan!", Color.Red);
                return;
            }
            if(TCutils.IsLeader(victims[0].TSPlayer.UserAccountName))
            {
                incArgs.Player.SendMessage("You can't kick another leader!", Color.Red);
                return;
            }
            List<string> clName = TCutils.GetClanByLeader(incArgs.Player.UserAccountName);
            List<string> clMembers = new List<string>();
            string rawMembers = "";
            var DBQuery = TCdb.DB.QueryReader("SELECT members FROM Clans WHERE clanname='" + clName[0] + "'");
            while (DBQuery.Read())
            {
                string[] arr = DBQuery.Get<string>("members").Split(',');
                foreach (string i in arr)
                {
                    clMembers.Add(i);
                }

            }
            for (int c = 0; c < clMembers.Count; c++ )
            {
                if (victims[0].TSPlayer.UserAccountName == clMembers[c])
                {
                    clMembers.RemoveAt(c);
                    break;
                }
            }
            if (clMembers.Count > 0)
            {
                rawMembers = string.Join(",", clMembers.ToArray());
            }
            TCdb.DB.QueryReader("UPDATE Clans SET members='" + rawMembers + "' WHERE clanname='" + clName[0] + "'");
            var user = new User();
            user.Name = victims[0].TSPlayer.UserAccountName;
            if (!victims[0].TSPlayer.Group.HasPermission("terraclans"))
            {
                TShock.Users.SetUserGroup(user, TShock.Config.DefaultRegistrationGroupName);
            }
            TCutils.ClanMsg(incArgs.Player.UserAccountName, victims[0].TSPlayer.Name + " has been kicked from the clan!", 1, false); 
            victims[0].TSPlayer.SendMessage("You have been kicked from the clan!", Color.Yellow);
        }

        public static void TCQuit(CommandArgs incArgs)
        {
            if (!incArgs.Player.IsLoggedIn)
            {
                incArgs.Player.SendMessage("You are not not logged in!", Color.Red);
                return;
            }
            if (!TCutils.AlreadyInClan(incArgs.Player.UserAccountName))
            {
                incArgs.Player.SendMessage("You are not in clan!", Color.Red);
                return;
            }
            if (TCutils.IsLeader(incArgs.Player.UserAccountName))
            {
                incArgs.Player.SendMessage("Leaders can't leave the clan!", Color.Red);
                return;
            }
            List<string> clName = TCutils.GetClanByMember(incArgs.Player.UserAccountName);
            List<string> clMembers = new List<string>();
            string rawMembers = "";
            var DBQuery = TCdb.DB.QueryReader("SELECT members FROM Clans WHERE clanname='" + clName[0] + "'");
            while (DBQuery.Read())
            {
                string[] arr = DBQuery.Get<string>("members").Split(',');
                foreach (string i in arr)
                {
                    clMembers.Add(i);
                }

            }
            for (int c = 0; c < clMembers.Count; c++)
            {
                if (incArgs.Player.UserAccountName == clMembers[c])
                {
                    clMembers.RemoveAt(c);
                    break;
                }
            }
            if (clMembers.Count > 0)
            {
                rawMembers = string.Join(",", clMembers.ToArray());
            }
            TCutils.ClanMsg(incArgs.Player.UserAccountName, incArgs.Player.Name + " has left the clan!", 1, true);
            TCdb.DB.QueryReader("UPDATE Clans SET members='" + rawMembers + "' WHERE clanname='" + clName[0] + "'");
            var user = new User();
            user.Name = incArgs.Player.UserAccountName;
            if (!incArgs.Player.Group.HasPermission("terraclans"))
            {
                TShock.Users.SetUserGroup(user, TShock.Config.DefaultRegistrationGroupName);
            }
            incArgs.Player.SendMessage("You have left the clan!", Color.Yellow);
        }

    }
}
