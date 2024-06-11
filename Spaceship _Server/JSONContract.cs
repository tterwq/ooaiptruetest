using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace Spaceship__Server
{
    public class JSONContract
        {
        [DataMember]
        public JsonDictionary Value { get; set; }
        }

    [Serializable]
    public class JsonDictionary : ISerializable
        {
        private Dictionary<string, object> m_entries;

        public JsonDictionary()
            {
            m_entries = new Dictionary<string, object>();
            }

        public JsonDictionary(Dictionary<string, object> data)
            {
            m_entries = data;
            }

        public IEnumerable<KeyValuePair<string, object>> Entries
            {
            get { return m_entries; }
            }
        public JsonDictionary(SerializationInfo info, StreamingContext context)
            {
            m_entries = new Dictionary<string, object>();
            foreach (var entry in info)
                {
                m_entries.Add(entry.Name, entry.Value);
                }
            }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
            foreach (var entry in m_entries)
                {
                info.AddValue(entry.Key, entry.Value);
                }
            }
        }
}
