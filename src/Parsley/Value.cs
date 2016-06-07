namespace Parsley
{
    using System;

    public abstract class Value<T> : IEquatable<T> where T : Value<T>
    {
        protected abstract object[] ImmutableFields();

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap around.
            {
                int hash = 17;

                foreach (var field in ImmutableFields())
                    if (field != null)
                        hash = hash * 23 + field.GetHashCode();

                return hash;
            }
        }

        public static bool operator ==(Value<T> x, Value<T> y)
        {
            if (ReferenceEquals(x, null))
                return ReferenceEquals(y, null);

            return x.Equals(y);
        }

        public static bool operator !=(Value<T> x, Value<T> y)
        {
            return !(x == y);
        }

        public override bool Equals(object other)
        {
            return other != null && Equals(other as T);
        }

        public virtual bool Equals(T other)
        {
            if (other == null)
                return false;

            var fields = ImmutableFields();
            var otherFields = other.ImmutableFields();

            for (int i = 0; i < fields.Length; i++)
            {
                object value1 = fields[i];
                object value2 = otherFields[i];

                if (value1 == null)
                {
                    if (value2 != null)
                        return false;
                }
                else if (value2 == null)
                    return false;
                else if (!value1.Equals(value2))
                    return false;
            }

            return true;
        }
    }
}
