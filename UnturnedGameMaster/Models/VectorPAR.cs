namespace UnturnedGameMaster.Models
{
    public struct VectorPAR
    {
        public Vector3S Position { get; set; }
        public byte Rotation { get; set; }

        public VectorPAR(Vector3S position, byte rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public override string ToString()
        {
            return $"{Position}, R: {Rotation}";
        }
    }
}
