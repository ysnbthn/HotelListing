using HotelListing.Data;
using HotelListing.IRepository;

namespace HotelListing.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _context;
        private IGenericRepository<Country> _countries;
        private IGenericRepository<Hotel> _hotels;

        public UnitOfWork(DatabaseContext context)
        {
            _context = context;
        }
        // null ise yeni generic repository classı yap
        public IGenericRepository<Country> Countries => _countries ??= new GenericRepository<Country>(_context);

        public IGenericRepository<Hotel> Hotels => _hotels ??= new GenericRepository<Hotel>(_context);

        public void Dispose() // işlem iptal olursa
        {
            // memory temizle
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task Save() // işlemde sorun olmazsa kaydet
        {
            await _context.SaveChangesAsync();
        }
    }
}
