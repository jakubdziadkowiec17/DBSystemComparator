using DBSystemComparator_API.Database;
using DBSystemComparator_API.Repositories.Interfaces;

namespace DBSystemComparator_API.Repositories.Implementations
{
    public class DataSetRepository : IDataSetRepository
    {

        public DataSetRepository()
        {
        }

        //public async Task<int> CreateDataSetAsync(DataSet dataSet)
        //{
        //    await _dbContext.DataSets.AddAsync(dataSet);
        //    await _dbContext.SaveChangesAsync();

        //    return dataSet.Id;
        //}
    }
}