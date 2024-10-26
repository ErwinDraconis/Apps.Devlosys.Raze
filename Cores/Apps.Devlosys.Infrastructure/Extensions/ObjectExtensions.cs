namespace System
{
    public static class ObjectExtensions
    {
        public static bool HasProperty(this object @this, string name)
        {
            var names = name.Split('.');
            Type type = @this.GetType();

            if (names.Length == 1)
            {
                return type.GetProperty(name) != null;
            }
            else if (names.Length > 1)
            {
                var child = type.GetProperty(names[0]).GetValue(@this);
                return child.HasProperty(names[1]);
            }
            else
            {
                return false;
            }
        }

        public static T GetProperty<T>(this object @this, string name)
        {
            var names = name.Split('.');
            Type type = @this.GetType();

            if (names.Length == 1)
            {
                return (T)type.GetProperty(name).GetValue(@this);
            }
            else if (names.Length > 1)
            {
                var child = type.GetProperty(names[0]).GetValue(@this);
                return child.GetProperty<T>(names[1]);
            }
            else
            {
                return default(T);
            }
        }

        public static T CastTo<T>(this object @this)
        {
            return typeof(T).IsValueType && @this != null
                ? (T)Convert.ChangeType(@this, typeof(T))
                : @this is T typeValue ? typeValue : default;
        }

    }
}
