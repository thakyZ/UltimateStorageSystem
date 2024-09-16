using System.Runtime.CompilerServices;

namespace UltimateStorageSystem.Utilities.Exceptions
{
    internal class RecurseMethodException : Exception
    {
        private const string RecursedTooManyStepsTemplate = "Method, {0}, recursed too many times, did {1} recursions passing the threshold of {2}";
        public        string MethodName;
        public        int    StepsTaken;
        public        int    Threshold;

        [Obsolete($"Use a method such as \"{nameof(RecurseMethodException)}.{nameof(RecursedTooManySteps)}\"")]
        public RecurseMethodException()
        {
            StepsTaken = 0;
            Threshold  = 0;
            MethodName = "unknown";
        }

        [Obsolete($"Use a method such as \"{nameof(RecurseMethodException)}.{nameof(RecursedTooManySteps)}\"")]
        public RecurseMethodException(string? message) : base(message)
        {
            StepsTaken = 0;
            Threshold  = 0;
            MethodName = "unknown";
        }

        [Obsolete($"Use a method such as \"{nameof(RecurseMethodException)}.{nameof(RecursedTooManySteps)}\"")]
        public RecurseMethodException(string? message, Exception? innerException) : base(message, innerException)
        {
            StepsTaken = 0;
            Threshold  = 0;
            MethodName = "unknown";
        }

        public RecurseMethodException(string internalMethodName, int recurseStep, int recurseThreshold, string? methodName) : base(FormatMessage(internalMethodName, recurseStep, recurseThreshold, methodName))
        {
            StepsTaken = recurseStep;
            Threshold  = recurseThreshold;
            MethodName = methodName ?? "unknown";
        }

        public RecurseMethodException(string internalMethodName, int recurseStep, int recurseThreshold, string? methodName, Exception? innerException) : base(FormatMessage(internalMethodName, recurseStep, recurseThreshold, methodName), innerException)
        {
            StepsTaken = recurseStep;
            Threshold  = recurseThreshold;
            MethodName = methodName ?? "unknown";
        }

        private static string? FormatMessage(string internalMethodName, int recurseStep, int recurseThreshold, string? callerName)
        {
            if (internalMethodName.Equals(nameof(RecursedTooManySteps), StringComparison.Ordinal))
            {
                return string.Format(RecursedTooManyStepsTemplate, callerName, recurseStep, recurseThreshold);
            }

            return null;
        }

        [DoesNotReturn]
        internal static void RecursedTooManySteps(int recurseStep, int recurseThreshold, [CallerMemberName] string? methodName = null)
        {
            throw new RecurseMethodException(nameof(RecursedTooManySteps), recurseStep, recurseThreshold, methodName);
        }

        [DoesNotReturn]
        internal static void RecursedTooManySteps(int recurseStep, int recurseThreshold, Exception? innerException, [CallerMemberName] string? methodName = null)
        {
            throw new RecurseMethodException(nameof(RecursedTooManySteps), recurseStep, recurseThreshold, methodName, innerException);
        }
    }
}