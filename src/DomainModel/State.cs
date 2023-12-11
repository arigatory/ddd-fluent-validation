using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace DomainModel;

public class State : ValueObject
{
    public static readonly State VA = new State("VA");
    public static readonly State DC = new State("DC");
    public static readonly State[] All = { VA, DC };

    public string Value { get; }

    private State(string value)
    {
        Value = value;
    }

    public static Result<State> Create(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result.Failure<State>("Value is required");

        string name = input.Trim();

        if (name.Length > 2) 
            return Result.Failure<State>("Value is too long");

        if (All.Any(x => x.Value == name.ToUpper()) == false)
            return Result.Failure<State>("State is invalid");

        return Result.Success(new State(name));
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }
}