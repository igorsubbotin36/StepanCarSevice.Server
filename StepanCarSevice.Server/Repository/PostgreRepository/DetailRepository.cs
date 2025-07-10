using Microsoft.EntityFrameworkCore;
using StepanCarSevice.Server.DbContexts;
using StepanCarSevice.Server.Entities;
using StepanCarSevice.Server.Repository.Interfaces;

namespace StepanCarSevice.Server.Repository.PostgreRepository
{
    public class DetailRepository : IDetailRepository
    {
        private readonly ILogger<DetailRepository> _logger;
        private readonly PostgreDbContext _dbContext;
        public DetailRepository(ILogger<DetailRepository> logger, PostgreDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task<bool> AddDetail(Detail detail)
        {
            try
            {
                _dbContext.Details.Add(detail);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($@"Деталь {detail.Code} добалена на склад!");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($@"Ошибка добавления детали {detail.Code} на склад");
                return false;
            }
        }

        public async Task<bool> DeleteDetail(int id)
        {
            Detail? detail = await GetDetailById(id);
            if (detail != null)
            {
                try
                {
                    _dbContext.Remove(detail);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($@"Деталь с айди {id} списана");
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError($@"Ошибка списания детали {id}");
                    return false;
                }
            }
            else
            {
                _logger.LogWarning($@"Деталь с айди {id} не найдена для списания");
                return false;
            }
        }

        public async Task<bool> EditDetail(Detail detail)
        {
            try
            {
                _dbContext.Update(detail);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($@"Для детали {detail.Id} обновлена информация");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($@"Ошибка обновления информации для детали {detail.Id}");
                return false;
            }
        }

        public async Task<List<Detail>> GetAllDetails()
        {
            return await _dbContext.Details.ToListAsync();
        }

        public async Task<List<Detail>> GetDetailByCode(string code)
        {
            return await _dbContext.Details.Where(x => x.Code == code).ToListAsync();
        }

        public async Task<Detail?> GetDetailById(int id)
        {
            return await _dbContext.Details.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
