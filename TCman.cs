using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;



namespace TerraClans
{
    public class TCman
    {

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

    }
}
