using System.Collections;
using System.Collections.Specialized;
using Terminal.Gui;

namespace FTP_Client.Windows.Content;

public class FileList : IListDataSource
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
    public bool IsMarked(int item)
    {
        throw new NotImplementedException();
    }
    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width,
        int start = 0)
    {
        throw new NotImplementedException();
    }
    public void SetMark(int item, bool value)
    {
        throw new NotImplementedException();
    }
    public IList ToList()
    {
        throw new NotImplementedException();
    }
    public int Count { get; }
    public int Length { get; }
    public bool SuspendCollectionChangedEvent { get; set; }
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
}