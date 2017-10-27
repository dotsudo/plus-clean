namespace Plus.HabboHotel.Rooms.PathFinding
{
    using System;

    public sealed class PathFinderNode : IComparable<PathFinderNode>
    {
        public int Cost = int.MaxValue;
        public bool InClosed = false;
        public bool InOpen = false;
        public PathFinderNode Next;
        public Vector2D Position;

        public PathFinderNode(Vector2D Position) => this.Position = Position;

        public int CompareTo(PathFinderNode Other) => Cost.CompareTo(Other.Cost);

        public override bool Equals(object obj) => obj is PathFinderNode && ((PathFinderNode) obj).Position.Equals(Position);

        public bool Equals(PathFinderNode Breadcrumb) => Breadcrumb.Position.Equals(Position);

        public override int GetHashCode() => Position.GetHashCode();
    }
}