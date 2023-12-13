using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using DomainModel;
using FluentValidation;

namespace Api;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(StateRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(0, 200);
        // .MustBeValueObject(StudentName.Create);

        RuleFor(x => x.Addresses).NotNull().SetValidator(new AddressesValidator(repository));

        RuleFor(x => x.Email)
            .MustBeValueObject(Email.Create)
            .When(x => x.Email != null);

    }
}

public class AddressesValidator : AbstractValidator<AddressDto[]>
{
    public AddressesValidator(StateRepository repository)
    {
        RuleFor(x => x)
            .ListMustContainNumberOfItems(1, 3)
            .ForEach(x =>
            {
                x.NotNull();
                x.ChildRules(address =>
                {
                    address.ClassLevelCascadeMode = CascadeMode.Stop;
                    address.RuleFor(x => x.State).MustBeValueObject(s =>
                    {
                        return State.Create(s, repository.GetAll());
                    });
                    address.RuleFor(y => y).MustBeEntity(y =>
                    {
                        return Address.Create(y.Street, y.City, y.State, y.ZipCode, repository.GetAll());
                    });
                });
            });
    }
}

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, TElement> MustBeEntity<T, TElement, TValueObject>(
    this IRuleBuilder<T, TElement> ruleBuilder, Func<TElement, Result<TValueObject, Error>> factoryMethod
) where TValueObject : DomainModel.Entity
    {
        return (IRuleBuilderOptions<T, TElement>)ruleBuilder.Custom((value, context) =>
       {
           Result<TValueObject, Error> result = factoryMethod(value);

           if (result.IsFailure)
           {
               context.AddFailure(result.Error.Serialize());
           }
       });
    }

    public static IRuleBuilderOptions<T, string> MustBeValueObject<T, TValueObject>(
        this IRuleBuilder<T, string> ruleBuilder, Func<string, Result<TValueObject, Error>> factoryMethod
    ) where TValueObject : ValueObject
    {
        return (IRuleBuilderOptions<T, string>)ruleBuilder.Custom((value, context) =>
        {
            Result<TValueObject, Error> result = factoryMethod(value);

            if (result.IsFailure)
            {
                context.AddFailure(result.Error.Serialize());
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
                context.AddFailure(Errors.General.CollectionIsTooSmall(min.Value, list.Count).Serialize());
            }

            if (max.HasValue && list.Count > max.Value)
            {
                context.AddFailure(Errors.General.CollectionIsTooLarge(max.Value, list.Count).Serialize());
            }
        });
    }

    public static IRuleBuilderOptions<T, TProperty> NotEmpty<T, TProperty>(
    this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return DefaultValidatorExtensions.NotEmpty(ruleBuilder)
            // TODO: overload WithMessage so as not to call .Serialize() every time
            .WithMessage(Errors.General.ValueIsRequired().Serialize());
    }

    public static IRuleBuilderOptions<T, string> Length<T>(
    this IRuleBuilder<T, string> ruleBuilder, int min, int max)
    {
        return DefaultValidatorExtensions.Length(ruleBuilder, min, max)
            .WithMessage(Errors.General.InvalidLength().Serialize());
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
    public EditPersonalInfoRequestValidator(StateRepository repository)
    {
        RuleFor(x => x.Name).NotEmpty().Length(0, 200);
        RuleFor(x => x.Addresses).NotNull().SetValidator(new AddressesValidator(repository));
    }
}