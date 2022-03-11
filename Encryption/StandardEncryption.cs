using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace A_Common_Library.Encryption
{
    public class StandardEncryption : IDisposable
    {
        private byte[][] keys = new byte[2][];
        private int key_size = 8;
        public byte[] salt { get; private set; } = new byte[8] { 42, 42, 118, 105, 67, 114, 80, 114 }; //defaulted to **viCrPr
        public byte[] IV { get; private set; } = new byte[8] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        public StandardEncryption()
        {
            Random PRNG = new Random(1585822569);

            this.keys[0] = new byte[this.key_size];
            this.keys[1] = new byte[this.key_size];

            PRNG.NextBytes(this.keys[0]);
            PRNG.NextBytes(this.keys[1]);
        }

        public void SetRGB(byte[] rgb_key, byte[] rgb_IV) 
        {
            this.salt = rgb_key;
            this.IV = rgb_IV;
        }

        public byte[][] GetRandomKeys() 
        {
            return keys;
        }

        #region Crypto Functions
        public string Encrypt(string Text)
        {
            if (string.IsNullOrEmpty(Text)) return string.Empty;

            using (DESCryptoServiceProvider Crypto_Service = new DESCryptoServiceProvider())
            using (MemoryStream Memory_Stream = new MemoryStream())
            using (CryptoStream Crypto_Stream = new CryptoStream(Memory_Stream, Crypto_Service.CreateEncryptor(keys[0], keys[1]), CryptoStreamMode.Write))
            {
                byte[] Input_Bytes = Encoding.UTF8.GetBytes(Text);

                Crypto_Stream.Write(Input_Bytes, 0, Input_Bytes.Length);

                Crypto_Stream.FlushFinalBlock();

                string Encrypted_Text = Convert.ToBase64String(Memory_Stream.ToArray());

                return Encrypted_Text;
            }
        }

        public string Decrypt(string Text)
        {
            if (string.IsNullOrEmpty(Text)) return string.Empty;

            using (DESCryptoServiceProvider Crypto_Service = new DESCryptoServiceProvider())
            using (MemoryStream Memory_Stream = new MemoryStream())
            using (CryptoStream Crypto_Stream = new CryptoStream(Memory_Stream, Crypto_Service.CreateEncryptor(keys[0], keys[1]), CryptoStreamMode.Write))
            {
                byte[] Input_Bytes = Convert.FromBase64String(Text);

                Crypto_Stream.Write(Input_Bytes, 0, Input_Bytes.Length);

                Crypto_Stream.FlushFinalBlock();

                Crypto_Stream.Close();

                return Encoding.UTF8.GetString(Memory_Stream.ToArray());
            }
        }

        public string Encrypt_Legacy(string Text)
        {
            if (string.IsNullOrEmpty(Text)) return string.Empty;

            using (DESCryptoServiceProvider Crypto_Service = new DESCryptoServiceProvider())
            using (MemoryStream Memory_Stream = new MemoryStream())
            using (CryptoStream Crypto_Stream = new CryptoStream(Memory_Stream, Crypto_Service.CreateEncryptor(this.salt, this.IV), CryptoStreamMode.Write))
            {
                byte[] Input_Bytes = Encoding.UTF8.GetBytes(Text);

                Crypto_Stream.Write(Input_Bytes, 0, Input_Bytes.Length);

                Crypto_Stream.FlushFinalBlock();

                string Encrypted_Text = Convert.ToBase64String(Memory_Stream.ToArray());

                return Encrypted_Text;
            }
        }

        public string Decrypt_Legacy(string Text)
        {
            if (string.IsNullOrEmpty(Text)) return string.Empty;

            using (DESCryptoServiceProvider Crypto_Service = new DESCryptoServiceProvider())
            using (MemoryStream Memory_Stream = new MemoryStream())
            using (CryptoStream Crypto_Stream = new CryptoStream(Memory_Stream, Crypto_Service.CreateEncryptor(this.salt, this.IV), CryptoStreamMode.Write))
            {
                byte[] input_bytes = Convert.FromBase64String(Text);

                Crypto_Stream.Write(input_bytes, 0, input_bytes.Length);

                Crypto_Stream.FlushFinalBlock();

                Crypto_Stream.Close();

                return Encoding.UTF8.GetString(Memory_Stream.ToArray());
            }
        }

        #endregion //Crypto Functions

        #region IDisposable Implementation
        private bool disposed = false;

        ~StandardEncryption()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                //Console.WriteLine("This is the first call to Dispose. Necessary clean-up will be done!");

                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //Console.WriteLine("Explicit call: Dispose is called by the user.");
                }
                else
                {
                    //Console.WriteLine("Implicit call: Dispose is called through finalization.");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

                // TODO: set large fields to null.

                disposed = true;
            }
            else
            {
                //Console.WriteLine("Dispose is called more than one time. No need to clean up!");
            }
        }
        #endregion
    }
}
