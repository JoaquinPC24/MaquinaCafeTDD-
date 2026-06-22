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

