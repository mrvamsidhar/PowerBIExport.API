using Azure;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json;

namespace PowerBIExport.API.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var errorDetails = new ErrorDetails()
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "Sorry, there is a system problem. Please try again later."
                    };

                    if (contextFeature != null)
                    {
                        logger.LogError(contextFeature.Error, $"Something went wrong: {contextFeature.Error}");

                        if (contextFeature.Error is RequestFailedException)
                        {
                            var ex = ((RequestFailedException)contextFeature.Error);
                            if (ex.Status == 429)
                            {
                                errorDetails.Message = "Server is busy. Please try again later.";
                            }
                            else
                            {
                                if (ex.ErrorCode.Equals("content_filter"))
                                {
                                    errorDetails.Message = ex.Message.Split("\n")[0];
                                }
                            }
                        }

                        await context.Response.WriteAsync(errorDetails.ToString());
                    }
                });
            });
        }
    }

    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
    internal static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddCrossOriginResourceSharing(this IServiceCollection services)
        {
            services.AddCors(
                options =>
                    options.AddDefaultPolicy(
                        policy =>
                            policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod()));

            return services;
        }
    }
}
