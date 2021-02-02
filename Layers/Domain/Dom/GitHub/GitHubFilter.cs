using AngleSharp.Dom;
using System;

namespace Domain.Dom.GitHub
{
    /// <summary>
    /// Static filters for GitHub elements
    /// </summary>
    public static class GitHubFilter
    {
        public static Func<IElement, bool> fileContents = (e) => e.LocalName == "div" &&
            e.GetAttribute("itemprop") == "text" &&
            e.ClassList.Contains("blob-wrapper") &&
            e.ClassList.Contains("data");

        public static Func<IElement, bool> readmeContent = (e) => e.LocalName == "div" &&
            e.Id.ContainsInvariant("readme") &&
            e.ClassList.Contains("Box-body") &&
            e.ClassList.Contains("readme");

        public static Func<IElement, bool> folderLists = (e) => e.LocalName == "div" && 
            e.ClassList.Contains("js-details-container") &&
            e.ClassList.Contains("Details");

        public static Func<IElement, bool> gridFolderList = (e) => e.LocalName == "div" &&
            e.GetAttribute("role") == "grid" &&
            e.GetAttribute("aria-labelledby") == "files";

        public static Func<IElement, bool> boxHeader = (e) => e.LocalName == "div" && e.ClassList.Contains("Box-header");

        public static Func<IElement, bool> fileNameOnContent = (e) => e.LocalName == "strong" && e.ClassList.Contains("final-path");
    }
}
