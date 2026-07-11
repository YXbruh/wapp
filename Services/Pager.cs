using System;

namespace CSA.Services
{
    [Serializable]
    public class Pager
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int Total { get; set; }

        public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
        public int Offset => Math.Max(0, (Page - 1) * PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
        public string Summary => $"Page {Page} of {Math.Max(1, TotalPages)} ({Total} total)";
    }
}
