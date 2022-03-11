using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace A_Common_Library.Data
{
    public static class DataUtility
    {
        //check if dataset/table is not empty
        public static bool NotEmpty(this DataRow data)
        {
            if (data is null) return false;

            return (data[0] != null);
        }

        public static bool NotEmpty(this DataTable data)
        {
            if (data is null
                || data.Rows.Count <= 0) return false;

            return (data.Rows[0] != null);
        }

        public static bool NotEmpty(this DataSet data)
        {
            if (data is null
                || data.Tables.Count <= 0) return false;

            return data.Tables[0].NotEmpty();
        }

        //DataTable builder
        public static DataTable NewIDColumn(this DataTable dt, string name)
        {
            return dt.NewColumn<int>(name, true);
        }

        public static DataTable NewColumn<T>(this DataTable dt, string name, bool auto_increment = false)
        {
            DataColumn dt_column = new DataColumn();
            dt_column.ColumnName = name;
            dt_column.DataType = typeof(T);
            dt_column.AutoIncrement = auto_increment;

            dt.Columns.Add(dt_column);

            return dt;
        }

        // get a DataRow from a DataTable that has a cell with certain value
        public static DataRow GetRow<T>(this DataTable data, string field_name, T value)
        {
            return data.AsEnumerable()
                .Where(r => r.Field<T>(field_name).Equals(value))
                .DefaultIfEmpty(null)
                .FirstOrDefault();
        }

        //get value from DataRowCell and convert it to appropiate type
        public static T CellValue<T>(this DataRow row, string field_name, T default_value)
        {
            try
            {
                return (row == null
                    || row[field_name] == DBNull.Value
                    || (T)Convert.ChangeType(row[field_name], typeof(T)) == null) ? default_value : (T)Convert.ChangeType(row[field_name], typeof(T));
            }
            catch
            {
                try
                {
                    return default_value;
                }
                catch
                {
                    return default(T);
                }
            }
        }

        public static T Value<T>(this DataRow row, string field_name)
        {
            return row.Value<T>(row.Table.Columns[field_name].Ordinal);
        }

        public static T Value<T>(this DataRow row, int field_index)
        {
            if (row is null) return default(T);

            try
            {
                string bool_true_conditions = "yes y ok true";

                if (string.IsNullOrEmpty(row[field_index].ToString()))
                {
                    if (typeof(T).Name.Equals(typeof(string).Name))
                    {
                        return (T)Convert.ChangeType(string.Empty, typeof(T));
                    }
                    else if (typeof(T).Name.Equals(typeof(DateTime).Name))
                    {
                        return (T)Convert.ChangeType(DateTime.MinValue, typeof(T));
                    }
                    else if (typeof(T).Name.Equals(typeof(bool).Name))
                    {
                        if (bool_true_conditions.Contains(row[field_index].ToString().ToLower())) return (T)Convert.ChangeType(true, typeof(T));
                        else return (T)Convert.ChangeType(false, typeof(T));
                    }
                    else return default(T);
                }

                return (T)Convert.ChangeType(row[field_index], typeof(T));
            }
            catch
            {
                Console.WriteLine($"VALUE<{typeof(T).Name}>({field_index}) {row[field_index]}");

                return default(T);
            }
        }

        //DataTable mapper to objects
        public static List<T> BindToList<T>(this DataTable data)
        {
            PropertyInfo property;
            Type type;

            MethodInfo method;
            object value;

            List<PropertyInfo> properties = typeof(T).GetProperties().AsEnumerable()
                .Where(prop => data.Columns.Contains(prop.Name) && prop.CanWrite)
                .ToList();

            List<T> result = new List<T>();

            string bool_true_conditions = "yes y ok true";

            foreach (DataRow row in data.Rows)
            {
                // Create the object of T
                var item = Activator.CreateInstance<T>();

                foreach (PropertyInfo prop in properties)
                {
                    try
                    {
                        type = prop.PropertyType;

                        if (type.Name.Equals(typeof(bool).Name))
                        {
                            if (!string.IsNullOrEmpty(row[prop.Name].ToString())
                                && bool_true_conditions.Contains(row[prop.Name].ToString().ToLower()))
                            {
                                prop.SetValue(item, true, null);
                            }
                            else
                            {
                                prop.SetValue(item, false, null);
                            }

                            continue;
                        }

                        method = typeof(DataUtility).GetMethod("GenericConvertTo")
                                .MakeGenericMethod(new Type[] { type });

                        value = method.Invoke(typeof(DataUtility), new object[] { row[prop.Name], null });

                        prop.SetValue(item, value, null);
                    }
                    catch
                    {
                        continue;
                    }
                }

                result.Add(item);
            }

            return result;
        }

        public static T ToObject<T>(this DataRow dataRow)
            where T : new()
        {
            T item = new T();
            PropertyInfo property;
            Type type;

            MethodInfo method;
            object value;

            List<PropertyInfo> properties = typeof(T).GetProperties().AsEnumerable()
                .Where(prop => dataRow.Table.Columns.Contains(prop.Name) && prop.CanWrite)
                .ToList();

            string bool_true_conditions = "yes y ok true";

            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    type = prop.PropertyType;

                    if (type.Name.Equals(typeof(bool).Name))
                    {
                        if (!string.IsNullOrEmpty(dataRow[prop.Name].ToString())
                            && bool_true_conditions.Contains(dataRow[prop.Name].ToString().ToLower()))
                        {
                            prop.SetValue(item, true, null);
                        }
                        else
                        {
                            prop.SetValue(item, false, null);
                        }

                        continue;
                    }

                    method = typeof(DataUtility).GetMethod("GenericConvertTo")
                            .MakeGenericMethod(new Type[] { type });

                    value = method.Invoke(typeof(DataUtility), new object[] { dataRow[prop.Name], null });

                    prop.SetValue(item, value, null);

                    //Console.WriteLine($"ToObject PARSED AS {prop.Name} {value} {dataRow[prop.Name]}");
                }
                catch
                {
                    //Console.WriteLine($"ToObject FAILED AT {prop.Name} {dataRow[prop.Name]}");

                    continue;
                }
            }

            return item;
        }

        public static T DeepClone<T>(this T obj)
        {
            if (!typeof(T).IsSerializable) return default(T);

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);
            }
        }

        public static T ToConstructorlessObject<T>(this DataRow dataRow)
        {
            object instance = Activator.CreateInstance(typeof(T));

            try
            {
                PropertyInfo property;
                Type type;

                foreach (DataColumn column in dataRow.Table.Columns)
                {
                    property = typeof(T).GetProperty(column.ColumnName);

                    if (property is null) continue;

                    type = property.PropertyType;

                    if (dataRow[column] != DBNull.Value 
                        && dataRow[column].ToString() != "NULL")
                    {
                        var value = ConvertTo(dataRow, column, type);

                        property.SetValue(instance, value, null);
                    }
                }

                return (T)instance;
            }
            catch (Exception ex)
            {
                instance = null;
                throw ex;
            }
        }

        public static PropertyInfo GetPropertyAttribute(Type type, string attributeName)
        {
            PropertyInfo property = type.GetProperty(attributeName);

            if (property != null)
            {
                return property;
            }

            return type.GetProperties()
                 .Where(p => p.IsDefined(typeof(DisplayAttribute), false) && p.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().Single().Name == attributeName)
                 .FirstOrDefault();
        }

        public static T GenericConvertTo<T>(this object value, object default_value = null)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                try
                {
                    if (default_value != null)
                    {
                        return (T)default_value;
                    }

                    return default(T);
                }
                catch
                {
                    return default(T);
                }
            }
        }

        public static object ConvertTo(this DataRow dr, DataColumn column, Type type)
        {
            MethodInfo method = typeof(DataUtility).GetMethod("GenericConvertTo")
                             .MakeGenericMethod(new Type[] { type });
            return method.Invoke(typeof(DataUtility), new object[] { dr[column], null });
        }

        public static object ChangeType(object value, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                return Convert.ChangeType(value, Nullable.GetUnderlyingType(type));
            }

            return Convert.ChangeType(value, type);
        }

        public static void Check_Remove<T>(this List<T> Source, T Item)
        {
            if (Source.Contains(Item))
            {
                Source.Remove(Item);
            }
        }

        public static void Add_Unique<T>(this List<T> Source, T Item)
        {
            if (!Source.Contains(Item))
            {
                Source.Add(Item);
            }
        }
    }
}