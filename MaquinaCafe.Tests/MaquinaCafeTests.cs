using NUnit.Framework;
using MaquinaCafe;
using System;


namespace MaquinaCafe.Tests
{
    [TestFixture]
    public class MaquinaCafeTests
    {
        private Cafetera _maquina;


        [SetUp]
        public void Inicializar()
        {
            _maquina = new Cafetera();
        }

            // TC-01
            [Test]
        public void TC01_InsertarMoneda_DebeAcumularSaldo()
        {
            _maquina.InsertarMoneda(25);

            Assert.AreEqual(25, _maquina.Saldo,
                "Al insertar 25, el saldo debe ser 25.");
        }

        // TC-02
        [Test]
        public void TC02_SeleccionarBebida_SinSaldo_NoDispensa()
        {
            bool dispensada = _maquina.SeleccionarBebida("Cafe");

            Assert.IsFalse(dispensada,
                "No debe dispensar cuando el saldo es insuficiente.");
        }

        // TC-03
        [Test]
        public void TC03_SeleccionarBebida_ConSaldo_DispensaYReduceInventario()
        {
            _maquina.AgregarInventario("Cafe", 5);
            _maquina.InsertarMoneda(75);

            int antes = _maquina.Inventario("Cafe");

            bool dispensada = _maquina.SeleccionarBebida("Cafe");

            int despues = _maquina.Inventario("Cafe");

            Assert.IsTrue(dispensada,
                "Debe dispensar cuando hay saldo suficiente.");

            Assert.AreEqual(antes - 1, despues,
                "Inventario debe disminuir en 1 tras dispensar.");
        }

        // TC-04
        [Test]
        public void TC04_Cancelar_DevuelveCambioTotal()
        {
            _maquina.InsertarMoneda(50);
            _maquina.InsertarMoneda(25);

            int cambio = _maquina.Cancelar();

            Assert.AreEqual(75, cambio,
                "Cancelar debe devolver la suma total insertada.");

            Assert.AreEqual(0, _maquina.Saldo,
                "Saldo debe quedar en 0 tras cancelar.");
        }

        // TC-05
        [Test]
        public void TC05_ExactChangeRequired_NoDevuelveCambio_SiNoHayExacto()
        {
            _maquina.ConfigurarExactChangeRequired(true);
            _maquina.InsertarMoneda(100);
            _maquina.AgregarInventario("Cafe", 1);

            bool dispensada = _maquina.SeleccionarBebida("Cafe");
            int saldoFinal = _maquina.Saldo;

            Assert.IsFalse(dispensada);

            Assert.AreEqual(0, saldoFinal);
        }

        // TC-06
        [Test]
        public void TC06_InventarioAgotado_NoDispensa()
        {
            _maquina.AgregarInventario("Cafe", 0);
            _maquina.InsertarMoneda(100);

            bool dispensada = _maquina.SeleccionarBebida("Cafe");

            Assert.IsFalse(dispensada);

            Assert.AreEqual(100, _maquina.Saldo);
        }

        // TC-07
        [Test]
        public void TC07_ModoMantenimiento_BloqueaVentas()
        {
            _maquina.SetModoMantenimiento(true);

            Assert.Throws<InvalidOperationException>(
                () => _maquina.InsertarMoneda(25));

            Assert.Throws<InvalidOperationException>(
                () => _maquina.SeleccionarBebida("Cafe"));
        }

        // TC-08
        [Test]
        public void TC08_FalloPorFaltaDeCambioParcial_RechazaCompraYReembolsa()
        {
            _maquina.AgregarInventario("Cafe", 1);
            _maquina.ConfigurarCambioDisponible(0);
            _maquina.InsertarMoneda(100);

            bool dispensada = _maquina.SeleccionarBebida("Cafe");
            int saldoFinal = _maquina.Saldo;

            Assert.IsFalse(dispensada);

            Assert.AreEqual(0, saldoFinal);
        }
    }
}