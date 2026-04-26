using System;

class CedulaValidator
{
    static void Main()
    {
        // Cédulas generadas como válidas
        string[] cedulas = {
            "0906191622",
            "1300788989",
            "2340551841",
            "1602973743",
            "1631645783",
            "0524267796",
            "2258298781",
            "2113199927",
            "0651190084",
            "0640103750"
        };

        Console.WriteLine("Validando cédulas con algoritmo Módulo 10\n");

        foreach (var cedula in cedulas)
        {
            bool isValid = ValidarCedulaEcuatoriana(cedula);
            Console.WriteLine($"{cedula}: {(isValid ? "✓ VÁLIDA" : "✗ INVÁLIDA")}");
        }
    }

    static bool ValidarCedulaEcuatoriana(string cedula)
    {
        // 1. Verificar formato
        if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10 || !System.Text.RegularExpressions.Regex.IsMatch(cedula, @"^\d{10}$"))
        {
            return false;
        }

        // 2. Validar provincia (01-24)
        int provincia = int.Parse(cedula.Substring(0, 2));
        if (provincia < 1 || provincia > 24)
        {
            return false;
        }

        // 3. Validar tercer dígito (0-5)
        int tercerDigito = int.Parse(cedula.Substring(2, 1));
        if (tercerDigito < 0 || tercerDigito > 5)
        {
            return false;
        }

        // 4. Algoritmo Módulo 10
        int[] factores = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
        int suma = 0;

        for (int i = 0; i < 9; i++)
        {
            int digito = int.Parse(cedula.Substring(i, 1));
            int producto = digito * factores[i];

            if (producto >= 10)
            {
                producto -= 9;
            }

            suma += producto;
        }

        int residuo = suma % 10;
        int digitoVerificadorCalculado = 10 - residuo;

        if (residuo == 0)
        {
            digitoVerificadorCalculado = 0;
        }

        int cedulaDigitoVerificador = int.Parse(cedula.Substring(9, 1));

        return digitoVerificadorCalculado == cedulaDigitoVerificador;
    }
}
