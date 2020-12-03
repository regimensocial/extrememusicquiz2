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
using System.Text.RegularExpressions;

using System.Data.SQLite;

namespace MusicQuiz2
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        public string songName = "";

        public int guess = 0;
        public int score = 0;

        public bool empty = false;

        SQLiteConnection m_dbConnection;
        SQLiteCommand cmd;
        SQLiteDataReader rdr;

        public Game()
        {
            InitializeComponent();

            TBXguess.Focus();

            string database = Directory.GetCurrentDirectory() + "/data.db";

            m_dbConnection = new SQLiteConnection($"Data Source={database}; Version=3;");
            m_dbConnection.Open();

            CHBendless.IsChecked = Player.endless;

            string stm = @"
                SELECT * FROM songs
                ORDER BY random()
                LIMIT 1;
            ";

            cmd = new SQLiteCommand(stm, m_dbConnection);
            rdr = cmd.ExecuteReader();

            var DBsongName = "";
            var DBartist = "";

            if (!rdr.HasRows)
            {
                TBerror.Visibility = Visibility.Visible;
                empty = true;

                LBtakeguess.Content = "There are no songs";
                TBsong.Text = "Not a song";
                LBmessage.Content = "There are no songs in the database.";
                BTNguess.Focus();
                TBXguess.IsEnabled = false;
                BTNguess.Content = "Song Menu";

                guess = 2;
            }

            while (rdr.Read())
            {
                DBsongName = rdr["songName"].ToString();
                DBartist = rdr["artist"].ToString();

                songName = DBsongName;

                if (Player.lastSong == DBsongName)
                {
                    this.Content = new Frame { Content = new Game() };
                } else
                {
                    DBsongName = DBsongName.Trim();
                    Player.lastSong = DBsongName;
                
                    string res = DBsongName;
                    string[] temp = res.Split(' ');
                    res = "";


                    foreach (string word in temp)
                    {
                        char firstletter = word[word.Length - word.Length];
                        string tempword = new string('*', word.Length);
                        tempword = firstletter + tempword.Remove(0, 1);
                        res += tempword + " ";
                    }

                    TBartist.Text = "by " + DBartist;
                
                    TBsong.Text = res;
                }
            }

            
        }

        private void handleProgress()
        {
            cmd = new SQLiteCommand(m_dbConnection);

            Player.localScore = Player.localScore + score;

            cmd.CommandText = $"UPDATE players SET score = score + {score} WHERE username = '{Player.Username}'";

            cmd.ExecuteNonQuery();

            Trace.WriteLine(Player.endless);

            
                if (score > 0)
                {
                    this.Content = new Frame { Content = new Game() };
                }
                else
                {
                    if (Player.endless)
                    {
                        this.Content = new Frame { Content = new Game() };
                    } else
                    {
                        this.Content = new Frame { Content = new GameEnd() };
                    }
                }
            

            // progress
        }

        private void BTNguess_Click(object sender, RoutedEventArgs e)
        {
            if (!empty)
            {
                guess++;

                if (guess != 3)
                {
                    if (TBXguess.Text.ToLower() == songName.ToLower())
                    {
                        LBtakeguess.Content = "It was";
                        TBsong.Text = songName;

                        TBXguess.IsEnabled = false;
                        BTNguess.Focus();
                        TBXguess.IsEnabled = false;


                        if (guess == 1)
                        {
                            score = 3;
                            
                            BTNguess.Content = "Continue!";
                            LBmessage.Content = "You got the song right first try!";
                            guess = 2;
                        }
                        else if (guess == 2)
                        {
                            score = 1;
                            BTNguess.Content = "Continue";
                            LBmessage.Content = "You got the song right";
                        }


                    }
                    else
                    {
                        if (guess == 1)
                        {
                            LBmessage.Content = "That ain't it.";
                        }
                        else
                        {
                            LBtakeguess.Content = "It was";
                            TBsong.Text = songName;
                            LBmessage.Content = "That still ain't it. You've failed.";
                            BTNguess.Focus();
                            TBXguess.IsEnabled = false;
                            BTNguess.Content = "Continue";
                        }
                    }
                }
                else
                {
                    handleProgress();
                }
            } else
            {
                this.Content = new Frame { Content = new SongMenu() };
            }

            
        }

        private void Grid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                TBXguess.Focus();

                BTNguess_Click(null, null);
                
                Trace.WriteLine("enter");
            }
        }

        private void BTNexit_Click(object sender, RoutedEventArgs e)
        {
            cmd = new SQLiteCommand(m_dbConnection);

            cmd.CommandText = $"UPDATE players SET score = score + {score} WHERE username = '{Player.Username}'";

            cmd.ExecuteNonQuery();

            this.Content = new Frame { Content = new GameEnd() };
        }

        private void CHBendless_Click(object sender, RoutedEventArgs e)
        {
            Player.endless = !Player.endless;
        }
    }
}
