namespace ConsultaPeso.Readers;

public interface ICardReader
{
    event EventHandler<string> CardRead;
    void ProcessKey(char keyChar);
    void Reset();
}
