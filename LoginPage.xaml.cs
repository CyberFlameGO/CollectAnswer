using CollectAnswers.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CollectAnswers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();

            LocalDatabase.lastPostId = -1;
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegisterPage));
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtUser.Text.Replace(" ", "").Length < 4)
            {
                textStatus.Text = "Uživateľské meno musí mať viac ako 4 znaky!";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            if (txtUser.Text.Length < 4)
            {
                textStatus.Text = "Uživateľské meno musí mať viac ako 4 znaky!";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            if (txtUser.Text.Length > 32)
            {
                textStatus.Text = "Uživateľské meno nesmie mať viac ako 32 znakov!";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            if (Regex.Match(txtUser.Text, "[^A-Za-z0-9_]+").Success)
            {
                textStatus.Text = "Uživateľské meno nesmie obsahovať diakritiku, medzery a iné znaky okrem anglickej abecedy!";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            if (txtPassword.Password.Length < 7)
            {
                textStatus.Text = "Heslo musí mať minimálne 8 znakov!";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            textStatus.Text = "Počkajte prosím, prebieha prihlásenie...";
            textStatus.Foreground = new SolidColorBrush(Colors.DarkGray);

            string response = await ServerCommunication.tryToLoginUser(txtUser.Text, txtPassword);

            JsonObject json = new JsonObject();

            JsonObject.TryParse(response, out json);

            if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("login") && json.ContainsKey("status"))
            {
                if (json.GetNamedValue("status").GetString().Equals("bad_password"))
                {
                    textStatus.Text = "Uživateľské meno neexistuje alebo ste zadali zlé heslo!";
                    textStatus.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else if (json.GetNamedValue("status").GetString().Equals("success"))
                {
                    LocalDatabase.token = json.GetNamedString("token");
                    int id = -1;
                    int.TryParse(json.GetNamedString("userId"), out id);
                    LocalDatabase.userId = id;
                    LocalDatabase.username = txtUser.Text;
                    textStatus.Text = "Boli ste úspešne prihlásený!";
                    textStatus.Foreground = new SolidColorBrush(Colors.Green);

                    Frame.Navigate(typeof(MainMenu));
                    return;
                }
                else
                {
                    textStatus.Text = "Pri prihlasovaní nastala chyba";
                    textStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            else
            {
                textStatus.Text = "Pri pripájaní sa na server nastala chyba";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
    }
}
