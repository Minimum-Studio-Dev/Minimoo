using UnityEditor;
using Minimoo.Tools;

namespace Minimoo.Tools.Editor
{
    /// </remarks>
    [InitializeOnLoad]
    public class TutorialTestModeEditor
    {
        private const string ENABLE_TUTORIAL_TEST_MENU = "MINIMOO/Tutorial/Enable Tutorial Test";
        private const string DISABLE_TUTORIAL_TEST_MENU = "MINIMOO/Tutorial/Disable Tutorial Test";

        [MenuItem(ENABLE_TUTORIAL_TEST_MENU, true)]
        private static bool ShowEnableTutorialTestMenu()
        {
            return !TutorialTestMode.IsEnabled;
        }

        [MenuItem(ENABLE_TUTORIAL_TEST_MENU)]
        private static void EnableTutorialTest()
        {
            TutorialTestMode.IsEnabled = true;
            EditorUtility.DisplayDialog("튜토리얼 테스트 모드",
                "튜토리얼 테스트 모드가 활성화되었습니다.\n게임을 시작하면 튜토리얼 테스트 모드로 실행됩니다.",
                "확인");
        }

        [MenuItem(DISABLE_TUTORIAL_TEST_MENU, true)]
        private static bool ShowDisableTutorialTestMenu()
        {
            return TutorialTestMode.IsEnabled;
        }

        [MenuItem(DISABLE_TUTORIAL_TEST_MENU)]
        private static void DisableTutorialTest()
        {
            TutorialTestMode.IsEnabled = false;
            EditorUtility.DisplayDialog("튜토리얼 테스트 모드",
                "튜토리얼 테스트 모드가 비활성화되었습니다.\n게임을 시작하면 일반 모드로 실행됩니다.",
                "확인");
        }
    }
}
