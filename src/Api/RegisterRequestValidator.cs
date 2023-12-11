using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using DomainModel;
using FluentValidation;

namespace Api;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(0, 200);
            // .MustBeValueObject(StudentName.Create);

        RuleFor(x => x.Email)
            .MustBeValueObject(Email.Create)
            .When(x => x.Email != null);

        RuleFor(x => x.Addresses).NotNull().SetValidator(new AddressesValidator());
    }
}

public class AddressesValidator : AbstractValidator<AddressDto[]>
{
    public AddressesValidator()
    {
        RuleFor(x => x)
            .ListMustContainNumberOfItems(1, 3)
            .ForEach(x =>
            {
                x.NotNull();
                x.ChildRules(address => {
                    address.ClassLevelCascadeMode = CascadeMode.Stop;
                    address.RuleFor(x => x.State).MustBeValueObject(State.Create);
                    address.RuleFor(y => y).MustBeEntity(y => Address.Create(y.Street, y.City, y.State, y.ZipCode));
                });
            });
    }
}

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, TElement> MustBeEntity<T, TElement, TValueObject>(
    this IRuleBuilder<T, TElement> ruleBuilder, Func<TElement, Result<TValueObject>> factoryMethod
) where TValueObject : DomainModel.Entity
    {
        return (IRuleBuilderOptions<T, TElement>)ruleBuilder.Custom((value, context) =>
       {
           Result<TValueObject> result = factoryMethod(value);

           if (result.IsFailure)
           {
               context.AddFailure(result.Error);
           }
       });
    }

    public static IRuleBuilderOptions<T, string> MustBeValueObject<T, TValueObject>(
        this IRuleBuilder<T, string> ruleBuilder, Func<string, Result<TValueObject>> factoryMethod
    ) where TValueObject : ValueObject
    {
        return (IRuleBuilderOptions<T, string>)ruleBuilder.Custom((value, context) =>
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            Result<TValueObject> result = factoryMethod(value);

            if (result.IsFailure)
            {
                context.AddFailure(result.Error);
            }
        });
    }

    public static IRuleBuilderOptionsConditions<T, IList<TElement>> ListMustContainNumberOfItems<T, TElement>(
        this IRuleBuilder<T, IList<TElement>> ruleBuilder, int? min = null, int? max = null)
    {
        return ruleBuilder.Custom((list, context) =>
        {
            if (min.HasValue && list.Count < min.Value)
            {
                context.AddFailure($"The list must contain {min.Value} items or more. It contains {list.Count} items.");
            }

            if (max.HasValue && list.Count > max.Value)
            {
                context.AddFailure($"The list must contain {max.Value} items or fewer. It contains {list.Count} items.");
            }
        });
    }
}

public class AddressValidator : AbstractValidator<AddressDto>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty().Length(0, 100);
        RuleFor(x => x.City).NotEmpty().Length(0, 40);
        RuleFor(x => x.State).NotEmpty().Length(0, 2);
        RuleFor(x => x.ZipCode).NotEmpty().Length(0, 5);
    }
}

public class EditPersonalInfoRequestValidator : AbstractValidator<EditPersonalInfoRequest>
{
    public EditPersonalInfoRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(0, 200);
        RuleFor(x => x.Addresses).NotNull().SetValidator(new AddressesValidator());
    }
}