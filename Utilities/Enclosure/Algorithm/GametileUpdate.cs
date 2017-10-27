namespace Plus.Utilities.Enclosure.Algorithm
{
    internal class GametileUpdate
    {
        internal GametileUpdate(int x, int y, byte value)
        {
            X = x;
            Y = y;
            Value = value;
        }

        internal byte Value { get; }
        internal int Y { get; }
        internal int X { get; }
    }
}