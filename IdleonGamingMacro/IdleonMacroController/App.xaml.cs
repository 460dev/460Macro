using IdleonMacroController.ViewModels;
using IdleonMacroController.Views;
using IdleonMacroController.Views.Gaming;
using IdleonMacroController.Views.Home;
using IdleonMacroController.Views.Menu;
using Prism.Ioc;
using Prism.Mvvm;
using System.Windows;

namespace IdleonMacroController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Menu>("Menu");
            containerRegistry.RegisterForNavigation<Home>("Home");
            containerRegistry.RegisterForNavigation<Gaming>("Gaming");
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            // Menu View に MenuViewModel を紐付ける
            ViewModelLocationProvider.Register<Menu, MenuViewModel>();
        }
    }
}
