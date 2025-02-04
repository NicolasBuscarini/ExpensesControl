using CodeFlow.Start.Package.Extensions;
using ExpensesControl.Application.Specs;
using ExpensesControl.Application.UseCases.Base.Records.Expense;
using ExpensesControl.Application.UseCases.Expenses.GetByIdExpense.Dto.Request;
using ExpensesControl.Application.UseCases.Expenses.GetByIdExpense.Dto.Response;
using ExpensesControl.Infrastructure.SqlServer.Repositories.Interface;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Serilog.Core.Enrichers;

namespace ExpensesControl.Application.UseCases.Expenses.GetByIdExpense;

/// <summary>
/// Use case for retrieving expenses by idExpense.
/// </summary>
/// <param name="unitOfWork">Unit of work to manage data access operations.</param>
/// <param name="validator">Validator for validating the request object.</param>
/// <param name="logger">Logger for logging information and debugging purposes.</param>
public class GetExpensesByIdExpenseUseCase(
	IUnitOfWork unitOfWork,
	IValidator<GetExpensesByIdCodeRequest> validator,
	ILogger<GetExpensesByIdExpenseUseCase> logger) : IRequestHandler<GetExpensesByIdCodeRequest, GetExpensesByIdResponse>
{
	/// <summary>
	/// Handles the process of retrieving expenses for a specific user.
	/// </summary>
	/// <param name="request">Request object containing the idExpense.</param>
	/// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
	/// <returns>A response object containing the list of expenses.</returns>
	public async Task<GetExpensesByIdResponse> Handle(GetExpensesByIdCodeRequest request, CancellationToken cancellationToken)
	{
		using (LogContext.Push(
			new PropertyEnricher("Id", request.Id)))
		{
			logger.LogDebug("Starting the process of getting expenses by id.");

			var response = new GetExpensesByIdResponse();
			if (!await validator.ValidateAndAddErrorsAsync(request, logger, response, cancellationToken))
				return response;

			var expenseSpecification = new ExpenseSpec(userCode: request.Id);
			var result = await unitOfWork.ExpenseRepository.ListBySpecificationAsync(expenseSpecification, cancellationToken);

			logger.LogInformation("Expenses retrieved successfully for id {Id}.", request.Id);

			response.SetResult(result.Select(expense => new ExpenseRecord(
				expense.Description,
				expense.StartDate,
				expense.EndDate,
				expense.Category,
				new RecurrenceRecord(
					expense.Recurrence.IsRecurring,
					expense.Recurrence.Periodicity,
					expense.Recurrence.MaxOccurrences),
				new PaymentRecord(
					expense.Payment.Type,
					expense.Payment.IsInstallment,
					expense.Payment.InstallmentCount,
					expense.Payment.Notes,
					expense.Payment.TotalValue),
				expense.Notes
			)).ToList());
			return response;
		}
	}
}
