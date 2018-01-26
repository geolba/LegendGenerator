using System;
using System.Reflection;
using System.Linq;

namespace LegendGenerator.App.Utils
{
    public static class StringExtensions
    {
        /// <summary>
        /// Truncates a string from a starting index to a maximum length.
        /// </summary>
        /// <param name="value">The string to truncate.</param>
        /// <param name="startIndex">The start index for truncating.</param>
        /// <param name="length">The maximum length of the returned string.</param>
        /// <returns>The input string, truncated to <paramref name="length"/> characters.</returns>
        public static string PartString(this string value, int startIndex, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
                //return String.Empty;
            }
            return value.Length <= length ? value : value.Substring(startIndex, length);
        }
    }

    public static class ObjectExtensions
    {
        public static void DeepCopy<T>(this T lObjSource, T lObjWithNewValues)
        {
            //T lObjCopy = (T)Activator.CreateInstance(typeof(T));
            foreach (PropertyInfo lObjSourceProperty in lObjSource.GetType().GetProperties())
            {
                var info = lObjWithNewValues.GetType().GetProperties().Where(x => x.Name == lObjSourceProperty.Name).FirstOrDefault();
                if (info == null)
                    return;
                if (!info.CanWrite)
                    return;
                var newValue = info.GetValue(lObjWithNewValues);
                lObjSourceProperty.SetValue
                (
                    lObjSource,
                     newValue
                );
            }
            //return lObjCopy;
        }
    }


}
