using System;
using System.IO;
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
using System.Data;
using System.Diagnostics;

using System.Data.SQLite;

namespace MusicQuiz2
{
    /// <summary>
    /// Interaction logic for GameEnd.xaml
    /// </summary>
    /// 

    public class playerItems {
        public string Player { get; set; }
        public int Score { get; set; }
    }

    public partial class GameEnd : Page
    {
        SQLiteConnection m_dbConnection;
        SQLiteCommand cmd;
        SQLiteDataReader rdr;

        public int UserScore;

        public GameEnd()
        {
            InitializeComponent();


            string database = Directory.GetCurrentDirectory() + "/data.db";

            m_dbConnection = new SQLiteConnection($"Data Source={database}; Version=3;");
            m_dbConnection.Open();

            string stm = @"
                SELECT username, score FROM players
                ORDER BY SCORE DESC
                LIMIT 5
            ";


            cmd = new SQLiteCommand(stm, m_dbConnection);
            rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                string user;

                if (rdr["username"].ToString() == Player.Username)
                {
                    user = rdr["username"].ToString() + " (you)";
                    UserScore = rdr.GetInt32(rdr.GetOrdinal("score"));
                    LBscore.Content = $"You got {Player.localScore} point{(Player.localScore != 1 ? 's' : default(char))}!\nYou have {UserScore} points in total.";
                    Player.localScore = 0;
                } else
                {
                    user = rdr["username"].ToString();
                }

                var data = new playerItems
                {
                    Player = user,
                    Score = rdr.GetInt32(rdr.GetOrdinal("score"))
                };

                
                var temp = results.Items.Add(data);

                
            }

        }

        private void BTNplayagain_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Frame { Content = new Game() };
        }

        private void BTNexit_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Frame { Content = new GameMenu() };
        }
    }
}
