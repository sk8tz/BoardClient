using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

using Board.Common;
using Board.Enums;
using Board.Models.System;
using Board.SystemData.System;

using Caliburn.Micro;

using PropertyChanged;

namespace Board.ViewModels
{
    public class Widget2ViewModel : Screen
    {
        public int ClientId { get; set; }
        public int DataCabinId { get; set; }
        public ObservableCollection<SystemTypeModel> SystemList { get; set; }
        public IBaseSystemViewModel ViewModel { get; set; }

        public async void Loaded()
        {
            SystemList = await SystemDataService.GetSystemList();
        }

        public void SwitchWidget(object sender, EventArgs e)
        {
            var listBox = sender as ListBox;
            if(listBox != null)
            {
                switch((int)listBox.SelectedValue)
                {
                    case (int)SystemTypeEnum.SiteAnalysisReport:
                        //ViewModel = IoC.Get<SiteAnalysisReportViewModel>();
                        break;
                    case (int)SystemTypeEnum.SiteDataBank:
                        //ViewModel = IoC.Get<SiteDataBankViewModel>();
                        break;
                    case (int)SystemTypeEnum.SiteRealtime:
                        //ViewModel = IoC.Get<SiteRealtimeViewModel>();
                        break;
                    case (int)SystemTypeEnum.Sponsor:
                        //ViewModel = IoC.Get<SponsorViewModel>();
                        break;
                    case (int)SystemTypeEnum.TrackAnalysisReport:
                        //ViewModel = IoC.Get<TrackAnalysisReportViewModel>();
                        break;
                    case (int)SystemTypeEnum.TrackDataBank:
                        //ViewModel = IoC.Get<TrackDataBankViewModel>();
                        break;
                    case (int)SystemTypeEnum.TrackRealtime:
                        //ViewModel = IoC.Get<TrackRealtimeViewModel>();
                        break;
                    case (int)SystemTypeEnum.Benchmark:
                        ViewModel = IoC.Get<BenchmarkViewModel>();
                        ViewModel.ClientId = ClientId;
                        ViewModel.DataCabinId = DataCabinId;
                        break;
                }
            }
        }
    }
}
