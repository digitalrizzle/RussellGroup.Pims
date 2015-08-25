using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RussellGroup.Pims.DataAccess.Models
{
    public class Content
    {
        public Content()
        {
        }

        public Content(byte[] data)
        {
            Data = data;
        }

        public int Id { get; set; }

        private byte[] _data;
        public byte[] Data
        {
            get { return _data; }
            set
            {
                _data = value;

                Hash = value != null ? GetHash(value) : null;
            }
        }

        public string Hash { get; set; }

        public static string GetHash(byte[] data)
        {
            if (data == null) return null;

            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                return Convert.ToBase64String(sha1.ComputeHash(data));
            }
        }
    }
}
