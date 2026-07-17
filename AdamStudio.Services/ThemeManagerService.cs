using AdamStudio.Services.Interfaces;
using ControlzEx.Theming;
using System.Collections.ObjectModel;
using System.Windows;

namespace AdamStudio.Services
{
    public class ThemeManagerService : IThemeManagerService
    {
        private readonly Application mCurrentApplication;
        private readonly ThemeManager mCurrentThemeManager;
        

        public ThemeManagerService() 
        {
            mCurrentApplication = Application.Current;
            mCurrentThemeManager = ThemeManager.Current;
            AppThemesCollection = mCurrentThemeManager.Themes;
        }

        public ReadOnlyObservableCollection<Theme> AppThemesCollection {  get; private set; }

        public Theme ChangeAppTheme(string themeName) 
        {
            var isThemeExist = mCurrentThemeManager.GetTheme(themeName) != null;

            if (isThemeExist)
                return mCurrentThemeManager.ChangeTheme(mCurrentApplication, themeName, false);

            return null;
        }

        public Theme ChangeAppTheme(Theme theme)
        {
            return mCurrentThemeManager.ChangeTheme(mCurrentApplication, theme.Name, false);
        }

        public Theme GetCurrentAppTheme()
        {
            return mCurrentThemeManager.DetectTheme();
        }

        

        public void Dispose()
        {
            
        }

    }
}
