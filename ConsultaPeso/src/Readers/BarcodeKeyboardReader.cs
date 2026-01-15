using System.Text;

namespace ConsultaPeso.Readers;

public class BarcodeKeyboardReader : ICardReader
{
    public event EventHandler<string>? CardRead;

    private readonly StringBuilder _buffer = new();
    private DateTime _lastKeyTime;

    // El lector de barras escribe muy rápido
    private const int MaxDelayBetweenKeys = 60;

    // Anti-duplicado
    private string? _lastCode;
    private DateTime _lastReadTime;

    public void ProcessKey(char keyChar)
    {
        var now = DateTime.Now;

        // Si hubo pausa grande, se asume nueva lectura
        if ((now - _lastKeyTime).TotalMilliseconds > MaxDelayBetweenKeys)
        {
            _buffer.Clear();
        }

        _lastKeyTime = now;

        // Fin de lectura (ENTER o TAB)
        if (keyChar == '\r' || keyChar == '\t')
        {
            EmitirLectura();
            return;
        }

        // Ignorar caracteres de control
        if (char.IsControl(keyChar))
            return;

        _buffer.Append(keyChar);
    }

    private void EmitirLectura()
    {
        if (_buffer.Length == 0)
            return;

        var code = _buffer.ToString().Trim();

        // Evitar doble lectura inmediata
        if (code == _lastCode &&
            (DateTime.Now - _lastReadTime).TotalMilliseconds < 500)
        {
            _buffer.Clear();
            return;
        }

        _lastCode = code;
        _lastReadTime = DateTime.Now;

        CardRead?.Invoke(this, code);
        _buffer.Clear();
    }

    public void Reset()
    {
        _buffer.Clear();
        _lastKeyTime = DateTime.MinValue;
    }
}