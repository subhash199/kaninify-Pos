using DataHandlerLibrary.Services;
using DataHandlerLibrary.Models;
using EposDataHandler.Models;

namespace EposRetail.Services
{
    public class GlobalErrorLogService
    {
        private readonly ErrorLogServices _errorLogService;
        private readonly UserSessionService _userSessionService;

        public GlobalErrorLogService(ErrorLogServices errorLogService, UserSessionService userSessionService)
        {
            _errorLogService = errorLogService;
            _userSessionService = userSessionService;
        }

        /// <summary>
        /// Global method to log errors with context information
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="methodName">The method where the error occurred</param>
        /// <param name="userAction">Description of what the user was trying to do</param>
        /// <param name="sourceClass">The class/component where the error occurred</param>
        /// <param name="severity">The severity level of the error (defaults to Error)</param>
        /// <returns>Task representing the async operation</returns>
        public async Task LogErrorAsync(
            Exception ex,
            string methodName,
            string userAction,
            string sourceClass,
            ErrorLogSeverity severity = ErrorLogSeverity.Error)
        {
            try
            {
                await _errorLogService.LogErrorAsync(
                    errorMessage: ex.InnerException?.Message ?? ex.Message,
                    errorType: ex.GetType().Name,
                    severity: severity,
                    sourceMethod: methodName,
                    sourceClass: sourceClass,
                    stackTrace: ex.StackTrace ?? "",
                    userAction: userAction,
                    userId: await _userSessionService.GetValidUserIdAsync(),
                    siteId: await _userSessionService.GetValidSiteIdAsync(),
                    tillId: await _userSessionService.GetValidTillIdAsync()
                );
            }
            catch (Exception logEx)
            {
                // Fallback logging to console if ErrorLogService fails
                Console.WriteLine($"Failed to log error: {logEx.Message}. Original error: {ex.Message}");
            }
        }

        /// <summary>
        /// Simplified logging method that automatically detects the calling class
        /// </summary>
        /// <param name="ex">The exception to log</param>
        /// <param name="methodName">The method where the error occurred</param>
        /// <param name="userAction">Description of what the user was trying to do</param>
        /// <param name="severity">The severity level of the error (defaults to Error)</param>
        /// <returns>Task representing the async operation</returns>
        public async Task LogErrorAsync(
            Exception ex,
            string methodName,
            string userAction,
            ErrorLogSeverity severity = ErrorLogSeverity.Error,
            [System.Runtime.CompilerServices.CallerMemberName] string callerName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "")
        {
            // Extract class name from file path
            var sourceClass = System.IO.Path.GetFileNameWithoutExtension(callerFilePath);

            await LogErrorAsync(ex, methodName, userAction, sourceClass, severity);
        }
    }
}