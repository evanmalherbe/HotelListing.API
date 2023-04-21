﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using HotelListing.API.Core.Exceptions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Build.Execution;

namespace HotelListing.API.Core.Repository
{
	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		private readonly HotelListingDbContext _context;
		private readonly IMapper _mapper;

		public GenericRepository(HotelListingDbContext context, IMapper mapper)
    {
			this._context = context;
			this._mapper = mapper;
		}
		public async Task<T> AddAsync(T entity)
		{
			await _context.AddAsync(entity);
			await _context.SaveChangesAsync();
			return entity;
		}

		public async Task<TResult> AddAsync<TSource, TResult>(TSource source)
		{
			var entity = _mapper.Map<T>(source);
			await _context.AddAsync(entity);
			await _context.SaveChangesAsync();

			return _mapper.Map<TResult>(entity);
		}

		//public async Task DeleteAsync(int id)
		//{
		//	var entity = await GetAsync(id);
		//	_context.Set<T>().Remove(entity);
		//	await _context.SaveChangesAsync();
		//}

		public async Task DeleteAsync(int id)
		{
			var entity = await GetAsync(id);
			if (entity == null)
			{
				throw new NotFoundException(typeof(T).Name, id);
			}

			_context.Set<T>().Remove(entity);
			await _context.SaveChangesAsync();
		}

		public async Task<bool> Exists(int id)
		{
			T entity = await GetAsync(id);
			return entity != null;
		}

		public async Task<List<T>> GetAllAsync()
		{
			return await _context.Set<T>()
				.ToListAsync();
		}

		public async Task<List<TResult>> GetAllAsync<TResult>()
		{
			return await _context.Set<T>()
					.ProjectTo<TResult>(_mapper.ConfigurationProvider)
					.ToListAsync();
		}

		public async Task<PagedResult<TResult>> GetAllPagedAsync<TResult>(QueryParameters queryParameters)
		{
			int totalSize = await _context.Set<T>().CountAsync();
			var items = await _context.Set<T>()
				.Skip(queryParameters.StartIndex)
				.Take(queryParameters.PageSize)
				.ProjectTo<TResult>(_mapper.ConfigurationProvider)
				.ToListAsync();

			return new PagedResult<TResult>
			{
				Items = items,
				PageNumber = queryParameters.PageNumber,
				RecordNumber = queryParameters.PageSize,
				TotalCount = totalSize
			};
		}
		public async Task<T> GetAsync(int? id)
		{
			if (id == null)
			{
				return null;
			}

			return await _context.Set<T>().FindAsync(id);
		}

		public async Task<TResult> GetAsync<TResult>(int? id)
		{
			var result = await _context.Set<T>().FindAsync(id);
			if (result == null)
			{
				throw new NotFoundException(typeof(T).Name, id.HasValue ? id : "No key provided.");
			}

			return _mapper.Map<TResult>(result);
		}

		public async Task UpdateAsync(T entity)
		{
			_context.Update(entity);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync<TSource>(int id, TSource source)
		{
			var entity = await GetAsync(id);

			if(entity == null)
			{
				throw new NotFoundException(typeof(T).Name, id);
			}

			_mapper.Map(source, entity);
			_context.Update(entity);
			await _context.SaveChangesAsync();	
		}
	}
}
