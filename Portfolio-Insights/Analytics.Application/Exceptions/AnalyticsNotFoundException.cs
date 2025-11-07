using System;

namespace Analytics.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when analytics for a given user or entity are not found.
    /// </summary>
    public class AnalyticsNotFoundException : Exception
    {
        public string UserName { get; }

        public AnalyticsNotFoundException(string userName)
            : base($"Analytics data not found for user '{userName}'.")
        {
            UserName = userName;
        }

        public AnalyticsNotFoundException(string userName, Exception innerException)
            : base($"Analytics data not found for user '{userName}'.", innerException)
        {
            UserName = userName;
        }
    }
}