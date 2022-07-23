#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "SpritePalette", menuName = "Skins/SpritePalette", order = 0)]
public class SpritePalette : ScriptableObject {
    [SerializeField] private bool enableBackups = false;
    [SerializeField] private bool exportAseprite = true;
    [SerializeField] private bool exportByTag = true;
    [SerializeField] private string asepritePath;
    private string path;
    public void GeneratePalette() {
        
        path = AssetDatabase.GetAssetPath(this);
        path = path.Substring(0, path.Length - 19);
        DirectoryInfo d = new DirectoryInfo(path); //Assuming Test is your Folder

        FileInfo[] files = d.GetFiles("*.png"); //Getting Text files

        PaletteDictionary palette = new PaletteDictionary();
        string str = null; 

        if (exportAseprite) {
            FileInfo[] export = d.GetFiles("*.aseprite");
            foreach(FileInfo file in export ) {
                if (exportByTag) {
                    List<string> tags = new List<string>();
                    System.Diagnostics.ProcessStartInfo getTags = new System.Diagnostics.ProcessStartInfo();
                    getTags.FileName = asepritePath;
                    getTags.Arguments = $"--list-tags {file.Name}";
                    getTags.WorkingDirectory = file.DirectoryName;
                    getTags.UseShellExecute = false;
                    getTags.RedirectStandardOutput = true;

                    Debug.Log($"finding tags: {getTags.FileName} {getTags.Arguments}");
                    var proc0 = new System.Diagnostics.Process();
                    proc0.StartInfo = getTags;
                    proc0.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) =>
                    {
                        if (e.Data != null) {
                            tags.Add(e.Data);
                            Debug.Log($"Found tag: {e.Data}");
                        }
                    });
                    proc0.Start();
                    proc0.BeginOutputReadLine();
                    proc0.WaitForExit();
                    Debug.Log($"Export finished with code {proc0.ExitCode}");

                    foreach (var tag in tags) {
                        System.Diagnostics.ProcessStartInfo exportAll = new System.Diagnostics.ProcessStartInfo();
                        exportAll.FileName = asepritePath;
                        exportAll.Arguments = $"-b --tag {tag} {file.Name} --sheet {file.Name.Replace(".aseprite", $"_{tag}.png")} --sheet-type horizontal";
                        exportAll.WorkingDirectory = file.DirectoryName;
                        exportAll.UseShellExecute = true;
                        exportAll.ErrorDialog = true;
                        Debug.Log($"running export: {exportAll.FileName} {exportAll.Arguments}");
                        var proc1 = System.Diagnostics.Process.Start(exportAll);
                        proc1.WaitForExit();
                        Debug.Log($"Export finished with code {proc1.ExitCode}");
                    } 
                } else {
                    System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
                    info.FileName = asepritePath;
                    info.Arguments = $"-b {file.Name} --sheet {file.Name.Replace(".aseprite", ".png")} --sheet-type horizontal";
                    info.WorkingDirectory = file.DirectoryName;
                    info.UseShellExecute = true;
                    info.ErrorDialog = true;
                    Debug.Log($"running export: {info.FileName} {info.Arguments}");
                    var proc = System.Diagnostics.Process.Start(info);
                    proc.WaitForExit();
                    Debug.Log($"Export finished with code {proc.ExitCode}");
                }
            }
        }

        foreach(FileInfo file in files ) {
            if (file.Name.Equals("palette.png"))
                continue;
            if (file.Name.Contains("-bak.png"))
                continue;
            AddImage(file, ref palette);
            if (str == null)
                str = file.Name;
            else 
                str += $", {file.Name}";
        }

        palette.Output(path);
        Debug.Log($"Generated palette for images: {str}");
        
    }

    private void AddImage(FileInfo info, ref PaletteDictionary palette) {
        var content = File.ReadAllBytes(info.FullName);
        var tex = new Texture2D(1, 1);
        tex.LoadImage(content);

        if (enableBackups && !File.Exists(info.FullName.Replace(".png", "-bak.png"))) {
            File.WriteAllBytes(info.FullName.Replace(".png", "-bak.png"), tex.EncodeToPNG());
        }

        for (int x = 0; x < tex.width; x++) {
            for (int y = 0; y < tex.height; y++) {
                Color32 col = tex.GetPixel(x, y);
                Vector2Int pos = palette.Get(col);
                col.r = (byte)pos.x;
                col.g = (byte) pos.y;

                tex.SetPixel(x, y, col);
            }
        }

        File.WriteAllBytes(info.FullName, tex.EncodeToPNG());
    }
    private class PaletteDictionary {
        
        Dictionary<Color32, Vector2Int> dict = new Dictionary<Color32, Vector2Int>();
        Vector2Int pos = Vector2Int.zero;
        int max = 0;
        public Vector2Int Get(Color32 color) {
            if (dict.ContainsKey(color))
                return dict[color];

            Vector2Int val = pos;
            dict.Add(color, pos);

            if (pos.x < max) {
                pos.x++;
            } else if (pos.y > 0) {
                pos.y--;
            } else {
                max++;
                pos.x = 0;
                pos.y = max;
            }
            
            return val;
        }

    

        public void Output(string path) {
            max = max + 1;
            var tex = new Texture2D(max, max);
            foreach(var val in dict) {
                tex.SetPixel(val.Value.x, val.Value.y, val.Key);
            }

            File.WriteAllBytes($"{path}palette.png", tex.EncodeToPNG());
        }
    }
}
#endif