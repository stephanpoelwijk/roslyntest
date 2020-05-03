using System;

namespace roslyntest.Models
{
    public class SomeDomainObject
    {
        public SomeDomainObject(string firstName, string lastName, int? someValue)
        {
            FirstName = firstName;
            LastName = lastName;
            SomeValue = someValue;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public int? SomeValue { get; }

        public void DoSomething()
        {
            Console.WriteLine("Hello World!");
        }
    }
}