using CollectAnswers.Models;
using CollectAnswers.Objects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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
    public sealed partial class MainMenu : Page
    {

        private ObservableCollection<Post> Posts = new ObservableCollection<Post>();

        public MainMenu()
        {
            this.InitializeComponent();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(PostPage), e.ClickedItem);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LocalDatabase.lastPostId = -1;
            LocalDatabase.token = "";
            LocalDatabase.userId = -1;
            LocalDatabase.username = "";

            Frame.Navigate(typeof(LoginPage));
        }

        private async void btnLoadQ_Click(object sender, RoutedEventArgs e)
        {
            await loadQuestions();
        }

        private void btnNewPost_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddPostPage));
        }

        private async Task loadQuestions()
        {
            text_loadQuestions.Text = "Načítavam nové príspevky";
            text_loadQuestions.Foreground = new SolidColorBrush(Colors.DarkGray);

            string response = await CollectAnswers.Objects.ServerCommunication.tryToGetPosts(LocalDatabase.lastPostId);

            JsonObject json = new JsonObject();

            JsonObject.TryParse(response, out json);

            if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("getposts") && json.ContainsKey("status"))
            {
                if (json.GetNamedValue("status").GetString().Equals("error"))
                {
                    text_loadQuestions.Text = "Nastala chyba pri načítavaní príspevkov";
                    text_loadQuestions.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
            }

            if (!json.ContainsKey("posts"))
            {
                text_loadQuestions.Text = "Nastala chyba pri načítavaní príspevkov";
                text_loadQuestions.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            JsonArray array = json.GetNamedArray("posts");

            foreach (JsonValue value in array)
            {
                int postId = (int) value.GetNumber();

                response = await CollectAnswers.Objects.ServerCommunication.tryToGetPostPerId(postId);

                json = new JsonObject();

                JsonObject.TryParse(response, out json);

                if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("getpostsperid") && json.ContainsKey("status"))
                {
                    if (json.GetNamedValue("status").GetString().Equals("error"))
                    {
                        text_loadQuestions.Text = "Nastala chyba pri načítavaní príspevkov";
                        text_loadQuestions.Foreground = new SolidColorBrush(Colors.Red);
                        continue;
                    }
                }

                if (LocalDatabase.lastPostId > postId || LocalDatabase.lastPostId == -1)
                    LocalDatabase.lastPostId = postId;

                JsonObject postObject = json.GetNamedObject("post");

                string text = postObject.GetNamedString("text");
                ObservableCollection<Reaction> reactions = new ObservableCollection<Reaction>();

                if (postObject.ContainsKey("reactionList"))
                {
                    JsonArray reactionsArray = postObject.GetNamedArray("reactionList");

                    foreach (JsonValue val in reactionsArray)
                    {
                        JsonObject reactObj = val.GetObject();
                        reactions.Add(new Reaction
                        {
                            id = (int)reactObj.GetNamedNumber("id"),
                            accountId = (int)reactObj.GetNamedNumber("accountId"),
                            textId = (int)reactObj.GetNamedNumber("textId"),
                            textType = (int)reactObj.GetNamedNumber("textType"),
                            type = (int)reactObj.GetNamedNumber("type"),
                            value = reactObj.GetNamedBoolean("value")
                        });
                    }
                }

                Posts.Add(new Post
                {
                    PostId = postId,
                    AuthorId = (int)postObject.GetNamedNumber("authorId"),
                    AuthorName = postObject.GetNamedString("authorName"),
                    Text = text,
                    Reactions = reactions,
                    PostComments = new ObservableCollection<PostComment>()
                });
            }
            text_loadQuestions.Text = "Nové príspevky boli načítané!";
            text_loadQuestions.Foreground = new SolidColorBrush(Colors.Green);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LocalDatabase.lastPostId = -1;
            await loadQuestions();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Posts.Clear();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SearchPage));
        }
    }
}
