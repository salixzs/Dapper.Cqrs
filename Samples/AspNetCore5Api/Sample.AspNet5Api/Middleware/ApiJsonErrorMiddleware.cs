using System;
using ConfigurationValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Salix.AspNetCore.Utilities;

namespace Sample.AspNet5Api.Middleware
{
    /// <summary>
    /// Own middleware with custom special exception handling, added to provided base middleware class.
    /// </summary>
    public class ApiJsonErrorMiddleware : ApiJsonExceptionMiddleware
    {
        public ApiJsonErrorMiddleware(RequestDelegate next, ILogger<ApiJsonExceptionMiddleware> logger, bool showStackTrace) : base(next, logger, showStackTrace)
        {
        }

        /// <summary>
        /// This method is called from base class handler to add more information to Json Error object.
        /// Here all special exception types should be handled, so API Json Error returns appropriate data.
        /// </summary>
        /// <param name="apiError">ApiError object, which gets returned from API in case of exception/error. Provided by </param>
        /// <param name="exception">Exception which got bubbled up from somewhere deep in API logic.</param>
        protected override ApiError HandleSpecialException(ApiError apiError, Exception exception)
        {
            if (exception is ConfigurationValidationException configurationException)
            {
                apiError.Status = 500;
                apiError.ErrorType = ApiErrorType.ConfigurationError;
                foreach (ConfigurationValidationItem configurationValidation in configurationException.ValidationData)
                {
                    apiError.ValidationErrors.Add(new ApiDataValidationError
                    {
                        PropertyName = $"{configurationValidation.ConfigurationSection}:{configurationValidation.ConfigurationItem}",
                        Message = configurationValidation.ValidationMessage
                    });
                }
            }

            if (exception is NotImplementedException noImplemented)
            {
                apiError.Status = 501;
                apiError.Title = "Functionality is not yet implemented.";
            }

            return apiError;
        }
    }
}
