namespace RedSocial.Application.Common;

public class Resultado<T>
{
    public bool Exito { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public T? Datos { get; init; }

    public static Resultado<T> Ok(T datos, string mensaje = "") => new() { Exito = true, Datos = datos, Mensaje = mensaje };
    public static Resultado<T> Error(string mensaje) => new() { Exito = false, Mensaje = mensaje };
}
