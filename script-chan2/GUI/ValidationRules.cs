using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace script_chan2.GUI
{
    public class NotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrEmpty((value ?? string.Empty).ToString()) ? new ValidationResult(false, Properties.Resources.ValidationRule_FieldRequired) : ValidationResult.ValidResult;
        }
    }

    public class WebhookExistsValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return Database.Database.Webhooks.Any(x => x.Name == (value ?? string.Empty).ToString()) ? new ValidationResult(false, Properties.Resources.ValidationRule_WebhookNameDuplicate) : ValidationResult.ValidResult;
        }
    }
}
