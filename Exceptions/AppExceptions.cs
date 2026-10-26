namespace StudySpace.Exceptions;

public class AppException : Exception
{
    public AppException(string message) : base(message) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
}

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base(message) { }
}

public class BookingConflictException : BusinessRuleException
{
    public BookingConflictException(string message) : base(message) { }
}
