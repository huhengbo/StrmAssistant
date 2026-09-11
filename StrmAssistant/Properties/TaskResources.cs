using System.Globalization;
using System.Resources;

namespace StrmAssistant.Properties
{
    internal static class TaskResources
    {
        private static readonly ResourceManager ResourceManager =
            new ResourceManager("StrmAssistant.Properties.TaskResources", typeof(TaskResources).Assembly);

        public static string GetString(string key, CultureInfo culture)
        {
            return ResourceManager.GetString(key, culture) ?? key;
        }
    }
}
