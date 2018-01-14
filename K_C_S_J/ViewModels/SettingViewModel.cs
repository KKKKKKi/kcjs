namespace K_C_S_J.ViewModels
{
    using Caliburn.Micro;

    public class SettingViewModel : Screen
    {
        public string LanguageCode
        {
            get;
            set;
        }

        public void SwitchLanguage(string key)
        {
            LanguageCode = key;
        }

        public void ApplyLanguageCode(string key)
        {
            switch (key)
            {
                default:
                    break;
            }
        }
    }
}
