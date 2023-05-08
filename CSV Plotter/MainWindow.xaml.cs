using CSV_Plotter.Models;
using CSV_Plotter.ViewModels;

using System.Windows;
using System.Windows.Controls;

namespace CSV_Plotter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}