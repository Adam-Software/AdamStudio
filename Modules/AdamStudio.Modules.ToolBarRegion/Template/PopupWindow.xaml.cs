using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AdamStudio.Modules.ToolBarRegion.Template
{
    public partial class PopupWindow : UserControl
    {
        public PopupWindow()
        {
            InitializeComponent();
        }

        #region DependencyProperty

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        private static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(PopupWindow), null);

        public ControlTemplate PopupWindowTemplate 
        {
            get { return (ControlTemplate)GetValue(PopupWindowTemplateProperty); }
            set { SetValue(PopupWindowTemplateProperty, value); }
        }

        private static readonly DependencyProperty PopupWindowTemplateProperty = DependencyProperty.Register(nameof(PopupWindowTemplate), typeof(ControlTemplate), typeof(PopupWindow), null);

        #endregion

        private void ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newSize = PopupExWindow.Height - e.VerticalChange;
            
            if (newSize > 0)
            {
                if (newSize > PopupExWindow.MaxHeight)
                    return;

                PopupExWindow.Height = newSize;
            } 
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }
    }
}
