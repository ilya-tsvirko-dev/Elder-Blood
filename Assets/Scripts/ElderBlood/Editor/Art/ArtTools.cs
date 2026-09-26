using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using ElderBlood.Services;
using UnityEngine;

namespace ElderBlood.Editor.Art {
    /// <summary>
    /// Menu entries for enforcing and auditing the pixel-art pipeline.
    /// </summary>
    public static class ArtTools {
        private const int SoftAlphaMin = 1;
        private const int SoftAlphaMax = 254;

        [MenuItem("Tools/Elder Blood/Art/Reimport All Sprites")]
        public static void ReimportAllSprites() {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites", "Assets/Art" });
            int fixedCount = 0;

            try {
                AssetDatabase.StartAssetEditing();

                for (int i = 0; i < guids.Length; i++) {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (!SpriteImportRules.TryResolve(path, out var domain)) {
                        continue;
                    }

                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) {
                        continue;
                    }

                    EditorUtility.DisplayProgressBar("Elder Blood", path, (float)i / guids.Length);

                    if (SpriteImportPostprocessor.Apply(importer, domain)) {
                        importer.SaveAndReimport();
                        fixedCount++;
                        GameLogger.Art($"[Art] Fixed import settings ({domain.Name}, PPU {domain.PixelsPerUnit}): {path}");
                    }
                }
            }
            finally {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }

            GameLogger.Art($"[Art] Reimport complete. {fixedCount} of {guids.Length} textures corrected.");
        }

        [MenuItem("Tools/Elder Blood/Art/Validate Art")]
        public static void ValidateArt() {
            var palette = LoadPalette();
            if (palette == null) {
                GameLogger.Error($"[Art] Palette not found at {SpriteImportRules.PaletteAssetPath}");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
            var report = new StringBuilder();
            report.AppendLine($"Elder Blood art validation - palette holds {palette.Count} colours");
            report.AppendLine();

            int dirtyFiles = 0;

            for (int i = 0; i < guids.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                if (!TryLoadPixels(path, out var pixels)) {
                    continue;
                }

                int softAlpha = 0;
                var offPalette = new HashSet<int>();

                for (int p = 0; p < pixels.Length; p++) {
                    var c = pixels[p];
                    if (c.a >= SoftAlphaMin && c.a <= SoftAlphaMax) {
                        softAlpha++;
                    }

                    if (c.a < 128) {
                        continue;
                    }

                    int key = (c.r << 16) | (c.g << 8) | c.b;
                    if (!palette.Contains(key)) {
                        offPalette.Add(key);
                    }
                }

                if (softAlpha == 0 && offPalette.Count == 0) {
                    continue;
                }

                dirtyFiles++;
                report.AppendLine(path);
                if (softAlpha > 0) {
                    report.AppendLine($"    soft alpha : {softAlpha} px (pixel art wants 0 or 255 only)");
                }

                if (offPalette.Count > 0) {
                    report.AppendLine($"    off-palette: {offPalette.Count} distinct colours");
                    int shown = 0;
                    foreach (int key in offPalette) {
                        if (shown >= 8) {
                            report.AppendLine("                 ...");
                            break;
                        }

                        report.AppendLine($"                 #{key:X6}");
                        shown++;
                    }
                }

                report.AppendLine();
            }

            report.AppendLine($"{dirtyFiles} file(s) need attention.");
            GameLogger.Art(report.ToString());
        }

        private static HashSet<int> LoadPalette() {
            string full = Path.Combine(Directory.GetCurrentDirectory(), SpriteImportRules.PaletteAssetPath);
            if (!File.Exists(full)) {
                return null;
            }

            var result = new HashSet<int>();
            string[] lines = File.ReadAllLines(full);

            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i].Trim();
                if (line.Length == 0 || line[0] == '#') {
                    continue;
                }

                string[] parts = line.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) {
                    continue;
                }

                if (int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int r)
                    && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int g)
                    && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int b)) {
                    result.Add((r << 16) | (g << 8) | b);
                }
            }

            return result;
        }

        private static bool TryLoadPixels(string assetPath, out Color32[] pixels) {
            pixels = null;
            string full = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            if (!File.Exists(full)) {
                return false;
            }

            // Load outside the AssetDatabase so the texture does not need Read/Write enabled.
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bool ok = tex.LoadImage(File.ReadAllBytes(full));
            if (ok) {
                pixels = tex.GetPixels32();
            }

            Object.DestroyImmediate(tex);
            return ok;
        }
    }
}
