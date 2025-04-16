using System.Net;

namespace RFD.Core;

public class IpAddressValidator
{
    public static bool IsValidIPv4(string ipAddress)
    {
        if (IPAddress.TryParse(ipAddress, out var address))
        {
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return false;

            // Проверка, что это не "0.0.0.0"
            if (address.GetAddressBytes().All(b => b == 0))
                return false;

            // Проверка, что это не multicast "224.0.0.0" - "239.255.255.255"
            var firstByte = address.GetAddressBytes()[0];
            if (firstByte >= 224 && firstByte <= 239)
                return false;

            return true;
        }
        return false;
    }
}