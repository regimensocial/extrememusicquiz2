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
    /// 

    public class Song
    {
        public int id { get; set; }
        public string songName { get; set; }
        public string artist { get; set; }
    }

    public partial class Game : Page
    {
        public Song currentSong;

        public int song = 0;

        public int guess = 0;
        public int score = 0;

        public int correctlyGuessed = 0;

        public bool empty = false;
        public bool finished = false;

        SQLiteConnection m_dbConnection;
        SQLiteCommand cmd;
        SQLiteDataReader rdr;


        public List<Song> SongList = new List<Song>();

        private void updatePercentages()
        {
            float cgSGC = (float)Math.Round(((float)correctlyGuessed / (float)song) * 100, 0);

            float cgSTC = (float)Math.Round(((float)correctlyGuessed / (float)SongList.Count) * 100, 0);

            if (double.IsNaN(cgSGC))
            {
                cgSGC = 0;
            }

            LBcorrectlyguessed.Content = LBcorrectlyguessed.Content = $"Correctly guessed:\n - In total: {correctlyGuessed}/{SongList.Count} ({cgSTC}%)\n - Of played: {correctlyGuessed}/{song} ({cgSGC}%)";
        }

        private void newSong()
        {
            guess = 0;
            score = 0;

            if (song >= 0 && song < SongList.Count) // song + 1 > SongList.Count
            {
                updatePercentages();

                BTNguess.Content = "This is it";
                LBtakeguess.Content = "Your song is...";
                LBmessage.Content = "";

                //Keyboard.ClearFocus();

                TBXguess.IsEnabled = true;
                TBXguess.Text = "";
                TBXguess.Focus();

                LBsongcount.Content = "Song #" + (song + 1) + "/" + SongList.Count;

                Trace.WriteLine($"song {song}, count {SongList.Count}");

                currentSong = SongList[song];
                Trace.WriteLine(currentSong.songName);

                currentSong.songName = currentSong.songName.Trim();

                string res = currentSong.songName;
                string[] temp = res.Split(' ');
                res = "";

                if (temp.Length > 0)
                {
                    foreach (string word in temp)
                    {
                        if (word.Length > 0)
                        {
                            char firstletter = word[word.Length - word.Length];
                            string tempword = new string('*', word.Length);
                            tempword = firstletter + tempword.Remove(0, 1);
                            res += tempword + " ";
                        } else
                        {
                            res = "";
                        }

                        
                    }
                } else
                {
                    res = "";
                }
                

                TBartist.Text = "by " + currentSong.artist;

                TBsong.Text = res;

            }
            else
            {
                finished = true;
                TBXguess.IsEnabled = false;
                BTNguess.Focus();

                BTNguess.Content = "End game";

                Trace.WriteLine("end");

                TBartist.Text = "";
                LBenterguess.Content = "";
                TBsong.Text = "You've run out of songs!";
                LBtakeguess.Content = "No songs left!";
                LBmessage.Content = "";
            }
        }



        public Game()
        {
            InitializeComponent();

            string database = Directory.GetCurrentDirectory() + "/data.db";

            m_dbConnection = new SQLiteConnection($"Data Source={database}; Version=3;");
            m_dbConnection.Open();

            CHBendless.IsChecked = Player.endless;
            

            string stm = @"
                SELECT * FROM songs
                ORDER BY random()
            ";

            cmd = new SQLiteCommand(stm, m_dbConnection);
            rdr = cmd.ExecuteReader();


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
                SongList.Add(new Song()
                {
                    songName = rdr["songName"].ToString(),
                    artist = rdr["artist"].ToString(),
                    id = rdr.GetInt32(rdr.GetOrdinal("id"))
                });
            }

            newSong();
        }

        private void handleProgress()
        {
            song++;
            
            cmd = new SQLiteCommand(m_dbConnection);

            Player.localScore = Player.localScore + score;

            cmd.CommandText = $"UPDATE players SET score = score + {score} WHERE username = '{Player.Username}'";

            cmd.ExecuteNonQuery();

            Trace.WriteLine(Player.endless);
 
            if (score > 0)
            {
                correctlyGuessed++;

                

                newSong();
            }
            else
            {
                if (Player.endless)
                {
                    newSong();

                }
                else
                {
                    this.Content = new Frame { Content = new GameEnd() };
                }
            }
        }

        private void BTNguess_Click(object sender, RoutedEventArgs e)
        {
            if (finished)
            {
                this.Content = new Frame { Content = new GameEnd() };

            } else if (!empty)
            {
                guess++;

                if (guess != 3)
                {
                    if (TBXguess.Text.ToLower() == currentSong.songName.ToLower())
                    {

                        LBtakeguess.Content = "It was";
                        TBsong.Text = currentSong.songName;

                        TBXguess.IsEnabled = false;
                        BTNguess.Focus();


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
                            TBsong.Text = currentSong.songName;
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
