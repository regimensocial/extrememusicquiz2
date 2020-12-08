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
using System.Text.RegularExpressions;

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
        public string Playlist { get; set; }
    }

    public class playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class SongMenu : Page
    {
        SQLiteConnection m_dbConnection;
        SQLiteCommand cmd;
        SQLiteDataReader rdr;

        public List<playlist> playlistList = new List<playlist>();

        public int UserScore;

        public List<songItems> selection = new List<songItems>();

        public string database = Directory.GetCurrentDirectory() + "/data.db";

        public SongMenu()
        {
            InitializeComponent();

            m_dbConnection = new SQLiteConnection($"Data Source={database}; Version=3;");
            m_dbConnection.Open();


            

            //

            string stm = @"
                SELECT * FROM playlists
            ";

            cmd = new SQLiteCommand(stm, m_dbConnection);
            rdr = cmd.ExecuteReader();


            if (!rdr.HasRows)
            {
   
            }

            while (rdr.Read())
            {
                CBplaylist.Items.Add(rdr["name"].ToString());
                CBplaylistdisplay.Items.Add(rdr["name"].ToString());

                playlistList.Add(new playlist
                {
                    Name = rdr["name"].ToString()
                });

            }
            CBplaylist.SelectedIndex = 0;


            RefreshSongs();
        }

        public void RefreshSongs()
        {
            if (newTable != null)
            {

                CBplaylistdisplay.SelectedValue = newTable;
                newTable = null;
            }


            results.Items.Clear();

            string stm = "SELECT * FROM songs";

            if (CBplaylistdisplay.SelectedIndex != 0)
            {
                stm = $@"
                    select * from songs  WHERE playlist = '{CBplaylistdisplay.SelectedValue}'
                ";
            }



            if (m_dbConnection != null)
            {

                

                cmd = new SQLiteCommand(stm, m_dbConnection);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {

                    var data = new songItems
                    {
                        Id = rdr.GetInt32(rdr.GetOrdinal("id")),
                        Artist = rdr["artist"].ToString(),
                        SongName = rdr["songName"].ToString(),
                        Playlist = rdr["playlist"].ToString()
                    };


                    var temp = results.Items.Add(data);
                }

                BTNdeselect_Click(null, null);

            }
        }

        private void BTNmenu_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Frame { Content = new GameMenu() };
        }

        private void results_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (results.SelectedItem is songItems)
            {

                BTNadd.Content = "Duplicate Song(s)";
                BTNdeselect.IsEnabled = true;
                BTNdel.IsEnabled = true;

                if (results.SelectedItems.Count > 1)
                {
                    BTNoverwrite.IsEnabled = false;
                    TBsongselect.IsEnabled = false;
                    TBartistselect.IsEnabled = false;

                    TBartistselect.Text = "";
                    TBsongselect.Text = "";
                    LBnamesong.Content = "Multiple items selected";
                    LBnameartist.Content = "";

                    selection.Clear();

                    foreach(songItems item in results.SelectedItems)
                    {
                        selection.Add(new songItems
                        {
                            Artist = item.Artist,
                            SongName = item.SongName,
                            Id = item.Id
                        });
                    }

                } else
                {
                    BTNoverwrite.IsEnabled = true;

                    TBsongselect.IsEnabled = true;
                    TBartistselect.IsEnabled = true;

                    selection.Clear();
                    selection.Add((songItems)results.SelectedItem);

                    CBplaylist.SelectedValue = selection[0].Playlist;

                    TBartistselect.Text = selection[0].Artist;
                    TBsongselect.Text = selection[0].SongName;
                    LBnamesong.Content = selection[0].SongName;
                    LBnameartist.Content = selection[0].Artist;   
                }
            }
        }

        private void BTNadd_Click(object sender, RoutedEventArgs e)
        {

            if (selection.Count > 1)
            {
                foreach (songItems sel in selection.ToList())
                {
                    cmd = new SQLiteCommand(@"
                        INSERT INTO songs (songName, artist, playlist) 
                        VALUES  (@dataSong, @dataArtist, @dataPlaylist)
                    ", m_dbConnection);

                    SQLiteParameter[] parameters = {
                        new SQLiteParameter("dataSong", sel.SongName),
                        new SQLiteParameter("dataArtist", sel.Artist),
                        new SQLiteParameter("dataPlaylist", CBplaylist.SelectedValue)
                    };

                    cmd.Parameters.AddRange(parameters);

                    cmd.ExecuteNonQuery();

                    RefreshSongs();

                }

            } else {
                cmd = new SQLiteCommand(@"
                INSERT INTO songs (songName, artist, playlist) 
                VALUES  (@dataSong, @dataArtist, @dataPlaylist)
                ", m_dbConnection);

                SQLiteParameter[] parameters = {
                    new SQLiteParameter("dataSong", TBsongselect.Text.Trim()),
                    new SQLiteParameter("dataArtist", TBartistselect.Text.Trim()),
                    new SQLiteParameter("dataPlaylist", CBplaylist.SelectedValue)
                };

                cmd.Parameters.AddRange(parameters);

                cmd.ExecuteNonQuery();

                RefreshSongs();
            }

            


            scrollToLatest();

        }

        private void scrollToLatest()
        {
            if (results.Items.Count >= 2)
            {
                results.ScrollIntoView(results.Items[results.Items.Count - 1]);
            }
        }

        private void BTNoverwrite_Click(object sender, RoutedEventArgs e)
        {
            if (selection != null) {

                
                    cmd = new SQLiteCommand("DELETE FROM songs WHERE id = @selectionID; INSERT INTO songs(id, songName, artist, playlist) VALUES(@selectionID, @dataSong, @dataArtist, @dataPlaylist)", m_dbConnection);

                    SQLiteParameter[] parameters = {
                        new SQLiteParameter("selectionID", selection[0].Id),
                        new SQLiteParameter("dataSong", TBsongselect.Text.Trim()),
                        new SQLiteParameter("dataArtist", TBartistselect.Text.Trim()),
                        new SQLiteParameter("dataPlaylist", selection[0].Playlist)
                    };

                    cmd.Parameters.AddRange(parameters);

                    cmd.ExecuteNonQuery();


                    RefreshSongs();
                

            }

        }

        private void BTNdeselect_Click(object sender, RoutedEventArgs e)
        {
            BTNadd.Content = "Add Song(s)";
            CBplaylist.IsEnabled = true;

            selection.Clear();
            TBartistselect.Text = "";
            TBsongselect.Text = ""; 
            LBnamesong.Content = "";
            LBnameartist.Content = "";
            TBartistselect.IsEnabled = true;
            TBsongselect.IsEnabled = true;
            BTNadd.IsEnabled = true;
            BTNdeselect.IsEnabled = false;
            BTNoverwrite.IsEnabled = false;
            BTNdel.IsEnabled = false;
        }

        private void BTNdel_Click(object sender, RoutedEventArgs e)
        {
            if (selection != null)
            {
                foreach (songItems sel in selection.ToList())
                {
                    cmd = new SQLiteCommand("DELETE FROM songs WHERE id = @selectionID;", m_dbConnection);

                    SQLiteParameter[] parameters = {
                        new SQLiteParameter("selectionID", sel.Id)
                    };

                    cmd.Parameters.AddRange(parameters);

                    cmd.ExecuteNonQuery();

                    RefreshSongs();
                }
                RefreshSongs();

            }
        }

        private void CBplaylistdisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSongs();
            var temp = 0;
            CBplaylist.SelectedValue = CBplaylistdisplay.SelectedValue;
            //var temp = playlistList.Where(x => x.Name.Contains("Main")).FirstOrDefault().Name;
            //var temp = playlistList.Find(x => x.Name.Contains("Main")).Name;
            if (CBplaylistdisplay.SelectedValue != null && playlistList.Find(x => x.Name == CBplaylistdisplay.SelectedValue.ToString()) != null)
            {
                temp = playlistList.Find(x => x.Name == CBplaylistdisplay.SelectedValue.ToString()).Id;
            }
            Trace.WriteLine(temp);
        }

        private string prevPlaylist = "";

        private void BTNoverwrite_MouseEnter(object sender, MouseEventArgs e)
        {
            prevPlaylist = CBplaylist.SelectedValue.ToString();
            CBplaylist.SelectedValue = selection[0].Playlist;
            CBplaylist.IsEnabled = false;
        }

        private void BTNoverwrite_MouseLeave(object sender, MouseEventArgs e)
        {
            CBplaylist.IsEnabled = true;
            CBplaylist.SelectedValue = prevPlaylist;
        }

        private string newTable;

        private void CBplaylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBplaylist.SelectedValue != null)
            {
                newTable = CBplaylist.SelectedValue.ToString();
            }
        }

    }
}
