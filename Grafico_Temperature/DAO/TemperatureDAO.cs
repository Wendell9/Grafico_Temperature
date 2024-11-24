using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Grafico_Temperature.Models;
using Newtonsoft.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using System.Text;
public class TemperatureDAO
{
	private static string IP = "191.235.241.244";
	private static string numeroDeTemperturas = "4";
	private readonly string _apiUrl = $"http://{IP}:8666/STH/v1/contextEntities/type/Temp/id/urn:ngsi-ld:Temp:001/attributes/temperature?lastN={numeroDeTemperturas}"; // URL da sua API
	private readonly string _apiUrlGetTemperature = $"http://{IP}:1026/v2/entities/urn:ngsi-ld:Temp:001/attrs/temperature";
	private readonly string _switchLED = $"http://{IP}:1026/v2/entities/urn:ngsi-ld:Temp:001/attrs";
	private readonly string _statusLED = $"http://{IP}:1026/v2/entities/urn:ngsi-ld:Temp:001/attrs/state";

	private readonly HttpClient _httpClient;

	public TemperatureDAO()
	{
		_httpClient = new HttpClient();
		_httpClient.DefaultRequestHeaders.Add("fiware-service", "smart");
		_httpClient.DefaultRequestHeaders.Add("fiware-servicepath", "/");
	}

	public async Task SwitchLed(string command)
	{
		string url = $"http://{IP}:1026/v2/entities/urn:ngsi-ld:Temp:001/attrs";

		var headers = new Dictionary<string, string>
	{
		{ "fiware-service", "smart" },
		{ "fiware-servicepath", "/" }
	};

		// Corpo da requisição com dicionário
		var body = new Dictionary<string, object>
	{
		{
			command, new
			{
				type = "command",
				value = ""
			}
		}
	};

		using (HttpClient client = new HttpClient())
		{
			try
			{
				// Adiciona os cabeçalhos (exceto Content-Type)
				foreach (var header in headers)
				{
					client.DefaultRequestHeaders.Add(header.Key, header.Value);
				}

				// Serializa o corpo da requisição
				string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);

				// Cria o conteúdo da requisição com o Content-Type configurado automaticamente
				var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

				// Envia o PATCH
				HttpResponseMessage response = await client.PatchAsync(url, content);

				// Trata a resposta
				if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
				{
					Console.WriteLine("Command sent successfully.");
				}
				else
				{
					Console.WriteLine($"Error sending command: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
			}
		}
	}
	


	public async Task<String> GetLEDStatus()
	{
		var response = await _httpClient.GetAsync(_statusLED);

		if (response.IsSuccessStatusCode)
		{
			var content = await response.Content.ReadAsStringAsync();
			var data = JsonConvert.DeserializeObject<LastTemperatureAPIResponse>(content);
			return data.value.ToString();
		}
		else
		{
			throw new Exception("Erro ao acessar a API: " + response.StatusCode);
		}
	}

	// Método para obter os dados de temperatura
	public async Task<List<TemperatureViewModel>> GetTemperatureDataAsync()
	{
		var response = await _httpClient.GetAsync(_apiUrl);

		if (response.IsSuccessStatusCode)
		{
			var content = await response.Content.ReadAsStringAsync();
			var data = JsonConvert.DeserializeObject<TemperatureApiResponse>(content);
			return MapTemperatureData(data);
		}
		else
		{
			throw new Exception("Erro ao acessar a API: " + response.StatusCode);
		}
	}

	public async Task<double> GetLastTemperature()
	{
		var response = await _httpClient.GetAsync(_apiUrlGetTemperature);
		if (response.IsSuccessStatusCode)
		{
			var content = await response.Content.ReadAsStringAsync();
			var data = JsonConvert.DeserializeObject<LastTemperatureAPIResponse>(content);
			double temp = Convert.ToDouble(data.value, System.Globalization.CultureInfo.InvariantCulture);
			return temp;
		}
		else
		{
			throw new Exception("Erro ao acessar a API: " + response.StatusCode);
		}
	}

	// Mapear a resposta da API para o modelo TemperatureViewModel
	private List<TemperatureViewModel> MapTemperatureData(TemperatureApiResponse data)
	{
		var temperatureList = new List<TemperatureViewModel>();
		foreach (var value in data.ContextResponses[0].ContextElement.Attributes[0].Values)
		{
			var temperature = new TemperatureViewModel
			{
				Temperature = Convert.ToDouble(value.AttrValue, System.Globalization.CultureInfo.InvariantCulture),
				Timestamp = DateTime.Parse(value.RecvTime)
			};
			temperatureList.Add(temperature);
		}
		return temperatureList;
	}
}

public class LastTemperatureAPIResponse
{
	public string type;
	public string value;
	public Metadata metadata;
}

public class Metadata
{
	public TimeInstant TimeInstant;
}

public class TimeInstant
{
	public string type;
	public string value;
}

// Model para mapear a resposta da API
public class TemperatureApiResponse
{
	public List<ContextResponse> ContextResponses { get; set; }
}

public class ContextResponse
{
	public ContextElement ContextElement { get; set; }
}

public class ContextElement
{
	public List<Attribute> Attributes { get; set; }
}

public class Attribute
{
	public List<Value> Values { get; set; }
}

public class Value
{
	public string AttrValue { get; set; }
	public string RecvTime { get; set; }
}
