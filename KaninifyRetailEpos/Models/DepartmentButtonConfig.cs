using System.Text.Json.Serialization;

namespace EposRetail.Models
{
    public enum DepartmentButtonTargetType
    {
        Product = 0,
        Group = 1
    }

    public class DepartmentButtonConfig
    {
        public int Position { get; set; }
        public string Label { get; set; } = "";
        public string IconPath { get; set; } = "";
        public string CssClass { get; set; } = "";
        public DepartmentButtonTargetType TargetType { get; set; } = DepartmentButtonTargetType.Product;
        public string? ProductBarcode { get; set; }
        public string? PageRoute { get; set; }
        public string? GroupKey { get; set; }

        [JsonIgnore]
        public string EffectiveProductBarcode => ProductBarcode ?? "";
    }

    public class DepartmentButtonsLayoutConfig
    {
        public int Version { get; set; } = 2;
        public List<DepartmentButtonConfig> RootButtons { get; set; } = new();
        public Dictionary<string, List<DepartmentButtonConfig>> Groups { get; set; } = new();
    }
}
