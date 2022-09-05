public class User
{
    private string _name;
    private bool _isNameSet;

    public bool IsNameSet => _isNameSet;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            _isNameSet = true;
        }
    }
}
