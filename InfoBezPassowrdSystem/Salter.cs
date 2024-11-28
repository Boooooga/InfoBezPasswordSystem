using Npgsql;
using System.Security.Cryptography;

namespace InfoBezPasswordSystem
{
    public class Salter
    {
        private byte[] salt;
        public Salter()
        {
            this.salt = GenerateRandomSalt();
        }
        public byte[] GetSalt { get { return this.salt; } }
        public byte[] GetNewSalt {  get 
            { 
                this.salt = GenerateRandomSalt();
                return this.salt; 
            } 
        }
        private byte[] GenerateRandomSalt(int length = 16)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var salt = new byte[length];
                rng.GetBytes(salt);
                return salt;
            }
        }
    }
}
