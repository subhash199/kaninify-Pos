using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class EncryptionSettings
    {
        string Salt { get; set; }
        string Pepper { get; set; }

        public EncryptionSettings(string salt, string pepper)
        {
            Salt = salt;
            Pepper = pepper;
        }

        public string GetSalt()
        {
            return Salt;
        }

        public string GetPepper()
        {
            return Pepper;
        }
    }
}
