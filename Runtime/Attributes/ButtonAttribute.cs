using UnityEngine;
using System;

namespace Minimoo.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string ButtonName { get; private set; }
        public float ButtonHeight { get; private set; }

        public ButtonAttribute(string buttonName = "", float buttonHeight = 20f)
        {
            ButtonName = buttonName;
            ButtonHeight = buttonHeight;
        }
    }
} 