namespace ce_toy_fx
{
    public class Unit
    {
        private Unit() { }

        public static Unit Value { get; } = new Unit();

        public override bool Equals(object obj)
        {
            return obj is Unit;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "()";
        }
    }

    public class FailUnit
    {
        private FailUnit() { }

        public static FailUnit Value { get; } = new FailUnit();

        public override bool Equals(object obj)
        {
            return obj is FailUnit;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "(<fail>)";
        }
    }

    public class PassUnit
    {
        private PassUnit() { }

        public static PassUnit Value { get; } = new PassUnit();

        public override bool Equals(object obj)
        {
            return obj is PassUnit;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "(<pass>)";
        }
    }
}
