using Application.Common;
using Application.Services.Interfaces;
using Domain.Services.Interfaces;
using Domain.Shared.Dto;
using Flunt.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public abstract class ScraperAppService : Notifiable, IAppScraper
    {
        protected readonly IScraper domainService;

        public ScraperAppService(IScraper domainService)
        {
            this.domainService = domainService;
        }

        public virtual async Task<ServiceTResult<ScraperDataResponse>> GetAsync(ScraperRequest request)
        {
            this.FastValidation(request);

            if (Invalid) return this.HandleResult<ScraperDataResponse>(null);

            var data = await this.domainService.GetAsync(request);
            
            return this.HandleResult(data);
        }

        public abstract Task<ServiceTResult<List<GroupingFileInformationResponse>>> GetGroupingFileInformationAsync(ScraperRequest request);

        /// <summary>
        /// Basic and fast validations on request contract.
        /// Useful before accessing the domain layer.
        /// </summary>
        /// <param name="request">Scraper contract request</param>
        protected virtual void FastValidation(ScraperRequest request)
        {
            if (request == null)
            {
                AddNotification("ScraperRequest", "Is null");
                return;
            }

            if (string.IsNullOrWhiteSpace(request.Url))
            {
                AddNotification("ScraperRequest.Url", "Is null/empty");
            }
        }

        /// <summary>
        /// Get Notifications from domain, application and return the generic ServiceTResult
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="data">The main result</param>
        /// <returns>ServiceTResult with main result</returns>
        protected virtual ServiceTResult<T> HandleResult<T>(T data)
        {
            AddNotifications(this.domainService.GetNotifiable());

            return new ServiceTResult<T>
            {
                Data = data,
                Notifications = base.Notifications,
                Status = Invalid ? ServiceResult.StatusResult.Error : ServiceResult.StatusResult.Ok
            };
        }

        /// <summary>
        /// Get Notifications from domain, application and return the ServiceResult without "data".
        /// Useful to return if the operation was successful or error.
        /// </summary>
        /// <returns>ServiceResult</returns>
        protected virtual ServiceResult HandleResult()
        {
            AddNotifications(this.domainService.GetNotifiable());

            return new ServiceResult
            {
                Notifications = base.Notifications,
                Status = Invalid ? ServiceResult.StatusResult.Error : ServiceResult.StatusResult.Ok
            };
        }
    }
}
