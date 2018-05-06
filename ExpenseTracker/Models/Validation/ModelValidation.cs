using ExpenseTracker.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Models
{
    // For reference: https://stackoverflow.com/questions/2281972/how-to-get-a-list-of-properties-with-a-given-attribute
    // And: https://stackoverflow.com/questions/2051065/check-if-property-has-attribute
    public static class ModelValidation<T> where T : class
    {
        #region Public Helpers

        /// <summary>
        /// Validates the model's properties against attributes defined on the model.
        /// Supported validation attributes: Required, StringLength, RegularExpression
        /// </summary>
        /// <param name="model">The model to validate</param>
        public static void ValidateModel(T model) {
            foreach (PropertyInfo property in typeof(T).GetProperties()) {
                ValidateProperyAttributes(property, model);
            }
        }

        #endregion // Public Helpers

        #region Private Helpers

        private static void ValidateProperyAttributes(PropertyInfo property, T model) {
            foreach (Attribute attribute in property.GetCustomAttributes()) {
                switch (attribute.GetType().Name) {
                    case nameof(RequiredAttribute):
                        ValidateRequiredAttribute((RequiredAttribute)attribute, property, model);
                        break;
                    case nameof(StringLengthAttribute):
                        ValidateStringLengthAttribute((StringLengthAttribute)attribute, property, model);
                        break;
                    case nameof(RegularExpressionAttribute):
                        ValidateRegularExpressionAttribute((RegularExpressionAttribute)attribute, property, model);
                        break;
                    default:
                        break;
                }
            }
        } 

        private static void ValidateRequiredAttribute(RequiredAttribute attribute, PropertyInfo property, T model) {
            var value = property.GetValue(model, null);
            if (value != null) {
                if (!(property.PropertyType == typeof(string) && value.ToString() == string.Empty && !attribute.AllowEmptyStrings)) {
                    return;
                }
            }
            throw new ModelValidationException(property.Name, "", attribute.ErrorMessage);
        }

        private static void ValidateStringLengthAttribute(StringLengthAttribute attribute, PropertyInfo property, T model) {
            string value = property.GetValue(model, null).ToString();
            if (value.Length <= attribute.MaximumLength && value.Length >= attribute.MinimumLength) {
                return;
            }
            throw new ModelValidationException(property.Name, value, attribute.ErrorMessage);
        }

        private static void ValidateRegularExpressionAttribute(RegularExpressionAttribute attribute, PropertyInfo property, T model) {
            string value = property.GetValue(model, null).ToString();
            if (Regex.IsMatch(value, attribute.Pattern)) {
                return;
            }
            throw new ModelValidationException(property.Name, value, attribute.ErrorMessage);
        }

        #endregion // Private Helpers
    }
}