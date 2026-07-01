namespace MaquinaCafe;

using System;
using System.Collections.Generic;

/// <summary>
/// Implementación de la cafetera usada por los tests.
/// Mantiene la API pública esperada por la suite de pruebas.
/// </summary>
public class Cafetera
{
    // Precios por bebida (puede ampliarse o inyectarse en el futuro)
    private readonly Dictionary<string, int> _precios = new()
    {
        { "Cafe", 75 }
    };

    private int _saldo = 0;
    private readonly Dictionary<string, int> _inventario = new();
    private bool _exactChangeRequired = false;
    private bool _modoMantenimiento = false;
    private int _cambioDisponible = 0;

    public int Saldo => _saldo;

    /// <summary>Inserta una moneda; lanza si está en modo mantenimiento.</summary>
    public void InsertarMoneda(int monto)
    {
        VerificarModoMantenimiento();
        if (monto <= 0) return;
        _saldo += monto;
    }

    /// <summary>Agrega o establece la cantidad de inventario para una bebida.</summary>
    public void AgregarInventario(string nombre, int cantidad)
    {
        if (!NombreValido(nombre)) return;
        _inventario[nombre] = Math.Max(0, cantidad);
    }

    /// <summary>Devuelve la cantidad en inventario para una bebida.</summary>
    public int Inventario(string nombre)
    {
        if (!NombreValido(nombre)) return 0;
        return _inventario.TryGetValue(nombre, out var c) ? c : 0;
    }

    /// <summary>Cancela la operación y devuelve todo el saldo; lanza si modo mantenimiento.</summary>
    public int Cancelar()
    {
        VerificarModoMantenimiento();
        int reembolso = _saldo;
        _saldo = 0;
        return reembolso;
    }

    /// <summary>Configura si se requiere exact change.</summary>
    public void ConfigurarExactChangeRequired(bool requerido)
    {
        _exactChangeRequired = requerido;
    }

    /// <summary>Activa o desactiva el modo mantenimiento.</summary>
    public void SetModoMantenimiento(bool activo)
    {
        _modoMantenimiento = activo;
    }

    /// <summary>Configura la cantidad de cambio disponible en la máquina.</summary>
    public void ConfigurarCambioDisponible(int cantidad)
    {
        _cambioDisponible = Math.Max(0, cantidad);
    }

    /// <summary>
    /// Intenta dispensar una bebida. Devuelve true si se dispensó correctamente.
    /// Comportamiento: no modifica saldo si inventario agotado o saldo insuficiente.
    /// Reembolsa (pone saldo a 0) en exact change o falta de cambio.
    /// </summary>
    public bool SeleccionarBebida(string nombre)
    {
        VerificarModoMantenimiento();
        if (!NombreValido(nombre)) return false;

        if (!TryObtenerPrecio(nombre, out var precio)) return false;

        var stock = Inventario(nombre);
        if (stock <= 0) return false;

        if (_saldo < precio) return false;

        int cambioNecesario = _saldo - precio;

        if (_exactChangeRequired && cambioNecesario > 0)
        {
            ReembolsarTotal();
            return false;
        }

        if (cambioNecesario > 0 && _cambioDisponible < cambioNecesario)
        {
            ReembolsarTotal();
            return false;
        }

        // Dispensa
        _inventario[nombre] = stock - 1;
        if (cambioNecesario > 0) _cambioDisponible -= cambioNecesario;
        _saldo = 0;
        return true;
    }

    // -----------------------
    // Métodos privados auxiliares
    // -----------------------

    private void VerificarModoMantenimiento()
    {
        if (_modoMantenimiento)
            throw new InvalidOperationException("Máquina en modo mantenimiento.");
    }

    private bool NombreValido(string nombre) => !string.IsNullOrEmpty(nombre);

    private bool TryObtenerPrecio(string nombre, out int precio) =>
        _precios.TryGetValue(nombre, out precio);

    private void ReembolsarTotal()
    {
        _saldo = 0;
    }
}
