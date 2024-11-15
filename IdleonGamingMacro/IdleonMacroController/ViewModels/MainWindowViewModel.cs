using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using IdleonMacroController.Views.Menu;

namespace IdleonMacroController.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        IEventAggregator _eventAggregator;

        private string _title = "IdleonMacroController";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            // 初期画面
            _regionManager.RegisterViewWithRegion("MainWindowRegion", typeof(Menu));

            // 画面遷移
            _eventAggregator.GetEvent<PubSubEvent<string>>().Subscribe(RecieveNavigatePath);
        }


        // 画面遷移用メソッド
        private void RecieveNavigatePath(string navigatePath)
        {
            if (navigatePath != null)
            {
                // 指定したパスに画面遷移
                _regionManager.RequestNavigate("MainWindowRegion", navigatePath);
            }
        }
    }
}
