using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using LegendGenerator.App.Model;

namespace LegendGenerator.App.Utils
{
    //[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DoesExistFileNameAttribute : ValidationAttribute
    {
        //public string FileFoundMessage = "Sorry but there is already an image with this name please rename your image";
        
        public string DependentBooleanPropertyName { get; set; }

        public DoesExistFileNameAttribute(string propertyName)
            : base("error")
        {
            DependentBooleanPropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var property = validationContext.ObjectType.GetProperty(DependentBooleanPropertyName);
                if (property == null)
                {
                    return new ValidationResult(
                        string.Format("Unknown property: {0}", DependentBooleanPropertyName)
                    );
                }

                string[] memberNames = new string[] { validationContext.MemberName };
                bool dependentBooleanValue = (bool)property.GetValue(validationContext.ObjectInstance, null);
                //bool checkAccess = ((FormularData)validationContext.ObjectInstance).ChkAccess;

                string fileName = value.ToString();
                if (File.Exists(fileName))
                {
                    return ValidationResult.Success;
                }
                else if (!File.Exists(fileName) && dependentBooleanValue == false )
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessage, memberNames);
                }
            }
            return ValidationResult.Success;
        }
    }

    public class RequiredAttributeDependentOnBoolFlag : ValidationAttribute
    {
      
        public string DependentBooleanFlagProperty { get; set; }

       
        /// Gets or sets a flag indicating whether the attribute should allow empty strings.   
        public bool AllowEmptyStrings { get; set; }

        public RequiredAttributeDependentOnBoolFlag()
            : base("error")
        {
            //DependentBooleanFlagProperty = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string[] memberNames = new string[] { validationContext.MemberName };
            var property = validationContext.ObjectType.GetProperty(DependentBooleanFlagProperty);
            if (property == null)
            {
                return new ValidationResult(
                    string.Format("Unknown property: {0}", DependentBooleanFlagProperty)
                );
            }
            bool dependentBooleanValue = (bool)property.GetValue(validationContext.ObjectInstance, null);

            //not checked, attribute is not required
            if (dependentBooleanValue == false)
            {
                return ValidationResult.Success;
            }
            //no given value, throw the error message
            if (value == null)
            {
                return new ValidationResult(ErrorMessage, memberNames);
            }
           

            // only check string length if empty strings are not allowed
            var stringValue = value as string;
            if (stringValue != null && AllowEmptyStrings == false)
            {
                bool isEmptyString = stringValue.Trim().Length == 0;
                if (isEmptyString == true)
                {
                    return new ValidationResult(ErrorMessage, memberNames);
                }
                
            }

            return ValidationResult.Success;



         
           
        }
    }

}
