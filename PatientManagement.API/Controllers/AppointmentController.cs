using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.API.DTOs;
using PatientManagement.Domain.Aggregates.PatientAggregate;
using PatientManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatientManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IRepository<Appointment> appointmentRepository;
        public AppointmentController(IRepository<Appointment> appointmentRepository)
        {
            this.appointmentRepository = appointmentRepository;
        }

        [HttpPost("CreateAppointment")]
        [ProducesResponseType(201)]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CreateAppointment(AppointmentDTO appointmentDTO)
        {
            try
            {
                var appointment = new Appointment(appointmentDTO.PatientId, appointmentDTO.DoctorId, appointmentDTO.DepartmentName, appointmentDTO.DateOfAppointment);
                appointmentRepository.Add(appointment);
                await appointmentRepository.SaveAsync();
                return StatusCode(201, appointment);
            }
            catch(Exception)
            {
                return StatusCode(500);
            }
         
        }

        [HttpGet("AppointmentHistory")]
        [ProducesResponseType(200, Type = typeof(List<AppointmentDTO>))]
        [AllowAnonymous]
        public IActionResult AppointmentHistory()
        {
            try
            {
                var appointments = appointmentRepository.Get();
                var dtos = from appointment in appointments
                           select new AppointmentDTO
                           {
                               Id = appointment.Id,
                               PatientId = appointment.PatientId,
                               DoctorId = appointment.DoctorId,
                               DepartmentName = appointment.DepartmentName,
                               DateOfAppointment = appointment.DateOfAppointment
                           };
                return Ok(dtos);
            }
            catch(Exception)
            {
                return StatusCode(500);
            }
            
        }

        [HttpPut("ResheduleAppointment/{Id}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> ResheduleAppointment(int Id, [FromBody] DateTime dateTime)
        {
            try
            {
                var appointment = appointmentRepository.GetById(Id);
                if (appointment == null)
                    return NotFound();
                appointment.ChangeAppointment(dateTime);
                appointmentRepository.Update(appointment);
                await appointmentRepository.SaveAsync();
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
            
        }

        [HttpDelete("DeleteAppointment/{Id}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> DeleteAppointment(int Id)
        {
            try
            {
                var appointment = appointmentRepository.GetById(Id);
                if (appointment == null)
                    return NotFound();
                appointmentRepository.Remove(appointment);
                await appointmentRepository.SaveAsync();
                return Ok();
            }
            catch(Exception)
            {
                return StatusCode(500);
            }

            
        }
    }
}
