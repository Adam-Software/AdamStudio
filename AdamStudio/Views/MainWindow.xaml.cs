using MahApps.Metro.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AdamStudio.Views
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb  t = sender as Thumb;
            t.Cursor = Cursors.Hand;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            PopupWindow.Height = PopupWindow.Height - e.VerticalChange;   
        }

        private void Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Thumb t = sender as Thumb;
            t.Cursor = null;
        }
    }
}
