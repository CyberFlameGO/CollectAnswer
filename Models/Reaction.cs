using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectAnswers.Models
{
    class Reaction
    {
        public int id { get; set; }
        public int accountId { get; set; }
        public int textId { get; set; }
        public int type { get; set; }
        public bool value { get; set; }
        public int textType { get; set; }
    }
}
