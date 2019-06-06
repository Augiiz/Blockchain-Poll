using System;
using System.Security.Cryptography;
using System.Text;


namespace lietuvostalentai
{
    [Serializable]
    public class Block
    {
        public int Index { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public string Data { get; set; }

        public Block(string previousHash, string data)
        {
            Index = 0;
            PreviousHash = previousHash;
            Data = data;
            Hash = CalculateHash();
        }

        public string CalculateHash()
        {
            SHA256 sha256 = SHA256.Create();
            byte[] HashDataBytes = Encoding.ASCII.GetBytes($"{PreviousHash}-{Data}");
            byte[] Encryptedhash = sha256.ComputeHash(HashDataBytes);
            return Convert.ToBase64String(Encryptedhash);

        }
    }
}
