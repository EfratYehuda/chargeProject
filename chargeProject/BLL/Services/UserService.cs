using AutoMapper;
using BLL.Exceptions;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO; // UserRoles
using DTO.USERDTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyApp.BLL.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly InterfaceUser _user;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokens;
        private readonly IPasswordHasher<Users> _hasher;

        public UserService(InterfaceUser userRepo, IMapper mapper, IPasswordHasher<Users> hasher, ITokenService tokens)
        {
            _tokens = tokens;
            _user = userRepo;
            _mapper = mapper;
            _hasher = hasher;
        }

        // Wrong email and wrong password both return null, so the client cannot tell which one was wrong.
        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await _user.GetByEmailAsync(email);
            if (user is null) return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed) return null;

            // Ask the database which profile this user really has
            bool hasRole = dto.Role switch
            {
                UserRoles.Driver => user.DriverProfile != null,
                UserRoles.Owner => user.OwnerProfile != null,
                _ => false
            };

            var response = _tokens.CreateToken(user);
            response.RequestedRole = dto.Role;
            response.HasRequestedProfile = hasRole;
            return response;
        }

        public async Task<UserProfileDto> GetByIdAsync(int id)
        {
            var entity = await _user.GetByIdAsync(id)
                ?? throw new NotFoundException("המשתמש לא נמצא");
            return _mapper.Map<UserProfileDto>(entity);
        }

        public async Task<List<UserProfileDto>> GetAllAsync()
        {
            var entities = await _user.GetAllAsync();
            return _mapper.Map<List<UserProfileDto>>(entities);
        }

        public async Task<UserProfileDto?> GetByEmailAsync(string email)
        {
            var entity = await _user.GetByEmailAsync(email);
            return entity == null ? null : _mapper.Map<UserProfileDto>(entity);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _user.EmailExistsAsync(email);
        }

        // Registration
        public async Task<UserProfileDto> AddAsync(RegisterRequestDto dto)
        {
            dto.Email = dto.Email.Trim().ToLowerInvariant();   // same normalization as LoginAsync
            if (await _user.EmailExistsAsync(dto.Email))
                throw new ConflictException("כתובת האימייל כבר קיימת במערכת");

            var entity = _mapper.Map<Users>(dto);
            entity.Access = UserRoles.User;   // nobody can register as Admin, whatever the client sends
            entity.PasswordHash = _hasher.HashPassword(entity, dto.Password);

            if (dto.Role == UserRoles.Driver)
                entity.DriverProfile = new Drivers();
            else if (dto.Role == UserRoles.Owner)
                entity.OwnerProfile = new Owner();

            var savedEntity = await _user.AddAsync(entity);

            // Re-read with all the Includes
            var fullEntity = await _user.GetByIdAsync(savedEntity.Id);
            return _mapper.Map<UserProfileDto>(fullEntity!);
        }

        public async Task<UserProfileDto> UpdateAsync(int id, UpdateUserProfileDto dto)
        {
            var entity = await _user.GetByIdAsync(id)
                ?? throw new NotFoundException("המשתמש לא נמצא");

            _mapper.Map(dto, entity);   // UpdateUserProfileDto holds only Name and Phone, so nothing else can change
            var updatedEntity = await _user.UpdateAsync(entity);

            return _mapper.Map<UserProfileDto>(updatedEntity);
        }

        public async Task DeleteAsync(int id)
        {
            if (!await _user.DeleteAsync(id))
                throw new NotFoundException("המשתמש לא נמצא");
        }

        // Creates the profile (if it does not exist yet) and returns a NEW token that contains the new profile id.
        public Task<AuthResponseDto> BecomeOwnerAsync(int userId) =>
            EnsureProfileAndIssueTokenAsync(userId, u => u.OwnerProfile != null, u => u.OwnerProfile = new Owner());

        public Task<AuthResponseDto> BecomeDriverAsync(int userId) =>
            EnsureProfileAndIssueTokenAsync(userId, u => u.DriverProfile != null, u => u.DriverProfile = new Drivers());

        private async Task<AuthResponseDto> EnsureProfileAndIssueTokenAsync(
            int userId, Func<Users, bool> hasProfile, Action<Users> addProfile)
        {
            var user = await _user.GetByIdAsync(userId)
                ?? throw new NotFoundException("המשתמש לא נמצא");

            if (!hasProfile(user))
            {
                addProfile(user);
                try
                {
                    await _user.UpdateAsync(user);
                }
                catch (DbUpdateException)
                {
                    // Another request (for example a double click) may have created it first. Re-read below.
                }

                user = await _user.GetByIdAsync(userId);
                if (user == null || !hasProfile(user))
                    throw new ConflictException("Could not create the profile. Please try again.");
            }

            return _tokens.CreateToken(user);
        }
    }
}