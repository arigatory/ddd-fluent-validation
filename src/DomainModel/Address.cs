using CSharpFunctionalExtensions;

namespace DomainModel
{
    public class Address : Entity
    {
        public string Street { get; }
        public string City { get; }
        public State State { get; }
        public string ZipCode { get; }

        private Address(string street, string city, State state, string zipCode)
        {
            Street = street;
            City = city;
            State = state;
            ZipCode = zipCode;
        }

        public static Result<Address, Error> Create(string street, string city, string state, string zipCode, string[] allStates)
        {
            State stateObject = State.Create(state, allStates).Value;

            street = (street ?? "").Trim();
            city = (city ?? "").Trim();
            zipCode = (zipCode ?? "").Trim();

            if (street.Length < 1 || street.Length > 100)
                return Errors.General.InvalidLength("street");
            if (city.Length < 1 || city.Length > 40)
                return Errors.General.InvalidLength("city");
            if (zipCode.Length < 1 || zipCode.Length > 5)
                return Errors.General.InvalidLength("zip code");

            return new Address(street, city, stateObject, zipCode);
        }

    }
}
