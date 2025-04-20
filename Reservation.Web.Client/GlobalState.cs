using System.Globalization;
using Reservation.Web.Client.CustomExtensions;

namespace Reservation.Web.Client;

public class GlobalState
{
    public event Action? OnStateChanged;
    
    private TimeZoneInfo _currentTimeZone = TimeZoneInfo.Local;
    private DateTime? _selectedDate = DateTime.Now;
    private DateTime? _selectedDateAdmin = DateTime.Now;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _identifier = string.Empty;

    public TimeZoneInfo CurrentTimeZone
    {
        get => _currentTimeZone;
        set
        {
            _currentTimeZone = value;
            NotifyStateChanged();
        }
    }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            _selectedDate = value;
            NotifyStateChanged();
        }
    }

    public DateTime? SelectedDateAdmin
    {
        get => _selectedDateAdmin;
        set
        {
            _selectedDateAdmin = value;
            NotifyStateChanged();
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            NotifyStateChanged();
        }
    }
    
    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            NotifyStateChanged();
        }
    }
    
    public string Identifier
    {
        get => _identifier;
        set
        {
            _identifier = value;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}