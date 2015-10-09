using System;

namespace Board.Models.Users
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int AuthorizationId { get; set; }
        public string AuthorizationName { get; set; }
        public DateTime CreteDate { get; set; }
    }
}