using System.Collections.Generic;
using TShockAPI;
using TShockAPI.DB;



namespace TerraClans
{
    public class TCsay
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
                    TCutils.ClanMsg(args.Player.UserAccountName, msg, 0, false);   
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

    }
}
