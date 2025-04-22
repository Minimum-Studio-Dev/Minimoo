using UnityEditor;
using UnityEngine;
using System.IO;

namespace Minimoo.Tools.Editor
{
    public class PrefabVariantCreator
    {
        const string CREATE_VARIANT_MENU = "MINIMOO/Prefab/Create Variant";

        [MenuItem(CREATE_VARIANT_MENU)]
        public static void CreateVariant()
        {
            // 선택된 오브젝트 가져오기
            var selectedObject = Selection.activeObject;
            if (selectedObject == null)
            {
                Debug.LogError("프리팹을 선택해주세요.");
                return;
            }

            string basePrefabPath = "Assets/Prefabs/Base.prefab"; // 기준 프리팹
            string targetPrefabPath = AssetDatabase.GetAssetPath(selectedObject); // 선택된 프리팹

            // 저장 위치 선택
            string defaultFileName = Path.GetFileNameWithoutExtension(targetPrefabPath) + "Variant.prefab";
            string defaultDirectory = Path.GetDirectoryName(targetPrefabPath);
            string variantSavePath = EditorUtility.SaveFilePanel(
                "Variant 프리팹 저장",
                defaultDirectory,
                defaultFileName,
                "prefab"
            );

            // 사용자가 취소했거나 경로가 비어있는 경우
            if (string.IsNullOrEmpty(variantSavePath))
            {
                return;
            }

            // 선택된 경로를 프로젝트 상대 경로로 변환
            variantSavePath = "Assets" + variantSavePath.Replace(Application.dataPath, "").Replace("\\", "/");

            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
            GameObject targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPrefabPath);

            if (basePrefab == null || targetPrefab == null)
            {
                Debug.LogError("Base 또는 Target 프리팹 경로가 잘못되었습니다.");
                return;
            }

            // 기존 프리팹을 인스턴스화
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);

            // 기존 프리팹에서 구성 복사
            CopyTransform(targetPrefab.transform, instance.transform);

            // Prefab Variant로 저장
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, variantSavePath);
            Object.DestroyImmediate(instance);

            Debug.Log("Variant 프리팹 생성 완료: " + variantSavePath);
        }

        static void CopyTransform(Transform source, Transform destination)
        {
            destination.name = source.name;
            destination.localPosition = source.localPosition;
            destination.localRotation = source.localRotation;
            destination.localScale = source.localScale;

            // 자식도 재귀적으로 복사
            for (int i = 0; i < source.childCount; i++)
            {
                Transform sourceChild = source.GetChild(i);
                Transform destChild = destination.Find(sourceChild.name);

                if (destChild != null)
                {
                    CopyTransform(sourceChild, destChild);
                }
            }
        }
    }
}

