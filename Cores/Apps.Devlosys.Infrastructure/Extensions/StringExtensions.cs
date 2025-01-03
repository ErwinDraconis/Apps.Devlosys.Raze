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


        public static string SafeExtractBetween(this string input, string start, string end)
        {
            try
            {
                int startIndex = input.IndexOf(start);
                if (startIndex == -1) return null;

                startIndex += start.Length; 
                int endIndex = input.IndexOf(end, startIndex);
                if (endIndex == -1) return null;

                return input.Substring(startIndex, endIndex - startIndex);
            }
            catch
            {
                return null;
            }
        }
    }
}