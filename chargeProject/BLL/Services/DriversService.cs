using AutoMapper;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO.USERDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class DriversService : IDriversService
    {
        private readonly InterfaceDriver _driverRepo;
        private readonly IMapper _mapper;

        public DriversService(InterfaceDriver driverRepo, IMapper mapper)
        {
            _driverRepo = driverRepo;
            _mapper = mapper;
        }

        public async Task<DriverDto?> GetByIdAsync(int id)
        {
            var entity = await _driverRepo.GetByIdAsync(id);
            return _mapper.Map<DriverDto>(entity);
        }

        public async Task<List<DriverDto>> GetAllAsync()
        {
            var entities = await _driverRepo.GetAllAsync();
            return _mapper.Map<List<DriverDto>>(entities);
        }

        public async Task<DriverDto?> GetByUserIdAsync(int userId)
        {
            var entity = await _driverRepo.GetByUserIdAsync(userId);
            return _mapper.Map<DriverDto>(entity);
        }

        public async Task<DriverDto> AddAsync(CreateDriverDto dto)
        {
            if (!await _driverRepo.UserExistsAsync(dto.UserId))
                throw new KeyNotFoundException($"משתמש {dto.UserId} לא נמצא");

            if (await _driverRepo.DriverExistsForUserAsync(dto.UserId))
                throw new InvalidOperationException("למשתמש כבר קיים פרופיל נהג");

            var entity = _mapper.Map<Drivers>(dto);
            var savedEntity = await _driverRepo.AddAsync(entity);

            // שולפים מחדש מה-DB כדי לקבל את פרטי ה-User שהצטרפו אליו עבור ה-DTO
            var fullEntity = await _driverRepo.GetByIdAsync(savedEntity.Id);
            return _mapper.Map<DriverDto>(fullEntity!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _driverRepo.DeleteAsync(id);
        }
    }
}