﻿using AutoMapper;
using BusinessObjects.Commons;
using BusinessObjects.Dtos.User;
using BusinessObjects.Entities;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedList<GeneralUserDto>> GetUsersAsync(
            string? search,
            string? role,
            string? status,
            string? sortBy,
            bool isDescending,
            int pageIndex,
            int pageSize)
        {
            var userRepo = _unitOfWork.GetRepository<User>();

            Expression<Func<User, bool>> filter = u =>
                (string.IsNullOrEmpty(search) || u.Fullname.Contains(search) || u.Email.Contains(search) || u.PhoneNumber.Contains(search)) &&
                (string.IsNullOrEmpty(role) || u.Role.ToString() == role) &&
                (string.IsNullOrEmpty(status) || u.Status.ToString() == status);

            Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null;

            if (!string.IsNullOrEmpty(sortBy))
            {
                orderBy = query =>
                {
                    return isDescending ? query.OrderByDescending(GetSortProperty(sortBy)) : query.OrderBy(GetSortProperty(sortBy));
                };
            }

            var paginatedUsers = await userRepo.GetPagedListAsync(filter, pageIndex, pageSize, orderBy);

            return new PaginatedList<GeneralUserDto>(
                paginatedUsers.Select(_mapper.Map<GeneralUserDto>).ToList(),
                paginatedUsers.Count,
                pageIndex,
                pageSize);
        }

        public async Task<GeneralUserDto?> GetUserByIdAsync(int userId)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var user = await userRepo.GetByIdAsync(userId);
            return user == null ? null : _mapper.Map<GeneralUserDto>(user);
        }

        private static Expression<Func<User, object>> GetSortProperty(string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "fullname" => u => u.Fullname,
                "email" => u => u.Email,
                "phonenumber" => u => u.PhoneNumber,
                "role" => u => u.Role,
                "status" => u => u.Status,
                _ => u => u.Id
            };
        }
    }
}
