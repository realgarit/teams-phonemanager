using System.Windows;
using System.Windows.Controls;
using teams_phonemanager.Services;

namespace teams_phonemanager.Helpers
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
            _loggingService.Log($"Selecting template for page: {item}", LogLevel.Info);
            
            if (item is string pageName)
            {
                DataTemplate template = pageName switch
                {
                    "Welcome" => WelcomeTemplate,
                    "GetStarted" => GetStartedTemplate,
                    "Variables" => VariablesTemplate,
                    "M365Groups" => M365GroupsTemplate,
                    "CallQueues" => CallQueuesTemplate,
                    "AutoAttendants" => AutoAttendantsTemplate,
                    "Holidays" => HolidaysTemplate,
                    _ => WelcomeTemplate
                };

                _loggingService.Log($"Selected template for page '{pageName}'", LogLevel.Info);
                return template;
            }

            _loggingService.Log("Item was not a string, defaulting to WelcomeTemplate", LogLevel.Warning);
            return WelcomeTemplate;
        }
    }
} 