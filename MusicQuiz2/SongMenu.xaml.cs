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

        public List<playlist> selectionPlaylists = new List<playlist>();


        public string database = Directory.GetCurrentDirectory() + "/data.db";

        public SongMenu()
        {
            InitializeComponent();

            PlaylistControls.Visibility = Visibility.Hidden;

            m_dbConnection = new SQLiteConnection($"Data Source={database}; Version=3;");
            m_dbConnection.Open();

            RefreshPlaylists();
        }

        public void RefreshPlaylists()
        {
            

            CBplaylist.Items.Clear();
            CBplaylistdisplay.Items.Clear();
            playlistList.Clear();
            gridPlaylistEdit.Items.Clear();

            string stm = @"
                SELECT * FROM playlists
            ";

            cmd = new SQLiteCommand(stm, m_dbConnection);
            rdr = cmd.ExecuteReader();

            CBplaylistdisplay.Items.Add("- All Playlists -");

            while (rdr.Read())
            {
                CBplaylist.Items.Add(rdr["name"].ToString());
                CBplaylistdisplay.Items.Add(rdr["name"].ToString());

                playlistList.Add(new playlist
                {
                    Name = rdr["name"].ToString()
                });

                gridPlaylistEdit.Items.Add(new playlist
                {
                    Name = rdr["name"].ToString()
                });

            }


            if (newTable == null)
            {
                CBplaylistdisplay.SelectedIndex = 0;
            }

            CBplaylist.SelectedIndex = 0;

            BTNdeselectplaylist_Click(null, null);

            newTable = oldPlaylist;

            Trace.WriteLine(oldPlaylist);

            RefreshSongs();
        }

        public void RefreshSongs()
        {
            if (newTable != null)
            {
                if (CBplaylistdisplay.Items.Contains(newTable))
                {
                    CBplaylistdisplay.SelectedValue = newTable;
                } else
                {
                    CBplaylistdisplay.SelectedIndex = 0;
                    
                }
                newTable = null;

            }


            results.Items.Clear();

            string stm = "SELECT * FROM songs";

            if (CBplaylistdisplay.SelectedIndex != 0)
            {
                stm = $@"
                    select * from songs  WHERE playlist = '{CBplaylistdisplay.SelectedValue}'
                ";
                DGplaylistID.Visibility = Visibility.Collapsed;
            } else
            {
                DGplaylistID.Visibility = Visibility.Visible;
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
                    LBnamesong.FontStyle = FontStyles.Italic;
                    LBnamesong.FontWeight = FontWeights.Normal;
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
                    LBnamesong.FontStyle = FontStyles.Normal;
                    LBnamesong.FontWeight = FontWeights.Bold;
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
            newTable = null;
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


        bool playlistControls = false;
        private void BTNplaylistedit_Click(object sender, RoutedEventArgs e)
        {
            playlistControls = !playlistControls;
            PlaylistControls.IsEnabled = playlistControls;

            if (PlaylistControls.IsEnabled)
            {
                BTNplaylistedit.Content = "Hide Menu";
                PlaylistControls.Visibility = Visibility.Visible;
            } else
            {
                BTNplaylistedit.Content = "Edit Playlists";
                PlaylistControls.Visibility = Visibility.Hidden;
            }

            string stm = @"
                SELECT * FROM playlists
            ";

            cmd = new SQLiteCommand(stm, m_dbConnection);
            rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                
            }
        }


        private void gridPlaylistEdit_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (gridPlaylistEdit.SelectedItem is playlist)
            {

                BTNupdateplaylist.IsEnabled = true;

                BTNdeselectplaylist.IsEnabled = true;

                BTNaddplaylist.IsEnabled = false;

                BTNdelplaylist.IsEnabled = true;

                selectionPlaylists.Clear();

                foreach (playlist item in gridPlaylistEdit.SelectedItems)
                {
                    selectionPlaylists.Add(new playlist
                    {
                        Name = item.Name,
                        Id = item.Id,
                    });
                }

                if (gridPlaylistEdit.SelectedItems.Count > 1)
                {
                    TBplaylists.IsEnabled = false;
                    BTNupdateplaylist.IsEnabled = false;
                }
                else
                {
                    TBplaylists.IsEnabled = true;
                    BTNupdateplaylist.IsEnabled = true;
                    TBplaylists.Text = selectionPlaylists[0].Name;
                    
                }
            }
        }

        private int deleteProgress = 0;

        private void BTNdeselectplaylist_Click(object sender, RoutedEventArgs e)
        {
            BTNdelplaylist.Content = "Delete";
            deleteProgress = 0;
            BTNaddplaylist.IsEnabled = true;
            LBplerror.Content = "";
            selectionPlaylists.Clear();
            TBplaylists.IsEnabled = true;
            BTNdeselectplaylist.IsEnabled = false;
            BTNupdateplaylist.IsEnabled = false;
            BTNdelplaylist.IsEnabled = false;
            TBplaylists.Text = "";
        }

        private string oldPlaylist;

        private void BTNupdateplaylist_Click(object sender, RoutedEventArgs e)
        {
            if (selectionPlaylists != null && TBplaylists.Text.Length > 0)
            {
                if (selectionPlaylists[0].Name != "Main")
                {
                    if (!CBplaylistdisplay.Items.Contains(TBplaylists.Text))
                    {

                        cmd = new SQLiteCommand(@"
                        UPDATE playlists
                        SET name = @selectionnewname
                        WHERE name = @selectionoldname;
                    ", m_dbConnection);

                        cmd.Parameters.AddRange(new SQLiteParameter[] {
                        new SQLiteParameter("selectionnewname", TBplaylists.Text),
                        new SQLiteParameter("selectionoldname", selectionPlaylists[0].Name)
                    });

                        cmd.ExecuteNonQuery();

                        cmd = new SQLiteCommand(@"
                        UPDATE songs
                        SET playlist = @selectionnewname
                        WHERE playlist = @selectionoldname;
                    ", m_dbConnection);


                        cmd.Parameters.AddRange(new SQLiteParameter[] {
                        new SQLiteParameter("selectionnewname", TBplaylists.Text),
                        new SQLiteParameter("selectionoldname", selectionPlaylists[0].Name)
                    });

                        cmd.ExecuteNonQuery();

                        oldPlaylist = TBplaylists.Text;

                        RefreshPlaylists();
                        BTNdeselectplaylist_Click(null, null);

                    }
                    else
                    {
                        LBplerror.Content = "Name taken";
                        TBplaylists.Text = "";
                    }
                } else
                {
                    LBplerror.Content = "Cannot rename Main";
                }
                    
            }
            else
            {
                LBplerror.Content = "Cannot be blank";
            }
        }

        private void BTNdelplaylist_Click(object sender, RoutedEventArgs e)
        {
            deleteProgress++;

            if (deleteProgress == 0)
            {
                BTNdelplaylist.Content = "Delete";
            } else if (deleteProgress == 1)
            {
                BTNdelplaylist.Content = "Are you sure?";
            } else if (deleteProgress == 2)
            {

                if (selectionPlaylists != null)
                {
                    

                    foreach (playlist sel in selectionPlaylists.ToList())
                    {
                        if (sel.Name != "Main")
                        {
                            cmd = new SQLiteCommand("DELETE FROM playlists WHERE name = @selectionName;", m_dbConnection);

                            SQLiteParameter[] parameters = {
                                new SQLiteParameter("selectionName", sel.Name)
                            };

                            cmd.Parameters.AddRange(parameters);

                            cmd.ExecuteNonQuery();

                            cmd = new SQLiteCommand("DELETE FROM songs WHERE playlist = @selectionName;", m_dbConnection);

                            cmd.Parameters.AddRange(new SQLiteParameter[] {
                                new SQLiteParameter("selectionName", sel.Name)
                            });

                            cmd.ExecuteNonQuery();

                            RefreshSongs();

                            RefreshPlaylists();

                            BTNdelplaylist.Content = "handl delete";


                            BTNdeselectplaylist_Click(null, null);
                            deleteProgress = 0;
                        } else
                        {
                            LBplerror.Content = "Cannot delete Main";
                        }  
                    }   
                }                
            }
        }

        private void BTNaddplaylist_Click(object sender, RoutedEventArgs e)
        {
            if (TBplaylists.Text.Length > 0)
            {
                if (!CBplaylistdisplay.Items.Contains(TBplaylists.Text))
                {

                    cmd = new SQLiteCommand(@"
                    INSERT INTO playlists(name)
                    VALUES(@selectionname);
                ", m_dbConnection);

                    cmd.Parameters.AddRange(new SQLiteParameter[] {
                    new SQLiteParameter("selectionname", TBplaylists.Text)
                });

                    cmd.ExecuteNonQuery();

                    oldPlaylist = TBplaylists.Text;

                    RefreshPlaylists();
                    BTNdeselectplaylist_Click(null, null);
                }
                else
                {
                    LBplerror.Content = "Name taken";
                    TBplaylists.Text = "";
                }
            } else
            {
                LBplerror.Content = "Cannot be blank";
            }
        }
    }
}
