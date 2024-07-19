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
        }
    }
}
