using System.Data;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using Prism.Ioc;
using Prism;
using DoMoreFindReplace.ViewModels;
using DoMoreFindReplace.Views;
using System.Reflection;


namespace DoMoreFindReplace
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
           

            var ListofViews = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(a => a.IsClass && a.Namespace != null && a.Namespace.Contains(@"DoMoreFindReplace.Views"))
                    .Where(t => !t.Name.StartsWith("MainWindow")).ToList();
            foreach (var View in ListofViews)
            {
                containerRegistry.RegisterForNavigation(View, View.Name);
            }


        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();
        }
    }
}
