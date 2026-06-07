using Microsoft.AspNetCore.Mvc;
using TaskLists.Api.Models.Responses;
using TaskLists.SharedKernel.Result;

namespace TaskLists.Api.Extensions;

internal static class ResultActionExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        return result.Successful
            ? new NoContentResult()
            : result.ToErrorActionResult();
    }

    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        Func<T, IActionResult> onSuccess)
    {
        return result.Successful
            ? onSuccess(result.Value)
            : result.ToErrorActionResult();
    }

    private static IActionResult ToErrorActionResult(this Result result)
    {
        var errorResponse = new ErrorResponse(
            result.Errors
                .Select(error => new ErrorItemResponse(error.Message, error.Code))
                .ToArray());

        return new ObjectResult(errorResponse)
        {
            StatusCode = result.Status switch
            {
                Result.ResultStatus.ValidationError => StatusCodes.Status400BadRequest,
                Result.ResultStatus.NotFound => StatusCodes.Status404NotFound,
                Result.ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
                Result.ResultStatus.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            }
        };
    }
}
