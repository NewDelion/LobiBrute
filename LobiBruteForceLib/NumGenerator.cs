using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobiBruteForceLib
{
    public class NumGenerator
    {
        public string Charset { get; private set; }
        public int Digits => _IndexArray.Length;
        private int[] _IndexArray;
        private int[] _EndArray;

        public NumGenerator(string charset, int digits)
        {
            Charset = charset ?? throw new ArgumentNullException("charset");
            _IndexArray = new int[digits];
            _EndArray = new int[digits];
            for (int i = 0; i < digits; ++i)
                _EndArray[i] = charset.Last();
        }

        public NumGenerator(string charset, string start)
        {
            Charset = charset ?? throw new ArgumentNullException("charset");
            if (start == null)
                throw new ArgumentNullException("start");
            if (start == "")
                throw new Exception("開始位置は1文字以上で指定してください");
            int digits = start.Length;
            _IndexArray = new int[digits];
            _EndArray = new int[digits];
            for (int i = 0; i < digits; ++i)
            {
                if((_IndexArray[i] = charset.IndexOf(start[i])) == -1)
                    throw new Exception("文字セットに含まれていない文字が開始位置に使用されています");

                _EndArray[i] = charset.Last();
            }
        }

        public NumGenerator(string charset, string start, string end)
        {
            Charset = charset ?? throw new ArgumentNullException("charset");
            if (start == null)
                throw new ArgumentNullException("start");
            if (end == null)
                throw new ArgumentNullException("end");
            if (start == "")
                throw new Exception("開始位置は1文字以上で指定してください");
            if (end == "")
                throw new Exception("終了位置は1文字以上で指定してください");
            if (start.Length != end.Length)
                throw new Exception("開始位置と終了位置の桁数を一致していません");
            int digits = start.Length;
            _IndexArray = new int[digits];
            _EndArray = new int[digits];
            for (int i = 0; i < digits; ++i)
            {
                if ((_IndexArray[i] = charset.IndexOf(start[i])) == -1)
                    throw new Exception("文字セットに含まれていない文字が開始位置に使用されています");
                if ((_EndArray[i] = charset.IndexOf(end[i])) == -1)
                    throw new Exception("文字セットに含まれていない文字が終了位置に使用されています");
            }
        }

        /// <summary>
        /// オーバーフローするとtrueを返します
        /// </summary>
        public bool Add(int amount)
        {
            var index = 0;
            do
            {
                _IndexArray[index] += amount;
                amount = _IndexArray[index] / Charset.Length;
                _IndexArray[index] %= Charset.Length;
            } while (++index < _IndexArray.Length);
            if (index == _IndexArray.Length && amount > 0)//Overflow
                return true;
            return false;
        }

        public override string ToString() => new string(_IndexArray.Select(c => Charset[c]).ToArray());
    }
}
