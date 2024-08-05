using System;

namespace VladimirsTool.Models
{
    public class WorksheetItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsSelected { get; set; }

        public WorksheetItem(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Path);
        }

        public override bool Equals(object obj)
        {
            WorksheetItem item = obj as WorksheetItem;
            return this.Name == item.Name && this.Path == item.Path;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
