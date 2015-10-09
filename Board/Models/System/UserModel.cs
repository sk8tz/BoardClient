using System;

namespace Board.Models.System
{
    public class UserModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int AuthorizationId { get; set; }
        public DateTime CreateDate { get; set; }
        public int CanExport { get; set; }
        public int CanLookAll { get; set; }
    }
}