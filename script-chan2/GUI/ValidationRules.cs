using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace script_chan2.GUI
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrEmpty((value ?? "").ToString()) ? new ValidationResult(false, "Field is required") : ValidationResult.ValidResult;
        }
    }

    public class WebhookExistsValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Database.Database.Webhooks.Any(x => x.Name == (value ?? "").ToString()) ? new ValidationResult(false, "Name is already in use") : ValidationResult.ValidResult;
        }
    }
}
