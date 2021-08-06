using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace testingpage.Models
{
    public class Convert<T>
    {
        public Convert() { }
        public Convert(T data)
        {
            Data = data;
        }
        public T Data { get; set; }
    }
}
