using AngleSharp.Dom;
using Domain.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Domain.Dom.GitHub
{
    public class GitHubParser
    {
        public IDocument Document { get; protected set; }
        
        public string Url { get; protected set; }

        private static readonly Dictionary<string, double> sizeRepresentationTerms = new Dictionary<string, double>
        {
            { "quilobyte", 1024 }, { "Kilobyte", 1024 }, { "Megabyte", 1024 * 1024 }, { "Gigabyte", 1024 * 1024 * 1024 }, { "Terabyte", 1024 * 1024 * 1024 }, 
            { "kb", 1024 }, { "mb", 1024 * 1024 },{ "gb", 1024 * 1024 * 1024 }, { "tb", 1024 * 1024 * 1024 }, 
            { "byte", 1 }
        };

        public enum GitHubPageType
        {
            Undefined = 0,
            FolderList,
            FileContent
        }

        public GitHubParser(IDocument document, string url)
        {
            this.Document = document;
            this.Url = url;
        }

        /// <summary>
        /// Find href that represent a Latest commit link
        /// </summary>
        /// <returns></returns>
        public string GetLastCommitHash()
        {
            var boxHeaders = this.Document.All.Where(GitHubFilter.boxHeader);

            foreach (var box in boxHeaders)
            {
                var hasLatestCommit = box.QuerySelector(".sr-only")?.Text() == "Latest commit";
                if (!hasLatestCommit) continue;

                var ahref = box.QuerySelector("a.text-mono");

                if (ahref != default)
                {
                    var href = ahref.GetAttribute("href");

                    if (href.ContainsInvariant("/commit/"))
                    {
                        return href.Split('/').LastOrDefault();
                    }
                }

                var includeFragment = box.QuerySelector("include-fragment");

                if (includeFragment != default)
                {
                    var src = includeFragment.GetAttribute("src");

                    if (src.ContainsInvariant("/tree-commit/"))
                    {
                        return src.Split('/').LastOrDefault();
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Search DOM sections to determine if the page is FileContent, FileContent specific for Readme ou FolderList
        /// </summary>
        /// <returns></returns>
        public GitHubPageType DiscoverPageType()
        {
            var fileContentIndication = this.Document.All.Any(GitHubFilter.fileContents);
            if (fileContentIndication) return GitHubPageType.FileContent;

            var readmeContentIndication = this.Document.All.Any(GitHubFilter.readmeContent);
            if (readmeContentIndication) return GitHubPageType.FileContent;

            var folderListIndication = this.Document.All.Any(GitHubFilter.folderLists);
            if (folderListIndication) return GitHubPageType.FolderList;

            return GitHubPageType.Undefined;
        }

        /// <summary>
        /// Get the File name, number of lines and size (convert to bytes)
        /// </summary>
        /// <returns>DTO ItemFileInformationResponse filled or NULL if doesnt exists</returns>
        public ItemFileInformationResponse GetFileInformation()
        {
            var fileName = this.GetFileNameOnFileContent();
            if (string.IsNullOrWhiteSpace(fileName))
                return default;

            var item = new ItemFileInformationResponse(fileName);

            var boxHeaders = this.Document.All.Where(GitHubFilter.boxHeader);
            
            foreach (var boxHeader in boxHeaders)
            {
                var textMono = boxHeader.QuerySelector("div.text-mono");
                if (textMono == default) continue;

                var text = textMono.Text();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var splitTerms = Regex.Replace(text, @"\r\n?|\n", "#").Split('#');
                        
                    foreach (var term in splitTerms)
                    {
                        if (string.IsNullOrWhiteSpace(term)) continue;
                            
                        if (term.ContainsInvariant("lines"))
                            item.Lines = this.GetFirstNumberOfTerm(term);
                        else
                            item.Bytes = this.TryGetTotalNumberBytes(term, item.Bytes);
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// Find size representation ( 12.5 KB ), extract value and convert to bytes
        /// </summary>
        /// <param name="term"></param>
        /// <param name="defaultTotalNumberBytes"></param>
        /// <returns></returns>
        protected double TryGetTotalNumberBytes(string term, double defaultTotalNumberBytes)
        {
            try
            {
                foreach (var sizeRepresentation in sizeRepresentationTerms)
                {
                    if (term.ContainsInvariant(sizeRepresentation.Key))
                    {
                        var numberOnly = Regex.Replace(term, "[^0-9.+-,]", "");
                        if (numberOnly == "0") return 0;
                        return double.Parse(numberOnly.Replace(".", ",")) * sizeRepresentation.Value;
                    }
                }
            }
            catch
            {
            }
         
            return defaultTotalNumberBytes;
        }

        /// <summary>
        /// Split white spaces and get the first value number. Example: "31 lines (26 sloc)" | Get 31
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        protected long GetFirstNumberOfTerm(string term)
        {
            try
            {
                return long.Parse(term.Trim().Split(' ')[0]);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Find file name on final path of breadcrumb
        /// </summary>
        /// <returns></returns>
        public string GetFileNameOnFileContent()
        {
            var fileName = this.Document.All.FirstOrDefault(GitHubFilter.fileNameOnContent);
            return fileName?.Text();
        }

        /// <summary>
        /// Find the list of folders/files to navigate
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GitHubLinkAccess> GetFolderListItens()
        {
            var itens = new List<GitHubLinkAccess>();
            var folderListElements = this.Document.All.Where(GitHubFilter.folderLists);

            foreach (var folderListElement in folderListElements)
            {
                var grid = folderListElement.QuerySelector("div[role='grid']");
                if (grid == default) continue;

                var rows = grid.QuerySelectorAll("div.Box-row");

                foreach (var row in rows)
                {
                    var isDirectory = row.QuerySelector("svg.octicon-file-directory");
                    var item = new GitHubLinkAccess();

                    if (isDirectory != default)
                    {
                        item.Type = GitHubLinkAccess.GitHubLinkAccessType.Folder;
                    }
                    else
                    {
                        var isFile = row.QuerySelector("svg.octicon-file") ?? row.QuerySelector("svg.octicon-file-symlink-file");
                        if (isFile != default) item.Type = GitHubLinkAccess.GitHubLinkAccessType.File;
                        else continue;
                    }

                    var aHref = row.QuerySelector("a.js-navigation-open");
                    if (aHref == default) continue;

                    item.Endpoint = aHref.GetAttribute("href");
                    itens.Add(item);
                }
            }

            return itens;
        }
    }
}
