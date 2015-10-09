namespace Board.Services.TargetAudience
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Data;
    using global::System.Reflection;

    public class DataTableConverter
    {
        private static readonly DataTableConverter Instance = new DataTableConverter();

        public static DataTableConverter Singleton
        {
            get
            {
                return Instance;
            }
        }

        public List<T> GetList<T>(DataTable source) where T : class, new()
        {
            if (source == null || source.Rows.Count == 0)
            {
                return null;
            }

            var list = new List<T>();
            foreach (DataRow dr in source.Rows)
            {
                var item = this.GetItem<T>(dr);
                list.Add(item);
            }

            return list;
        }

        private T GetItem<T>(DataRow source) where T : class,new()
        {
            if (source == null)
            {
                return null;
            }

            var item = new T();
            var targetType = typeof(T);

            foreach (PropertyInfo propertyInfo in targetType.GetProperties())
            {
                if (!propertyInfo.CanWrite)
                {
                    continue;
                }

                if (!source.Table.Columns.Contains(propertyInfo.Name))
                {
                    continue;
                }

                var type = Type.GetType(propertyInfo.PropertyType.FullName);

                if (type == null)
                {
                    continue;
                }

                var value = Convert.ChangeType(source[propertyInfo.Name], type);
                propertyInfo.SetValue(item, value, null);
            }

            return item;
        }
    }
}
