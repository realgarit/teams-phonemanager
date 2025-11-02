using System.Windows;
using System.Windows.Controls;
using teams_phonemanager.Services;

namespace teams_phonemanager.Converters
{
    public class PageTemplateSelector : DataTemplateSelector
    {
        private readonly LoggingService _loggingService;

        public PageTemplateSelector()
        {
            _loggingService = LoggingService.Instance;
        }

        public required DataTemplate WelcomeTemplate { get; set; }
        public required DataTemplate GetStartedTemplate { get; set; }
        public required DataTemplate VariablesTemplate { get; set; }
        public required DataTemplate M365GroupsTemplate { get; set; }
        public required DataTemplate CallQueuesTemplate { get; set; }
        public required DataTemplate AutoAttendantsTemplate { get; set; }
        public required DataTemplate HolidaysTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is string pageName)
            {
                var normalizedPageName = pageName.Replace(" ", "");
                var normalizedWelcome = ConstantsService.Pages.Welcome.Replace(" ", "");
                var normalizedGetStarted = ConstantsService.Pages.GetStarted.Replace(" ", "");
                var normalizedVariables = ConstantsService.Pages.Variables.Replace(" ", "");
                var normalizedM365Groups = ConstantsService.Pages.M365Groups.Replace(" ", "");
                var normalizedCallQueues = ConstantsService.Pages.CallQueues.Replace(" ", "");
                var normalizedAutoAttendants = ConstantsService.Pages.AutoAttendants.Replace(" ", "");
                var normalizedHolidays = ConstantsService.Pages.Holidays.Replace(" ", "");
                
                DataTemplate template = normalizedPageName switch
                {
                    var name when name == normalizedWelcome => WelcomeTemplate,
                    var name when name == normalizedGetStarted => GetStartedTemplate,
                    var name when name == normalizedVariables => VariablesTemplate,
                    var name when name == normalizedM365Groups => M365GroupsTemplate,
                    var name when name == normalizedCallQueues => CallQueuesTemplate,
                    var name when name == normalizedAutoAttendants => AutoAttendantsTemplate,
                    var name when name == normalizedHolidays => HolidaysTemplate,
                    _ => WelcomeTemplate
                };

                _loggingService.Log($"Selected template for page '{pageName}' (normalized: '{normalizedPageName}')", LogLevel.Info);
                return template;
            }

            _loggingService.Log("Item was not a string, defaulting to WelcomeTemplate", LogLevel.Warning);
            return WelcomeTemplate;
        }
    }
}
