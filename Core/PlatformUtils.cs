namespace RFD.Core;

public static class PlatformUtils
{
    public static readonly bool IsWindows = OperatingSystem.IsWindows();
    public static readonly bool IsMacOS = OperatingSystem.IsMacOS();
    public static readonly bool IsLinux = OperatingSystem.IsLinux();
}