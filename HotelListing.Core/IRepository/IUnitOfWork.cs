using HotelListing.Data;

namespace HotelListing.Core.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Country> Countries { get; }
        IGenericRepository<Hotel> Hotels { get; }
        Task Save(); // yapılanları onaylayıp kaydetmek için

    }
}
