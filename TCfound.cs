using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;



namespace TerraClans
{
    public class TCfound
    {

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
            coFounder[0].TSPlayer.SendMessage("/tcfound accept - accepts the invitation. ", Color.Orange);
            coFounder[0].TSPlayer.SendMessage("/tcfound decline - declines the invitation. ", Color.Orange);
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
