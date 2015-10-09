using System;

using Board.Enums;
using Board.Models.Control;
using Board.Models.System;

namespace Board.Common
{
    public interface IBaseSystemViewModel
    {
        int ClientId { get; set; }
        int DataCabinId { get; set; }
        SystemTypeEnum SystemType { get; set; }
        DisplayTypeModel DisplayType { get; set; }
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }


        void LoadBaseData();
        void SetDisplayTypeEnabled(string displayTypeString);
        void MainTabChanged();
        void SetDisplayTypeBySelectedConditions();
        string GetHeader();
        void GetParameters();
        bool VerifyParameters();
        void OkClick();
        void CancleClick();
    }
}