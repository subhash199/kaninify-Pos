using System.Text.Json;
using System.Text.Json.Nodes;
using EposRetail.Models;

namespace EposRetail.Services
{
    public class DepartmentButtonsLayoutStorage
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public async Task<DepartmentButtonsLayoutConfig> LoadAsync(int siteId, int? tillId)
        {
            var settingsPath = GetAppSettingsPath();
            if (!File.Exists(settingsPath))
                return await LoadFromLegacyFilesOrDefaultAsync(siteId, tillId);

            try
            {
                var root = await ReadJsonObjectAsync(settingsPath);
                var layoutNode = GetLayoutNode(root, siteId, tillId)
                                 ?? GetLayoutNode(root, siteId, null);

                if (layoutNode == null)
                    return await LoadFromLegacyFilesOrDefaultAsync(siteId, tillId);

                var json = layoutNode.ToJsonString(JsonOptions);
                var loaded = await TryReadLayoutFromJsonAsync(json);
                if (loaded != null)
                    return loaded;
            }
            catch
            {
            }

            return await LoadFromLegacyFilesOrDefaultAsync(siteId, tillId);
        }

        public async Task SaveAsync(int siteId, int? tillId, DepartmentButtonsLayoutConfig layout)
        {
            layout.RootButtons = layout.RootButtons.OrderBy(b => b.Position).ToList();
            layout.Groups = NormalizeGroups(layout.Groups);

            var settingsPath = GetAppSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);

            JsonObject root;
            try
            {
                root = File.Exists(settingsPath) ? await ReadJsonObjectAsync(settingsPath) : new JsonObject();
            }
            catch
            {
                root = new JsonObject();
            }

            var serializedLayout = JsonSerializer.SerializeToNode(layout, JsonOptions) as JsonObject ?? new JsonObject();
            SetLayoutNode(root, siteId, tillId, serializedLayout);

            var json = root.ToJsonString(JsonOptions);
            await File.WriteAllTextAsync(settingsPath, json);
        }

        public DepartmentButtonsLayoutConfig CreateDefaultLayout()
        {
            return new DepartmentButtonsLayoutConfig
            {
                RootButtons = GetDefaultButtons()
            };
        }

        private static async Task<DepartmentButtonsLayoutConfig?> TryReadLayoutFromJsonAsync(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var defaults = GetDefaultButtons();
            const string fallbackIconPath = "images/icons/svg/apps_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg";

            if (json.TrimStart().StartsWith("["))
            {
                var parsedRoot = JsonSerializer.Deserialize<List<DepartmentButtonConfig>>(json, JsonOptions) ?? new();
                var layout = new DepartmentButtonsLayoutConfig
                {
                    RootButtons = MergeDefaults(defaults, parsedRoot)
                };
                SanitizeIconPaths(layout, fallbackIconPath);
                return layout;
            }

            var parsedLayout = JsonSerializer.Deserialize<DepartmentButtonsLayoutConfig>(json, JsonOptions);
            if (parsedLayout == null)
                return null;

            var result = new DepartmentButtonsLayoutConfig
            {
                Version = parsedLayout.Version,
                RootButtons = MergeDefaults(defaults, parsedLayout.RootButtons),
                Groups = NormalizeGroups(parsedLayout.Groups)
            };
            SanitizeIconPaths(result, fallbackIconPath);
            return result;
        }

        private static void SanitizeIconPaths(DepartmentButtonsLayoutConfig layout, string fallbackIconPath)
        {
            foreach (var button in layout.RootButtons.Concat(layout.Groups.SelectMany(g => g.Value)))
            {
                var icon = (button.IconPath ?? "").Trim();
                if (string.IsNullOrWhiteSpace(icon))
                {
                    button.IconPath = fallbackIconPath;
                    continue;
                }

                if (icon.StartsWith("bi", StringComparison.OrdinalIgnoreCase))
                {
                    button.IconPath = fallbackIconPath;
                }
            }
        }

        private static string GetAppSettingsPath()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(root, "KaninifyRetailEpos", "AppSettings.json");
        }

        private static async Task<JsonObject> ReadJsonObjectAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            if (string.IsNullOrWhiteSpace(json))
                return new JsonObject();

            var node = JsonNode.Parse(json);
            return node as JsonObject ?? new JsonObject();
        }

        private static JsonNode? GetLayoutNode(JsonObject root, int siteId, int? tillId)
        {
            if (!root.TryGetPropertyValue("DepartmentButtons", out var departmentButtonsNode))
                return null;

            var departmentButtons = departmentButtonsNode as JsonObject;
            if (departmentButtons == null)
                return null;

            if (!departmentButtons.TryGetPropertyValue("Layouts", out var layoutsNode))
                return null;

            var layouts = layoutsNode as JsonObject;
            if (layouts == null)
                return null;

            var siteKey = $"site-{siteId}";
            if (!layouts.TryGetPropertyValue(siteKey, out var siteNode))
                return null;

            var siteLayouts = siteNode as JsonObject;
            if (siteLayouts == null)
                return null;

            var tillKey = tillId.HasValue ? $"till-{tillId.Value}" : "till-default";
            if (!siteLayouts.TryGetPropertyValue(tillKey, out var layoutNode))
                return null;

            return layoutNode;
        }

        private static void SetLayoutNode(JsonObject root, int siteId, int? tillId, JsonObject layout)
        {
            var departmentButtons = EnsureObject(root, "DepartmentButtons");
            var layouts = EnsureObject(departmentButtons, "Layouts");

            var siteKey = $"site-{siteId}";
            var siteLayouts = EnsureObject(layouts, siteKey);

            var tillKey = tillId.HasValue ? $"till-{tillId.Value}" : "till-default";
            siteLayouts[tillKey] = layout;
        }

        private static JsonObject EnsureObject(JsonObject parent, string propertyName)
        {
            if (parent.TryGetPropertyValue(propertyName, out var node) && node is JsonObject existing)
                return existing;

            var created = new JsonObject();
            parent[propertyName] = created;
            return created;
        }

        private static async Task<DepartmentButtonsLayoutConfig> LoadFromLegacyFilesOrDefaultAsync(int siteId, int? tillId)
        {
            var legacyBaseDir = GetLegacyBaseDirectory(siteId);
            var legacyTillPath = GetLegacyFilePath(legacyBaseDir, tillId);
            if (File.Exists(legacyTillPath))
            {
                var json = await File.ReadAllTextAsync(legacyTillPath);
                var loaded = await TryReadLayoutFromJsonAsync(json);
                if (loaded != null)
                    return loaded;
            }

            if (tillId.HasValue)
            {
                var legacySitePath = GetLegacyFilePath(legacyBaseDir, null);
                if (File.Exists(legacySitePath))
                {
                    var json = await File.ReadAllTextAsync(legacySitePath);
                    var loaded = await TryReadLayoutFromJsonAsync(json);
                    if (loaded != null)
                        return loaded;
                }
            }

            return new DepartmentButtonsLayoutConfig
            {
                RootButtons = GetDefaultButtons()
            };
        }

        private static string GetLegacyBaseDirectory(int siteId)
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(root, "KaninifyRetailEpos", "DepartmentButtons", $"site-{siteId}");
        }

        private static string GetLegacyFilePath(string baseDir, int? tillId)
        {
            var suffix = tillId.HasValue ? $"till-{tillId.Value}" : "till-default";
            return Path.Combine(baseDir, $"{suffix}.json");
        }

        private static List<DepartmentButtonConfig> MergeDefaults(List<DepartmentButtonConfig> defaults, List<DepartmentButtonConfig> saved)
        {
            var normalized = saved
                .Where(b => b.Position is >= 1 and <= 12)
                .GroupBy(b => b.Position)
                .Select(g => g.First())
                .ToDictionary(b => b.Position, b => b);

            return defaults
                .Select(d =>
                {
                    if (!normalized.TryGetValue(d.Position, out var s))
                        return d;

                    return new DepartmentButtonConfig
                    {
                        Position = d.Position,
                        Label = string.IsNullOrWhiteSpace(s.Label) ? d.Label : s.Label,
                        IconPath = string.IsNullOrWhiteSpace(s.IconPath) ? d.IconPath : s.IconPath,
                        CssClass = string.IsNullOrWhiteSpace(s.CssClass) ? d.CssClass : s.CssClass,
                        TargetType = s.TargetType,
                        ProductBarcode = s.ProductBarcode,
                        PageRoute = s.PageRoute,
                        GroupKey = s.GroupKey
                    };
                })
                .ToList();
        }

        private static Dictionary<string, List<DepartmentButtonConfig>> NormalizeGroups(Dictionary<string, List<DepartmentButtonConfig>>? groups)
        {
            var result = new Dictionary<string, List<DepartmentButtonConfig>>(StringComparer.OrdinalIgnoreCase);
            if (groups == null)
                return result;

            foreach (var kvp in groups)
            {
                var key = kvp.Key?.Trim();
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                var list = kvp.Value ?? new();
                result[key] = list
                    .Where(b => b.Position is >= 2 and <= 13)
                    .GroupBy(b => b.Position)
                    .Select(g => g.First())
                    .Select(b =>
                    {
                        b.Position = Math.Clamp(b.Position, 2, 13);
                        return b;
                    })
                    .Where(b => !IsLegacyPlaceholderButton(b))
                    .OrderBy(b => b.Position)
                    .ToList();
            }

            return result;
        }

        private static bool IsLegacyPlaceholderButton(DepartmentButtonConfig button)
        {
            if (button.TargetType != DepartmentButtonTargetType.Product)
                return false;

            if (!string.IsNullOrWhiteSpace(button.ProductBarcode))
                return false;

            if (!string.Equals(button.IconPath, "images/icons/svg/apps_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.Equals(button.CssClass, "background-colour-light-green", StringComparison.OrdinalIgnoreCase))
                return false;

            var label = (button.Label ?? "").Trim();
            if (!label.StartsWith("Item ", StringComparison.OrdinalIgnoreCase))
                return false;

            return int.TryParse(label.Substring(5).Trim(), out _);
        }

        private static List<DepartmentButtonConfig> GetDefaultButtons() =>
            new()
            {
                new() { Position = 1, Label = "Alcohol", IconPath = "images/icons/svg/liquor_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-red", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Alcohol" },
                new() { Position = 2, Label = "Tobacco", IconPath = "images/icons/svg/smoking_rooms_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-red", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Tobacco" },
                new() { Position = 3, Label = "Grocery", IconPath = "images/icons/svg/grocery_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-green", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Grocery" },
                new() { Position = 4, Label = "Fruit & Veg", IconPath = "images/icons/svg/nutrition_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-green", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Fruit & Veg" },
                new() { Position = 5, Label = "Lottery", IconPath = "images/icons/svg/casino_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-red", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Lottery" },
                new() { Position = 6, Label = "Scratch", IconPath = "images/icons/svg/front_hand_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-red", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Scratch" },
                new() { Position = 7, Label = "Paypoint", IconPath = "images/icons/svg/settings_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-yellow", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Paypoint" },
                new() { Position = 8, Label = "Payzone", IconPath = "images/icons/svg/settings_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-yellow", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Payzone" },
                new() { Position = 9, Label = "Chilled", IconPath = "images/icons/svg/ac_unit_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-blue", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Chilled" },
                new() { Position = 10, Label = "Frozen", IconPath = "images/icons/svg/mode_cool_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-blue", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Frozen Food" },
                new() { Position = 11, Label = "Ice Cream", IconPath = "images/icons/svg/icecream_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-blue", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Ice Cream" },
                new() { Position = 12, Label = "Slush", IconPath = "images/icons/svg/snowing_heavy_24dp_1F1F1F_FILL0_wght400_GRAD0_opsz24.svg", CssClass = "background-colour-light-blue", TargetType = DepartmentButtonTargetType.Product, ProductBarcode = "Slush" }
            };
    }
}
