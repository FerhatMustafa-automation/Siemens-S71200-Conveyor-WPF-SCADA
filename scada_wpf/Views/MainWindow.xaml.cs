using System.Windows;
using DigitalTwinScada.ViewModels;

namespace DigitalTwinScada.Views
{
    /// <summary>
    /// MainWindow.xaml için arkadaki temiz etkileşim kodu.
    /// MVVM standardı gereği iş mantığı içermez, sadece DataContext bağlar.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            if (DataContext is MainViewModel vm)
            {
                vm.DisconnectCommand.Execute(null);
            }
        }
    }
}
