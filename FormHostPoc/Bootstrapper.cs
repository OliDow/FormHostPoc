using System.Windows;
using FormHostPoc.ViewModels;
using FormHostPoc.Views;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

namespace FormHostPoc
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            var shell = new ShellView() { DataContext = Container.Resolve<ShellViewModel>() };
            shell.Show();

            return shell;
        }
    }
}