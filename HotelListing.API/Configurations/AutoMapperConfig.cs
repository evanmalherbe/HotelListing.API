using AutoMapper;
using HotelListing.API.Data;
using HotelListing.API.Models.Country;
using HotelListing.API.Models.Hotels;

namespace HotelListing.API.Configurations
{
	public class AutoMapperConfig : Profile
	{
        public AutoMapperConfig()
        {
            CreateMap<Country, CreateCountryDTO>().ReverseMap();
            CreateMap<Country, GetCountryDTO>().ReverseMap();
            CreateMap<Country, GetCountryDetailsDTO>().ReverseMap();
            CreateMap<Country, UpdateCountryDTO>().ReverseMap();

            CreateMap<Hotel, GetHotelDTO>().ReverseMap();

        }
    }
}
