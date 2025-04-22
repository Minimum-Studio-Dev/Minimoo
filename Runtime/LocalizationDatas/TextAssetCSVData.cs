using UnityEngine;
using System.IO;
using Minimoo.Attributes;
using Minimoo.Extensions;

namespace Minimoo.LocalizationDatas
{
    [CreateAssetMenu(fileName = "TextAsset CSV Data", menuName = "Minimoo/Localization/TextAsset CSV Data")]
    public class TextAssetCSVData : CSVData
    {
        [SerializeField] private TextAsset _textAsset;

        private void OnEnable()
        {
            if (_textAsset != null)
            {
                ParseCSV(_textAsset.text);
            }
        }

        [Button("Parse TextAsset")]
        public void ParseTextAsset()
        {
            if (_textAsset == null)
            {
                D.Error("TextAsset이 지정되지 않았습니다.");
                return;
            }

            ParseCSV(_textAsset.text);
            D.Log($"CSV 데이터 파싱이 완료되었습니다. (총 {_rows.Count}행)");
        }

        public void SetTextAsset(TextAsset textAsset)
        {
            _textAsset = textAsset;
            ParseCSV(textAsset.text);
        }
    }
} 