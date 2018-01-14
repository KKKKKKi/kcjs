namespace K_C_S_J.Helpers
{
    using System;
    using System.Windows;

    public static class LanguageHelper
    {
        public static void LoadLanguageResource(string resourceName)
        {
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
            {
                Source = new Uri(resourceName, UriKind.RelativeOrAbsolute)
            };
        }
    }
}
