using IdleonMacroController.Views.Home;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdleonMacroController.ViewModels
{
    public class DashBoardViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        IEventAggregator _eventAggregator;

        public DelegateCommand<string> NavigateCommand { get; private set; }

        public DashBoardViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            // 初期画面の設定
            _regionManager.RegisterViewWithRegion("MenuRegion", typeof(Home));

            // 画面遷移
            NavigateCommand = new DelegateCommand<string>(RecieveNavigatePath);
        }

        // 画面遷移用メソッド
        private void RecieveNavigatePath(string navigatePath)
        {
            if (navigatePath != null)
            {
                // 指定したパスに画面遷移
                _regionManager.RequestNavigate("MenuRegion", navigatePath);
            }
        }
    }
}
