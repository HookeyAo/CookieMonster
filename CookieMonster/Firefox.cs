using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;


namespace CookieMonster
{
    public class Firefox
    {
        public class Cookie
        {
            public string path { get; set; }
            public string domain { get; set; }
            public string name { get; set; }
            public string value { get; set; }
            public Int64 expirationdate { get; set; }
        }

        public static string GetProfilePath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Mozilla\Firefox\Profiles\";
  
        }
        
        public static void ReadCookies(string profilePath, string domain, bool enc, string name)
        {

            // Wrapper for multiple profiles
            foreach (string dbPath in Directory.GetFiles(profilePath, "cookies.sqlite", SearchOption.AllDirectories))
            {

                string connectionString = "Data Source=" + dbPath + ";pooling=false";

                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    string sql;

                    if (enc == false) { sql = "SELECT path,name,value,expiry,host FROM moz_cookies WHERE host LIKE '%"; } else { sql = "SELECT path,name,value,expiry,host FROM moz_cookies WHERE host LIKE '%"; }

                    sql += domain + "%'";

                    if (name != string.Empty) { sql += " AND name = '" + name + "'"; }

                    cmd.CommandText = sql;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        string cookieValue;

                        while (reader.Read())
                        {
                            string cookiePath = (string)reader[0];
                            string cookieName = (string)reader[1];
                            Int64 cookieExpire = (Int64)reader[3];
                            string cookieDomain = (string)reader[4];


                            if (enc == false)
                            {
                                cookieValue = (string)reader[2];
                            }
                            else
                            {
                                var encData = (byte[])reader[2];
                                var decData = ProtectedData.Unprotect(encData, null, DataProtectionScope.CurrentUser);
                                cookieValue = Encoding.ASCII.GetString(decData);
                            }

                            Cookie cookie = new Cookie
                            {
                                path = cookiePath,
                                domain = cookieDomain,
                                name = cookieName,
                                value = cookieValue,
                                expirationdate = cookieExpire

                            };

                            string json = JsonConvert.SerializeObject(cookie);
                            Console.WriteLine(json);

                        }

                    }

                    conn.Close();

                }
            }
            
        }
        
    }

}
