using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobiBruteForceLib
{
    public class Response
    {
        public int StatusCode { get; private set; }
        public string Content { get; private set; }

        public Response(int StatusCode, string Content)
        {
            this.StatusCode = StatusCode;
            this.Content = Content;
        }
    }
}
