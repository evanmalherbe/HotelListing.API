using AutoMapper;
using HotelListing.API.Data;
using HotelListing.API.Models.Country;
using HotelListing.API.Models.Hotels;
using HotelListing.API.Models.Users;

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

            CreateMap<Hotel, CreateHotelDTO>().ReverseMap();
            CreateMap<Hotel, GetHotelDTO>().ReverseMap();
            CreateMap<Hotel, GetHotelDetailsDTO>().ReverseMap();
            CreateMap<Hotel, UpdateHotelDTO>().ReverseMap();

            CreateMap<ApiUserDTO, ApiUser>().ReverseMap();
        }
    }
}
