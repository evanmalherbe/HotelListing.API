using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Repository
{
	public class HotelsRepository : GenericRepository<Hotel>, IHotelsRepository
	{
		private readonly HotelListingDbContext _context;

		public HotelsRepository(HotelListingDbContext context, IMapper mapper) : base(context, mapper)
		{
			this._context = context;
		}

		public async Task<Hotel> GetDetails(int id)
		{
			return await _context.Hotels.Include(q => q.Country)
				.FirstOrDefaultAsync(q => q.Id == id);
		}
   }
}
