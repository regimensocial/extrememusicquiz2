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

namespace MusicQuiz2
{
    /// <summary>
    /// Interaction logic for GamePage.xaml
    /// </summary>
    public partial class GameMenu : Page
    {
        public GameMenu()
        {
            InitializeComponent();
            userinfo.Text = "Logged in as '" + Player.Username + "'";
            Trace.WriteLine("Page change");
            // TEMPORARY
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Frame { Content = new Game() };
        }

        private void BTNlogout_Click(object sender, RoutedEventArgs e)
        {
            Player.Username = "";
            Player.lastSong = "";
            Player.endless = false;
            Player.localScore = 0;

            MainWindow newGame = new MainWindow();
            newGame.Show();

            Application.Current.Windows[0].Close();



        }

        private void BTNsongmenu_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Frame { Content = new SongMenu() };
        }
    }
}
