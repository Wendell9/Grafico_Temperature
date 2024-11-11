using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Grafico_Temperature.Models;
using Newtonsoft.Json;

public class TemperatureDAO
{
	private static string IP = "191.235.241.244:8666";
	private static string numeroDeTemperturas = "4";
    private readonly string _apiUrl = $"http://{IP}/STH/v1/contextEntities/type/Temp/id/urn:ngsi-ld:Temp:001/attributes/temperature?lastN={numeroDeTemperturas}"; // URL da sua API
	private readonly HttpClient _httpClient;

	public TemperatureDAO()
	{
		_httpClient = new HttpClient();
		_httpClient.DefaultRequestHeaders.Add("fiware-service", "smart");
		_httpClient.DefaultRequestHeaders.Add("fiware-servicepath", "/");
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

	// Mapear a resposta da API para o modelo TemperatureViewModel
	private List<TemperatureViewModel> MapTemperatureData(TemperatureApiResponse data)
	{
		var temperatureList = new List<TemperatureViewModel>();
		foreach (var value in data.ContextResponses[0].ContextElement.Attributes[0].Values)
		{
			var temperature = new TemperatureViewModel
			{
				Temperature = Convert.ToDouble(value.AttrValue),
				Timestamp = DateTime.Parse(value.RecvTime)
			};
			temperatureList.Add(temperature);
		}
		return temperatureList;
	}
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
