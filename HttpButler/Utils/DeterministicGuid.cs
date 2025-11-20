using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace HttpButler.Utils;

internal static class DeterministicGuid
{
    // El namespace pre-calculado en formato Big-Endian (RFC 4122)
    private static readonly byte[] NamespaceBytes;

    static DeterministicGuid()
    {
        var source = new Guid("9a692101-09fa-41ce-8c0f-97d36b915581");
        NamespaceBytes = source.ToByteArray();

        // Convertir a Big-Endian
        SwapByteOrder(NamespaceBytes);
    }

    public static Guid FromString(string input)
    {
        // Calcular tamaño necesario: 16 bytes (Namespace) + longitud del string UTF8
        int inputByteCount = Encoding.UTF8.GetByteCount(input);
        int totalLength = 16 + inputByteCount;

        // Usar stackalloc para velocidad si es pequeño (común en GUIDs), o ArrayPool si es enorme.
        // 256 bytes es suficiente para la mayoría de IDs. Si es mayor, alquilamos memoria.
        byte[]? rentedArray = null;
        Span<byte> buffer = totalLength <= 256
            ? stackalloc byte[totalLength]
            : (rentedArray = ArrayPool<byte>.Shared.Rent(totalLength));

        try
        {
            // Copiar Namespace (ya está en Big-Endian) al inicio del buffer
            new ReadOnlySpan<byte>(NamespaceBytes).CopyTo(buffer);

            // Escribir el string input directamente después del namespace
            Encoding.UTF8.GetBytes(input, buffer[16..]);

            // Calcular SHA1 directamente en el stack (SHA1 produce 20 bytes)
            Span<byte> hashBuffer = stackalloc byte[20];
            SHA1.HashData(buffer.Slice(0, totalLength), hashBuffer);

            // Modificar bits para versión y variante (sobre el hashBuffer directamente)
            // Byte 6: Versión 5
            hashBuffer[6] = (byte)((hashBuffer[6] & 0x0F) | (5 << 4));
            // Byte 8: Variante RFC 4122
            hashBuffer[8] = (byte)((hashBuffer[8] & 0x3F) | 0x80);

            // Convertir de Big-Endian a Little-Endian para el formato interno de .NET
            // Solo necesitamos los primeros 16 bytes del hash
            Span<byte> guidBytes = hashBuffer[..16];
            SwapByteOrder(guidBytes);

            return new Guid(guidBytes);
        }
        finally
        {
            // Devolver el array al pool.
            if (rentedArray is not null)
                ArrayPool<byte>.Shared.Return(rentedArray);
        }
    }

    // Optimizado para trabajar con Span<byte> in-place
    private static void SwapByteOrder(Span<byte> guid)
    {
        // Swap de los primeros 4 bytes (int)
        (guid[0], guid[3]) = (guid[3], guid[0]);
        (guid[1], guid[2]) = (guid[2], guid[1]);

        // Swap de los siguientes 2 bytes (short)
        (guid[4], guid[5]) = (guid[5], guid[4]);

        // Swap de los siguientes 2 bytes (short)
        (guid[6], guid[7]) = (guid[7], guid[6]);
    }
}
