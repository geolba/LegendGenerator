using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LegendGenerator.App.Utils
{
    public abstract class NotifyBase : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly Dictionary<string, object> propertyValues;

        protected NotifyBase()
        {
            propertyValues = new Dictionary<string, object>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(Expression<Func<T>> expression, T value)
        {
            string propertyName = GetPropertyNameFromExpression(expression);
            Set(propertyName, value);
        }

        public static string GetPropertyNameFromExpression<T>(Expression<Func<T>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body;
            return memberExpression.Member.Name;
        }

        protected void Set<T>(string name,
            T value)
        {
            if (propertyValues.ContainsKey(name))
            {
                propertyValues[name] = value;
                OnPropertyChanged(name);
            }
            else
            {
                propertyValues.Add(name, value);
                OnPropertyChanged(name);
            }
        }

        protected T Get<T>(string name)
        {
            if (propertyValues.ContainsKey(name))
            {
                return (T)propertyValues[name];
            }
            return default(T);
        }

        protected T Get<T>(Expression<Func<T>> expression)
        {
            string propertyName = GetPropertyNameFromExpression(expression);
            return Get<T>(propertyName);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            string propertyName = GetPropertyNameFromExpression(expression);
            OnPropertyChanged(propertyName);
        }

        public virtual bool IsValid()
        {
            return GetValidationErrors() == string.Empty;
        }

        # region IDataErrorInfo

        public string Error
        {
            get
            {
                return this.GetValidationErrors();
            }
        }

        public string this[string propertyName]
        {
            get
            {
                return GetValidationErrors(propertyName);
            }
        }

        #endregion

        #region private methods 

        protected virtual string GetValidationErrors(string columnName)
        {
            var vc = new ValidationContext(this, null, null);
            var vResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this, vc, vResults, true))
            {
                string error = "";
                foreach (var ve in vResults)
                {
                    if (ve.MemberNames.Contains(columnName, StringComparer.CurrentCultureIgnoreCase))
                    {
                        error += ve.ErrorMessage + Environment.NewLine;
                    }

                }
                return error;
            }
            return "";
        }

        protected virtual string GetValidationErrors()
        {
            var vc = new ValidationContext(this, null, null);
            var vResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(this, vc, vResults, true))
            {
                return vResults.Aggregate("", (current, ve) => current + (ve.ErrorMessage + Environment.NewLine));
            }

            return "";
        }

        #endregion

    }
}