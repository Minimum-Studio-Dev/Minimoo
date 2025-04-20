namespace Minimoo.Utilities
{
    public static class VersionUtility
    {
        public static int StringToNumer(string version)
        {
            var versionArray = version.Split('.');

            if (versionArray.Length == 3)
            {
                var versionNumber = int.Parse(versionArray[0] + int.Parse(versionArray[1]).ToString("D3") + int.Parse(versionArray[2]).ToString("D3"));
                return versionNumber;
            }
            else
                return 0;
        }
    }
}
