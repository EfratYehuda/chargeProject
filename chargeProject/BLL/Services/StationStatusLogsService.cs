using AutoMapper;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO; // מניח ש-StationStatuses נמצא שם
using DTO.STATIONDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class StationStatusLogsService : IStationStatusLogsService
    {
        private readonly InterfaceStationStatusLogs _logsRepo;
        private readonly IMapper _mapper;

        public StationStatusLogsService(InterfaceStationStatusLogs logsRepo, IMapper mapper)
        {
            _logsRepo = logsRepo;
            _mapper = mapper;
        }

        public async Task<StationStatusLogDto?> GetByIdAsync(int id)
        {
            var entity = await _logsRepo.GetByIdAsync(id);
            return _mapper.Map<StationStatusLogDto>(entity);
        }

        public async Task<List<StationStatusLogDto>> GetAllAsync()
        {
            var entities = await _logsRepo.GetAllAsync();
            return _mapper.Map<List<StationStatusLogDto>>(entities);
        }

        public async Task<List<StationStatusLogDto>> GetByStationIdAsync(int stationId)
        {
            var entities = await _logsRepo.GetByStationIdAsync(stationId);
            return _mapper.Map<List<StationStatusLogDto>>(entities);
        }

        public async Task<StationStatusLogDto?> GetLatestByStationIdAsync(int stationId)
        {
            var entity = await _logsRepo.GetLatestByStationIdAsync(stationId);
            return _mapper.Map<StationStatusLogDto>(entity);
        }

        public async Task<List<StationStatusLogDto>> GetFaultReportsByOwnerIdAsync(int ownerId, int take = 20)
        {
            // שולחים את סטטוס התקלה ל-DAL כדי למנוע תלות ב-DTO בשכבת הנתונים
            var entities = await _logsRepo.GetFaultReportsByOwnerIdAsync(ownerId, StationStatuses.Faulty, take);
            return _mapper.Map<List<StationStatusLogDto>>(entities);
        }

        public async Task<StationStatusLogDto> AddAsync(CreateStationStatusLogDto dto)
        {
            if (!await _logsRepo.StationExistsAsync(dto.StationId))
                throw new KeyNotFoundException($"עמדה {dto.StationId} לא נמצאה");

            if (dto.CurrentDriverId.HasValue && !await _logsRepo.DriverExistsAsync(dto.CurrentDriverId.Value))
                throw new KeyNotFoundException($"נהג {dto.CurrentDriverId} לא נמצא");

            var entity = _mapper.Map<StationStatusLogs>(dto);
            entity.UpdatedAt = DateTime.Now;

            var savedEntity = await _logsRepo.AddAsync(entity);

            // שולפים מחדש כדי לקבל את האובייקטים המקושרים (שם הנהג, שם העמדה) עבור ה-DTO
            var fullEntity = await _logsRepo.GetByIdAsync(savedEntity.Id);
            return _mapper.Map<StationStatusLogDto>(fullEntity!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _logsRepo.DeleteAsync(id);
        }
    }
}