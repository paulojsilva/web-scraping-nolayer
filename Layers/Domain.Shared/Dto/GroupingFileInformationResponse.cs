using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Domain.Shared.Dto
{
    public class GroupingFileInformationResponse
    {
        public GroupingFileInformationResponse(IGrouping<string, ItemFileInformationResponse> grouping)
        {
            this.Extension = string.IsNullOrWhiteSpace(grouping.Key) ? "WITHOUT_EXTENSION" : grouping.Key;
            this.AddFiles(grouping);
        }

        public string Extension { get; protected set; }
        public long TotalNumberLines => Details.Sum(f => f.Lines);
        public double TotalNumberBytes => Details.Sum(f => f.Bytes);
        public int TotalNumberFiles => Details.Count;
        public List<ItemFileInformationResponse> Details { get; protected set; } = new List<ItemFileInformationResponse>();

        public void AddFile(ItemFileInformationResponse file) => this.Details.Add(file);
        public void AddFile(string fileName, long lines, double bytes) => AddFile(new ItemFileInformationResponse(fileName, lines, bytes));
        public void AddFiles(IGrouping<string, ItemFileInformationResponse> files) => this.Details.AddRange(files);
    }

    public class ItemFileInformationResponse
    {
        public ItemFileInformationResponse(string fileName)
        {
            this.FileName = fileName;
        }

        public ItemFileInformationResponse(string fileName, long lines, double bytes) : this(fileName)
        {
            this.Lines = lines;
            this.Bytes = bytes;
        }

        public string FileName { get; set; }
        public long Lines { get; set; }
        public double Bytes { get; set; }

        public string Extension() => Path.GetExtension(this.FileName);
    }
}