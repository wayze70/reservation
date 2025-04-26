namespace Reservation.Web.Client;

public class GlobalState
{
    public event Action? OnStateChanged;
    public event Action? IdentifierChanged;
    
    private string _identifier = string.Empty;
    private TimeZoneInfo _currentTimeZone = TimeZoneInfo.Local;
    private DateTime? _selectedDate = DateTime.Today;
    private DateTime? _selectedDateAdmin = DateTime.Today;
    private string _title = string.Empty;
    private string _description = string.Empty;

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
            if (_identifier != value)
            {
                _identifier = value;
                IdentifierChanged?.Invoke();
            }
        }
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}