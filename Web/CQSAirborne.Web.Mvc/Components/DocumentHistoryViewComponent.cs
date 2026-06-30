using CQSAirborne.Model.Document;
using CQSAirborne.Web.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Web.Mvc.Components
{
    public class DocumentHistoryViewComponent : ViewComponent
    {
        private readonly DocumentService _documentService;
        public DocumentHistoryViewComponent(DocumentService documentService)
        {
            _documentService = documentService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int documentId)
        {
            var response = await _documentService.GetDocumentHistoryPlantsNew(documentId);
            if (response.IsSuccess)
            {
                return View("DocumentHistoryPartial", new DocumentHistoryMainModel
                {
                    DocumentId = documentId,
                    Plants = response.Data
                });
            }
            return View("DocumentHistoryPartial", new DocumentHistoryMainModel { });
        }
    }
}
