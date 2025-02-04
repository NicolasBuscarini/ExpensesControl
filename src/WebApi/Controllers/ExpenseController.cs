using CodeFlow.Start.Package.WebTransfer.Base;
using CodeFlow.Start.Package.WebTransfer.Base.Response;
using ExpensesControl.Application.UseCases.Expenses.Create.Dto.Request;
using ExpensesControl.Application.UseCases.Expenses.GetByIdExpense.Dto.Request;
using ExpensesControl.Application.UseCases.Expenses.GetByUser.Dto.Request;
using ExpensesControl.Application.UseCases.Expenses.Import.Dto.Request;
using ExpensesControl.Application.UseCases.Expenses.Import.Dto.Response;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace ExpensesControl.WebApi.Controllers;

/// <summary>
/// Controller for expense management operations.
/// </summary>
/// <param name="mediator"></param>
[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
[SwaggerTag("Operations related to expense management")]
public class ExpenseController(IMediator mediator) : ControllerBase
{
	/// <summary>
	/// Creates a new expense.
	/// </summary>
	/// <param name="request">The expense creation request.</param>
	/// <returns>Returns the response of the expense creation.</returns>
	[HttpPost]
	[SwaggerOperation(
		Summary = "Create a new expense",
		Description = "Creates a new expense with the provided details.")]
	[SwaggerResponse(StatusCodes.Status200OK, "Expense created successfully", typeof(IResultResponse<CreateResponse>))]
	[SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data", typeof(BaseResponse))]
	[SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal error", typeof(BaseResponse))]
	public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseRequest request)
	{
		var response = await mediator.Send(request);

		if (!response.IsSuccess && response.ErrorType.HasValue && response.ErrorType.Value == ErrorType.BusinessRuleError)
			return BadRequest(response);
		if (response.IsSuccess) return Ok(response);
		return StatusCode(StatusCodes.Status500InternalServerError, response);
	}

	/// <summary>
	/// Gets expenses by user.
	/// </summary>
	/// <param name="userCode">The user code to search expenses for.</param>
	/// <returns>Returns the list of expenses for the given user code.</returns>
	[HttpGet("/user")]
	[SwaggerOperation(
		Summary = "Get expenses by user code",
		Description = "Fetches expenses for the given user code.")]
	[SwaggerResponse(StatusCodes.Status200OK, "Expenses retrieved successfully", typeof(GetExpensesByUserCodeRequest))]
	[SwaggerResponse(StatusCodes.Status204NoContent, "No expenses found for the given user code", null)]
	[SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid user code", typeof(BaseResponse))]
	public async Task<IActionResult> GetExpensesByUser([FromHeader, Required] int userCode)
	{
		var request = new GetExpensesByUserCodeRequest(default) { UserCode = userCode };

		var response = await mediator.Send(request);

		if (!response.IsSuccess && response.ErrorType == ErrorType.BusinessRuleError)
			return BadRequest(response);
		if (response.IsSuccess && (response.Result == null || !response.Result.Any()))
			return NoContent();
		if (response.IsSuccess && response.Result != null && response.Result.Any())
			return Ok(response);
		return StatusCode(StatusCodes.Status500InternalServerError, response);
	}

	/// <summary>
	/// Gets expenses by id.
	/// </summary>
	/// <param name="id">The IdExpense to search expenses for.</param>
	/// <returns>Returns the list of expenses for the given IdExpense.</returns>
	[HttpGet]
	[SwaggerOperation(
		Summary = "Get expenses by IdExpense",
		Description = "Fetches expenses for the given IdExpense.")]
	[SwaggerResponse(StatusCodes.Status200OK, "Expenses retrieved successfully", typeof(GetExpensesByIdCodeRequest))] //
	[SwaggerResponse(StatusCodes.Status204NoContent, "No expenses found for the given IdExpense", null)]
	[SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid IdExpense", typeof(BaseResponse))]
	public async Task<IActionResult> GetExpensesByIdExpense([FromHeader, Required] int idExpense)
	{
		var request = new GetExpensesByIdCodeRequest(default) { Id = idExpense };

		var response = await mediator.Send(request);

		if (!response.IsSuccess && response.ErrorType == ErrorType.BusinessRuleError)
			return BadRequest(response);
		if (response.IsSuccess && (response.Result == null || !response.Result.Any()))
			return NoContent();
		if (response.IsSuccess && response.Result != null && response.Result.Any())
			return Ok(response);
		return StatusCode(StatusCodes.Status500InternalServerError, response);
	}

	/// <summary>
	/// Imports expenses from a CSV file.
	/// </summary>
	/// <param name="file">The CSV file containing expense records.</param>
	/// <returns>Returns the result of the import process.</returns>
	[HttpPost("import")]
	[SwaggerOperation(
		Summary = "Import expenses from a CSV file",
		Description = "Imports expense records from the provided CSV file.")]
	[SwaggerResponse(StatusCodes.Status200OK, "Expenses imported successfully", typeof(ImportExpensesResponse))]
	[SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid file format or contents", typeof(BaseResponse))]
	[SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal error", typeof(BaseResponse))]
	public async Task<IActionResult> ImportExpenses([FromHeader, Required] int userCode, IFormFile file)
	{

		var request = new ImportExpensesRequest(userCode, file);
		var response = await mediator.Send(request);

		if (!response.IsSuccess && response.ErrorType == ErrorType.BusinessRuleError)
			return BadRequest(response);
		if (response.IsSuccess) return Ok(response);
		return StatusCode(StatusCodes.Status500InternalServerError, response);
	}
}
