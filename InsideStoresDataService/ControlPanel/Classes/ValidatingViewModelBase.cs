using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel.DomainServices.Client;
using MEFedMVVM.Services.Contracts;
using System.ComponentModel.Composition;
using Intersoft.Client.Framework;

namespace ControlPanel
{


    public class ValidatingViewModelBase : ValidatingModelBase
    {

        public AppSvc AppService
        {
            get { return AppSvc.Current; }
        }


		/// <summary>
		/// Validates a specific entity 
		/// </summary>
		public void ValidateEntity(Entity entity)
		{
			ClearErrors();
			var errors = new Collection<ValidationResult>();

            // validating object
			Validator.TryValidateObject(entity, new ValidationContext(entity, null, null), errors, true);

            // foreach error add it 
			foreach (var validationResult in errors)
			{
				foreach (var memberName in validationResult.MemberNames)
					AddErrorToPropertyAndNotifyErrorChanges(memberName, new ValidationErrorInfo() { ErrorCode = memberName.GetHashCode(), ErrorMessage = validationResult.ErrorMessage });
			}
		}

    }
}
