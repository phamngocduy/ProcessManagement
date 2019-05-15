using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace ProcessManagement.Tests.Support
{
    public static class TestModelHelper
    {
        public static IList<ValidationResult> ValidateModel(this Controller controller, object viewModel)
        {
            controller.ModelState.Clear();

            ValidationContext validationContext = new ValidationContext(viewModel, null, null);
            List<ValidationResult> validationResults = new List<ValidationResult>();

            Validator.TryValidateObject(viewModel, validationContext, validationResults, true);

            foreach (ValidationResult result in validationResults)
            {
                foreach (string name in result.MemberNames)
                {
                    controller.ModelState.AddModelError(name, result.ErrorMessage);
                }
            }

            return validationResults;
        }
    }
}
