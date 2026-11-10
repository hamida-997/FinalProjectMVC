using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FinalProjectMVC.Helpers
{
    // Describes the current page for the _Pagination partial
    public class PagerVM
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
        public int FirstItem => TotalItems == 0 ? 0 : (Page - 1) * PageSize + 1;
        public int LastItem => Math.Min(Page * PageSize, TotalItems);

        // Optional "#reviews"-style anchor added to page links
        public string? Fragment { get; set; }
    }

    public static class Paging
    {
        public const int DefaultPageSize = 10;
        public const string ViewDataKey = "Pager";

        // The API returns whole lists, so pages are cut here. Out-of-range page numbers are
        // clamped (e.g. after deleting the last item on the last page).
        public static List<T> Paginate<T>(this IEnumerable<T> source, int page, ViewDataDictionary viewData,
            int pageSize = DefaultPageSize, string? fragment = null)
        {
            var list = source as IList<T> ?? source.ToList();
            var pager = new PagerVM { PageSize = pageSize, TotalItems = list.Count, Fragment = fragment };
            pager.Page = Math.Clamp(page, 1, pager.TotalPages);

            viewData[ViewDataKey] = pager;
            return list.Skip((pager.Page - 1) * pageSize).Take(pageSize).ToList();
        }

        public static PagerVM? GetPager(this ViewDataDictionary viewData) => viewData[ViewDataKey] as PagerVM;
    }
}
