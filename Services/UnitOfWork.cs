﻿using DataAccessObjects.DAOs;
using DataAccessObjects;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Repositories;

namespace Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FindingHealthcareSystemContext _context;
        private readonly Dictionary<Type, object> _repositories;

        public UnitOfWork(FindingHealthcareSystemContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IGenericRepository<T> GetRepository<T>() where T : class
        {
            if (!_repositories.ContainsKey(typeof(T)))
            {
                var dao = new GenericDAO<T>(_context);
                var repository = new GenericRepository<T>(dao);
                _repositories[typeof(T)] = repository;
            }

            return (IGenericRepository<T>)_repositories[typeof(T)];
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
