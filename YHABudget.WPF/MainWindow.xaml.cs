using System.Windows;
using YHABudget.Core.ViewModels;

namespace YHABudget.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel mainViewModel)
        {
            InitializeComponent();
            DataContext = mainViewModel;
        }
    }
}