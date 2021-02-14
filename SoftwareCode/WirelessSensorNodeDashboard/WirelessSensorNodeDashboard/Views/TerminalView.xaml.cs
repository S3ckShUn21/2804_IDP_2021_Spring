using System.Windows.Controls;
using WirelessSensorNodeDashboard.ViewModels;

namespace WirelessSensorNodeDashboard.Views
{
    public partial class TerminalView : UserControl
    {
        public TerminalView()
        {
            InitializeComponent();
            DataContext = new TerminalViewModel();
        }
    }
}
