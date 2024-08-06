using System.Collections.Generic;
using System.Linq;

namespace VladimirsTool.Models
{
    public class KeyHeaderStore
    {
        private static KeyHeaderStore _instance = null;
        private Dictionary<string, KeySettings> _keys = new Dictionary<string, KeySettings>();
        private KeyHeaderStore() { }
        public static KeyHeaderStore GetInstance()
        {
            return _instance ?? (_instance = new KeyHeaderStore());
        }

        public bool Contains(string header) => _keys.ContainsKey(header);
        public KeySettings GetSettings(string header) => (KeySettings)_keys[header]?.Clone();

        public void ClearKeys() => _keys.Clear();

        public void SetKeys(IEnumerable<KeySettings> keys)
        {
            _keys.Clear();
            foreach(var key in keys)
                _keys.Add(key.Header, key);
        }
    }
}
