using Func;

namespace amethyst.Domain;

public class UnexpectedResultException(Result result) : Exception;