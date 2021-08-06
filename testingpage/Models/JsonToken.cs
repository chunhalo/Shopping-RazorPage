using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace testingpage.Models
{
    public class JsonToken
    {
        public string token { get; set; }
        public IList<string> role { get; set; }
    }
}
