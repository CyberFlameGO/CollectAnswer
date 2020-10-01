using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectAnswers.Models
{
    class Post
    {
        public int PostId { get; set; }
        public string Text { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string PostIdConverted() => "#" + PostId;
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
        public ObservableCollection<Reaction> Reactions { get; set; }
        public ObservableCollection<PostComment> PostComments { get; set; }
    }
}
