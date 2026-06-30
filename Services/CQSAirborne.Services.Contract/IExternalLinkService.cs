using CQSAirborne.Model.ExternalLink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface IExternalLinkService
    {
        IQueryable<ExternalLinkListModel> GetAll();
        AddEditExternalLinkModel GetCreateModel();
        bool Insert(AddEditExternalLinkModel addEditExternalLinkModel, int userId);
        AddEditExternalLinkModel GetById(int id);
        Task<AddEditExternalLinkModel> GetByIdAsync(int id);
        bool Update(AddEditExternalLinkModel addEditExternalLinkModel, int userId);

    }
}
