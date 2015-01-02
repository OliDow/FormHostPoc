using FormHostPoc.ViewModels;
using Microsoft.Practices.Unity;

namespace FormHostPoc.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();
        }

        public ShellView(ShellViewModel vm)
        {
            DataContext = vm;
        }
        
    }
}
