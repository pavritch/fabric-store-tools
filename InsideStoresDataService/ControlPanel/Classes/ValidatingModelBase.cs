using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace ControlPanel
{

        // good article:
        // http://www.silverlight.net/learn/whitepapers/introducing-new-inotifydataerrorinfo-interface/

        // typical usage

        // in the XAML binding
        // ValidatesOnNotifyDataErrors=True

        // private string _lastName = string.Empty;
        //
        //[Required(ErrorMessage = "My error message.")]
        //public string LastName
        //{
        //    get
        //    {
        //        return _lastName;
        //    }

        //    set
        //    {
        //        if (_lastName == value && value != string.Empty)
        //        {
        //            return;
        //        }

        //        // first validate and if there is no error set propery
        //        Validate(value, LastNamePropertyName);

        //        _lastName = value;

        //        // Update bindings, no broadcast
        //        RaisePropertyChanged("LastName");
        //    }
        //}


        //private void _submit()
        //{
        //    this.Validate();

        //    if(this.HasErrors)
        //    {
        //        // dont submit form
        //    }
        //    else
        //    {
        //        // submit form
        //    }
        //}

        // to specify a default value for a property
        // [DefaultPropertyValue(Value = "")] 


    // NOTE: This implementation is not yet quite true to spec for top level entity errors.

    /// <summary>
    /// Standard base class for data models and view models.
    /// </summary>
    public class ValidatingModelBase : PropertyChangedBase, INotifyDataErrorInfo
    {

        #region Locals

        protected Dictionary<string, List<ValidationErrorInfo>> modelErrors = new Dictionary<string, List<ValidationErrorInfo>>();

        #endregion

        #region INotifyDataErrorInfo        

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// Getting errors for some property
        /// </summary>
        /// <param name="propertyName">Property name for getting errors</param>
        /// <returns>IEnumerable</returns>
        public IEnumerable GetErrors(string propertyName)
        {

            // When propertyName is a valid property name, the GetErrors method must return an
            // enumerable set of objects that represent the current validation errors associated 
            // with this property. When propertyName is null or empty, the GetErrors method must
            // return an enumerable set of objects that represent top-level or cross-property 
            // validation errors. In both cases, the returned value can be null or an empty IEnumerable 
            // set to signify that there is no more known error for the provided argument
            
            if (string.IsNullOrEmpty(propertyName))
                return modelErrors.Values;

            var result = modelErrors.ContainsKey(propertyName) ? modelErrors[propertyName] : new List<ValidationErrorInfo>();
            return result;
        }

        /// <summary>
        /// Returns bool if object have error
        /// </summary>
        public bool HasErrors
        {
            get { return modelErrors.Count > 0; }
        }
        
#endregion

        #region Raise Notifications

        /// <summary>
        /// NotifyErrorChanged
        /// </summary>
        /// <param name="propertyName">Property name that error changed</param>
        private void RaiseErrorsChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            // DataErrorsChangedEventArgs.PropertyName points to a public property of the business
            // object. Alternatively, it is null or empty when the set of top-level/cross-property
            // errors has changed.

            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));

            // If the business object also implements INotifyPropertyChanged and HasErrors is exposed 
            // publicly, then PropertyChanged must be raised each time the HasErrors value changes.

            RaisePropertyChanged(() => HasErrors);
        }

        /// <summary>
        /// Notify change in errors - typesafe.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        private void RaiseErrorsChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            RaiseErrorsChanged(property.GetMemberInfo().Name);
        }

        #endregion

        #region Remove Errors
            
        /// <summary>
        /// Remove error with given code from given property
        /// </summary>
        /// <param name="propertyName">Property to remove error</param>
        /// <param name="errorCode">Error code</param>
        protected virtual void RemoveErrorFromPropertyAndNotifyErrorChanges(string propertyName, int errorCode)
        {
            if (!modelErrors.ContainsKey(propertyName))
                return;

            RemoveErrorFromPropertyIfErrorCodeAlreadyExist(propertyName, errorCode);
            RaiseErrorsChanged(propertyName);
        }

        /// <summary>
        /// Remove a specific error for a property - typesafe.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <param name="errorCode"></param>
        protected virtual void RemoveErrorFromPropertyAndNotifyErrorChanges<TProperty>(Expression<Func<TProperty>> property, int errorCode)
        {
            RemoveErrorFromPropertyAndNotifyErrorChanges(property.GetMemberInfo().Name, errorCode);
        }
                
        /// <summary>
        /// Removing error from property if that error exist for property
        /// </summary>
        /// <param name="propertyName">Property to remove error</param>
        /// <param name="errorCode">Error code</param>
        private void RemoveErrorFromPropertyIfErrorCodeAlreadyExist(string propertyName, int errorCode)
        {
            if (!modelErrors.ContainsKey(propertyName))
                return;

            var errorToRemove = modelErrors[propertyName].SingleOrDefault(error => error.ErrorCode == errorCode);

            if (errorToRemove == null)
                return;

            this.modelErrors[propertyName].Remove(errorToRemove);
         
            if (modelErrors[propertyName].Count == 0)
                modelErrors.Remove(propertyName);
        }

        /// <summary>
        /// Removing all errors for some property
        /// </summary>
        /// <param name="propertyName">Property name for removeing errors</param>
        protected virtual void RemoveAllErrorsForProperty(string propertyName)
        {
            if (modelErrors.ContainsKey(propertyName))
                modelErrors.Remove(propertyName);
        }

        /// <summary>
        /// Removes all errors for a given property - typesafe.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        protected virtual void RemoveAllErrorsForProperty<TProperty>(Expression<Func<TProperty>> property)
        {
            RemoveAllErrorsForProperty(property.GetMemberInfo().Name);
        }

        #endregion

        #region Add Errors

        /// <summary>
        /// Adding error to property
        /// </summary>
        /// <param name="propertyName">Proprty name to add error</param>
        /// <param name="errorInfo">Error info</param>
        protected virtual void AddErrorToPropertyAndNotifyErrorChanges(string propertyName, ValidationErrorInfo errorInfo)
        {
            RemoveErrorFromPropertyIfErrorCodeAlreadyExist(propertyName, errorInfo.ErrorCode);
            if (!modelErrors.ContainsKey(propertyName))
                modelErrors.Add(propertyName, new List<ValidationErrorInfo>());
        
            modelErrors[propertyName].Add(errorInfo);
        
            RaiseErrorsChanged(propertyName);
        }

        /// <summary>
        /// Add a new error to the collection - typesafe.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <param name="errorInfo"></param>
        protected virtual void AddErrorToPropertyAndNotifyErrorChanges<TProperty>(Expression<Func<TProperty>> property, ValidationErrorInfo errorInfo)
        {
            AddErrorToPropertyAndNotifyErrorChanges(property.GetMemberInfo().Name, errorInfo);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates property and adds any errors to collection.
        /// </summary>
        /// <param name="value">Value that will be set</param>
        /// <param name="propertyName">Property name for validation</param>
        protected virtual void ValidateProperty(string propertyName,object value)
        {
            try
            {
                // remove all current errors so if now is good to remove existing
                RemoveAllErrorsForProperty(propertyName);

                // validate
                Validator.ValidateProperty(value, 
                                           new ValidationContext(this, null, null)
                                               {
                                                   MemberName = propertyName
                                               });
            }
            catch(ValidationException e)
            {
                // foreach error add it for property
                AddErrorToPropertyAndNotifyErrorChanges(propertyName,new ValidationErrorInfo(e.GetHashCode(),e.Message));
            }
        }

        /// <summary>
        /// Validates property - typesafe.
        /// </summary>
        /// <param name="value">Value that will be set</param>
        /// <param name="propertyName">Property name for validation</param>
        protected void ValidateProperty<TProperty>(Expression<Func<TProperty>> property, object value)
        {
            ValidateProperty(property.GetMemberInfo().Name, value);
        }

        /// <summary>
        /// Check if property has error.
        /// </summary>
        /// <param name="propertyName">name of the property</param>
        /// <returns>bool</returns>
        protected bool HasPropertyError(string propertyName)
        {
            if (modelErrors.ContainsKey(propertyName))
                return true;

            return false;
        }

        /// <summary>
        /// Typesafe version of check to see if a given property has an error.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        protected bool HasPropertyError<TProperty>(Expression<Func<TProperty>> property)
        {
            return HasPropertyError(property.GetMemberInfo().Name);
        }

		/// <summary>
		/// Validates model
		/// </summary>
		public void Validate()
		{
			ClearErrors();
			var erors = new Collection<ValidationResult>();

            // validating object
			Validator.TryValidateObject(this, new ValidationContext(this, null, null), erors, true);

            // foreach error add it 
			foreach (var validationResult in erors)
			{
				foreach (var memberName in validationResult.MemberNames)
					AddErrorToPropertyAndNotifyErrorChanges(memberName, new ValidationErrorInfo(memberName.GetHashCode(), validationResult.ErrorMessage));
			}
		}

        /// <summary>
        /// Validate a property, throw exception if error.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        protected void Validate(string propertyName, object value)
        {
            Validator.ValidateProperty(value,
                                       new ValidationContext(this, null, null)
                                           {
                                               MemberName = propertyName
                                           });
        }

        /// <summary>
        /// Validate a property, throw exception if error. Typesafe.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="value"></param>
        /// <param name="property"></param>
        protected void Validate<TProperty>(Expression<Func<TProperty>> property, object value)
        {
            Validate(property.GetMemberInfo().Name, value);
        }

        /// <summary>
        /// Clear all errors for the view model.
        /// </summary>
		public void ClearErrors()
		{
			var propertyNames = modelErrors.Keys.ToList();

			foreach (var property in propertyNames)
			{
				modelErrors.Remove(property);

				RaiseErrorsChanged(property);
			}
		}

        #endregion

        #region Default Values

        public void SetDefaults()
        {
            var properties = this.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                var attributes = propertyInfo.GetCustomAttributes(true);
                var defaultAttribute = attributes.OfType<DefaultPropertyValueAttribute>().FirstOrDefault();

                if (defaultAttribute == null)
                    continue;

                propertyInfo.SetValue(this,
                                      propertyInfo.PropertyType == typeof (DateTime)
                                          ? DateTime.Parse((string) defaultAttribute.Value)
                                          : defaultAttribute.Value, null);
            }
        }

        #endregion

        #region Dispose

		protected void Dispose(bool disposing)
		{
			ErrorsChanged = null;
        }

        #endregion

    }
}
