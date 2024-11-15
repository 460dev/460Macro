using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdleonMacroController.ViewModels
{
    public class GamingViewModel : BindableBase
    {
        public DelegateCommand StartStopCommand { get; private set; }

        public ReactiveProperty<string> StartStopText { get; set; } = new ReactiveProperty<string>();


        public GamingViewModel()
        {
            StartStopCommand = new DelegateCommand(StartStop);
        }

        private void StartStop()
        {

        }
    }
}
