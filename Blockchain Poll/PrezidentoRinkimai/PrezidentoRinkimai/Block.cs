using System;
using System.Security.Cryptography;
using System.Text;


namespace PrezidentoRinkimai
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

        //Calculate current hash
        public string CalculateHash()
        {
            //Create 256 bytes for hash
            SHA256 sha256 = SHA256.Create();

            //Generate "seed" for hash
            byte[] HashDataBytes = Encoding.ASCII.GetBytes($"{PreviousHash}-{Data}");
            //Create hash
            byte[] Encryptedhash = sha256.ComputeHash(HashDataBytes);

            //Convert to string
            return Convert.ToBase64String(Encryptedhash);

        }
    }
}
