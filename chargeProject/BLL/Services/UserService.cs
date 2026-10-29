using AutoMapper;
using DAL.Classes;
using DAL.Repositories.IRepositories;
using DTO; // עבור UserRoles
using DTO.USERDTO;
using Microsoft.AspNetCore.Identity;
using MyApp.BLL.Services;


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
        public async Task<bool> Register(RegisterRequestDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _user.EmailExistsAsync(email))
                return false;

            // התפקיד נקבע בשרת, לא מגיע מהלקוח
            var user = new Users {
                Email = email,
                Name = dto.Name,
                Phone = dto.Phone,
                Access = "User"
            };
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            await _user.AddAsync(user);
            return true;   // הקונטרולר יחליט מה להחזיר ללקוח
        }

        public async Task<AuthResponseDto>? LoginAsync(LoginRequestDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await _user.GetByEmailAsync(email);
            if (user is null)
                return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            return  _tokens.CreateToken(user);
        }


        public async Task<UserProfileDto?> GetByIdAsync(int id)
        {
            var entity = await _user.GetByIdAsync(id);
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
            return _mapper.Map<UserProfileDto>(entity);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _user.EmailExistsAsync(email);
        }

        public async Task<UserProfileDto> AddAsync(RegisterRequestDto dto)
        {
            if (await _user.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("כתובת האימייל כבר קיימת במערכת");

            var entity = _mapper.Map<Users>(dto);
            entity.PasswordHash = _hasher.HashPassword(entity, dto.Password);
            // הלוגיקה העסקית של יצירת הפרופיל מתבצעת כאן
            if (dto.Role == UserRoles.Driver)
                entity.DriverProfile = new Drivers();
            else if (dto.Role == UserRoles.Owner)
                entity.OwnerProfile = new Owner();

            var savedEntity = await _user.AddAsync(entity);

            // שולפים מחדש עם כל ה-Includes
            var fullEntity = await _user.GetByIdAsync(savedEntity.Id);
            return _mapper.Map<UserProfileDto>(fullEntity!);
        }

        public async Task<UserProfileDto?> UpdateAsync(int id, UpdateUserProfileDto dto)
        {
            var entity = await _user.GetByIdAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            var updatedEntity = await _user.UpdateAsync(entity);

            return _mapper.Map<UserProfileDto>(updatedEntity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _user.DeleteAsync(id);
        }
    }
}