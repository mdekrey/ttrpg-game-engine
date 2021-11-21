using System;

namespace GameEngine
{
    public abstract record Either<T1, T2>
    {
        public sealed record Left(T1 Value) : Either<T1, T2>
        {
            public override TResult Fold<TResult>(Func<T1, TResult> left, Func<T2, TResult> right) => left(Value);

            public override bool IsLeft(out T1 left, out T2 right)
            {
                left = Value;
                right = default(T2)!;
                return true;
            }
        }

        public sealed record Right(T2 Value) : Either<T1, T2>
        {
            public override TResult Fold<TResult>(Func<T1, TResult> left, Func<T2, TResult> right) => right(Value);

            public override bool IsLeft(out T1 left, out T2 right)
            {
                left = default(T1)!;
                right = Value;
                return false;
            }
        }

        private Either() { }

        public abstract bool IsLeft(out T1 left, out T2 right);
        public abstract TResult Fold<TResult>(Func<T1, TResult> left, Func<T2, TResult> right);

        public static implicit operator Either<T1, T2>(T1 left) => new Left(left);
        public static implicit operator Either<T1, T2>(T2 right) => new Right(right);

    }
}
