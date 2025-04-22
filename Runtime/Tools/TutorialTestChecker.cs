namespace Minimoo.Tools
{
    public static class TutorialTestMode
    {
        private const string EDITOR_PREFS_KEY = "editor_tutorial_test_mode";

        public static bool IsEnabled
        {
            get
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorPrefs.HasKey(EDITOR_PREFS_KEY))
                {
                    UnityEditor.EditorPrefs.SetBool(EDITOR_PREFS_KEY, false);
                }
                return UnityEditor.EditorPrefs.GetBool(EDITOR_PREFS_KEY, false);
#else
                return false;
#endif
            }
            set
            {
#if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetBool(EDITOR_PREFS_KEY, value);
#endif
            }
        }
    }
}
