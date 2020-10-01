using CollectAnswers.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace CollectAnswers.Models
{
    class PostComment
    {
        public int CommentId { get; set; }
        public int AuthorId { get; set; }
        public String AuthorName { get; set; }
        public int PostId { get; set; }
        public String Text { get; set; }
        public List<Reaction> Reactions { get; set; }
        public string ReactionsCountConverted()
        {
            if (Reactions != null)
            {
                int count = 0;
                foreach (Reaction reaction in Reactions)
                {
                    if (reaction.value)
                        count++;
                }
                return count + "";
            }
            return "0";
        }

        public SolidColorBrush getLikeButtonBackground()
        {
            if (HasUserLike())
                return new SolidColorBrush(Colors.LightGray);
            return new SolidColorBrush(Colors.White);
        }

        public string getLikeButtonIcon()
        {
            if (HasUserLike())
                return "\uE8E0";
            return "\uE8E1";
        }

        public bool HasUserLike()
        {
            if (Reactions != null)
            {
                foreach (Reaction reaction in Reactions)
                {
                    if (reaction.value && reaction.accountId == LocalDatabase.userId)
                        return true;
                }
                return false;
            }
            return false;
        }
    }
}
