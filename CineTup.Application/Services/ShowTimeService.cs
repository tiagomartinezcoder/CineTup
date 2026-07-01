using CineTup.Application.Abstractions;
using CineTup.Application.Abstractions.Infraestructure;
using CineTup.Application.Exceptions;
using CineTup.Application.Mapper;
using CineTup.Application.Requests;
using CineTup.Application.Responses;
using CineTup.Domain.Entities;

namespace CineTup.Application.Services
{
    public class ShowTimeService : IShowTimeService
    {
        private readonly IShowTimeRepository _showTimeRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMovieRepository _movieRepository;

        public ShowTimeService(
            IShowTimeRepository showTimeRepository,
            ITicketRepository ticketRepository,
            IMovieRepository movieRepository)
        {
            _showTimeRepository = showTimeRepository;
            _ticketRepository = ticketRepository;
            _movieRepository = movieRepository;
        }

        public async Task<List<ShowTimeResponse>> GetAllAsync()
        {
            var showTimes = await _showTimeRepository.GetAllAsync();
            return showTimes
                .Select(x => x.ToShowTimeResponse())
                .ToList();
        }

        public async Task<ShowTimeResponse> GetByIdAsync(int id)
        {
            var showTime = await _showTimeRepository.GetByIdAsync(id);
            if (showTime == null)
                throw new NotFoundException("No se encontro la funcion con id '{id}'");
            return showTime.ToShowTimeResponse();
        }

        public async Task<ShowTimeResponse> CreateAsync(ShowTimeRequest request)
        {
            var movie = await _movieRepository.GetByIdAsync(request.MovieId);
            if (movie == null)
                throw new NotFoundException("No se encontró la película especificada.");

            var startTime = request.StartTime!.Value;
            var endTime = startTime.AddMinutes(movie.Duration);

            if (await _showTimeRepository.ExistsOverlappingShowTimeAsync(request.MovieId, startTime, endTime))
                throw new ValidationException("La función se superpone con otra existente para esta película.");

            var newShowTime = request.ToShowTime();

            var createdShowTime = await _showTimeRepository.AddAsync(newShowTime);

            for (int seat = 1; seat <= 30; seat++)
            {
                await _ticketRepository.AddAsync(new Ticket
                {
                    ShowTimeId = newShowTime.Id,
                    SeatNumber = seat,
                    IsAvailable = true
                });
            }

            return newShowTime.ToShowTimeResponse();
        }

        public async Task UpdateAsync(ShowTimeRequest request, int id)
        {
            var showTimeToUpdate = await _showTimeRepository.GetByIdAsync(id);

            if (showTimeToUpdate == null)
                throw new NotFoundException("No se encontro la funcion con id '{id}'");

            var movie = await _movieRepository.GetByIdAsync(request.MovieId);
            if (movie == null)
                throw new NotFoundException("No se encontró la película especificada.");

            var startTime = request.StartTime!.Value;
            var endTime = startTime.AddMinutes(movie.Duration);

            if (await _showTimeRepository.ExistsOverlappingShowTimeAsync(request.MovieId, startTime, endTime, id))
                throw new ValidationException("La función se superpone con otra existente para esta película.");

            showTimeToUpdate.MovieId = request.MovieId;
            showTimeToUpdate.StartTime = startTime;
            showTimeToUpdate.TicketPrice = request.TicketPrice;

            await _showTimeRepository.UpdateAsync(showTimeToUpdate);
        }

        public async Task DeleteAsync(int id)
        {
            var showTime = await _showTimeRepository.GetByIdAsync(id);

            if (showTime == null)
                throw new NotFoundException("No se encontro la funcion con id '{id}'");

            await _showTimeRepository.DeleteAsync(id);
        }
    }
}