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
                        //coming
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

    }
}
