using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Configuration;

namespace CCSortApp.Database {

    public class DBSource {
        const string dbname = "rankccs.db"; // Default Database name
        const string dbpath = "App_Data/"; // Data Source Path
        private SqliteConnection con;
        private SqliteConnectionStringBuilder scb = new SqliteConnectionStringBuilder();

        public DBSource() {
            scb.DataSource = dbpath + dbname;
            this.con = new SqliteConnection(scb.ConnectionString);
        }

        private void initTable() {
            using (var cont = new SqliteConnection(this.scb.ConnectionString)) {
                cont.Open(); // open connection
                var tempTable = cont.CreateCommand();
                tempTable.CommandText = 
                    "CREATE TABLE IF NOT EXISTS creditcards (cid INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, cardtype VARCHAR(30), cardno VARCHAR(16), createdon TIMESTAMP, entryby VARCHAR(15))";
                tempTable.ExecuteNonQuery();
            }
        }

        private bool initDBExist() {
            if (File.Exists(dbpath + dbname))
                return true;
            return false;
        }

        private bool initDB() {
            FileStream fs = File.Create(dbpath + dbname);
            if (fs.Length != 0)
                return false;
            return true;
        }

        public SqliteConnection init() {
            try {
                bool DBExist = this.initDBExist();
                if (!DBExist) {
                    initDB();
                    initTable();
                }
                    
                if(this.con.State == System.Data.ConnectionState.Closed) {
                    this.con.Open();
                    return this.con;
                }
                    
                if(this.con.State == System.Data.ConnectionState.Broken) {
                    this.con.Close();
                    this.con.Open();
                }
                
            }catch(Exception? ax) {
                Console.WriteLine(ax.Message); // change this code later
            }
            return this.con;
        }

    }

}
