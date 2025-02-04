using CodeFlow.Start.Package.WebTransfer.Base.Response;
using ExpensesControl.Application.UseCases.Base.Records.Expense;

namespace ExpensesControl.Application.UseCases.Expenses.GetByIdExpense.Dto.Response;
public class GetExpensesByIdResponse : BaseResponse, IResultResponse<IEnumerable<ExpenseRecord>>
{
	/// <summary>
	/// The response value of the use case.
	/// </summary>
	public IEnumerable<ExpenseRecord>? Result { get; set; } = default;

	/// <summary>
	/// Sets the result of the use case.
	/// </summary>
	/// <param name="result">The result returned by the use case.</param>
	public void SetResult(IEnumerable<ExpenseRecord> result)
	{
		Result = result;
	}
}