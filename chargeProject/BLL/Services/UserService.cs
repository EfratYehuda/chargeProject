using AutoMapper;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO; // עבור UserRoles
using DTO.USERDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly InterfaceUser _userRepo;
        private readonly IMapper _mapper;

        public UserService(InterfaceUser userRepo, IMapper mapper)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public async Task<UserProfileDto?> GetByIdAsync(int id)
        {
            var entity = await _userRepo.GetByIdAsync(id);
            return _mapper.Map<UserProfileDto>(entity);
        }

        public async Task<List<UserProfileDto>> GetAllAsync()
        {
            var entities = await _userRepo.GetAllAsync();
            return _mapper.Map<List<UserProfileDto>>(entities);
        }

        public async Task<UserProfileDto?> GetByEmailAsync(string email)
        {
            var entity = await _userRepo.GetByEmailAsync(email);
            return _mapper.Map<UserProfileDto>(entity);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _userRepo.EmailExistsAsync(email);
        }

        public async Task<UserProfileDto> AddAsync(RegisterRequestDto dto, string passwordHash)
        {
            if (await _userRepo.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("כתובת האימייל כבר קיימת במערכת");

            var entity = _mapper.Map<Users>(dto);
            entity.PasswordHash = passwordHash;

            // הלוגיקה העסקית של יצירת הפרופיל מתבצעת כאן
            if (dto.Role == UserRoles.Driver)
                entity.DriverProfile = new Drivers();
            else if (dto.Role == UserRoles.Owner)
                entity.OwnerProfile = new Owner();

            var savedEntity = await _userRepo.AddAsync(entity);

            // שולפים מחדש עם כל ה-Includes
            var fullEntity = await _userRepo.GetByIdAsync(savedEntity.Id);
            return _mapper.Map<UserProfileDto>(fullEntity!);
        }

        public async Task<UserProfileDto?> UpdateAsync(int id, UpdateUserProfileDto dto)
        {
            var entity = await _userRepo.GetByIdAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            var updatedEntity = await _userRepo.UpdateAsync(entity);

            return _mapper.Map<UserProfileDto>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _userRepo.DeleteAsync(id);
        }
    }
}