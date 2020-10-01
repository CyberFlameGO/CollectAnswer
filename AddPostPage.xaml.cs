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
    public sealed partial class AddPostPage : Page
    {
        public AddPostPage()
        {
            this.InitializeComponent();
        }

        private async void btnPost_Click(object sender, RoutedEventArgs e)
        {
            if (txtPost.Text.Replace(" ", "").Length < 8)
            {
                textStatus.Text = "Príspevok musí mať minimálne 8 znakov!";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            if (txtPost.Text.Length > 512)
            {
                textStatus.Text = "Príspevok môže mať maximálne 512 znakov!";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            textStatus.Text = "Počkajte prosím, prebieha pridávanie príspevku...";
            textStatus.Foreground = new SolidColorBrush(Colors.DarkGray);

            string response = await ServerCommunication.tryToPost(txtPost.Text);

            JsonObject json = new JsonObject();

            JsonObject.TryParse(response, out json);

            if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("post") && json.ContainsKey("status"))
            {
                if (json.GetNamedValue("status").GetString().Equals("error"))
                {
                    textStatus.Text = "Pri pridavaní príspevku nastala chyba";
                    textStatus.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else if (json.GetNamedValue("status").GetString().Equals("success"))
                {
                    textStatus.Text = "Príspevok bol úspešne pridaný!";
                    textStatus.Foreground = new SolidColorBrush(Colors.Green);

                    LocalDatabase.lastPostId = -1;

                    Frame.Navigate(typeof(MainMenu));
                    return;
                }
                else
                {
                    textStatus.Text = "Pri pridavaní príspevku nastala chyba";
                    textStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            else
            {
                textStatus.Text = "Pri pridavaní príspevku nastala chyba so serverom";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void Button_HandleBack(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                return;
            }
            return;
        }
    }
}
