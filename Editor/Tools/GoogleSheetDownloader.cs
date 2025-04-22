using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minimoo.Tools.Editor
{
    [InitializeOnLoad]
    public class GoogleSheetDownloader
    {
        static GoogleSheetDownloader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                DownloadAllSheets();
            }
        }

        private static void DownloadAllSheets()
        {
            var sheetDataAssets = FindAllSheetDataAssets();
            foreach (var sheetData in sheetDataAssets)
            {
                sheetData.Download();
            }
        }

        private static List<GoogleSheetData> FindAllSheetDataAssets()
        {
            var guids = AssetDatabase.FindAssets("t:GoogleSheetData");
            return guids.Select(guid =>
                AssetDatabase.LoadAssetAtPath<GoogleSheetData>(
                    AssetDatabase.GUIDToAssetPath(guid))).ToList();
        }
    }
}