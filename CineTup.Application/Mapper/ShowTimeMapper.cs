using CineTup.Application.Requests;
using CineTup.Application.Responses;
using CineTup.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CineTup.Application.Mapper
{
    public static class ShowTimeMapper
    {
        public static ShowTime ToShowTime(this ShowTimeRequest ShowTimeRequest)
        {
            return new ShowTime
            {
                MovieId = ShowTimeRequest.MovieId,
                StartTime = ShowTimeRequest.StartTime!.Value,
                TicketPrice = ShowTimeRequest.TicketPrice,
                IsDeleted = false
            };
        }

        public static ShowTimeResponse ToShowTimeResponse(this ShowTime ShowTime)
        {
            return new ShowTimeResponse
            {
                Id = ShowTime.Id,
                MovieId = ShowTime.MovieId,
                StartTime = ShowTime.StartTime,
                TicketPrice = ShowTime.TicketPrice
            };
        }

        public static void ApplyUpdate(this ShowTime showTime, ShowTimeUpdateRequest update)
        {
            if (update.MovieId.HasValue)
                showTime.MovieId = update.MovieId.Value;
            if (update.StartTime.HasValue)
                showTime.StartTime = update.StartTime.Value;
            if (update.TicketPrice.HasValue)
                showTime.TicketPrice = update.TicketPrice.Value;
        }
    }
}
