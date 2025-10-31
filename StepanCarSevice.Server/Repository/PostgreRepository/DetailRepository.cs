using Microsoft.EntityFrameworkCore;
using StepanCarService.Server.DbContexts;
using StepanCarService.Server.Entities;
using StepanCarService.Server.Repository.Interfaces;

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
        public async Task<Detail> AddingModelToDetail(DetailAddingModel model)
        {
            Detail detail = new Detail()
            {
                Code = model.Code,
                Name = model.Name,
                Price = model.Price,
                CarModelId = model.CarModelId,
                Count = model.Count
            };
            return detail;
        }
        public async Task<bool> AddDetail(DetailAddingModel detail)
        {
            try
            {
                _dbContext.Details.Add(await AddingModelToDetail(detail));
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

        public async Task<bool> DeleteDetails(int[] ids)
        {
            List<Detail> details = new List<Detail>();
            foreach (int id in ids)
            {
                Detail? detail = await GetDetailById(id);
                if (detail != null)
                {
                    detail.Count--;
                    details.Add(detail);
                    _logger.LogInformation($@"Деталь {id} подготовлена для списания");
                }
                else
                {
                    _logger.LogError($@"Ошибка подготовки для списания детали {id}");
                    return false;
                }
            }
            if (details.Count != 0)
            {
                try
                {
                    _dbContext.Update(details);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($@"Запчасти списаны");
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError($@"Ошибка списания деталей");
                    return false;
                }
            }
            else
            {
                _logger.LogWarning($@"Детали не найдены для списания (передан пустой массив id)");
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
