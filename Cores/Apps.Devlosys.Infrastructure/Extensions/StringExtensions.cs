namespace System
{
    public static class StringExtensions
    {
        public static string Between(this string str, string firstString, string lastString)
        {
            try
            {
                int pos1 = str.IndexOf(firstString) + firstString.Length;
                int pos2 = str.Substring(pos1).IndexOf(lastString);

                return str.Substring(pos1, pos2);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
