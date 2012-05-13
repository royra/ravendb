using System;
using System.Linq;

namespace Raven.WebConsole.Entities
{
    public class Password
    {
        private byte[] Hash { get; set; }
        private byte[] Salt { get; set; }

        public int Version { get; private set; }

        public const int CURRENT_VERSION = 1;
        public const int NUM_ITERATIONS = 2000;
        public const int HASH_SIZE = 24;
        public const int SALT_SIZE = 256;

// ReSharper disable UnusedMember.Local
        private Password()
// ReSharper restore UnusedMember.Local
        {
        }

        public Password(string cleartext)
        {
            if (cleartext == null) throw new ArgumentNullException("cleartext");

            using (var p = new System.Security.Cryptography.Rfc2898DeriveBytes(cleartext, SALT_SIZE, NUM_ITERATIONS))
            {
                Hash = p.GetBytes(HASH_SIZE);
                Salt = p.Salt;
                Version = CURRENT_VERSION;
            }
        }

        public bool Check(string password)
        {
            if (Version != CURRENT_VERSION)
                throw new Exception(string.Format("Invalid password version; expected '{0}', received '{1}'", CURRENT_VERSION, Version));

            using (var p = new System.Security.Cryptography.Rfc2898DeriveBytes(password, Salt, NUM_ITERATIONS))
            {
                return p.GetBytes(HASH_SIZE).SequenceEqual(Hash);
            }
        }
    }
}