using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ModularMonolithSample.BuildingBlocks.Models;

namespace ModularMonolithSample.BuildingBlocks.Filters;

/// <summary>
/// Action filter that automatically wraps responses in ApiResponse format
/// </summary>
public class ApiResponseFilter : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null)
        {
            // Exception handling is done by global middleware
            return;
        }

        if (context.Result is ObjectResult objectResult)
        {
            // Don't wrap if already wrapped
            if (IsAlreadyWrapped(objectResult.Value))
            {
                base.OnActionExecuted(context);
                return;
            }

            // Wrap the response
            var apiResponse = ApiResponse<object>.SuccessResult(
                objectResult.Value!, 
                GetSuccessMessage(context)
            );
            
            apiResponse.TraceId = context.HttpContext.TraceIdentifier;
            
            objectResult.Value = apiResponse;
        }
        else if (context.Result is EmptyResult || context.Result is NoContentResult)
        {
            var apiResponse = ApiResponse.SuccessResult("Operation completed successfully");
            apiResponse.TraceId = context.HttpContext.TraceIdentifier;
            
            context.Result = new OkObjectResult(apiResponse);
        }

        base.OnActionExecuted(context);
    }

    private static bool IsAlreadyWrapped(object? value)
    {
        if (value == null) return false;
        
        var type = value.GetType();
        
        // Check if it's already an ApiResponse
        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            return genericType == typeof(ApiResponse<>) || 
                   genericType == typeof(PagedResponse<>);
        }
        
        return type == typeof(ApiResponse);
    }

    private static string GetSuccessMessage(ActionExecutedContext context)
    {
        return context.HttpContext.Request.Method.ToUpper() switch
        {
            "GET" => "Data retrieved successfully",
            "POST" => "Resource created successfully", 
            "PUT" => "Resource updated successfully",
            "PATCH" => "Resource updated successfully",
            "DELETE" => "Resource deleted successfully",
            _ => "Operation completed successfully"
        };
    }
} 