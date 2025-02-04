using ExpensesControl.Application.UseCases.Expenses.GetByIdExpense.Dto.Response;
using MediatR;

namespace ExpensesControl.Application.UseCases.Expenses.GetByIdExpense.Dto.Request;
public record GetExpensesByIdCodeRequest(int Id) : IRequest<GetExpensesByIdResponse>;