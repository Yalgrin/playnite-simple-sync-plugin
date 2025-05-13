using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class CompanyMapper : AbstractMapper<Company, CompanyDto>
    {
        public CompanyDto ToDto(Company company)
        {
            return new CompanyDto
            {
                Id = company.Id.ToString(),
                Name = company.Name
            };
        }
    }
}