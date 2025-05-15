using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GamedayTracker.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace GamedayTracker.Views.Pages
{
    /// <summary>
    /// Interaction logic for AddPlayerPage.xaml
    /// </summary>
    public partial class AddPlayerPage : INavigableView<AddPlayerPageViewModel>
    {
        public AddPlayerPageViewModel ViewModel { get; }
        public AddPlayerPage(AddPlayerPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
