﻿namespace jamster.engine.Domain;

public class UnexpectedResultException(Result result) : Exception
{
    public Result Result { get; } = result;
}