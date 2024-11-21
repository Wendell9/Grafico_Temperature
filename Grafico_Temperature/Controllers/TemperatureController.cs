using Grafico_Temperature.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TemperatureController : Controller
{
	private readonly TemperatureDAO _temperatureDAO;

	public TemperatureController()
	{
		_temperatureDAO = new TemperatureDAO(); // Inicializa a classe DAO
	}

	// Método para obter os dados da API e passar para a View
	public async Task<IActionResult> Index()
	{
		try
		{
			// Chama o DAO para obter os dados da API
			var temperatureData = await _temperatureDAO.GetTemperatureDataAsync();

			// Preenche o ViewModel diretamente no Controller
			var viewModel = new TemperatureGraphViewModel();
			viewModel.Temperatures = temperatureData.Select(t => t.Temperature).ToList();
			viewModel.Timestamps = temperatureData.Select(t => t.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")).ToList();
			viewModel.AverageTemperature = 0.0;
			viewModel.LastTemperature = 0.0;

			return View(viewModel); // Passa o ViewModel para a View
		}
		catch (Exception ex)
		{
			// Em caso de erro na requisição da API, exibe a mensagem de erro
			ViewBag.ErrorMessage = "Erro ao obter dados: " + ex.Message;
			return View();
		}


	}
	[HttpGet]
	public async Task<IActionResult> GetTemperatureData()
	{
		var timestamps = new List<string> { /* Dados de timestamp */ };
		var temperatures = new List<double> { /* Dados de temperatura */ };

		var temperatureData = await _temperatureDAO.GetTemperatureDataAsync();

		// Preenche o ViewModel diretamente no Controller
		temperatures = temperatureData.Select(t => t.Temperature).ToList();
		timestamps = temperatureData.Select(t => t.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")).ToList();
		var lastTemperature = await _temperatureDAO.GetLastTemperature();

		return Json(new
		{
			Timestamps = timestamps,
			Temperatures = temperatures,
			Lasttemperature = lastTemperature
		});
	}

	public IActionResult RenderGraficoTemperatura(int setPoint)
	{
		TemperatureGraphViewModel gr = new TemperatureGraphViewModel();
		gr.SetPoint = setPoint;
		// Aqui, você passa o modelo necessário para a partial view
		return PartialView("~/Views/Temperature/pvGraficoTemperatura.cshtml", gr);
	}


}