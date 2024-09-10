﻿using System;
using System.Linq;
using MapleLib.MapleCryptoLib;
using MapleLib.WzLib;
using MapleLib.WzLib.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MapleLib.Configuration
{
    public class EncryptionKey
    {
        private string _iv = "00 00 00 00";
        private string _aesUserKey = "";
        private WzMutableKey _wzKey;

        [JsonProperty("Name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("MapleVersion")]
        [JsonConverter(typeof(StringEnumConverter))]
        public WzMapleVersion MapleVersion { get; set; } = WzMapleVersion.CUSTOM;

        [JsonProperty("Iv")]
        public string Iv {
            get => _iv;
            set {
                if (value.Length != (4 * 3 - 1)) throw new Exception("IV must be 4 bytes");
                if (string.Equals(_iv, value, StringComparison.Ordinal)) return;
                _iv = value;
                _wzKey = null; // force re-generate
            }
        }

        [JsonProperty("AesUserKey")]
        public string AesUserKey {
            get => _aesUserKey;
            set {
                if (value.Length != (32 * 3 - 1)) throw new Exception("AES User Key must be 32 bytes");
                if (string.Equals(_aesUserKey, value, StringComparison.Ordinal)) return;
                _aesUserKey = value;
                _wzKey = null; // force re-generate 
            }
        }

        [JsonIgnore]
        public WzMutableKey WzKey
        {
            get
            {
                if (_wzKey != null)
                    return _wzKey;
                var iv = _iv.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();
                var bytes = _aesUserKey.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();

                var aesUserKey = new byte[MapleCryptoConstants.MAPLESTORY_USERKEY_DEFAULT.Length];
                for (int i = 0; i < aesUserKey.Length; i += 4)
                {
                    aesUserKey[i] = bytes[i / 4];
                    aesUserKey[i + 1] = 0;
                    aesUserKey[i + 2] = 0;
                    aesUserKey[i + 3] = 0;
                }

                _wzKey = WzKeyGenerator.GenerateWzKey(iv, aesUserKey);
                return _wzKey;
            }
        }
    }
}