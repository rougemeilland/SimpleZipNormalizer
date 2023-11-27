namespace Utility.Text
{
    public enum Base64EncodingType
    {
        Rfc4648Encoding = 0,
        Rfc2045Encoding = 1,
        Rfc4880Encoding = 2,

        Default = Rfc4648Encoding,
        MimeEncoding = Rfc2045Encoding,
        Radix64Encoding = Rfc4880Encoding,
    }
}
