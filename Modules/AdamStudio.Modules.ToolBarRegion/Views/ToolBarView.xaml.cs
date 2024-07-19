using ControlzEx;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AdamStudio.Modules.ToolBarRegion.Views
{
    public partial class ToolBarView : UserControl
    {
        public ToolBarView()
        {
            InitializeComponent();
        }

        private void ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            PopupWindow.Height = PopupWindow.Height - e.VerticalChange;

            //var newSize = new Size();
            //newSize.Height = PopupWindow.Height = e.VerticalChange;
            //newSize.Width = PopupWindow.Width;

            //PopupWindow.RenderSize = newSize;
        }
    }
}
