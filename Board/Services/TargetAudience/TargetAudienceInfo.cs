using System.Collections.Generic;

using Convert = System.Convert;

namespace Board.Services.TargetAudience
{
    public class TargetAudienceInfo
    {        
        public string Age { get; set; }

        public string Gender { get; set; }
        public string Zone { get; set; }
        public string Education { get; set; }
        public string Marriage { get; set; }
        public string Income { get; set; }

//        public string MinAge { get; set; }
//        public int MaxAge { get; set; }
//        public List<int> Gender { get; set; }
//        public List<int> Zone { get; set; }
//        public List<int> Education { get; set; }
//        public List<int> Marriage { get; set; }
//        public List<int> Income { get; set; }
//
//        public List<int> ToList(string selectedItems)
//        {
//            var list = new List<int>();
//            var items = selectedItems.Split(',');
//            foreach (var item in items)
//            {
//                list.Add(Convert.ToInt32(item));
//            }
//
//            return list;
//        } 
    }
}
