﻿using CinemaApi.Data;
using CinemaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CinemaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private CinemaDbContext _dbContext;
        public ReservationsController(CinemaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpPost]
        public IActionResult Post([FromBody] Reservation reservationObj)
        {
            reservationObj.ReservationTime = DateTime.Now;
            _dbContext.Reservations.Add(reservationObj);
            _dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetReservations()
        {
            var reservations = (from reservation in _dbContext.Reservations
                               join customer in _dbContext.Users on reservation.UserId equals customer.Id
                               join movie in _dbContext.Movies on reservation.MovieId equals movie.Id
                               select new
                               {
                                   Id = reservation.Id,
                                   ReservationTime = reservation.ReservationTime,
                                   CustomerName = customer.Id,
                                   MovieName = movie.Name
                               }).FirstOrDefault();
            return Ok(reservations);
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetReservationDetail(int id)
        {
            var reservationResult = from reservation in _dbContext.Reservations
                                    join customer in _dbContext.Users on reservation.UserId equals customer.Id
                                    join movie in _dbContext.Movies on reservation.MovieId equals movie.Id
                                    where reservation.Id == id
                                    select new
                                    {
                                        Id = reservation.Id,
                                        ReservationTime = reservation.ReservationTime,
                                        CustomerName = customer.Id,
                                        MovieName = movie.Name,
                                        Email = customer.Email,
                                        Qty = reservation.Qty,
                                        Price = reservation.Price,
                                        Phone = reservation.Phone,
                                        PlayingTime = movie.PlayingTime,
                                        PlayingDate = movie.PlayingDate,
                                    };
            return Ok(reservationResult);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var reservation = _dbContext.Reservations.Find(id);
            if (reservation == null)
            {
                return NotFound("No record found against this Id");
            }
            else
            {
                _dbContext.Reservations.Remove(reservation);
                _dbContext.SaveChanges();
                return Ok("Record deleted");
            }
        }
    }
}
