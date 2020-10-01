using CollectAnswers.Models;
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
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Windows.Storage;
using Windows.Media.Core;
using Windows.Media.Playback;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CollectAnswers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {

        private ObservableCollection<Post> Posts = new ObservableCollection<Post>();
        private int lastPostId = -1;
        public SearchPage()
        {
            this.InitializeComponent();
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

        private void loadMoreButton_Click(object sender, RoutedEventArgs e)
        {
            loadSearchPosts();
        }

        private async Task loadSearchPosts()
        {
            postsInfo.Text = "Vyhľadávam príspevky...";
            postsInfo.Foreground = new SolidColorBrush(Colors.DarkGray);

            string response = await CollectAnswers.Objects.ServerCommunication.tryToGetSearchPosts(lastPostId, txtPost.Text);

            JsonObject json = new JsonObject();

            JsonObject.TryParse(response, out json);

            if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("getsearchposts") && json.ContainsKey("status"))
            {
                if (json.GetNamedValue("status").GetString().Equals("error"))
                {
                    postsInfo.Text = "Nastala chyba pri načitaní príspevkov";
                    postsInfo.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
            }

            if (!json.ContainsKey("searchposts"))
            {
                postsInfo.Text = "Nastala chyba pri načitaní príspevkov";
                postsInfo.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            JsonArray array = json.GetNamedArray("searchposts");

            foreach (JsonValue value in array)
            {
                int postId = (int)value.GetNumber();

                response = await CollectAnswers.Objects.ServerCommunication.tryToGetPostPerId(postId);

                json = new JsonObject();

                JsonObject.TryParse(response, out json);

                if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("getpostsperid") && json.ContainsKey("status"))
                {
                    if (json.GetNamedValue("status").GetString().Equals("error"))
                    {
                        postsInfo.Text = "Nastala chyba pri načitaní príspevkov";
                        postsInfo.Foreground = new SolidColorBrush(Colors.Red);
                        continue;
                    }
                }

                if (lastPostId > postId || lastPostId == -1)
                    lastPostId = postId;

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
            postsInfo.Text = "Vyhľadávanie bolo úspešné! Nových príspekov: " + array.Count;
            postsInfo.Foreground = new SolidColorBrush(Colors.Green);
        }

        private void txtPost_FocusDisengaged(Control sender, FocusDisengagedEventArgs args)
        {
            loadSearchPosts();
        }

        private async void txtPost_TextChanged(object sender, TextChangedEventArgs e)
        {
            lastPostId = -1;
            Posts.Clear();
            postsInfo.Text = "Pre dalšie vyhľadanie, kliknite na tlacidlo Vyhľadať viac";
            postsInfo.Foreground = new SolidColorBrush(Colors.Green);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            lastPostId = -1;
            txtPost.Focus(FocusState.Keyboard);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(PostPage), e.ClickedItem);
        }
    }
}
