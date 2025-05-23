using System;

namespace KeyValueManager.App.Models
{
    public class KeyValueEntry
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 