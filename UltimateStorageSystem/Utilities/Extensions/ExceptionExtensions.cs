using System.Text;

namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class ExceptionExtensions
    {
        public static string GetFullyQualifiedExceptionMessage(this Exception exception, string? message = null)
        {
            var sb = new StringBuilder()
                     .Append('[')
                     .Append(exception.GetType().Name)
                     .Append("] ");

            if (message is not null)
            {
                sb.AppendLine(message);
            }

            sb.AppendLine(exception.Message);

            if (exception.HelpLink is not null)
            {
                sb.Append("Get Help At:")
                  .AppendLine(exception.HelpLink);
            }

            if (exception.StackTrace is not null)
            {
                sb.AppendLine(exception.StackTrace);
            }

            return sb.ToString();
        }
    }
}
