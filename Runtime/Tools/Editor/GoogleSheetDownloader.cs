using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Minimoo.Tools
{
    [InitializeOnLoad]
    public class GoogleSheetDownloader
    {
        private const string CREATE_SHEET_DATA_MENU = "MINIMOO/Google Sheet/Create Sheet Data";

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

        private static async void DownloadAllSheets()
        {
            var sheetDataAssets = FindAllSheetDataAssets();
            foreach (var sheetData in sheetDataAssets)
            {
                if (!sheetData.IsDownloaded)
                {
                    var success = await sheetData.EnsureDataDownloaded();
                    if (!success)
                    {
                        Debug.LogError($"시트 다운로드 실패: {sheetData.name}");
                    }
                }
            }
        }

        private static List<GoogleSheetData> FindAllSheetDataAssets()
        {
            var guids = AssetDatabase.FindAssets("t:GoogleSheetData");
            return guids.Select(guid => 
                AssetDatabase.LoadAssetAtPath<GoogleSheetData>(
                    AssetDatabase.GUIDToAssetPath(guid))).ToList();
        }

        [MenuItem(CREATE_SHEET_DATA_MENU)]
        public static void CreateGoogleSheetData()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "구글 시트 데이터 생성",
                "NewGoogleSheetData",
                "asset",
                "구글 시트 데이터를 생성할 위치를 선택하세요.");

            if (string.IsNullOrEmpty(path))
                return;

            var sheetData = ScriptableObject.CreateInstance<GoogleSheetData>();
            AssetDatabase.CreateAsset(sheetData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = sheetData;
        }
    }
}