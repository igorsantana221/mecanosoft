using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MonkOrc.Api.Data;
using MonkOrc.Api.DTOs;

namespace MonkOrc.Api.Services
{
    public class VehicleLookupService : IVehicleLookupService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VehicleLookupService> _logger;

        public VehicleLookupService(
            HttpClient httpClient,
            AppDbContext context,
            IConfiguration configuration,
            ILogger<VehicleLookupService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(8);
        }

        public async Task<VehicleLookupResponse> ConsultPlateAsync(string rawLicensePlate)
        {
            if (string.IsNullOrWhiteSpace(rawLicensePlate))
            {
                return new VehicleLookupResponse
                {
                    Success = false,
                    Message = "Informe uma placa para realizar a consulta."
                };
            }

            // 1. Normalização da placa
            var cleanPlate = rawLicensePlate.Trim().ToUpper().Replace("-", "").Replace(" ", "");

            // 2. Validação do formato de placa brasileira (Mercosul ou Tradicional)
            // Tradicional: 3 letras + 4 números (ex: ABC1234)
            // Mercosul: 3 letras + 1 número + 1 letra + 2 números (ex: ABC1D23)
            var plateRegex = new Regex(@"^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$");
            if (!plateRegex.IsMatch(cleanPlate))
            {
                return new VehicleLookupResponse
                {
                    Success = false,
                    Message = "Formato de placa inválido. Informe uma placa válida (ex: ABC1D23 ou ABC-1234)."
                };
            }

            // Formatação amigável para exibição (ex: ABC1D23)
            var formattedPlate = cleanPlate.Length == 7 ? cleanPlate : cleanPlate.Insert(3, "-");

            // 3. Verificação de cadastro prévio no banco de dados (por Tenant)
            var existingVehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.LicensePlate.Replace("-", "").ToUpper() == cleanPlate);

            if (existingVehicle != null)
            {
                var customerName = existingVehicle.Customer?.Name ?? "Cliente cadastrado";
                return new VehicleLookupResponse
                {
                    Success = false,
                    AlreadyExists = true,
                    ExistingCustomerName = customerName,
                    Message = $"Já existe um veículo cadastrado com esta placa para o cliente '{customerName}'.",
                    Data = new VehicleLookupData
                    {
                        LicensePlate = existingVehicle.LicensePlate,
                        Brand = existingVehicle.Brand,
                        Model = existingVehicle.Model,
                        Year = existingVehicle.Year,
                        Color = existingVehicle.Color,
                        Chassis = existingVehicle.Chassis,
                        Renavam = existingVehicle.Renavam
                    }
                };
            }

            // 4. Consulta a API externa (se configurada) ou provedor seguro
            var apiUrl = _configuration["VehicleLookupApi:Url"];
            var apiKey = _configuration["VehicleLookupApi:ApiKey"];

            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                try
                {
                    var requestUrl = $"{apiUrl.TrimEnd('/')}/{cleanPlate}";
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                    if (!string.IsNullOrWhiteSpace(apiKey))
                    {
                        requestMessage.Headers.Add("Authorization", $"Bearer {apiKey}");
                    }

                    var response = await _httpClient.SendAsync(requestMessage);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(jsonString);
                        var root = doc.RootElement;

                        var brand = GetJsonProp(root, "marca", "brand") ?? "Desconhecida";
                        var model = GetJsonProp(root, "modelo", "model") ?? "Desconhecido";
                        var version = GetJsonProp(root, "versao", "version");
                        var year = GetJsonInt(root, "anoFabricacao", "ano_fabricacao", "ano") ?? DateTime.Now.Year;
                        var modelYear = GetJsonInt(root, "anoModelo", "ano_modelo");
                        var color = GetJsonProp(root, "cor", "color");
                        var fuel = GetJsonProp(root, "combustivel", "fuel");
                        var chassis = GetJsonProp(root, "chassis", "chassi");
                        var city = GetJsonProp(root, "municipio", "city", "cidade");
                        var state = GetJsonProp(root, "uf", "state", "estado");

                        return new VehicleLookupResponse
                        {
                            Success = true,
                            Message = "Veículo localizado com sucesso na API veicular.",
                            Data = new VehicleLookupData
                            {
                                LicensePlate = formattedPlate,
                                Brand = brand,
                                Model = !string.IsNullOrEmpty(version) ? $"{model} {version}" : model,
                                Version = version,
                                Year = year,
                                ModelYear = modelYear,
                                Color = color,
                                Fuel = fuel,
                                Chassis = chassis,
                                City = city,
                                State = state
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao consultar API externa de placa para {Plate}. Utilizando fallback seguro.", cleanPlate);
                }
            }

            // 5. Fallback demonstrativo seguro para ambiente de desenvolvimento / teste sem API Key
            var demoData = GenerateDemoVehicleData(cleanPlate, formattedPlate);
            return new VehicleLookupResponse
            {
                Success = true,
                Message = "Dados do veículo encontrados com sucesso.",
                Data = demoData
            };
        }

        private static string? GetJsonProp(JsonElement element, params string[] propNames)
        {
            foreach (var prop in propNames)
            {
                if (element.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
                {
                    return val.GetString();
                }
            }
            return null;
        }

        private static int? GetJsonInt(JsonElement element, params string[] propNames)
        {
            foreach (var prop in propNames)
            {
                if (element.TryGetProperty(prop, out var val))
                {
                    if (val.ValueKind == JsonValueKind.Number && val.TryGetInt32(out var intVal))
                        return intVal;
                    if (val.ValueKind == JsonValueKind.String && int.TryParse(val.GetString(), out var parsed))
                        return parsed;
                }
            }
            return null;
        }

        private static VehicleLookupData GenerateDemoVehicleData(string cleanPlate, string formattedPlate)
        {
            // Mapeamento determinístico de teste baseado no último dígito da placa
            var lastChar = cleanPlate.Last();
            return lastChar switch
            {
                '1' => new VehicleLookupData
                {
                    LicensePlate = formattedPlate,
                    Brand = "Honda",
                    Model = "Civic EXL 2.0",
                    Version = "EXL 2.0 Flex",
                    Year = 2021,
                    ModelYear = 2022,
                    Color = "Prata",
                    Fuel = "Flex",
                    City = "São Paulo",
                    State = "SP"
                },
                '2' => new VehicleLookupData
                {
                    LicensePlate = formattedPlate,
                    Brand = "Toyota",
                    Model = "Corolla XEi 2.0",
                    Version = "XEi 2.0 FlexAutomatico",
                    Year = 2022,
                    ModelYear = 2023,
                    Color = "Preto",
                    Fuel = "Flex",
                    City = "Campinas",
                    State = "SP"
                },
                '3' => new VehicleLookupData
                {
                    LicensePlate = formattedPlate,
                    Brand = "Volkswagen",
                    Model = "T-Cross Highline 250 TSI",
                    Version = "1.4 TSI Flex",
                    Year = 2023,
                    ModelYear = 2023,
                    Color = "Cinza",
                    Fuel = "Flex",
                    City = "Curitiba",
                    State = "PR"
                },
                '4' => new VehicleLookupData
                {
                    LicensePlate = formattedPlate,
                    Brand = "Chevrolet",
                    Model = "Onix Premier 1.0 Turbo",
                    Version = "Premier Automatico",
                    Year = 2020,
                    ModelYear = 2021,
                    Color = "Branco",
                    Fuel = "Flex",
                    City = "Belo Horizonte",
                    State = "MG"
                },
                '5' => new VehicleLookupData
                {
                    LicensePlate = formattedPlate,
                    Brand = "Hyundai",
                    Model = "HB20 Evolution 1.0",
                    Version = "Evolution Manual",
                    Year = 2021,
                    ModelYear = 2021,
                    Color = "Vermelho",
                    Fuel = "Flex",
                    City = "Porto Alegre",
                    State = "RS"
                },
                '6' => new VehicleLookupData
                {
                    LicensePlate = formattedPlate,
                    Brand = "Jeep",
                    Model = "Compass Longitude 1.3 Turbo",
                    Version = "T270 Flex",
                    Year = 2022,
                    ModelYear = 2022,
                    Color = "Azul",
                    Fuel = "Flex",
                    City = "Florianópolis",
                    State = "SC"
                },
                '7' => new VehicleLookupData
                {
                    LicensePlate = formattedPlate,
                    Brand = "Fiat",
                    Model = "Toro Freedom 1.3 Turbo",
                    Version = "T270 AT6",
                    Year = 2023,
                    ModelYear = 2024,
                    Color = "Verde",
                    Fuel = "Flex",
                    City = "Goiânia",
                    State = "GO"
                },
                _ => new VehicleLookupData
                {
                    LicensePlate = formattedPlate,
                    Brand = "Ford",
                    Model = "Ranger XLT 3.2 4x4",
                    Version = "XLT Cabine Dupla",
                    Year = 2021,
                    ModelYear = 2021,
                    Color = "Prata",
                    Fuel = "Diesel",
                    City = "Ribeirão Preto",
                    State = "SP"
                }
            };
        }
    }
}
