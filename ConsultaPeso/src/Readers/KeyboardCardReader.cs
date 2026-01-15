namespace ConsultaPeso.Readers;

using System.Text;

public class KeyboardCardReader : ICardReader
{
    public event EventHandler<string>? CardRead;

    private readonly StringBuilder _buffer = new();
    private DateTime _lastKeyTime;

    // Ajustable según lector (ms)
    private const int MaxDelayBetweenKeys = 80;
    
    private string? _lastCode;
    private DateTime _lastRead;
    public void ProcessKey(char keyChar)
    {
        var now = DateTime.Now;

        // Si hay mucha pausa, asumimos nueva lectura
        if ((now - _lastKeyTime).TotalMilliseconds > MaxDelayBetweenKeys)
        {
            _buffer.Clear();
        }

        _lastKeyTime = now;

        // ENTER = fin de lectura
        if (keyChar == '\r')
        {
            if (_buffer.Length > 0)
            {
                CardRead?.Invoke(this, _buffer.ToString());
                _buffer.Clear();
            }

            return;
        }

        // Ignorar caracteres de control raros
        if (char.IsControl(keyChar))
            return;

        if (keyChar == '\r')
        {
            var code = _buffer.ToString();

            if (code == _lastCode &&
                (DateTime.Now - _lastRead).TotalMilliseconds < 500)
                return;

            _lastCode = code;
            _lastRead = DateTime.Now;

            CardRead?.Invoke(this, code);
            _buffer.Clear();
        }
        
        _buffer.Append(keyChar);
    }

    public void Reset()
    {
        _buffer.Clear();
        _lastKeyTime = DateTime.MinValue;
    }
}
