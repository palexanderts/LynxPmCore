using LynxPmCore.Shared.Common;
using Microsoft.AspNetCore.Mvc;

namespace LynxPmCore.Api.Extensions;

internal static class ResultExtensions
{
    internal static IActionResult ToActionResult(this Result result, ControllerBase ctrl) =>
        result.IsSuccess ? ctrl.NoContent() : result.Error.ToActionResult(ctrl);

    internal static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase ctrl) =>
        result.IsSuccess ? ctrl.Ok(result.Value) : result.Error.ToActionResult(ctrl);

    private static IActionResult ToActionResult(this Error error, ControllerBase ctrl) =>
        error.Code.EndsWith(".NotFound")  ? ctrl.NotFound(error)  :
        error.Code.EndsWith(".Conflict")  ? ctrl.Conflict(error)  :
        error.Code.StartsWith("Validation.") ? ctrl.UnprocessableEntity(error) :
        ctrl.BadRequest(error);
}
