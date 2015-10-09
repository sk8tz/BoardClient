namespace Board.Models.Control
{
    public interface IWidgetInfo
    {
        int Id { get; set; }
        string ItemTitle { get; set; }
        int GroupIndex { get; set; }
        double ItemWidth { get; set; }
        double ItemHeight { get; set; }
        bool IsLoading { get; set; }
    }
}