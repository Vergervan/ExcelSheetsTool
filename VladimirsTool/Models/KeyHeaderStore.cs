using System.Collections.Generic;
using System.Linq;

namespace VladimirsTool.Models
{
    public class KeyHeaderStore
    {
        private static KeyHeaderStore _instance = null;
        private Dictionary<string, KeySettings> _keys = new Dictionary<string, KeySettings>();
        private Dictionary<string, KeySettings> _dateKeys = new Dictionary<string, KeySettings>();
        private KeyHeaderStore() { }
        public static KeyHeaderStore GetInstance()
        {
            return _instance ?? (_instance = new KeyHeaderStore());
        }
        public bool HasKeys => _keys.Count > 0;
        public bool HasDateKeys => _dateKeys.Count > 0;
        public bool Contains(string header) => _keys.ContainsKey(header);
        public bool ContainsDate(string header) => _dateKeys.ContainsKey(header);
        public IDictionary<string, KeySettings> GetDateKeys() => _dateKeys;
        public KeySettings GetSettings(string header)
        {
            KeySettings settings;
            if (!_keys.TryGetValue(header, out settings)) return null;
            return (KeySettings) settings.Clone();
        }

        public void ClearKeys()
        {
            _keys.Clear();
            _dateKeys.Clear();
        }

        public void SetKeys(IEnumerable<KeySettings> keys)
        {
            _keys.Clear();
            _dateKeys.Clear();
            foreach (var key in keys)
            {
                _keys.Add(key.Header, key);
                if (key.IsDate)
                    _dateKeys.Add(key.Header, key);
            }
        }
    }
}
