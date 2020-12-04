using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

using System.Data.SQLite;

namespace MusicQuiz2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class Player
    {
        public static string Username = "developmentUser";

        public static string lastSong = "";

        public static bool endless = false;

        public static int localScore = 0;
    }

    public partial class MainWindow : Window
    {

        public static string RandomString(int length)
        {
            Random random = new Random();
            
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            

            //  TEMPORARY REMOVE AFTER
            // 
        }

        private void CheckValues(bool signin)
        {
            string content = "";
            usernameinput.Text = usernameinput.Text.Trim();

            if (!string.IsNullOrEmpty(usernameinput.Text) && !string.IsNullOrEmpty(password.Password) && usernameinput.Text.All(char.IsLetterOrDigit))
            {
                // If every data be correct

                string database = Directory.GetCurrentDirectory() + "/data.db";

                SQLiteConnection m_dbConnection = new SQLiteConnection($"Data Source={database}; Version=3;");
                m_dbConnection.Open();

                // base start
                
                SQLiteCommand cmd = new SQLiteCommand(m_dbConnection);
                
                cmd.CommandText = $@"
                CREATE TABLE IF NOT EXISTS 'players' (
                    'id'    INTEGER,
	                'username'  TEXT,
	                'hash'  TEXT,
	                'salt'  TEXT,
	                'score' INTEGER NOT NULL DEFAULT 0,
	                PRIMARY KEY('id')
                ); CREATE TABLE IF NOT EXISTS 'songs' (
	                'id'	INTEGER,
	                'songName'	TEXT,
	                'artist'	TEXT,
	                'difficulty'	INT,
	                'playCount'	INT,
	                'dateAdded'	TEXT,
	                PRIMARY KEY('id')
                );";

                cmd.ExecuteNonQuery();

                // base end: read after

                cmd = new SQLiteCommand("SELECT * FROM players WHERE username = @dataUsername", m_dbConnection);

                SQLiteParameter[] parameters = {
                    new SQLiteParameter("dataUsername", usernameinput.Text),
                };

                cmd.Parameters.AddRange(parameters);

                SQLiteDataReader rdr = cmd.ExecuteReader();

                var DBhash = "";
                var DBsalt = "";

                while (rdr.Read())
                {
                    DBhash = rdr["hash"].ToString();

                    DBsalt = rdr["salt"].ToString();
                }

                if (!signin) // If creating account
                {
                    if (DBhash.Length <= 0)
                    {
                        string salt = RandomString(new Random().Next(7, 14));
                        string hash = Base64Encode(System.Text.Encoding.ASCII.GetString(new System.Security.Cryptography.SHA256Managed().ComputeHash(System.Text.Encoding.ASCII.GetBytes(System.Text.Encoding.ASCII.GetString(new System.Security.Cryptography.SHA256Managed().ComputeHash(Encoding.ASCII.GetBytes(password.Password))) + salt))));

                        cmd = new SQLiteCommand("INSERT INTO players(username, hash, salt) VALUES(@dataUsername, @dataHash, @dataSalt)", m_dbConnection);

                        cmd.Parameters.AddRange(new SQLiteParameter[]{
                            new SQLiteParameter("dataUsername", usernameinput.Text),
                            new SQLiteParameter("dataHash", hash),
                            new SQLiteParameter("dataSalt", salt),
                        });

                        cmd.ExecuteNonQuery();


                        Player.Username = usernameinput.Text;
                        content += "Account created. Logged in. \n";
                        this.Content = new GameMenu(); 

                    } else
                    {
                        content += "Account already exists \n";
                    }

                    

                    // check

                    

                } else { // If logging in

                    string hash = Base64Encode(System.Text.Encoding.ASCII.GetString(new System.Security.Cryptography.SHA256Managed().ComputeHash(System.Text.Encoding.ASCII.GetBytes(System.Text.Encoding.ASCII.GetString(new System.Security.Cryptography.SHA256Managed().ComputeHash(Encoding.ASCII.GetBytes(password.Password))) + DBsalt))));


                    if (hash == DBhash)
                    {
                        Player.Username = usernameinput.Text;
                        content += "Logging in.\n";
                        this.Content = new GameMenu();
                    }
                    else
                    {
                        content += "Password is wrong or account doesn't exist.\n";
                    }
                    //*/
                }

            } else
            {
                if (usernameinput.Text.Length <= 0)
                {
                    content += "Your username can't be empty\n";
                }

                if (password.Password.Length <= 0)
                {
                    content += "Your password can't be empty\n";
                }

                if (!usernameinput.Text.All(char.IsLetterOrDigit))
                {
                    content += "Your username is all kinds of messed up friend\n";
                }
            }

            outputbox.Content = content;
        }

        private void Signup(object sender, RoutedEventArgs e)
        {
            CheckValues(false);
        }

        private void Signin(object sender, RoutedEventArgs e)
        {
            CheckValues(true);
        }

        private void Grid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                CheckValues(true);
            }
        }

        private void BTNclose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows[0].Close();
        }
    }
}
