namespace Plus.Communication.RCON.Commands
{
    public interface IRconCommand
    {
        string Parameters { get; }
        string Description { get; }
        bool TryExecute(string[] parameters);
    }
}