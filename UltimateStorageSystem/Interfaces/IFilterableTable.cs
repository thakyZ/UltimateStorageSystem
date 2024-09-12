namespace UltimateStorageSystem.Interfaces;
public interface IFilterableTable
{
    void FilterItems(string searchText);
    void SortItemsBy(string sortBy, bool ascending);
    int  ScrollIndex { get; set; }
}