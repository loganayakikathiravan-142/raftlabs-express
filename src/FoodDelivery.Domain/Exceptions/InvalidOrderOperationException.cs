namespace FoodDelivery.Domain.Exceptions;

public class InvalidOrderOperationException : DomainException
{
    public InvalidOrderOperationException(string message) : base(message)
    {
    }
}
