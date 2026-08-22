using System.Runtime.CompilerServices;
namespace CSDL.Extensions {
    public static class IntExtensions {
        /// <summary>
        ///     Throws via <see cref="Error.ThrowIfError"/> if the value is the SDL failure is equal to check.
        /// </summary>
        internal static int ThrowIfInvalid(this int value, int check, [CallerArgumentExpression(nameof(value))] string? operation = null) {
            if (value == check) {
                Error.ThrowIfError(operation ?? "SDL operation");
            }

            return value;
        }

        /// <summary>
        ///     Logs the current SDL error (via <see cref="Error.LogError"/>) if the value is the SDL failure is equal to check, without throwing.
        /// </summary>
        internal static int LogIfInvalid(this int value, int check, [CallerArgumentExpression(nameof(value))] string? operation = null) {
            if (value == check) {
                Error.LogError(operation ?? "SDL operation");
            }

            return value;
        }
    }
}
