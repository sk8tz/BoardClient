namespace Board.Models.Users
{
    public class UserToken
    {
        public string Access_Token { get; set; }
        public string Expires_In { get; set; }
        public string Refresh_Token { get; set; }
    }
}