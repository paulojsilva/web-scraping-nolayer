using Dasync.Collections;
using Domain.Dom.GitHub;
using Domain.Services.Interfaces;
using Domain.Shared.Configuration;
using Domain.Shared.Dto;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Domain.Services.Implementations
{
    public class GitHubScraperService : ScraperService, IScraper
    {
        protected ConcurrentBag<ItemFileInformationResponse> temporaryFiles = new ConcurrentBag<ItemFileInformationResponse>();
        protected string lastCommitHash;
        protected bool usedCache;

        public GitHubScraperService(IHttpClientFactory httpClientFactory, ICache cache, IOptions<AppSettings> settings) : base(httpClientFactory, cache, settings)
        {
        }

        /// <summary>
        /// Public method exposed to perform the Scraper
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<List<GroupingFileInformationResponse>> GetGroupingFileInformationAsync(ScraperRequest request)
        {
            try
            {
                this.httpClient = CreateHttpClient();

                this.host = request.Host;

                await this.ProcessAsync(request.Url, true);

                this.SaveCacheIfNecessary();
            }
            catch (Exception ex)
            {
                AddNotification(ex.GetType().Name, ex.GetMessageConcatenatedWithInner());
            }

            return this.GroupByExtension();
        }

        /// <summary>
        /// Use HttpClient to get full webpages and DOM Parser to analyze HTML elements
        /// </summary>
        /// <param name="url"></param>
        /// <param name="root">Indicate if the URL is the main/master/first/root</param>
        /// <returns>Task</returns>
        public virtual async Task ProcessAsync(string url, bool root = false)
        {
            if (Invalid)
            {
                // The process runs recursively and in parallel
                // Some iterate can throw Exception, so this point stop the cycle
                return;
            }

            try
            {
                using (var httpResponse = await httpClient.GetAsync(url))
                {
                    if (this.InvalidHttpResponse(httpResponse)) return;

                    using (var stream = await httpResponse.Content.ReadAsStreamAsync())
                    {
                        using (var document = documentParser.ParseDocument(stream))
                        {
                            var parser = new GitHubParser(document, url);

                            await this.DeterminePageContentAsync(parser, root);
                            
                            document.Close();
                        }

                        stream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                AddNotification(ex.GetType().Name, ex.GetMessageConcatenatedWithInner());
            }
        }

        /// <summary>
        /// Decide if
        /// Is the time to get file informations (the current page represent a file content)
        /// Or need to continue the process (the current page represent a folder list)
        /// </summary>
        /// <param name="parser">Contain full DOM Elements (IDocument) and helpers to use it</param>
        /// <param name="root">Indicate if the URL is the main/master/first/root</param>
        /// <returns></returns>
        public virtual async Task DeterminePageContentAsync(GitHubParser parser, bool root)
        {
            if (root)
            {
                // Here, i use the cache! The key is Last commit hash.
                // Uses the URL like Key is fastest then last commit hash, because to get last commit, i need to load the first page
                // But if new commit was push, the URL key returns outdated result
                this.lastCommitHash = parser.GetLastCommitHash();
                var cachedFileInformation = this.cache.Get<ConcurrentBag<ItemFileInformationResponse>>(lastCommitHash);
                if (cachedFileInformation != default)
                {
                    this.temporaryFiles = cachedFileInformation;
                    this.usedCache = true;
                    return;
                }
            }

            var pageType = parser.DiscoverPageType();

            switch (pageType)
            {
                case GitHubParser.GitHubPageType.FileContent:
                    this.GetSynthesizedFileInformation(parser);
                    break;

                case GitHubParser.GitHubPageType.FolderList:
                    await this.IterateFolderListAsync(parser);
                    break;
            }
        }

        /// <summary>
        /// Get the filename and total (line, bytes)
        /// </summary>
        /// <param name="parser">Contain full DOM Elements (IDocument) and helpers to use it</param>
        public virtual void GetSynthesizedFileInformation(GitHubParser parser) => this.temporaryFiles.Add(parser.GetFileInformation());

        /// <summary>
        /// Use ProcessAsync recursively to access all folders and files
        /// </summary>
        /// <param name="parser">Contain full DOM Elements (IDocument) and helpers to use it</param>
        /// <returns>A Task</returns>
        public async Task IterateFolderListAsync(GitHubParser parser)
        {
            var elementsToNavigate = parser.GetFolderListItens();

            await elementsToNavigate.ParallelForEachAsync(async element =>
            {
                await this.ProcessAsync(host + element.Endpoint);

            }, maxDegreeOfParallelism: GetRandomMaxDegreeOfParallelism());
        }

        /// <summary>
        /// Dealing with temporaryFiles and grouping by extensions
        /// </summary>
        /// <returns></returns>
        protected List<GroupingFileInformationResponse> GroupByExtension()
        {
            var response = new List<GroupingFileInformationResponse>();

            if (temporaryFiles.IsEmpty && Valid)
            {
                AddNotification("EmptyResult", "Nothing was found. Maybe the repository is empty or the page is not from GitHub Repositories.");
                return response;
            }

            var grouping = this.temporaryFiles.GroupBy(g => g.Extension());

            foreach (var group in grouping)
            {
                response.Add(new GroupingFileInformationResponse(group));
            }

            return response;
        }

        /// <summary>
        /// If cache was used and finded, the key exists, so keep then.
        /// But if doesnt exists, we need create and Set Cache
        /// </summary>
        protected void SaveCacheIfNecessary()
        {
            if (Invalid) return;
            if (usedCache) return;
            if (string.IsNullOrWhiteSpace(lastCommitHash)) return;

            this.cache.Set(lastCommitHash, this.temporaryFiles);
        }

        /// <summary>
        /// Valid the HttpResponse using Notification pattern (EnsureSuccessStatusCode throws Exception)
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <returns></returns>
        protected bool InvalidHttpResponse(HttpResponseMessage httpResponse)
        {
            if (httpResponse == default)
            {
                AddNotification(nameof(HttpResponseMessage), "is null");
            }
            else if (!httpResponse.IsSuccessStatusCode)
            {
                AddNotification(nameof(HttpResponseMessage), $"{(int)httpResponse.StatusCode} {httpResponse.StatusCode}");
            }

            return Invalid;
        }

        /// <summary>
        /// If MaxDegreeOfParallelism param is 2 and PercentualToUseParallelism is 0.4, it means that 40% of processing uses 2 parallel tasks
        /// </summary>
        /// <returns>1 (no parallel) or MaxDegreeOfParallelism param (run in parallel)</returns>
        protected int GetRandomMaxDegreeOfParallelism()
        {
            var random = new Random().NextDouble();

            return (random <= this.settings.Value.PercentualToUseParallelism) ? this.settings.Value.MaxDegreeOfParallelism : 1;
        }
    }
}