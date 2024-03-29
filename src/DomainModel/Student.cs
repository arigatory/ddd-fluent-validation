﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace DomainModel
{
    public class Student : Entity
    {
        public Email Email { get; }
        public string Name { get; private set; }
        public Address[] Addresses { get; private set; }

        private readonly List<Enrollment> _enrollments = new List<Enrollment>();
        public virtual IReadOnlyList<Enrollment> Enrollments => _enrollments.ToList();

        protected Student()
        {
        }

        public Student(Email email, string name, Address[] addresses)
            : this()
        {
            Email = email;
            EditPersonalInfo(name, addresses);
        }

        public void EditPersonalInfo(string name, Address[] addresses)
        {
            Name = name;
            Addresses = addresses;
        }

        public virtual Result<object, Error> Enroll(Course course, Grade grade)
        {
            if (_enrollments.Count >= 2)
                return Errors.Student.TooManyEnrollments();

            if (_enrollments.Any(x => x.Course == course))
                return Errors.Student.AlreadyEnrolled(course.Name);

            var enrollment = new Enrollment(this, course, grade);
            _enrollments.Add(enrollment);

            return new object();
        }
    }
}
