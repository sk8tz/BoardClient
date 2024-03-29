using System;
using System.Collections.Generic;
using System.Windows;

using Board.ViewModels;

using Caliburn.Micro;

namespace Board
{
    public class AppBootstrapper : BootstrapperBase
    {
        private SimpleContainer _container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.PerRequest<IShell, ShellViewModel>();
            _container.PerRequest<MainViewModel>();
            _container.PerRequest<WidgetViewModel>();
            _container.PerRequest<SettingViewModel>();
            _container.PerRequest<Widget2ViewModel>();
            _container.PerRequest<SiteRealtimeViewModel>();
            _container.PerRequest<SponsorViewModel>();
            _container.PerRequest<TrackAnalysisReportViewModel>();
            _container.PerRequest<TrackRealtimeViewModel>();
            _container.PerRequest<BenchmarkViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = _container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }
    }
}