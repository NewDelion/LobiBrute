using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.IO;

namespace LobiBruteForceLib
{
    public class LobiBruteForcer
    {
        private string _IP { get; set; }
        private string _Host { get; set; }
        private string _EndPoint { get; set; }
        private Func<Response, CheckResult> _Checker { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ip">WebサーバのIPアドレス</param>
        /// <param name="host">Hostヘッダに指定する値</param>
        /// <param name="endpoint">要求するリソース。総当たりするIDを挿入する場所は{ID}とすることで指定できます。</param>
        /// <param name="checker">レスポンスを評価する関数</param>
        public LobiBruteForcer(string ip, string host, string endpoint, Func<Response, CheckResult> checker)
        {
            _IP = ip;
            _Host = host;
            _EndPoint = endpoint;
            _Checker = checker;
        }

        /// <summary>
        /// 入力と同じ順番でリストが返されるとは限りません
        /// </summary>
        public IEnumerable<string> FilterAvailable(IEnumerable<string> id_list)
        {
            if (id_list.Count() == 0)
                yield break;
            var try_list = new List<string>(id_list);
            var retry_list = new List<string>();
            do
            {
                using (var client = new TcpClient())
                {
                    client.Connect(IPAddress.Parse(_IP), 443);
                    using (var ns = client.GetStream())
                    using (var ssl = new SslStream(ns, false, (_, __, ___, ____) => true, null))
                    {
                        ssl.AuthenticateAsClient(_Host, null, SslProtocols.Tls12, false);
                        using (var writer = new StreamWriter(ssl, Encoding.ASCII, 1024, true))
                            writer.Write(GetRequest(try_list));
                        foreach (var id in id_list)
                        {
                            var response = ReadResponseOne(ssl);
                            var result = _Checker(response);
                            if (result == CheckResult.Available)
                                yield return id;
                            else if (result == CheckResult.Retry)
                                retry_list.Add(id);
                            else if (result == CheckResult.Unknown)
                                throw new UnknownResponseException(response);
                        }
                    }
                }
                try_list.Clear();
                try_list.AddRange(retry_list);
                retry_list.Clear();
            } while (try_list.Count > 0);
        }

        private string GetRequest(IEnumerable<string> list)
        {
            var builder = new StringBuilder();
            foreach (var id in list)
            {
                builder.AppendLine($"GET {_EndPoint.Replace("{ID}", id)} HTTP/1.1");
                builder.AppendLine($"Host: {_Host}");
                builder.AppendLine("Connection: keep-alive");
                builder.AppendLine("Accept: */*");
                builder.AppendLine("Accept-Language: ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7");
                builder.AppendLine();
            }
            return builder.ToString();
        }

        private Response ReadResponseOne(Stream stream)
        {
            var header = new List<string>();
            for (string line; (line = Encoding.UTF8.GetString(ReadLine(stream))) != ""; header.Add(line)) ;
            int StatusCode = int.Parse(header[0].Substring(9, 3));

            using(var ms = new MemoryStream())
            {
                var buffer = new byte[1024];
                if (header.Exists(d => d.StartsWith("Content-Length")))
                {
                    int ContentLength = int.Parse(header.First(d => d.StartsWith("Content-Length")).Substring(16));
                    do
                    {
                        var size = stream.Read(buffer, 0, Math.Min(buffer.Length, ContentLength));
                        ms.Write(buffer, 0, size);
                        ContentLength -= size;
                    } while (ContentLength > 0);
                }
                else//Chunked
                {
                    while (true)
                    {
                        int ChunkSize = Convert.ToInt32(Encoding.ASCII.GetString(ReadLine(stream)), 16);
                        if (ChunkSize == 0)
                            break;
                        while(ChunkSize > 0)
                        {
                            var size = stream.Read(buffer, 0, Math.Min(buffer.Length, ChunkSize));
                            ms.Write(buffer, 0, size);
                            ChunkSize -= size;
                        }
                        int newline = 2;
                        do newline -= stream.Read(new byte[newline], 0, newline); while (newline > 0);
                    }
                }
                return new Response(StatusCode, Encoding.UTF8.GetString(ms.ToArray()));
            }
        }

        private byte[] ReadLine(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    var buf = stream.ReadByte();
                    if (buf == -1)
                        break;
                    if (unchecked((char)buf) == '\r')
                    {
                        stream.ReadByte();
                        break;
                    }
                    else if (unchecked((char)buf) == '\n')
                    {
                        break;
                    }
                    else
                    {
                        ms.WriteByte(unchecked((byte)buf));
                    }
                }
                return ms.ToArray();
            }
        }
    }
}
