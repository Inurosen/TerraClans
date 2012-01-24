using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;

namespace TerraClans
{
    class TCdb
    {
        public static string terraDB = Path.Combine(TerraClans.terraPath, "terraclans.sqlite");
        public static IDbConnection DB;
        public static SqlTableEditor SQLEditor;
        public static SqlTableCreator SQLWriter;

        public static void InitTerraDB()
        {
            string sql = Path.Combine(terraDB);
            if (!File.Exists(terraDB))
            {
                SqliteConnection.CreateFile(terraDB);
            }
            DB = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
            SQLEditor = new SqlTableEditor(DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter = new SqlTableCreator(DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            var table = new SqlTable("Clans",
            new SqlColumn("clanname", MySqlDbType.Text) { Unique = true },
            new SqlColumn("clangroup", MySqlDbType.Text),
            new SqlColumn("leaders", MySqlDbType.Text),
            new SqlColumn("members", MySqlDbType.Text),
            new SqlColumn("motd", MySqlDbType.Text),
            new SqlColumn("invites", MySqlDbType.Text)
);
            SQLWriter.EnsureExists(table);
            table = new SqlTable("FoundQueue",
            new SqlColumn("clanname", MySqlDbType.Text) { Unique = true },
            new SqlColumn("founder", MySqlDbType.Text),
            new SqlColumn("cofounder", MySqlDbType.Text)
);
            SQLWriter.EnsureExists(table);
        }

    }
}
