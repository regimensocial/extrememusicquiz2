using System;
using System.IO;
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

using System.Data.SQLite;

namespace MusicQuiz2
{
    /// <summary>
    /// Interaction logic for SongMenu.xaml
    /// </summary>
    /// 

    public class songItems
    {
        public int Id { get; set; }
        public string Artist { get; set; }
        public string SongName { get; set; }
    }

    public partial class SongMenu : Page
    {
        SQLiteConnection m_dbConnection;
        SQLiteCommand cmd;
        SQLiteDataReader rdr;

        public int UserScore;

        public string database = Directory.GetCurrentDirectory() + "/data.db";

        public SongMenu()
        {
            InitializeComponent();

            m_dbConnection = new SQLiteConnection($"Data Source={database}; Version=3;");
            m_dbConnection.Open();

            RefreshSongs(false);


        }

        public void RefreshSongs(bool dir)
        {
            

            string stm = @"
                SELECT id, songName, artist FROM songs
            ";


            cmd = new SQLiteCommand(stm, m_dbConnection);
            rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {

                var data = new songItems
                {
                    Id = rdr.GetInt32(rdr.GetOrdinal("id")),
                    Artist = rdr["artist"].ToString(),
                    SongName = rdr["songName"].ToString()
                };


                var temp = results.Items.Add(data);


            }
        }

        private void BTNmenu_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Frame { Content = new GameMenu() };
        }

        private void results_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            songItems temp = (songItems)results.SelectedItem;

            Trace.WriteLine(temp.SongName);

            TBartistselect.Text = temp.Artist;
            TBsongselect.Text = temp.SongName;
        }

        private void BTNadd_Click(object sender, RoutedEventArgs e)
        {
            cmd = new SQLiteCommand(m_dbConnection);

            cmd.CommandText = $@"INSERT INTO songs(songName, artist) VALUES('{TBsongselect.Text}', '{TBartistselect.Text}')";

            cmd.ExecuteNonQuery();

            RefreshSongs(false);

        }
    }
}
