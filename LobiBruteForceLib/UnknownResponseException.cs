using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobiBruteForceLib
{
    public class UnknownResponseException : Exception
    {
        public Response ResponseData { get; private set; }
        public UnknownResponseException(Response response) => ResponseData = response;
    }
}
