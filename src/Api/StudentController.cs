using System;
using System.Linq;
using CSharpFunctionalExtensions;
using DomainModel;
using Microsoft.AspNetCore.Mvc;

namespace Api;

[Route("api/students")]
public class StudentController : ApplicationController
{
    private readonly StudentRepository _studentRepository;
    private readonly CourseRepository _courseRepository;
    private readonly StateRepository _stateRepository;

    public StudentController(StudentRepository studentRepository, CourseRepository courseRepository, StateRepository stateRepository)
    {
        _stateRepository = stateRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    [HttpPost]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var addresses = request.Addresses
            .Select(x => Address.Create(x.Street, x.City, x.State, x.ZipCode, _stateRepository.GetAll()).Value)
            .ToArray();
        var email = Email.Create(request.Email).Value;
        var studentName = request.Name.Trim();

        Student existingStudent = _studentRepository.GetByEmail(email);
        if (existingStudent != null)
            return Error(Errors.Student.EmailIsTaken(email.Value));

        var student = new Student(email, studentName, addresses);
        _studentRepository.Save(student);

        var response = new RegisterResponse
        {
            Id = student.Id
        };
        return Ok(response);
    }

    [HttpPut("{id}")]
    public IActionResult EditPersonalInfo(long id, [FromBody] EditPersonalInfoRequest request)
    {
        Student student = _studentRepository.GetById(id);
        if (student == null)
            return Error(Errors.General.NotFound(), nameof(id));

        var addresses = request.Addresses
           .Select(x => Address.Create(x.Street, x.City, x.State, x.ZipCode, _stateRepository.GetAll()).Value)
           .ToArray();

        var studentName = request.Name.Trim();

        student.EditPersonalInfo(studentName, addresses);

        _studentRepository.Save(student);

        return Ok();
    }

    [HttpPost("{id}/enrollments")]
    public IActionResult Enroll(long id, [FromBody] EnrollRequest request)
    {
        Student student = _studentRepository.GetById(id);
        if (student == null) return Error(Errors.General.NotFound(), nameof(id));

        for (int i = 0; i < request.Enrollments.Length; i++)
        {
            CourseEnrollmentDto dto = request.Enrollments[i];

            Grade grade = Grade.Create(dto.Grade).Value;

            string courseName = (dto.Course ?? "").Trim();
            Course course = _courseRepository.GetByName(courseName);

            if (course == null) return Error(Errors.General.ValueIsInvalid(), $"{nameof(request.Enrollments)}[{i}].{nameof(dto.Course)}");

            Result<object, Error> result = student.Enroll(course, grade);

            if (result.IsFailure) return Error(result.Error);
        }

        return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult Get(long id)
    {
        Student student = _studentRepository.GetById(id);

        var resonse = new GetResonse
        {
            Addresses = student.Addresses.Select(x => new AddressDto
            {
                Street = x.Street,
                City = x.City,
                State = x.State.Value,
                ZipCode = x.ZipCode,
            }).ToArray(),
            Email = student.Email.Value,
            Name = student.Name,
            Enrollments = student.Enrollments.Select(x => new CourseEnrollmentDto
            {
                Course = x.Course.Name,
                Grade = x.Grade.ToString()
            }).ToArray()
        };
        return Ok(resonse);
    }
}
