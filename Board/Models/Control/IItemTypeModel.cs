namespace Board.Models.Control
{
    public interface IItemTypeModel
    {
        int Id { get; set; }
        string EnName { get; set; }
        string CnName { get; set; }
        int SystemTypeId { get; set; }
        string CnDescription { get; set; }
        string EnDescription { get; set; }
        bool IsChecked { get; set; }
    }
}