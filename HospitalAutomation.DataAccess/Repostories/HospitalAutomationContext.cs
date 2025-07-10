using Microsoft.EntityFrameworkCore;

namespace HospitalAutomation.DataAccess.Repositories
{
    public class HospitalAutomationContext
    {
        internal void SaveChanges()
        {
            throw new NotImplementedException();
        }

        internal async Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        internal DbSet<object> Set<T>()
        {
            throw new NotImplementedException();
        }
    }
}