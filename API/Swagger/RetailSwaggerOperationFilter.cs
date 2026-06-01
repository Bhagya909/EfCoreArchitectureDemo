using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Swagger;

public class RetailSwaggerOperationFilter : IOperationFilter
{
    private static readonly Dictionary<string, string> ResponseDescriptions = new()
    {
        ["200"] = "Request completed successfully.",
        ["201"] = "Resource created successfully.",
        ["204"] = "Resource deleted or command completed with no response body.",
        ["400"] = "The request failed validation or contains invalid input.",
        ["404"] = "The requested resource was not found.",
        ["409"] = "The request conflicts with the current resource state.",
        ["422"] = "The command was understood but cannot be processed in the current business state.",
        ["500"] = "An unexpected server error occurred."
    };

    private static readonly Dictionary<string, string> ParameterDescriptions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = "Resource identifier.",
            ["productId"] = "Product identifier.",
            ["categoryId"] = "Category identifier.",
            ["customerId"] = "Customer identifier.",
            ["orderId"] = "Order identifier.",
            ["page"] = "One-based page number.",
            ["pageNumber"] = "One-based page number.",
            ["pageSize"] = "Number of records to return. The API may cap this value.",
            ["searchTerm"] = "Optional text search applied to names or SKUs.",
            ["sortBy"] = "Sort field. Supported values depend on the endpoint, for example name or price.",
            ["status"] = "Optional order status filter.",
            ["quantity"] = "Requested stock quantity. Must be greater than zero.",
            ["threshold"] = "Low-stock threshold. Products at or below this quantity are returned.",
            ["lowStockThreshold"] = "Low-stock threshold used when calculating inventory summary metrics.",
            ["top"] = "Maximum number of recent transactions to return."
        };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var action = context.ApiDescription.ActionDescriptor.RouteValues;

        if (action.TryGetValue("controller", out var controller) &&
            action.TryGetValue("action", out var actionName))
        {
            operation.OperationId ??= $"{controller}_{actionName}";
        }

        if (operation.Parameters is not null)
        {
            foreach (var parameter in operation.Parameters)
            {
                if (!string.IsNullOrWhiteSpace(parameter.Name) &&
                    string.IsNullOrWhiteSpace(parameter.Description) &&
                    ParameterDescriptions.TryGetValue(parameter.Name, out var description))
                {
                    parameter.Description = description;
                }
            }
        }

        if (operation.Responses is null)
            return;

        foreach (var response in operation.Responses)
        {
            if (ResponseDescriptions.TryGetValue(response.Key, out var description) &&
                string.IsNullOrWhiteSpace(response.Value.Description))
            {
                response.Value.Description = description;
            }

            var content = response.Value.Content;

            if (IsProblemDetailsStatusCode(response.Key) &&
                content is not null &&
                content.Count == 0)
            {
                content["application/problem+json"] = new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        typeof(ProblemDetails),
                        context.SchemaRepository)
                };
            }
        }

        if (!operation.Responses.ContainsKey("500"))
        {
            var serverErrorResponse = new OpenApiResponse
            {
                Description = ResponseDescriptions["500"],
                Content = new Dictionary<string, OpenApiMediaType>()
            };

            serverErrorResponse.Content["application/problem+json"] =
                new OpenApiMediaType
                {
                    Schema = context.SchemaGenerator.GenerateSchema(
                        typeof(ProblemDetails),
                        context.SchemaRepository)
                };

            operation.Responses["500"] = serverErrorResponse;
        }
    }

    private static bool IsProblemDetailsStatusCode(string statusCode)
    {
        return statusCode is "400" or "404" or "409" or "422" or "500";
    }
}
