namespace Board.Models.System
{
    public class ProgramModel
    {
        public int Id { get; set; }
        public string ProgramName { get; set; }
        public string ProgramEnName { get; set; }
        public string ProgramType { get; set; }
        public string ProgramEnType { get; set; }

        public bool IsSelected { get; set; }
    }
}