using ExpensesControl.Application.UseCases.Expenses.GetByIdExpense.Dto.Request;
using FluentValidation;

namespace ExpensesControl.Application.UseCases.Expenses.GetByIdExpense.Validator;
public class GetExpensesByIdRequestValidator : AbstractValidator<GetExpensesByIdCodeRequest>
{
	public GetExpensesByIdRequestValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0).WithMessage("O id do expense deve ser um número inteiro positivo.");
	}
}