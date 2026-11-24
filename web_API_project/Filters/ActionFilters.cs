using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace web_API_project.Filters
{
    public class ValidateModelAttribute : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
                context.Result = new BadRequestObjectResult(new
                {
                    message = "The request contains invalid data.",
                    errors = context.ModelState.Where(item => item.Value?.Errors.Count > 0)
                        .ToDictionary(item => item.Key, item => item.Value!.Errors.Select(error => error.ErrorMessage))
                });
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }

    public class ExecutionTimerAttribute(ILogger<ExecutionTimerAttribute> logger) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var timer = Stopwatch.StartNew();
            await next();
            timer.Stop();
            logger.LogInformation("{Method} {Path} completed in {ElapsedMilliseconds} ms",
                context.HttpContext.Request.Method, context.HttpContext.Request.Path, timer.ElapsedMilliseconds);
        }
    }
}
