using ControlzEx.Theming;
using System;
using System.Collections.ObjectModel;

namespace AdamStudio.Services.Interfaces
{
    public interface IThemeManagerService : IDisposable
    {
        public ReadOnlyObservableCollection<Theme> AppThemesCollection { get; }

        public Theme GetCurrentAppTheme();
        public Theme ChangeAppTheme(string themeName);
        public Theme ChangeAppTheme(Theme themeName);

    }
}
