using System.Threading.Tasks;
using UserDataService.Repositories;

namespace UserDataService.Services
{
    public interface IDataService
    {
        Task<bool> DeleteUserAsync(int userId);
    }
    
    public class DataService : IDataService
    {
        private readonly IUserDataRepository _userDataRepo;

        public DataService(IUserDataRepository userDataRepo)
        {
            _userDataRepo = userDataRepo;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userDataRepo.GetByIdAsync(userId);
            if (user == null)
                return false;

            await _userDataRepo.DeleteUserAsync(user);
            return true;
        }
    }
}