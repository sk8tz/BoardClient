using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Board.Controls;
using Board.Models.System;

namespace Board.SystemData.Base
{
    public class BaseSystemData
    {
        public static Dictionary<string, string> BaseDataDictionary;

        public static ObservableCollection<Node> TableSource;

        public static ResultValue SystemData;

        public virtual async Task<Dictionary<string, string>> GetBaseData(Dictionary<string, object> parameterDictionary = null)
        {
            return await new Task<Dictionary<string, string>>(null);
        }

        public virtual async Task<ObservableCollection<Node>> GetTableSource(Dictionary<string, object> parameterDictionary = null)
        {
            return await new Task<ObservableCollection<Node>>(null);
        }

        public virtual async Task<ResultValue> GetSystemData(Dictionary<string, object> parameterDictionary)
        {
            return await new Task<ResultValue>(null);
        }
    }
}