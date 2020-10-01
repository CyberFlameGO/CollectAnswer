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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CollectAnswers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PostPage : Page
    {

        private Post post;
        private MediaPlayer mediaPlayer;
        private int lastCommentId = -1;
        public PostPage()
        {
            this.InitializeComponent();
            this.mediaPlayer = new MediaPlayer();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            post = (Post) e.Parameter;
        }

        private async void btnPost_Click(object sender, RoutedEventArgs e)
        {
            if (txtPost.Text.Replace(" ", "").Length < 8)
            {
                textStatus.Text = "Komentár musí mať minimálne 8 znakov!";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            if (txtPost.Text.Length > 512)
            {
                textStatus.Text = "Komentár nesmie mať viac ako 512 znakov!";
                textStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            textStatus.Text = "Počkajte prosím, komentár sa pridáva...";
            textStatus.Foreground = new SolidColorBrush(Colors.DarkGray);

            string response = await ServerCommunication.tryToPostComment(post.PostId, txtPost.Text);

            JsonObject json = new JsonObject();

            JsonObject.TryParse(response, out json);

            if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("comment") && json.ContainsKey("status"))
            {
                if (json.GetNamedValue("status").GetString().Equals("error"))
                {
                    textStatus.Text = "Nastala chyba pri pridávaní komentára";
                    textStatus.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else if (json.GetNamedValue("status").GetString().Equals("success"))
                {
                    textStatus.Text = "Komentár bol úspešne pridaný";
                    textStatus.Foreground = new SolidColorBrush(Colors.Green);
                    txtPost.Text = string.Empty;

                    await loadComments();
                    return;
                }
                else
                {
                    textStatus.Text = "Nastala chyba pri pridávaní komentára";
                    textStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            else
            {
                textStatus.Text = "Nastala chyba pri pripajaní sa na server...";
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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            postId.Text = "#" + post.PostId;
            postText.Text = post.Text;
            authorName.Text = "Pridal: " + post.AuthorName;

            likeButtonDesign();

            await loadComments();
        }

        private async void likeButton_Click(object sender, RoutedEventArgs e)
        {
            Reaction newReaction = null;
            foreach (Reaction reaction in post.Reactions)
            {
                if (reaction.accountId == LocalDatabase.userId)
                {
                    newReaction = reaction;
                }
            }

            if(newReaction == null)
            {
                newReaction = new Reaction { accountId = LocalDatabase.userId, textId = post.PostId, textType = 0, value = false, type = 0 };
                post.Reactions.Add(newReaction);
            }
            string response = await CollectAnswers.Objects.ServerCommunication.tryToLike(post.PostId, newReaction.textType, !newReaction.value);

            JsonObject json = new JsonObject();

            JsonObject.TryParse(response, out json);

            if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("reaction") && json.ContainsKey("status"))
            {
                if (json.GetNamedValue("status").GetString().Equals("error"))
                {
                    commentsInfo.Text = "Nastala chyba pri pridávaní reakcie";
                    commentsInfo.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
            }
            newReaction.value = !newReaction.value;

            likeButtonDesign();
        }

        private void likeButtonDesign()
        {
            foreach (Reaction reaction in post.Reactions)
            {
                if (reaction.accountId == LocalDatabase.userId)
                {
                    if(reaction.value)
                    {
                        likeButtonLikesIcon.Text = "\uE8E0";
                        likeButton.Background = new SolidColorBrush(Colors.LightGray);
                    }
                    else
                    {
                        likeButtonLikesIcon.Text = "\uE8E1";
                        likeButton.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }
            likeButtonLikesCount.Text = post.ReactionsCountConverted();
        }

        private async void playButton_Click(object sender, RoutedEventArgs e)
        {
            await SynthesisToSpeakerAsync(post.Text);
        }

        private async void playButtonComment_Click(object sender, RoutedEventArgs e)
        {
            PostComment comment = (PostComment)((FrameworkElement)sender).DataContext;

            await SynthesisToSpeakerAsync(comment.Text);
        }

        public async Task SynthesisToSpeakerAsync(string text)
        {
            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key and service region (e.g., "westus").
            // The default language is "en-us".
            var config = SpeechConfig.FromSubscription("e8405912637c45d5a0b46c74af7882fc", "westeurope");
            config.SpeechSynthesisLanguage = "sk-SK";

            // Creates a speech synthesizer using the default speaker as audio output.
            using (var synthesizer = new SpeechSynthesizer(config))
            {
                using (var result = await synthesizer.SpeakTextAsync(text))
                {
                    if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        System.Diagnostics.Debug.WriteLine($"Speech synthesized to speaker for text [{text}]");

                        using (var audioStream = AudioDataStream.FromResult(result))
                        {
                            // Save synthesized audio data as a wave file and user MediaPlayer to play it
                            var filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "outputaudio_for_playback.wav");
                            await audioStream.SaveToWaveFileAsync(filePath);
                            mediaPlayer.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(filePath));
                            mediaPlayer.Play();
                        }
                    }
                    else if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                        System.Diagnostics.Debug.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            System.Diagnostics.Debug.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                            System.Diagnostics.Debug.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                            System.Diagnostics.Debug.WriteLine($"CANCELED: Did you update the subscription info?");
                        }
                    }
                }
            }
        }

        private void loadMoreButton_Click(object sender, RoutedEventArgs e)
        {
            loadComments();
        }

        private async Task loadComments()
        {
            commentsInfo.Text = "Načítavam nové komentáre...";
            commentsInfo.Foreground = new SolidColorBrush(Colors.DarkGray);

            string response = await CollectAnswers.Objects.ServerCommunication.tryToGetComments(post.PostId, lastCommentId);

            JsonObject json = new JsonObject();

            JsonObject.TryParse(response, out json);

            if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("getcomments") && json.ContainsKey("status"))
            {
                if (json.GetNamedValue("status").GetString().Equals("error"))
                {
                    commentsInfo.Text = "Nastala chyba pri načítaní komentárov";
                    commentsInfo.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
            }

            if (!json.ContainsKey("comments"))
            {
                commentsInfo.Text = "Nastala chyba pri načítaní komentárov";
                commentsInfo.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            JsonArray array = json.GetNamedArray("comments");

            foreach (JsonValue value in array)
            {
                int commentId = (int)value.GetNumber();

                response = await CollectAnswers.Objects.ServerCommunication.tryToGetCommentPerId(post.PostId, commentId);

                json = new JsonObject();

                JsonObject.TryParse(response, out json);

                if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("getcommentsperid") && json.ContainsKey("status"))
                {
                    if (json.GetNamedValue("status").GetString().Equals("error"))
                    {
                        commentsInfo.Text = "Nastala chyba pri načítaní komentárov";
                        commentsInfo.Foreground = new SolidColorBrush(Colors.Red);
                        continue;
                    }
                }

                if (lastCommentId < commentId || lastCommentId == -1)
                    lastCommentId = commentId;

                JsonObject postObject = json.GetNamedObject("comment");

                string text = postObject.GetNamedString("text");
                List<Reaction> reactions = new List<Reaction>();

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

                post.PostComments.Add(new PostComment
                {
                    PostId = post.PostId,
                    CommentId = commentId,
                    AuthorId = (int)postObject.GetNamedNumber("authorId"),
                    AuthorName = postObject.GetNamedString("authorName"),
                    Text = text,
                    Reactions = reactions
                });
            }
            commentsInfo.Text = "Nové komentáre boli úspešne načítané";
            commentsInfo.Foreground = new SolidColorBrush(Colors.Green);
        }

        private async void likeButtonComment_Click(object sender, RoutedEventArgs e)
        {
            PostComment comment = (PostComment) ((FrameworkElement)sender).DataContext;
            Button button = (Button)sender;
            TextBlock likeButtonCommentLikesCount  = (TextBlock) ((Button)sender).FindName("likeButtonCommentLikesCount");
            TextBlock likeButtonCommentLikesIcon = (TextBlock)((Button)sender).FindName("likeButtonCommentLikesIcon");

            Reaction newReaction = null;
            foreach (Reaction reaction in comment.Reactions)
            {
                if (reaction.accountId == LocalDatabase.userId)
                {
                    newReaction = reaction;
                }
            }

            if (newReaction == null)
            {
                newReaction = new Reaction { accountId = LocalDatabase.userId, textId = post.PostId, textType = 1, value = false, type = 0 };
                comment.Reactions.Add(newReaction);
            }
            string response = await CollectAnswers.Objects.ServerCommunication.tryToLike(comment.CommentId, newReaction.textType, !newReaction.value);

            JsonObject json = new JsonObject();

            JsonObject.TryParse(response, out json);

            if (json.ContainsKey("type") && json.GetNamedValue("type").GetString().Equals("reaction") && json.ContainsKey("status"))
            {
                if (json.GetNamedValue("status").GetString().Equals("error"))
                {
                    commentsInfo.Text = "Nastala chyba pri pridávaní reakcie";
                    commentsInfo.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
            }
            newReaction.value = !newReaction.value;

            foreach (Reaction reaction in comment.Reactions)
            {
                if (reaction.accountId == LocalDatabase.userId)
                {
                    if (reaction.value)
                    {
                        likeButtonCommentLikesIcon.Text = "\uE8E0";
                        button.Background = new SolidColorBrush(Colors.LightGray);
                    }
                    else
                    {
                        likeButtonCommentLikesIcon.Text = "\uE8E1";
                        button.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }
            likeButtonCommentLikesCount.Text = comment.ReactionsCountConverted();
        }
    }
}
