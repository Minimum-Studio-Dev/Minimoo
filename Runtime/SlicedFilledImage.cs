using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_2017_4 || UNITY_2018_2_OR_NEWER
using UnityEngine.U2D;
#endif
using Sprites = UnityEngine.Sprites;

namespace Minimoo
{
    #if UNITY_EDITOR
    using UnityEditor;

    // Custom Editor to order the variables in the Inspector similar to Image component
    [CustomEditor( typeof( SlicedFilledImage ) ), CanEditMultipleObjects]
    public class SlicedFilledImageEditor : Editor
    {
        private SerializedProperty spriteProp, colorProp, materialProp, raycastTargetProp, maskableProp;
        private SerializedProperty fillDirectionProp, fillAmountProp, fillCenterProp, pixelsPerUnitMultiplierProp;
        private GUIContent spriteLabel, colorLabel, materialLabel, raycastTargetLabel, maskableLabel;
        private GUIContent fillDirectionLabel, fillAmountLabel, fillCenterLabel, pixelsPerUnitMultiplierLabel;

        private void OnEnable()
        {
            spriteProp = serializedObject.FindProperty("m_Sprite");
            colorProp = serializedObject.FindProperty("m_Color");
            materialProp = serializedObject.FindProperty("m_Material");
            raycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");
            maskableProp = serializedObject.FindProperty("m_Maskable");
            fillDirectionProp = serializedObject.FindProperty("m_FillDirection");
            fillAmountProp = serializedObject.FindProperty("m_CustomFillAmount");
            fillCenterProp = serializedObject.FindProperty("m_CustomFillCenter");
            pixelsPerUnitMultiplierProp = serializedObject.FindProperty("m_CustomPixelsPerUnitMultiplier");

            spriteLabel = new GUIContent("Source Image");
            colorLabel = new GUIContent("Color");
            materialLabel = new GUIContent("Material");
            raycastTargetLabel = new GUIContent("Raycast Target");
            maskableLabel = new GUIContent("Maskable");
            fillDirectionLabel = new GUIContent("Fill Direction");
            fillAmountLabel = new GUIContent("Fill Amount");
            fillCenterLabel = new GUIContent("Fill Center");
            pixelsPerUnitMultiplierLabel = new GUIContent("Pixels Per Unit Multiplier");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(spriteProp, spriteLabel);
            EditorGUILayout.PropertyField(colorProp, colorLabel);
            EditorGUILayout.PropertyField(materialProp, materialLabel);
            EditorGUILayout.PropertyField(raycastTargetProp, raycastTargetLabel);
            EditorGUILayout.PropertyField(maskableProp, maskableLabel);
            EditorGUILayout.PropertyField(fillDirectionProp, fillDirectionLabel);
            EditorGUILayout.PropertyField(fillAmountProp, fillAmountLabel);
            EditorGUILayout.PropertyField(fillCenterProp, fillCenterLabel);
            EditorGUILayout.PropertyField(pixelsPerUnitMultiplierProp, pixelsPerUnitMultiplierLabel);

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif

    // Credit: https://bitbucket.org/Unity-Technologies/ui/src/2018.4/UnityEngine.UI/UI/Core/Image.cs
    [AddComponentMenu( "UI/Sliced Filled Image", 11 )]
    public class SlicedFilledImage : Image
    {
        public enum FillDirection { Right = 0, Left = 1, Up = 2, Down = 3 }

        [SerializeField]
        private FillDirection m_FillDirection;
        public FillDirection fillDirection
        {
            get => m_FillDirection;
            set
            {
                if (m_FillDirection != value)
                {
                    m_FillDirection = value;
                    SetVerticesDirty();
                }
            }
        }

        [Range(0, 1)]
        [SerializeField]
        private float m_CustomFillAmount = 1f;
        public float fillAmount
        {
            get => m_CustomFillAmount;
            set
            {
                var clamped = Mathf.Clamp01(value);
                if (!Mathf.Approximately(m_CustomFillAmount, clamped))
                {
                    m_CustomFillAmount = clamped;
                    SetVerticesDirty();
                }
            }
        }

        [SerializeField]
        private bool m_CustomFillCenter = true;
        public bool fillCenter
        {
            get => m_CustomFillCenter;
            set
            {
                if (m_CustomFillCenter != value)
                {
                    m_CustomFillCenter = value;
                    SetVerticesDirty();
                }
            }
        }

        [SerializeField]
        private float m_CustomPixelsPerUnitMultiplier = 1f;
        public float pixelsPerUnitMultiplier
        {
            get => m_CustomPixelsPerUnitMultiplier;
            set => m_CustomPixelsPerUnitMultiplier = Mathf.Max(0.01f, value);
        }

        public float customPixelsPerUnit
        {
            get
            {
                var spritePixelsPerUnit = 100f;
                if (overrideSprite)
                    spritePixelsPerUnit = overrideSprite.pixelsPerUnit;
                var referencePixelsPerUnit = 100f;
                if (canvas)
                    referencePixelsPerUnit = canvas.referencePixelsPerUnit;
                return m_CustomPixelsPerUnitMultiplier * spritePixelsPerUnit / referencePixelsPerUnit;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (overrideSprite == null)
            {
                base.OnPopulateMesh(vh);
                return;
            }
            GenerateSlicedFilledSprite(vh);
        }

        private void GenerateSlicedFilledSprite(VertexHelper vh)
        {
            vh.Clear();
            if (m_CustomFillAmount < 0.001f)
                return;
            var rect = GetPixelAdjustedRect();
            var outer = Sprites.DataUtility.GetOuterUV(overrideSprite);
            var padding = Sprites.DataUtility.GetPadding(overrideSprite);
            var hasBorder = overrideSprite.border.sqrMagnitude > 0f;
            if (!hasBorder)
            {
                var size = overrideSprite.rect.size;
                var spriteW = Mathf.RoundToInt(size.x);
                var spriteH = Mathf.RoundToInt(size.y);
                var vertices = new Vector4(
                    rect.x + rect.width * (padding.x / spriteW),
                    rect.y + rect.height * (padding.y / spriteH),
                    rect.x + rect.width * ((spriteW - padding.z) / spriteW),
                    rect.y + rect.height * ((spriteH - padding.w) / spriteH));
                GenerateFilledSprite(vh, vertices, outer, m_CustomFillAmount);
                return;
            }
            var inner = Sprites.DataUtility.GetInnerUV(overrideSprite);
            var border = GetAdjustedBorders(overrideSprite.border / customPixelsPerUnit, rect);
            padding = padding / customPixelsPerUnit;
            var s_SlicedVertices = new Vector2[4];
            var s_SlicedUVs = new Vector2[4];
            s_SlicedVertices[0] = new Vector2(padding.x, padding.y);
            s_SlicedVertices[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);
            s_SlicedVertices[1].x = border.x;
            s_SlicedVertices[1].y = border.y;
            s_SlicedVertices[2].x = rect.width - border.z;
            s_SlicedVertices[2].y = rect.height - border.w;
            for (var i = 0; i < 4; ++i)
            {
                s_SlicedVertices[i].x += rect.x;
                s_SlicedVertices[i].y += rect.y;
            }
            s_SlicedUVs[0] = new Vector2(outer.x, outer.y);
            s_SlicedUVs[1] = new Vector2(inner.x, inner.y);
            s_SlicedUVs[2] = new Vector2(inner.z, inner.w);
            s_SlicedUVs[3] = new Vector2(outer.z, outer.w);
            float rectStartPos, _1OverTotalSize;
            if (m_FillDirection == FillDirection.Left || m_FillDirection == FillDirection.Right)
            {
                rectStartPos = s_SlicedVertices[0].x;
                var totalSize = (s_SlicedVertices[3].x - s_SlicedVertices[0].x);
                _1OverTotalSize = totalSize > 0f ? 1f / totalSize : 1f;
            }
            else
            {
                rectStartPos = s_SlicedVertices[0].y;
                var totalSize = (s_SlicedVertices[3].y - s_SlicedVertices[0].y);
                _1OverTotalSize = totalSize > 0f ? 1f / totalSize : 1f;
            }
            for (var x = 0; x < 3; x++)
            {
                var x2 = x + 1;
                for (var y = 0; y < 3; y++)
                {
                    if (!m_CustomFillCenter && x == 1 && y == 1)
                        continue;
                    var y2 = y + 1;
                    float sliceStart, sliceEnd;
                    switch (m_FillDirection)
                    {
                        case FillDirection.Right:
                            sliceStart = (s_SlicedVertices[x].x - rectStartPos) * _1OverTotalSize;
                            sliceEnd = (s_SlicedVertices[x2].x - rectStartPos) * _1OverTotalSize;
                            break;
                        case FillDirection.Up:
                            sliceStart = (s_SlicedVertices[y].y - rectStartPos) * _1OverTotalSize;
                            sliceEnd = (s_SlicedVertices[y2].y - rectStartPos) * _1OverTotalSize;
                            break;
                        case FillDirection.Left:
                            sliceStart = 1f - (s_SlicedVertices[x2].x - rectStartPos) * _1OverTotalSize;
                            sliceEnd = 1f - (s_SlicedVertices[x].x - rectStartPos) * _1OverTotalSize;
                            break;
                        case FillDirection.Down:
                            sliceStart = 1f - (s_SlicedVertices[y2].y - rectStartPos) * _1OverTotalSize;
                            sliceEnd = 1f - (s_SlicedVertices[y].y - rectStartPos) * _1OverTotalSize;
                            break;
                        default:
                            sliceStart = sliceEnd = 0f;
                            break;
                    }
                    if (sliceStart >= m_CustomFillAmount)
                        continue;
                    var vertices = new Vector4(s_SlicedVertices[x].x, s_SlicedVertices[y].y, s_SlicedVertices[x2].x, s_SlicedVertices[y2].y);
                    var uvs = new Vector4(s_SlicedUVs[x].x, s_SlicedUVs[y].y, s_SlicedUVs[x2].x, s_SlicedUVs[y2].y);
                    float fillAmount = (m_CustomFillAmount - sliceStart) / (sliceEnd - sliceStart);
                    GenerateFilledSprite(vh, vertices, uvs, fillAmount);
                }
            }
        }

        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            var originalRect = rectTransform.rect;
            for (var axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;
                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
                var combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }

        private void GenerateFilledSprite(VertexHelper vh, Vector4 vertices, Vector4 uvs, float fillAmount)
        {
            if (m_CustomFillAmount < 0.001f)
                return;
            float uvLeft = uvs.x;
            float uvBottom = uvs.y;
            float uvRight = uvs.z;
            float uvTop = uvs.w;
            if (fillAmount < 1f)
            {
                if (m_FillDirection == FillDirection.Left || m_FillDirection == FillDirection.Right)
                {
                    if (m_FillDirection == FillDirection.Left)
                    {
                        vertices.x = vertices.z - (vertices.z - vertices.x) * fillAmount;
                        uvLeft = uvRight - (uvRight - uvLeft) * fillAmount;
                    }
                    else
                    {
                        vertices.z = vertices.x + (vertices.z - vertices.x) * fillAmount;
                        uvRight = uvLeft + (uvRight - uvLeft) * fillAmount;
                    }
                }
                else
                {
                    if (m_FillDirection == FillDirection.Down)
                    {
                        vertices.y = vertices.w - (vertices.w - vertices.y) * fillAmount;
                        uvBottom = uvTop - (uvTop - uvBottom) * fillAmount;
                    }
                    else
                    {
                        vertices.w = vertices.y + (vertices.w - vertices.y) * fillAmount;
                        uvTop = uvBottom + (uvTop - uvBottom) * fillAmount;
                    }
                }
            }
            var s_Vertices = new Vector3[4];
            var s_UVs = new Vector2[4];
            s_Vertices[0] = new Vector3(vertices.x, vertices.y);
            s_Vertices[1] = new Vector3(vertices.x, vertices.w);
            s_Vertices[2] = new Vector3(vertices.z, vertices.w);
            s_Vertices[3] = new Vector3(vertices.z, vertices.y);
            s_UVs[0] = new Vector2(uvLeft, uvBottom);
            s_UVs[1] = new Vector2(uvLeft, uvTop);
            s_UVs[2] = new Vector2(uvRight, uvTop);
            s_UVs[3] = new Vector2(uvRight, uvBottom);
            var startIndex = vh.currentVertCount;
            for (var i = 0; i < 4; i++)
                vh.AddVert(s_Vertices[i], color, s_UVs[i]);
            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            type = Type.Sliced;
        }
        public new Type type
        {
            get => base.type;
            private set => base.type = value;
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
        #endif
    }
}