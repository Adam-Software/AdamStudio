using AdamStudio.Services.Interfaces;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;

namespace AdamStudio.Services
{
    public class AvalonEditService : IAvalonEditService
    {
        #region Var

        private readonly IFileManagmentService mFileManagmentService;
        private readonly HighlightingManager mHighlightingManager;

        #endregion

        #region ~

        public AvalonEditService(IServiceProvider serviceProvider)
        {    
            mFileManagmentService = serviceProvider.GetService<IFileManagmentService>();
            mHighlightingManager = HighlightingManager.Instance;
        }

        #endregion

        #region Public field

        public ReadOnlyCollection<IHighlightingDefinition> HighlightingDefinitions
        {
            get => mHighlightingManager.HighlightingDefinitions;
        }

        #endregion

        #region Public methods

        public void RegisterHighlighting(string highlightingName, byte[] xmlByteArray)
        {
            var xml = mFileManagmentService.ReadTextAsXml(xmlByteArray);
            var definition = HighlightingLoader.Load(xml, mHighlightingManager);
            mHighlightingManager.RegisterHighlighting(highlightingName, Array.Empty<string>(), definition);
            
        }

        public IHighlightingDefinition GetDefinition(string name)
        {
            IHighlightingDefinition definition = mHighlightingManager.GetDefinition(name);
            return definition;
        }

        public void Dispose()
        {
            
        }

        #endregion

    }
}
