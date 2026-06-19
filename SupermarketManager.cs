using System;
using System.Collections.Generic;
using UnityEngine;

namespace SupermarketEmpire
{
    // ==========================================
    // 1. ESTRUCTURAS DE DATOS Y ENUMS GLOBALES
    // ==========================================
    public enum TipoCliente { Cazaofertas, Gourmet, Ansioso, Ladron }
    public enum EstadoCliente { Comprando, EnCola, Pagando, Fugándose, Saliendo }

    [System.Serializable]
    public class DatosProducto
    {
        public string nombre;
        public float costeBase;
        public float precioVenta;
        public float elasticidad; // 0 = Necesidad (sal), 1 = Capricho (chocolatinas)
        public int diasParaCaducar;
        public bool requiereFrio;
    }

    // ==========================================
    // 2. EL CEREBRO DEL CLIENTE (MÓDULO 1 y 4)
    // ==========================================
    public class CustomerAgent : MonoBehaviour
    {
        public TipoCliente tipo;
        public EstadoCliente estadoActual = EstadoCliente.Comprando;
        
        public float presupuesto;
        public float paciencia = 100f;
        public float nivelHambre = 1.0f; // Multiplicador de impulsos
        
        public List<string> listaCompra = new List<string>();
        public List<string> carrito = new List<string>();
        
        private Vector3 destinoActual;
        private bool haRobadoAlgo = false;

        public void InicializarCliente(TipoCliente tipoGenerado, float dinero)
        {
            tipo = tipoGenerado;
            presupuesto = dinero;
            if (tipo == TipoCliente.Ansioso) paciencia = 40f; // Se enfadan rápido
            if (tipo == TipoCliente.Ladron) haRobadoAlgo = true; // Mentalidad de hurto
        }

        void Update()
        {
            // Sistema de degradación de paciencia si está esperando
            if (estadoActual == EstadoCliente.EnCola)
            {
                paciencia -= Time.deltaTime * (tipo == TipoCliente.Ansioso ? 3.0f : 1.0f);
                if (paciencia <= 0)
                {
                    AbandonoYCaos();
                }
            }

            // Simulación del "Efecto Aromático" de la Panadería
            DetectarOlorPanaderia();
        }

        private void DetectarOlorPanaderia()
        {
            // Simulación de colisión con partículas de olor invisibles
            float distanciaAPanaderia = Vector3.Distance(transform.position, new Vector3(10, 0, 10)); // Coordenadas del horno
            if (distanciaAPanaderia < 5.0f && nivelHambre == 1.0f)
            {
                nivelHambre = 2.5f; // El hambre se dispara por el olor
                if (!listaCompra.Contains("Donuts de Glaseado Real"))
                {
                    listaCompra.Add("Donuts de Glaseado Real"); // Compra por impulso
                    Debug.Log($"[IA] Cliente {tipo} olió pan dulce. Añadido capricho a la lista.");
                }
            }
        }

        private void AbandonoYCaos()
        {
            estadoActual = EstadoCliente.Saliendo;
            Debug.LogWarning($"[IA] ¡Cliente {tipo} se ha hartado de esperar! Tira el carro al suelo.");
            // Al tirar el carro, se genera una pérdida de orden/limpieza en la tienda
            SupermarketManager.Instance.RegistrarMerma(15.0f, "Rotura por cliente enfadado");
            Destroy(gameObject);
        }

        public void IntentarSalirPorPuerta()
        {
            if (tipo == TipoCliente.Ladron && haRobadoAlgo)
            {
                estadoActual = EstadoCliente.Fugándose;
                SupermarketManager.Instance.SistemaSeguridad.ActivarAlarmaArco(this);
            }
            else
            {
                estadoActual = EstadoCliente.Saliendo;
                Destroy(gameObject);
            }
        }
    }

    // ==========================================
    // 3. MINIJUEGO DE CORTE Y FRESCOS (MÓDULO 2)
    // ==========================================
    public class SeccionFrescos : MonoBehaviour
    {
        public float SimularCorteCarne(float pesoObjetivo, float precisionJugador)
        {
            // precisionJugador es un valor de 0.0 a 1.0 basado en el pulso del ratón en el minijuego
            float variacion = (1.0f - precisionJugador) * (pesoObjetivo * 0.3f); 
            float pesoFinal = pesoObjetivo + UnityEngine.Random.Range(-variacion, variacion);

            float diferencia = Mathf.Abs(pesoObjetivo - pesoFinal);

            if (diferencia <= pesoObjetivo * 0.03f)
            {
                Debug.Log($"[Frescos] ¡Corte Perfecto! {pesoFinal}g para un objetivo de {pesoObjetivo}g. +20% Propina.");
                return pesoFinal;
            }
            else if (pesoFinal > pesoObjetivo)
            {
                float exceso = pesoFinal - pesoObjetivo;
                Debug.LogWarning($"[Frescos] Te has pasado por {exceso}g. El exceso va directo a mermas orgánicas.");
                SupermarketManager.Instance.RegistrarMerma(exceso * 0.02f, "Exceso de corte en carnicería");
                return pesoObjetivo;
            }
            
            Debug.Log($"[Frescos] Corte aceptable de {pesoFinal}g.");
            return pesoFinal;
        }
    }

    // ==========================================
    // 4. SEGURIDAD Y VIGILANCIA (MÓDULO 4)
    // ==========================================
    public class SistemaSeguridad
    {
        public bool arcosActivos = true;

        public void ActivarAlarmaArco(CustomerAgent sospechoso)
        {
            if (!arcosActivos) return;

            Debug.LogError("[SEGURIDAD] ¡BEEP BEEP! El arco detector ha pitado. Sospechoso intentando huir.");
            
            // Simulación de probabilidad de intercepción del guardia IA
            float habilidadGuardia = 0.85f; // 85% de efectividad contratada
            if (UnityEngine.Random.value <= habilidadGuardia)
            {
                InterstellarInterrogatorio(sospechoso);
            }
            else
            {
                Debug.LogError("[SEGURIDAD] El ladrón esquivó al guardia. Pérdida total de los productos hurtados.");
                SupermarketManager.Instance.RegistrarMerma(85.50f, "Hurto con éxito (Fuga)");
                UnityEngine.Object.Destroy(sospechoso.gameObject);
            }
        }

        private void InterstellarInterrogatorio(CustomerAgent ladron)
        {
            Debug.Log("[SEGURIDAD] Ladrón interceptado. Recuperados $85.50 en productos y aplicada multa de $200.");
            SupermarketManager.Instance.InyectarLiquidezInmediata(200.0f); // Multa administrativa in-game
            UnityEngine.Object.Destroy(ladron.gameObject);
        }
    }

    // ==========================================
    // 5. MANAGER CENTRAL Y FINANZAS (MÓDULO 3)
    // ==========================================
    public class SupermarketManager : MonoBehaviour
    {
        public static SupermarketManager Instance { get; private set; }

        // Variables Económicas Diarias
        private float ingresosBrutos = 0f;
        private float costesAdquisicion = 0f;
        private float costesSuministros = 420f; // Luz, refrigeración fija
        private float totalMermas = 0f;
        private List<string> historialMermas = new List<string>();

        public SistemaSeguridad SistemaSeguridad;
        public SeccionFrescos SeccionFrescos;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            SistemaSeguridad = new SistemaSeguridad();
            SeccionFrescos = gameObject.AddComponent<SeccionFrescos>();
        }

        public void RegistrarVenta(float cantidad)
        {
            ingresosBrutos += cantidad;
        }

        public void RegistrarGastoStock(float cantidad)
        {
            costesAdquisicion += cantidad;
        }

        public void RegistrarMerma(float costePerdido, string motivo)
        {
            totalMermas += costePerdido;
            historialMermas.Add($"- ${costePerdido:F2}: {motivo}");
        }

        public void InyectarLiquidezInmediata(float cantidad)
        {
            ingresosBrutos += cantidad;
        }

        // Trigger del fin de la jornada (Módulo Financiero Completo)
        public void CerrarCajaYGenerarBalance()
        {
            float impuestoRetail = ingresosBrutos * 0.15f;
            float beneficioNeto = ingresosBrutos - (costesAdquisicion + costesSuministros + totalMermas + impuestoRetail);

            Console.Clear(); // Simulación de render en consola de desarrollo
            Debug.Log("============================================================");
            Debug.Log($"           BALANCE DE SITUACIÓN - FIN DE LA JORNADA         ");
            Debug.Log("============================================================");
            Debug.Log($"(+) INGRESOS BRUTOS DE CAJA:               ${ingresosBrutos:F2}");
            Debug.Log($"(-) COSTE DE ADQUISICIÓN MAYORISTA (B2B):  $-{costesAdquisicion:F2}");
            Debug.Log($"(-) SUMINISTROS ELÉCTRICOS FIJOS:          $-{costesSuministros:F2}");
            Debug.Log($"(-) IMPUESTO DE RETAIL LOCAL (15%):        $-{impuestoRetail:F2}");
            Debug.Log($"(-) TOTAL PÉRDIDAS POR MERMA / HURTO:      $-{totalMermas:F2}");
            
            Debug.Log("--- DESGLOSE CRÍTICO DE MERMAS ---");
            foreach (var registro in historialMermas)
            {
                Debug.Log(registro);
            }
            Debug.Log("----------------------------------");

            if (beneficioNeto >= 0)
                Debug.Log($"(=) BENEFICIO NETO DEL DÍA:                <color=green>+${beneficioNeto:F2}</color>");
            else
                Debug.Log($"(=) BALANCE DE PÉRDIDAS NETAS:             <color=red>${beneficioNeto:F2}</color>");
            Debug.Log("============================================================");

            // Resetear métricas para el día siguiente
            ingresosBrutos = 0f;
            costesAdquisicion = 0f;
            totalMermas = 0f;
            historialMermas.Clear();
        }
    }
}
