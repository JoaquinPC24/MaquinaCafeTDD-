Red
namespace MaquinaCafe;

using System.Collections.Generic;


    public class Cafetera
    {
        private int _saldo = 0;
        private Dictionary<string, int> _inventario = new();

        public int Saldo => _saldo;

        public void InsertarMoneda(int monto)
        {
            // implementación mínima intencionalmente vacía para mantener tests en RED
        }

        public bool SeleccionarBebida(string nombre)
        {
            return true;
        }

        public void AgregarInventario(string nombre, int cantidad)
        {
            _inventario[nombre] = cantidad;
        }

        public int Inventario(string nombre)
        {
            return 0;
        }

        public int Cancelar()
        {
            int reembolso = 0;
            _saldo = 0;
            return reembolso;
        }

        public void ConfigurarExactChangeRequired(bool requerido) { }
        public void SetModoMantenimiento(bool activo) { }
        public void ConfigurarCambioDisponible(int cantidad) { }
    }

green
namespace MaquinaCafe;

using System;
using System.Collections.Generic;

public class Cafetera
{
    private readonly Dictionary<string, int> _precios = new()
    {
        { "Cafe", 75 }
    };

    private int _saldo = 0;
    private Dictionary<string, int> _inventario = new();
    private bool _exactChangeRequired = false;
    private bool _modoMantenimiento = false;
    private int _cambioDisponible = 0;

    public int Saldo => _saldo;

    public void InsertarMoneda(int monto)
    {
        if (_modoMantenimiento)
            throw new InvalidOperationException("Máquina en modo mantenimiento.");

        if (monto <= 0) return;
        _saldo += monto;
    }

    public void AgregarInventario(string nombre, int cantidad)
    {
        if (string.IsNullOrEmpty(nombre)) return;
        _inventario[nombre] = Math.Max(0, cantidad);
    }

    public int Inventario(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return 0;
        return _inventario.TryGetValue(nombre, out var c) ? c : 0;
    }

    public int Cancelar()
    {
        if (_modoMantenimiento)
            throw new InvalidOperationException("Máquina en modo mantenimiento.");

        int reembolso = _saldo;
        _saldo = 0;
        return reembolso;
    }

    public void ConfigurarExactChangeRequired(bool requerido)
    {
        _exactChangeRequired = requerido;
    }

    public void SetModoMantenimiento(bool activo)
    {
        _modoMantenimiento = activo;
    }

    public void ConfigurarCambioDisponible(int cantidad)
    {
        _cambioDisponible = Math.Max(0, cantidad);
    }

    public bool SeleccionarBebida(string nombre)
    {
        if (_modoMantenimiento)
            throw new InvalidOperationException("Máquina en modo mantenimiento.");

        if (string.IsNullOrEmpty(nombre)) return false;

        if (!_precios.TryGetValue(nombre, out var precio))
            return false;

        var stock = Inventario(nombre);
        if (stock <= 0)
            return false; // inventario agotado: no tocar saldo

        if (_saldo < precio)
            return false; // saldo insuficiente: no tocar saldo

        int cambioNecesario = _saldo - precio;

        // Exact change required -> rechazar y reembolsar todo
        if (_exactChangeRequired && cambioNecesario > 0)
        {
            _saldo = 0;
            return false;
        }

        // Falta de cambio disponible -> rechazar y reembolsar todo
        if (cambioNecesario > 0 && _cambioDisponible < cambioNecesario)
        {
            _saldo = 0;
            return false;
        }

        // Dispensa: decrementar inventario, ajustar cambio disponible y consumir saldo
        _inventario[nombre] = stock - 1;

        if (cambioNecesario > 0)
            _cambioDisponible -= cambioNecesario;

        _saldo = 0;
        return true;
    }
}
refactor
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
 definiciones

Proyecto MaquinaCafeTDD
Breve descripción del proyecto y propósito: implementación de una Cafetera con una suite de pruebas unitarias (NUnit) que valida el comportamiento esperado. El repositorio sigue un flujo TDD: RED → GREEN → REFACTOR y contiene los tests TC-01 a TC-08 que definen la especificación mínima de la máquina.

Qué es TDD
Test Driven Development es una práctica de desarrollo donde se escribe primero una prueba automatizada que falla, luego se implementa el código mínimo para que la prueba pase, y finalmente se refactoriza manteniendo las pruebas verdes.
Beneficios clave: especificación ejecutable, diseño guiado por pruebas, regresiones detectadas temprano y confianza para refactorizar.

Ciclo TDD aplicado
Pasos del ciclo aplicado en este proyecto

RED

Escribir o mantener tests que describen el comportamiento esperado. Ejecutar dotnet test y confirmar que al menos uno falla.

Commit: test(tc-all): keep all 8 tests in RED (initial failing tests)

GREEN

Implementar la mínima lógica necesaria en Cafetera para que todos los tests pasen. Ejecutar dotnet test y confirmar que todos pasan.

Commit: feat(tc-all): implement Cafetera to satisfy all 8 tests (GREEN)

REFACTOR

Limpiar y reorganizar el código sin cambiar el comportamiento observable. Ejecutar dotnet test para asegurar que las pruebas siguen verdes.

Commit: refactor(tc-all): extract helpers, document methods and improve readability

Este ciclo se repite para cada nueva especificación o test agregado.

Cómo ejecutar el proyecto y las pruebas
Requisitos

.NET SDK compatible instalado.

Git (opcional para flujo de commits).

Comandos básicos

bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar tests
dotnet test
Flujo recomendado local

bash
# Crear rama de trabajo
git checkout -b feat/tc-all

# Guardar RED (tests fallando)
git add MaquinaCafe.Tests/MaquinaCafeTests.cs
git commit -m "test(tc-all): keep all 8 tests in RED (initial failing tests)"

# Implementar y comprobar GREEN
# (editar MaquinaCafe/Cafetera.cs)
git add MaquinaCafe/Cafetera.cs
git commit -m "feat(tc-all): implement Cafetera to satisfy all 8 tests (GREEN)"
dotnet test

# Refactorizar y confirmar
git add MaquinaCafe/Cafetera.cs
git commit -m "refactor(tc-all): extract helpers, document methods and improve readability"
dotnet test
Resumen de casos de prueba TC-01 a TC-08
TC	Descripción	Comportamiento clave
TC-01	Insertar moneda acumula saldo	InsertarMoneda(25) → Saldo == 25
TC-02	Seleccionar sin saldo no dispensa	SeleccionarBebida("Cafe") con saldo 0 → devuelve false
TC-03	Seleccionar con saldo dispensa y reduce inventario	Con inventario > 0 y InsertarMoneda(100) → SeleccionarBebida("Cafe") devuelve true y Inventario decrementa en 1
TC-04	Cancelar devuelve cambio total	Insertar monedas y Cancelar() devuelve suma y deja Saldo == 0
TC-05	Exact change required rechaza si hay cambio	ConfigurarExactChangeRequired(true) + InsertarMoneda(100) → SeleccionarBebida devuelve false y Saldo == 0
TC-06	Inventario agotado no dispensa	Inventario 0 + InsertarMoneda(100) → SeleccionarBebida devuelve false y Saldo permanece
TC-07	Modo mantenimiento bloquea ventas	SetModoMantenimiento(true) → InsertarMoneda y SeleccionarBebida lanzan InvalidOperationException
TC-08	Falta de cambio parcial rechaza y reembolsa	ConfigurarCambioDisponible(0) + InsertarMoneda(100) → SeleccionarBebida devuelve false y Saldo == 0

