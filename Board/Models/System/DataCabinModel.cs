using System;
using PropertyChanged;

namespace Board.Models.System
{
    [ImplementPropertyChanged]
    public class DataCabinModel
    {
        public int Id { get; set; }
        public string DataCabinName { get; set; }
        public int DataCabinTypeId { get; set; }
        public int ClientId { get; set; }
        public int? UserId { get; set; }
        public int DateTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }


        public bool CanChange { get; set; }
        public bool CanDelete { get; set; }
    }
}