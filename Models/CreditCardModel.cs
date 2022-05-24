using CCSortApp.Database;
using CCSortApp.Validators;
using Microsoft.Data.Sqlite;

namespace CCSortApp.Models
{
    public class CreditCard
    {
        public int? ItemID { get; set; }
        public string? CardGroup { get; set; }
        public string? CardNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? EntryType { get; set; }
        public string? EntryInfo { get; set; }
        public CreditCard(string cType, string cNum, DateTime created, string entry)
        {
            this.CardGroup = cType;
            this.CardNo = cNum;
            this.CreatedOn = created;
            this.EntryType = entry;
        }

        public CreditCard(int cid, string cType, string cNum, DateTime created, string entry)
        {
            this.ItemID = cid;
            this.CardGroup = cType;
            this.CardNo = cNum;
            this.CreatedOn = created;
            this.EntryType = entry;
        }
    }

    public class CreditCardService
    {
        SqliteConnection scon;
        public CreditCardService()
        {
            this.scon = new DBSource().init();
        }

        public static SqliteCommand? cmd;
        public static SqliteDataReader? dr;
        public string addCard(CreditCard card)
        {
            var cmd = this.scon.CreateCommand();
            cmd.CommandText = "INSERT INTO creditcards (cardtype, cardno, createdon, entryby) VALUES (@0, @1, @2, @3)";
            cmd.Parameters.AddWithValue("@0", card.CardGroup);
            cmd.Parameters.AddWithValue("@1", card.CardNo);
            cmd.Parameters.AddWithValue("@2", card.CreatedOn);
            cmd.Parameters.AddWithValue("@3", card.EntryType);
            var status = cmd.ExecuteNonQuery();
            if (status > 0)
                return "Card Added Successfully!";
            return "Card could not be saved, action failed";
        }

        public string addCardBatch(Dictionary<string, string> cards)
        {
            try
            {
                using (var tra = this.scon.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    
                    foreach (KeyValuePair<string, string> card in cards)
                    {
                        var currentDateTime = DateTime.Now;
                        var cmd = new SqliteCommand("INSERT INTO creditcards (cardtype, cardno, createdon, entryby) VALUES (@0, @1, @2, @3)", this.scon, tra);
                        cmd.Parameters.AddWithValue("@0", card.Value.ToString());
                        cmd.Parameters.AddWithValue("@1", card.Key.ToString());
                        cmd.Parameters.AddWithValue("@2", currentDateTime);
                        cmd.Parameters.AddWithValue("@3", "Batch Upload");
                        cmd.ExecuteNonQuery();
                    }
                    tra.Commit();
                    return "Cards Added Successfully!";
                }
            }
            catch (Exception asx)
            {
                return asx.StackTrace + "\n" + asx.Message;
            }
        }

        public int existCard(string? card)
        {
            var cmd = this.scon.CreateCommand();
            cmd.CommandText = "SELECT Count(*) FROM creditcards WHERE cardno = @1";
            cmd.Parameters.AddWithValue("@1", card);
            var status = cmd.ExecuteScalar();
            if (status == null || Convert.ToInt16(status) == 0)
                return 0;
            return 1;
        }

        public List<CreditCard> getAll()
        {

            List<CreditCard> items = new List<CreditCard>();
            var cmd = this.scon.CreateCommand();
            cmd.CommandText = "SELECT cid, cardtype, cardno, createdon, entryby FROM creditcards ORDER BY createdon DESC";
            SqliteDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                items.Add(new CreditCard(dr.GetInt16(0), dr.GetString(1), dr.GetString(2), dr.GetDateTime(3), dr.GetString(4)));
            }
            return items;
        }

        public List<string> cardNums() {
            List<string> items = new List<string>();
            var cmd = this.scon.CreateCommand();
            cmd.CommandText = "SELECT cardno FROM creditcards ORDER BY cardno ASC";
            SqliteDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                items.Add(dr.GetString(0));
            }
            return items;
        }

        public List<object> getSummary()
        {
            List<object> items = new List<object>();
            var cmd = this.scon.CreateCommand();
            cmd.CommandText = "SELECT cardtype, count(cardtype) AS totalcards FROM creditcards GROUP BY cardtype";
            SqliteDataReader dr = cmd.ExecuteReader();
            while (dr.Read()) {
                items.Add(new List<object>() {dr.GetString(0), dr.GetInt16(1)});
            }
            return items;
        }
    }

}