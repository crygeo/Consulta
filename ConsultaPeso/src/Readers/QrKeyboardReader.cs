using System.Text;

namespace ConsultaPeso.Readers;

public class QrKeyboardReader : ICardReader
{
    public event EventHandler<string>? CardRead;

    private readonly StringBuilder _buffer = new();
    private DateTime _lastKeyTime;

    // El QR suele llegar MUY rápido
    private const int MaxDelayBetweenKeys = 40;

    // Protección contra lecturas duplicadas
    private string? _lastValue;
    private DateTime _lastReadTime;

    public void ProcessKey(char keyChar)
    {
        var now = DateTime.Now;

        // Si hubo pausa, empezamos lectura nueva
        if ((now - _lastKeyTime).TotalMilliseconds > MaxDelayBetweenKeys)
        {
            _buffer.Clear();
        }

        _lastKeyTime = now;

        // Fin de lectura (ENTER o TAB)
        if (keyChar == '\r' || keyChar == '\n' || keyChar == '\t')
        {
            EmitirLectura();
            return;
        }

        // Ignorar controles
        if (char.IsControl(keyChar))
            return;

        _buffer.Append(keyChar);
    }

    private void EmitirLectura()
    {
        if (_buffer.Length == 0)
            return;

        var value = _buffer.ToString().Trim();

        // Evitar duplicados muy seguidos
        if (value == _lastValue &&
            (DateTime.Now - _lastReadTime).TotalMilliseconds < 500)
        {
            _buffer.Clear();
            return;
        }

        _lastValue = value;
        _lastReadTime = DateTime.Now;

        CardRead?.Invoke(this, value);
        _buffer.Clear();
    }

    public void Reset()
    {
        _buffer.Clear();
        _lastKeyTime = DateTime.MinValue;
    }
}